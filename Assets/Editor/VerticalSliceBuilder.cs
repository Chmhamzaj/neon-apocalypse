#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.AI;
using System.IO;
using NeonApocalypse.AI;
using NeonApocalypse.AI.Squad;
using NeonApocalypse.Campaign;
using NeonApocalypse.Combat;
using NeonApocalypse.Core;
using NeonApocalypse.Player;
using NeonApocalypse.Progression;
using NeonApocalypse.UI;
using NeonApocalypse.World;
using NeonApocalypse.Presentation;
using NeonApocalypse.Performance;
using NeonApocalypse.World.Mega;
using NeonApocalypse.Vehicles;

namespace NeonApocalypse.EditorTools
{
    public static class VerticalSliceBuilder
    {
        private const string ScenePath = "Assets/Scenes/NeonApocalypse_VerticalSlice.unity";
        private static Material floorMat;
        private static Material wallMat;
        private static Material accentMat;
        private static Material enemyMat;
        private static Material eliteMat;
        private static Material bossMat;
        private static Material playerMat;
        private static Material lootMat;
        private static Material rubbleMat;

        [MenuItem("Neon Apocalypse/Build Vertical Slice")]
        public static void Build()
        {
            EnsureFolders();
            CreateMaterials();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            RenderSettings.ambientLight = new Color(0.16f, 0.18f, 0.24f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.045f, 0.06f, 0.09f);
            RenderSettings.fogDensity = 0.0085f;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            CreateLighting();
            CreateArena();
            var player = CreatePlayer();
            CreateRegion01Layout(player.transform);
            CreateProductionWorld(player.transform);
            CreateHDProductionAssets();
            CreateHDContentManifest();
            CreateWorldDirector();
            CreateWorldStreaming();
            CreateMegaWorld(player.transform);
            CreateOpenWorldGameplay(player.transform);
            CreateNPCIntelligenceLayer();
            CreatePlayerVehicle(player.transform);
            CreateSpawnDirector();
            CreateAISquadCoordinator();
            CreateCamera(player.transform);
            CreateEnemies(player.transform);
            CreateBoss();
            CreateMissionSystem();
            CreateCampaignDatabase();
            CreateHUD();
            CreatePresentation(player.transform);
            CreatePerformanceDirector();
            CreateGameManager();
            CreateSessionBootstrap();
            EditorSceneManager.SaveScene(scene, ScenePath);
            SetAndroidDefaults();
            Selection.activeGameObject = player;
            Debug.Log("NEON APOCALYPSE vertical slice created at " + ScenePath);
            EditorUtility.DisplayDialog("NEON APOCALYPSE", "Vertical slice built. Open the generated scene and press Play.", "Let's Go");
        }

        private static void EnsureFolders()
        {
            string[] folders = { "Assets/Scenes", "Assets/Prefabs", "Assets/Art", "Assets/Materials" };
            foreach (string folder in folders)
            {
                if (!AssetDatabase.IsValidFolder(folder))
                {
                    string parent = Path.GetDirectoryName(folder).Replace("\\", "/");
                    string name = Path.GetFileName(folder);
                    AssetDatabase.CreateFolder(parent, name);
                }
            }
        }

        private static void CreateMaterials()
        {
            AssetDatabase.Refresh();
            floorMat = MakeMat("NA_Floor", new Color(0.035f, 0.045f, 0.07f), 0.75f, 0.25f, "T_Asphalt_HD");
            wallMat = MakeMat("NA_Wall", new Color(0.10f, 0.12f, 0.16f), 0.65f, 0.15f, "T_Concrete_HD");
            accentMat = MakeMat("NA_Accent", new Color(0.0f, 0.9f, 1.0f), 0.25f, 0.7f, null);
            enemyMat = MakeMat("NA_Enemy", new Color(0.42f, 0.07f, 0.08f), 0.35f, 0.2f, "T_Metal_HD");
            eliteMat = MakeMat("NA_Elite", new Color(0.55f, 0.15f, 0.03f), 0.35f, 0.25f, "T_Metal_HD");
            bossMat = MakeMat("NA_Boss", new Color(0.25f, 0.03f, 0.35f), 0.25f, 0.6f, "T_Panel_HD");
            playerMat = MakeMat("NA_Player", new Color(0.12f, 0.48f, 0.65f), 0.25f, 0.35f, "T_Panel_HD");
            lootMat = MakeMat("NA_Loot", new Color(0.15f, 0.85f, 0.35f), 0.2f, 0.8f, null);
            SetEmission(accentMat, new Color(0.0f, 1.6f, 2.0f));
            SetEmission(lootMat, new Color(0.2f, 1.0f, 0.4f));
            rubbleMat = MakeMat("NA_Rubble", new Color(0.30f, 0.27f, 0.24f), 0.65f, 0.18f, "T_Rubble_HD");

            BindHDMaterialSet(floorMat, "Asphalt_Wet");
            BindHDMaterialSet(wallMat, "Concrete_Aged");
            BindHDMaterialSet(enemyMat, "Metal_Painted");
            BindHDMaterialSet(eliteMat, "Metal_Rusted");
            BindHDMaterialSet(bossMat, "Panel_Future");
            BindHDMaterialSet(playerMat, "Panel_Future");
            BindHDMaterialSet(rubbleMat, "Rubble_Stone");
        }


        private static void BindHDMaterialSet(Material mat, string materialKey)
        {
            if (!mat) return;
            string root = "Assets/Resources/HDMaterials/";
            string albedoPath = root + "T_" + materialKey + "_ALBEDO_4K.png";
            string normalPath = root + "T_" + materialKey + "_NORMAL_4K.png";
            string ormPath = root + "T_" + materialKey + "_ORM_4K.png";
            if (!AssetDatabase.LoadAssetAtPath<Texture2D>(albedoPath))
                albedoPath = root + "T_" + materialKey + "_ALBEDO_2K.jpg";
            if (!AssetDatabase.LoadAssetAtPath<Texture2D>(albedoPath))
                albedoPath = root + "T_" + materialKey + "_ALBEDO_2K.png";
            if (!AssetDatabase.LoadAssetAtPath<Texture2D>(normalPath))
                normalPath = root + "T_" + materialKey + "_NORMAL_2K.png";
            if (!AssetDatabase.LoadAssetAtPath<Texture2D>(ormPath))
                ormPath = root + "T_" + materialKey + "_ORM_2K.jpg";
            if (!AssetDatabase.LoadAssetAtPath<Texture2D>(ormPath))
                ormPath = root + "T_" + materialKey + "_ORM_2K.png";

            var albedo = AssetDatabase.LoadAssetAtPath<Texture2D>(albedoPath);
            var normal = AssetDatabase.LoadAssetAtPath<Texture2D>(normalPath);
            var orm = AssetDatabase.LoadAssetAtPath<Texture2D>(ormPath);
            if (albedo)
            {
                if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", albedo);
                if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", albedo);
            }
            if (normal && mat.HasProperty("_BumpMap"))
            {
                mat.EnableKeyword("_NORMALMAP");
                mat.SetTexture("_BumpMap", normal);
            }
            if (orm)
            {
                if (mat.HasProperty("_MetallicGlossMap")) mat.SetTexture("_MetallicGlossMap", orm);
                if (mat.HasProperty("_OcclusionMap")) mat.SetTexture("_OcclusionMap", orm);
            }
            mat.enableInstancing = true;
            EditorUtility.SetDirty(mat);
        }

        private static Material MakeMat(string name, Color color, float metallic, float smoothness, string textureStem)
        {
            string path = $"Assets/Materials/{name}.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (!mat)
            {
                mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
                AssetDatabase.CreateAsset(mat, path);
            }
            mat.color = color;
            mat.SetFloat("_Metallic", metallic);
            mat.SetFloat("_Smoothness", smoothness);
            mat.enableInstancing = true;
            if (!string.IsNullOrEmpty(textureStem))
            {
                var tex = AssetDatabase.LoadAssetAtPath<Texture2D>($"Assets/Art/GeneratedTextures/{textureStem}.png");
                if (tex)
                {
                    if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", tex);
                    if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", tex);
                }
            }
            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static void SetEmission(Material mat, Color color)
        {
            if (!mat) return;
            if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", color);
            mat.EnableKeyword("_EMISSION");
            EditorUtility.SetDirty(mat);
        }

        private static void CreateLighting()
        {
            var sun = new GameObject("Sun");
            var light = sun.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            light.color = new Color(0.72f, 0.82f, 1f);
            sun.transform.rotation = Quaternion.Euler(48f, -35f, 0);

            for (int i = 0; i < 8; i++)
            {
                var g = new GameObject("NeonLight_" + i);
                var l = g.AddComponent<Light>();
                l.type = LightType.Point;
                l.range = 15f;
                l.intensity = 5f;
                l.color = i % 2 == 0 ? new Color(0f, 0.8f, 1f) : new Color(0.8f, 0.1f, 1f);
                g.transform.position = new Vector3(-18 + (i % 4) * 12, 4, -12 + (i / 4) * 24);
            }
        }

        private static void CreateArena()
        {
            var root = new GameObject("FallenCity_TrainingDistrict");
            CreateCube("Ground", root.transform, new Vector3(0, -0.5f, 0), new Vector3(70, 1, 70), floorMat);
            CreateCube("NorthWall", root.transform, new Vector3(0, 5, 34), new Vector3(70, 10, 1), wallMat);
            CreateCube("SouthWall", root.transform, new Vector3(0, 5, -34), new Vector3(70, 10, 1), wallMat);
            CreateCube("WestWall", root.transform, new Vector3(-34, 5, 0), new Vector3(1, 10, 70), wallMat);
            CreateCube("EastWall", root.transform, new Vector3(34, 5, 0), new Vector3(1, 10, 70), wallMat);

            for (int i = 0; i < 20; i++)
            {
                float x = ((i * 17) % 52) - 26;
                float z = ((i * 29) % 52) - 26;
                if (Mathf.Abs(x) < 8f && Mathf.Abs(z) < 8f) continue;
                float h = 1.5f + (i % 4) * 1.2f;
                var block = CreateCube("RuinBlock_" + i, root.transform, new Vector3(x, h * 0.5f, z), new Vector3(2.5f + (i % 3), h, 2.2f + ((i + 1) % 3)), wallMat);
                block.transform.rotation = Quaternion.Euler(0, (i * 17) % 30, 0);
            }

            for (int i = 0; i < 12; i++)
            {
                var beacon = CreateCube("NeonBeacon_" + i, root.transform, new Vector3(-28 + (i % 6) * 11, 2f, -28 + (i / 6) * 56), new Vector3(0.35f, 4f, 0.35f), accentMat);
                var l = beacon.AddComponent<Light>();
                l.type = LightType.Point;
                l.range = 7f;
                l.intensity = 3f;
                l.color = (i % 2 == 0) ? Color.cyan : new Color(0.85f, 0.15f, 1f);
            }
        }

        private static GameObject CreatePlayer()
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = "Player_Hero";
            go.tag = "Player";
            go.transform.position = new Vector3(0, 1.2f, -20);
            go.layer = 2;
            go.transform.localScale = new Vector3(0.7f, 1.1f, 0.7f);
            go.GetComponent<Renderer>().sharedMaterial = playerMat;
            Object.DestroyImmediate(go.GetComponent<CapsuleCollider>());
            CreateHeroArmor(go.transform);
            var cc = go.AddComponent<CharacterController>();
            cc.height = 2.2f;
            cc.radius = 0.45f;
            cc.center = new Vector3(0, 0, 0);
            var health = go.AddComponent<Health>();
            health.maxHealth = 250f;
            var weaponRoot = new GameObject("PhotonRifle");
            weaponRoot.transform.SetParent(go.transform);
            weaponRoot.transform.localPosition = new Vector3(0.55f, 0.5f, 0.8f);
            weaponRoot.transform.localRotation = Quaternion.identity;
            var barrel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            barrel.name = "RifleMesh";
            barrel.transform.SetParent(weaponRoot.transform);
            barrel.transform.localPosition = new Vector3(0, 0, 0.7f);
            barrel.transform.localRotation = Quaternion.Euler(90, 0, 0);
            barrel.transform.localScale = new Vector3(0.09f, 0.7f, 0.09f);
            barrel.GetComponent<Renderer>().sharedMaterial = wallMat;
            Object.DestroyImmediate(barrel.GetComponent<CapsuleCollider>());
            var muzzle = new GameObject("Muzzle");
            muzzle.transform.SetParent(weaponRoot.transform);
            muzzle.transform.localPosition = new Vector3(0, 0, 1.4f);
            var weapon = weaponRoot.AddComponent<Weapon>();
            weapon.muzzle = muzzle.transform;
            weapon.damage = 34f;
            weapon.fireRate = 9f;
            weapon.magazineSize = 30;
            weapon.reloadSeconds = 1.15f;
            weapon.range = 120f;
            var pc = go.AddComponent<PlayerController>();
            pc.equippedWeapon = weapon;
            return go;
        }


        private static void CreateHeroArmor(Transform hero)
        {
            var chest = CreateCube("Hero_ChestArmor", hero, new Vector3(0, 0.35f, 0.05f), new Vector3(0.95f, 0.9f, 0.48f), playerMat);
            chest.transform.localRotation = Quaternion.Euler(-6f, 0f, 0f);
            var shoulderL = CreateCube("Hero_Shoulder_L", hero, new Vector3(-0.62f, 0.48f, 0f), new Vector3(0.26f, 0.42f, 0.52f), wallMat);
            var shoulderR = CreateCube("Hero_Shoulder_R", hero, new Vector3(0.62f, 0.48f, 0f), new Vector3(0.26f, 0.42f, 0.52f), wallMat);
            var helmet = CreateCube("Hero_Helmet", hero, new Vector3(0f, 1.15f, 0.02f), new Vector3(0.82f, 0.45f, 0.72f), wallMat);
            var visor = CreateCube("Hero_Visor", hero, new Vector3(0f, 1.18f, 0.38f), new Vector3(0.62f, 0.15f, 0.08f), accentMat);
            var pack = CreateCube("Hero_Backpack", hero, new Vector3(0f, 0.45f, -0.43f), new Vector3(0.68f, 0.78f, 0.22f), wallMat);
            foreach (var r in hero.GetComponentsInChildren<Renderer>()) r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        }

        private static void CreateRegion01Layout(Transform player)
        {
            var root = new GameObject("Region01_FallenCity_ProductionLayout");
            var chapter = new[]
            {
                new Vector3(-24f, 0f, -4f), new Vector3(-8f, 0f, 6f),
                new Vector3(10f, 0f, 15f), new Vector3(0f, 0f, 28f)
            };
            for (int i = 0; i < chapter.Length; i++)
            {
                var marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                marker.name = "StoryCheckpoint_" + (i + 1);
                marker.transform.SetParent(root.transform);
                marker.transform.position = chapter[i];
                marker.transform.localScale = new Vector3(1.4f, 0.08f, 1.4f);
                marker.GetComponent<Renderer>().sharedMaterial = accentMat;
                Object.DestroyImmediate(marker.GetComponent<Collider>());
            }

            // District landmarks: deliberately modular so final art can replace each block without touching gameplay.
            for (int i = 0; i < 14; i++)
            {
                float angle = i * 25.7f * Mathf.Deg2Rad;
                float radius = 13f + (i % 4) * 3.5f;
                var landmark = CreateCube("DistrictLandmark_" + i, root.transform,
                    new Vector3(Mathf.Cos(angle) * radius, 2.5f + (i % 3) * 1.8f, Mathf.Sin(angle) * radius + 8f),
                    new Vector3(3f + (i % 3), 5f + (i % 4) * 2f, 3f + ((i + 1) % 3)), wallMat);
                landmark.transform.rotation = Quaternion.Euler(0f, i * 17f, 0f);
                var activation = landmark.AddComponent<DistanceActivation>();
                activation.target = player;
                activation.activeDistance = 105f;
            }

            var directorGo = new GameObject("Region01Director");
            var director = directorGo.AddComponent<NeonApocalypse.Campaign.Region01Director>();
            director.player = player;
            director.checkpoints = new Transform[chapter.Length];
            for (int i = 0; i < chapter.Length; i++) director.checkpoints[i] = root.transform.Find("StoryCheckpoint_" + (i + 1));
        }

        private static void CreateWorldDirector()
        {
            var sun = GameObject.Find("Sun");
            var go = new GameObject("WorldDirector");
            var director = go.AddComponent<WorldDirector>();
            director.sun = sun ? sun.GetComponent<Light>() : null;
            director.timeOfDay = 20f;
        }

        private static void CreateWorldStreaming()
        {
            var go = new GameObject("WorldChunkStreamer");
            var streamer = go.AddComponent<WorldChunkStreamer>();
            streamer.activeRadius = 1;
            streamer.checkEvery = 0.25f;
        }


        private static void CreateMegaWorld(Transform player)
        {
            string cfgPath = "Assets/Resources/MegaWorldConfig.asset";
            var cfg = AssetDatabase.LoadAssetAtPath<MegaWorldConfig>(cfgPath);
            if (!cfg)
            {
                cfg = ScriptableObject.CreateInstance<MegaWorldConfig>();
                AssetDatabase.CreateAsset(cfg, cfgPath);
            }
            cfg.cellsX = 30; cfg.cellsZ = 24; cfg.cellSize = 700f; cfg.seed = 917401;
            cfg.activeRadius = 1; cfg.visualRadius = 2; cfg.buildingsPerUrbanCell = 28; cfg.propsPerCell = 18;
            EditorUtility.SetDirty(cfg); AssetDatabase.SaveAssets();

            var worldGo = new GameObject("MEGA_WORLD_21x16_8KM");
            var mega = worldGo.AddComponent<MegaWorldDirector>();
            mega.config = cfg; mega.player = player;
            mega.groundMaterial = floorMat; mega.buildingMaterial = wallMat; mega.accentMaterial = accentMat; mega.propMaterial = rubbleMat;
            var quality = worldGo.AddComponent<MegaWorldRuntimeQuality>();
            quality.targetCamera = Camera.main; quality.mobileSafe = true;
            var telemetry = worldGo.AddComponent<WorldScaleTelemetry>(); telemetry.config = cfg;
        }

        private static void CreateSpawnDirector()
        {
            var go = new GameObject("SpawnDirector");
            var director = go.AddComponent<SpawnDirector>();
            director.maxAlive = 14;
            director.spawnEvery = 10f;
            director.burst = 2;
            director.profiles = new[]
            {
                new SpawnDirector.SpawnProfile { archetype = EnemyArchetype.Grunt, weight = 5 },
                new SpawnDirector.SpawnProfile { archetype = EnemyArchetype.Rusher, weight = 3 },
                new SpawnDirector.SpawnProfile { archetype = EnemyArchetype.Gunner, weight = 2 },
                new SpawnDirector.SpawnProfile { archetype = EnemyArchetype.Elite, weight = 1 }
            };

            director.spawnPoints = new Transform[6];
            for (int i = 0; i < 6; i++)
            {
                var point = new GameObject("SpawnPoint_" + i);
                point.transform.position = new Vector3(-26 + (i % 3) * 26, 1f, 10 + (i / 3) * 15);
                director.spawnPoints[i] = point.transform;
            }
        }

        private static void CreateCamera(Transform player)
        {
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.fieldOfView = 72f;
            cam.nearClipPlane = 0.05f;
            cam.farClipPlane = 350f;
            camGo.AddComponent<AudioListener>();
            var rig = camGo.AddComponent<SimpleThirdPersonCamera>();
            rig.target = player;
            rig.offset = new Vector3(0, 4.5f, -7.5f);
        }

        private static void CreateEnemies(Transform player)
        {
            int index = 0;
            EnemyArchetype[] types = { EnemyArchetype.Grunt, EnemyArchetype.Grunt, EnemyArchetype.Rusher, EnemyArchetype.Gunner, EnemyArchetype.Elite, EnemyArchetype.Grunt, EnemyArchetype.Rusher, EnemyArchetype.Gunner };
            Vector3[] positions = { new(-12, 1, -8), new(12, 1, -4), new(-16, 1, 5), new(16, 1, 7), new(-10, 1, 17), new(11, 1, 17), new(-20,1,24), new(20,1,24) };
            foreach (var type in types)
            {
                var go = GameObject.CreatePrimitive(type == EnemyArchetype.Gunner ? PrimitiveType.Cube : PrimitiveType.Capsule);
                go.name = "Enemy_" + type + "_" + index++;
                go.transform.position = positions[index - 1];
                go.transform.localScale = type == EnemyArchetype.Elite ? Vector3.one * 1.35f : Vector3.one;
                go.GetComponent<Renderer>().sharedMaterial = type == EnemyArchetype.Elite ? eliteMat : enemyMat;
                Object.DestroyImmediate(go.GetComponent<Collider>());
                go.AddComponent<SphereCollider>();
                var h = go.AddComponent<Health>();
                h.maxHealth = type == EnemyArchetype.Elite ? 650f : type == EnemyArchetype.Gunner ? 180f : 140f;
                var ai = go.AddComponent<EnemyAI>();
                ai.archetype = type;
                CreateFloatingHealthBar(go, 1.65f);
            }
        }

        private static void CreateBoss()
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = "BOSS_NEMESIS";
            go.transform.position = new Vector3(0, 2.4f, 25);
            go.transform.localScale = new Vector3(2.3f, 2.4f, 2.3f);
            go.GetComponent<Renderer>().sharedMaterial = bossMat;
            Object.DestroyImmediate(go.GetComponent<Collider>());
            var col = go.AddComponent<CapsuleCollider>();
            col.radius = 1f;
            col.height = 2f;
            var health = go.AddComponent<Health>();
            health.maxHealth = 1800f;
            var boss = go.AddComponent<BossAI>();
            boss.maxHealth = 1800f;
            CreateFloatingHealthBar(go, 5f, true);
        }

        private static void CreateFloatingHealthBar(GameObject owner, float height, bool boss = false)
        {
            var canvas = new GameObject("HealthBarCanvas");
            canvas.transform.SetParent(owner.transform);
            canvas.transform.localPosition = Vector3.up * height;
            var c = canvas.AddComponent<Canvas>();
            c.renderMode = RenderMode.WorldSpace;
            canvas.AddComponent<CanvasScaler>();
            var bg = new GameObject("BG");
            bg.transform.SetParent(canvas.transform, false);
            var image = bg.AddComponent<Image>();
            image.color = Color.black;
            var bgRt = bg.GetComponent<RectTransform>();
            bgRt.sizeDelta = boss ? new Vector2(3.8f, 0.35f) : new Vector2(1.8f, 0.22f);
            var fill = new GameObject("Fill");
            fill.transform.SetParent(bg.transform, false);
            var slider = fill.AddComponent<Slider>();
            slider.minValue = 0; slider.maxValue = 1; slider.value = 1;
            slider.direction = Slider.Direction.LeftToRight;
            var rt = fill.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            var whb = canvas.AddComponent<WorldHealthBar>();
            whb.slider = slider;
            whb.target = owner.GetComponent<Health>();
            canvas.transform.localScale = boss ? Vector3.one * 0.16f : Vector3.one * 0.1f;
        }

        private static void CreateMissionSystem()
        {
            var go = new GameObject("MissionSystem");
            var mission = go.AddComponent<MissionSystem>();
            mission.missionTitle = "THE FALLEN CITY";
            mission.activeObjectives = new[]
            {
                new Objective { type = ObjectiveType.Kill, id = "Grunt", required = 3 },
                new Objective { type = ObjectiveType.Collect, id = "NEON_SHARD", required = 3 },
                new Objective { type = ObjectiveType.Kill, id = "BOSS", required = 1 }
            };
        }

        private static void CreateCampaignDatabase()
        {
            if (!Object.FindFirstObjectByType<CampaignDatabase>())
                new GameObject("CampaignDatabase").AddComponent<CampaignDatabase>();
        }

        private static void CreateHUD()
        {
            var canvasGo = new GameObject("HUD_Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasGo.AddComponent<GraphicRaycaster>();
            if (!Object.FindFirstObjectByType<EventSystem>())
            {
                var es = new GameObject("EventSystem");
                es.AddComponent<EventSystem>();
                es.AddComponent<StandaloneInputModule>();
            }

            var hud = canvasGo.AddComponent<MobileHud>();
            hud.healthText = CreateText(canvasGo.transform, "Health", "HP", new Vector2(40, -40), new Vector2(500, 70), 28, TextAnchor.UpperLeft);
            hud.xpText = CreateText(canvasGo.transform, "XP", "LV 1 XP 0", new Vector2(40, -100), new Vector2(700, 60), 24, TextAnchor.UpperLeft);
            hud.ammoText = CreateText(canvasGo.transform, "Ammo", "AMMO 30/30", new Vector2(-40, -40), new Vector2(400, 70), 28, TextAnchor.UpperRight);
            hud.missionText = CreateText(canvasGo.transform, "Mission", "THE FALLEN CITY", new Vector2(0, -45), new Vector2(700, 60), 30, TextAnchor.UpperCenter);
            hud.messageText = CreateText(canvasGo.transform, "Message", "", new Vector2(0, -135), new Vector2(1000, 100), 34, TextAnchor.UpperCenter);

            var joystickGo = CreateUIElement("Joystick", canvasGo.transform, new Vector2(220, 210), new Vector2(300, 300));
            var joyImage = joystickGo.AddComponent<Image>(); joyImage.color = new Color(0.1f, 0.18f, 0.25f, 0.45f);
            var joy = joystickGo.AddComponent<MobileJoystick>();
            var handle = CreateUIElement("Handle", joystickGo.transform, Vector2.zero, new Vector2(115, 115));
            var handleImage = handle.AddComponent<Image>(); handleImage.color = new Color(0.2f, 0.8f, 1f, 0.65f);
            joy.handle = handle.GetComponent<RectTransform>(); joy.radius = 105f;
            hud.joystick = joy;

            hud.fireButton = CreateButton(canvasGo.transform, "FIRE", new Vector2(-205, 235), new Vector2(170, 170));
            hud.reloadButton = CreateButton(canvasGo.transform, "RELOAD", new Vector2(-90, 400), new Vector2(130, 80));
            hud.dashButton = CreateButton(canvasGo.transform, "DASH", new Vector2(-340, 365), new Vector2(130, 80));
            var driveButton = CreateButton(canvasGo.transform, "DRIVE", new Vector2(-455, 235), new Vector2(170, 90));
            driveButton.onClick.AddListener(() => {
                if (NeonApocalypse.Vehicles.VehicleEnterExit.ActiveVehicle)
                    NeonApocalypse.Vehicles.VehicleEnterExit.ExitActiveVehicle();
                else
                    NeonApocalypse.Vehicles.VehicleEnterExit.EnterNearestVehicle(4.5f);
            });
        }

        private static Text CreateText(Transform parent, string name, string value, Vector2 position, Vector2 size, int fontSize, TextAnchor anchor)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(position.x < 0 ? 1 : position.x > 0 ? 0 : 0.5f, position.y < 0 ? 1 : 0);
            rt.pivot = new Vector2(position.x < 0 ? 1 : position.x > 0 ? 0 : 0.5f, 1);
            rt.anchoredPosition = new Vector2(position.x, position.y);
            rt.sizeDelta = size;
            var text = go.AddComponent<Text>();
            text.text = value;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = anchor;
            text.color = Color.white;
            return text;
        }

        private static Button CreateButton(Transform parent, string label, Vector2 anchored, Vector2 size)
        {
            var go = CreateUIElement(label + "Button", parent, anchored, size);
            var img = go.AddComponent<Image>(); img.color = new Color(0.05f, 0.2f, 0.3f, 0.72f);
            var button = go.AddComponent<Button>();
            var textGo = new GameObject("Text");
            textGo.transform.SetParent(go.transform, false);
            var txt = textGo.AddComponent<Text>();
            txt.text = label; txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); txt.fontSize = 22; txt.alignment = TextAnchor.MiddleCenter; txt.color = Color.white;
            var rt = textGo.GetComponent<RectTransform>(); rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            return button;
        }

        private static GameObject CreateUIElement(string name, Transform parent, Vector2 anchored, Vector2 size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(anchored.x < 0 ? 1 : anchored.x > 0 ? 0 : 0.5f, anchored.y < 0 ? 1 : 0);
            rt.pivot = new Vector2(anchored.x < 0 ? 1 : anchored.x > 0 ? 0 : 0.5f, anchored.y < 0 ? 1 : 0);
            rt.anchoredPosition = anchored;
            rt.sizeDelta = size;
            return go;
        }

        private static GameObject CreateCube(string name, Transform parent, Vector3 position, Vector3 scale, Material mat)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent);
            go.transform.position = position;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = mat;
            return go;
        }



        private static void CreateOpenWorldGameplay(Transform player)
        {
            var go = new GameObject("OPEN_WORLD_GAMEPLAY");
            var director = go.AddComponent<NeonApocalypse.World.OpenWorld.OpenWorldGameplayDirector>();
            director.player = player;
        }

        private static void CreateNPCIntelligenceLayer()
        {
            var aiGo = new GameObject("NPC_INTELLIGENCE_LAYER");
            aiGo.AddComponent<NeonApocalypse.World.OpenWorld.PoliceInvestigationDirector>();
            aiGo.AddComponent<NeonApocalypse.World.OpenWorld.CrimeEvidenceSystem>();
            aiGo.AddComponent<NeonApocalypse.AI.Tactics.TacticalCoverDirector>();
            aiGo.AddComponent<NeonApocalypse.AI.Squad.CompanionRelationshipSystem>();
            aiGo.AddComponent<NeonApocalypse.World.OpenWorld.FactionConflictDirector>();
            aiGo.AddComponent<NeonApocalypse.AI.Squad.CompanionTacticalDirector>();
            aiGo.AddComponent<NeonApocalypse.Vehicles.TrafficScheduleDirector>();
            aiGo.AddComponent<NeonApocalypse.Performance.NPCSimulationLODDirector>();
        }

        private static void CreatePlayerVehicle(Transform player)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "PLAYER_VEHICLE_VANGUARD";
            go.transform.position = new Vector3(4.5f, 1.05f, -17.5f);
            go.transform.localScale = new Vector3(1.9f, 1.15f, 4.2f);
            go.GetComponent<Renderer>().sharedMaterial = accentMat;
            var bodyCollider = go.GetComponent<BoxCollider>();
            if (bodyCollider) bodyCollider.size = new Vector3(1f, 0.78f, 1f);
            var rb = go.AddComponent<Rigidbody>();
            rb.mass = 1450f;
            rb.drag = 0.18f;
            rb.angularDrag = 3.0f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            var health = go.AddComponent<Health>();
            health.maxHealth = 1200f;
            var vehicle = go.AddComponent<VehicleController>();
            var spec = go.AddComponent<NeonApocalypse.Vehicles.VehicleSpecBinder>();
            spec.spec = new NeonApocalypse.Vehicles.VehicleSpec { id = "vanguard_armored", vehicleClass = NeonApocalypse.Vehicles.VehicleClass.Armored, maxSpeedKph = 210f, acceleration = 1.08f, steering = 0.92f, armor = 3.8f, grip = 7.5f };
            vehicle.engineForce = 11500f;
            vehicle.reverseForce = 5600f;
            vehicle.maxSpeed = 36f;
            vehicle.steeringTorque = 5.5f;
            vehicle.lateralGrip = 8.0f;
            vehicle.downforce = 34f;

            var seat = new GameObject("DriverSeat");
            seat.transform.SetParent(go.transform, false);
            seat.transform.localPosition = new Vector3(0f, 0.55f, 0.15f);
            vehicle.driverSeat = seat.transform;

            var exit = new GameObject("ExitPoint");
            exit.transform.SetParent(go.transform, false);
            exit.transform.localPosition = new Vector3(1.8f, 0f, 0f);
            vehicle.exitPoint = exit.transform;

            var camTarget = new GameObject("VehicleCameraTarget");
            camTarget.transform.SetParent(go.transform, false);
            camTarget.transform.localPosition = new Vector3(0f, 1.5f, 0.2f);
            vehicle.cameraTarget = camTarget.transform;
            go.AddComponent<VehicleMobileInput>();

            var interaction = go.AddComponent<VehicleEnterExit>();
            interaction.interactionRadius = 4.2f;

            BuildWheelVisual(go.transform, new Vector3(-0.95f, -0.35f, 1.4f));
            BuildWheelVisual(go.transform, new Vector3(0.95f, -0.35f, 1.4f));
            BuildWheelVisual(go.transform, new Vector3(-0.95f, -0.35f, -1.4f));
            BuildWheelVisual(go.transform, new Vector3(0.95f, -0.35f, -1.4f));

            var info = new GameObject("VehicleLabel");
            info.transform.SetParent(go.transform, false);
            info.transform.localPosition = Vector3.up * 1.3f;
            var light = info.AddComponent<Light>();
            light.type = LightType.Point; light.range = 5f; light.intensity = 3f;
            light.color = Color.cyan;
        }

        private static void BuildWheelVisual(Transform root, Vector3 localPosition)
        {
            var wheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            wheel.name = "Wheel";
            wheel.transform.SetParent(root, false);
            wheel.transform.localPosition = localPosition;
            wheel.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            wheel.transform.localScale = new Vector3(0.36f, 0.14f, 0.36f);
            wheel.GetComponent<Renderer>().sharedMaterial = wallMat;
            Object.DestroyImmediate(wheel.GetComponent<Collider>());
        }

        private static void CreateHDProductionAssets()
        {
            AssetDatabase.Refresh();
            var root = new GameObject("HD_PRODUCTION_ASSETS_REGION_01").transform;
            string[] meshNames =
            {
                "Tower_Module_HD", "Neon_Kiosk_HD", "Street_Barrier_HD",
                "Street_Lamp_HD", "Vanguard_Sedan_HD", "Armored_Vanguard_HD", "Street_Bike_HD"
            };
            Vector3[] positions =
            {
                new Vector3(-38f,0f,22f), new Vector3(-18f,0f,36f), new Vector3(22f,0f,30f),
                new Vector3(38f,0f,8f), new Vector3(-32f,0f,-28f), new Vector3(18f,0f,-30f),
                new Vector3(34f,0f,-18f), new Vector3(-6f,0f,40f), new Vector3(8f,0f,-42f)
            };
            for (int i = 0; i < positions.Length; i++)
            {
                string path = "Assets/Resources/HDMeshes/" + meshNames[i % meshNames.Length] + ".obj";
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (!prefab) continue;
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                instance.name = "HDAsset_" + meshNames[i % meshNames.Length] + "_" + i;
                instance.transform.SetParent(root, true);
                instance.transform.position = positions[i];
                instance.transform.rotation = Quaternion.Euler(0f, (i * 37f) % 360f, 0f);
                float scale = meshNames[i % meshNames.Length].Contains("Tower") ? 1.8f : 1.0f;
                instance.transform.localScale = Vector3.one * scale;
                var renderers = instance.GetComponentsInChildren<Renderer>(true);
                foreach (var r in renderers)
                {
                    if (r) r.sharedMaterial = (i % 3 == 0) ? wallMat : ((i % 3 == 1) ? accentMat : playerMat);
                    r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                    r.receiveShadows = true;
                }
            }
        }


        private static void CreateHDContentManifest()
        {
            var existing = GameObject.Find("HD_CONTENT_RESOURCE_MANIFEST");
            if (existing) return;
            var go = new GameObject("HD_CONTENT_RESOURCE_MANIFEST");
            go.AddComponent<HDContentResourceManifest>();
        }

        private static void CreateProductionWorld(Transform player)
        {
            var go = new GameObject("PRODUCTION_WORLD_REGION_01");
            var builder = go.AddComponent<ProductionWorldBuilder>();
            builder.player = player; builder.activeDistance = 155f; builder.buildingCount = 52; builder.debrisCount = 120;
            builder.floorMaterial = floorMat; builder.wallMaterial = wallMat; builder.accentMaterial = accentMat; builder.rubbleMaterial = rubbleMat;
            builder.BuildRuntimeDistrict();
        }

        private static void CreatePresentation(Transform player)
        {
            var go = new GameObject("CinematicDirector");
            var director = go.AddComponent<CinematicDirector>();
            director.player = player; director.gameplayCamera = Camera.main;
            var hud = GameObject.Find("HUD_Canvas");
            director.overlay = hud ? hud.GetComponent<Canvas>() : null;
        }

        private static void CreateAISquadCoordinator()
        {
            var existing = GameObject.Find("AI_SquadCoordinator");
            if (existing) return;
            var go = new GameObject("AI_SquadCoordinator");
            var coordinator = go.AddComponent<AISquadCoordinator>();
            coordinator.maxAgents = 32;
            coordinator.thinkEvery = 0.32f;
        }

        private static void CreatePerformanceDirector()
        {
            var go = new GameObject("PerformanceDirectorV2");
            var stability = go.AddComponent<MobileStabilityDirector>();
            stability.targetFps = 60; stability.physicsHz = 60; stability.maximumDeltaTime = 0.08f;
            stability.defaultSolverIterations = 6; stability.defaultSolverVelocityIterations = 2;
            stability.shadowDistance = 65f; stability.pixelLightCount = 2; stability.lodBias = 1.0f;
            var perf = go.AddComponent<PerformanceDirectorV2>();
            perf.targetFps = 60; perf.minRenderScale = 0.78f; perf.maxRenderScale = 1f; perf.response = 1.8f; perf.frameBudgetMs = 16.67f;
            perf.adjustEverySeconds = 1.0f; perf.slowWindowsBeforeDrop = 2; perf.fastWindowsBeforeRaise = 3;
        }

        private static void CreateGameManager()
        {
            if (!Object.FindFirstObjectByType<GameManager>()) new GameObject("GameManager").AddComponent<GameManager>();
        }

        private static void CreateSessionBootstrap()
        {
            new GameObject("SessionBootstrap").AddComponent<SessionBootstrap>();
        }

        private static void SetAndroidDefaults()
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.neonapocalypse.studio");
            PlayerSettings.productName = "NEON APOCALYPSE";
            PlayerSettings.companyName = "Neon Apocalypse Studio";
            PlayerSettings.bundleVersion = "1.0.0";
            PlayerSettings.Android.bundleVersionCode = 100;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel35;
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            QualitySettings.pixelLightCount = 2; QualitySettings.shadowDistance = 55f;
            QualitySettings.shadowResolution = ShadowResolution.Medium; QualitySettings.skinWeights = SkinWeights.FourBones;
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.Enable; QualitySettings.streamingMipmapsActive = true;
            QualitySettings.streamingMipmapsMemoryBudget = 512f; QualitySettings.maxQueuedFrames = 1;
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        }
    }
}
#endif
