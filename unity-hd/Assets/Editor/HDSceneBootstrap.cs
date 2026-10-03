#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using NitroStreetRacing = NitroStreetRush.Racing;

namespace NitroStreetRush.Editor
{
    public static class HDSceneBootstrap
    {
        public const string ScenePath = "Assets/Scenes/HDProduction.unity";

        [MenuItem("Nitro Street Rush/Create HD Production Scene")]
        public static void EnsureScene()
        {
            if (System.IO.File.Exists(ScenePath)) return;
            System.IO.Directory.CreateDirectory("Assets/Scenes");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var systems = new GameObject("RaceSystems");
            var events = systems.AddComponent<NitroStreetRacing.RaceStateEvents>();
            var save = systems.AddComponent<NitroStreetRacing.SaveGame>();
            var progress = systems.AddComponent<NitroStreetRacing.RaceProgress>();
            systems.AddComponent<NitroStreetRacing.RaceFlowController>();
            systems.AddComponent<NitroStreetRacing.RaceTimer>();
            systems.AddComponent<NitroStreetRacing.RaceComboSystem>();
            systems.AddComponent<NitroStreetRacing.GameplayFeedback>();
            systems.AddComponent<NitroStreetRacing.RaceResultsController>();
            systems.AddComponent<NitroStreetRacing.RaceObjectiveSystem>();
            systems.AddComponent<NitroStreetRacing.RaceMarkers>();
            systems.AddComponent<NitroStreetRacing.TrackProgressTracker>();
            var pauseController = systems.AddComponent<NitroStreetRacing.RacePauseController>();
            var restartController = systems.AddComponent<NitroStreetRacing.RaceRestartController>();
            systems.AddComponent<NitroStreetRacing.MobileSteering>();

            var pathGo = new GameObject("RacePath");
            var racePath = pathGo.AddComponent<NitroStreetRacing.RaceSplinePath>();
            var controlParent = new GameObject("ControlPoints");
            controlParent.transform.SetParent(pathGo.transform);
            Vector3[] controlPositions =
            {
                new Vector3(0f, 0f, 0f),
                new Vector3(65f, 0f, 180f),
                new Vector3(140f, 5f, 390f),
                new Vector3(-130f, 12f, 620f),
                new Vector3(-220f, 3f, 900f),
                new Vector3(-40f, 18f, 1160f),
                new Vector3(180f, 10f, 1440f),
                new Vector3(125f, 6f, 1700f),
                new Vector3(0f, 0f, 2020f)
            };
            var controlPoints = new Transform[controlPositions.Length];
            for (int i = 0; i < controlPositions.Length; i++)
            {
                var point = new GameObject($"ControlPoint_{i:00}");
                point.transform.SetParent(controlParent.transform);
                point.transform.position = controlPositions[i];
                controlPoints[i] = point.transform;
            }
            var splineSerialized = new SerializedObject(racePath);
            var pointsProperty = splineSerialized.FindProperty("controlPoints");
            pointsProperty.arraySize = controlPoints.Length;
            for (int i = 0; i < controlPoints.Length; i++)
                pointsProperty.GetArrayElementAtIndex(i).objectReferenceValue = controlPoints[i];
            splineSerialized.ApplyModifiedPropertiesWithoutUndo();
            racePath.Rebuild();

            var roadRoot = new GameObject("Road");
            const int roadSegmentCount = 28;
            float segmentLength = racePath.TotalLength / roadSegmentCount;
            for (int i = 0; i < roadSegmentCount; i++)
            {
                float d0 = i * segmentLength;
                float d1 = (i + 1) * segmentLength;
                float mid = (d0 + d1) * 0.5f;
                var road = GameObject.CreatePrimitive(PrimitiveType.Cube);
                road.name = $"RoadSegment_{i:00}";
                road.transform.SetParent(roadRoot.transform);
                road.transform.position = racePath.GetPoint(mid) + Vector3.down * 0.15f;
                var tangent = racePath.GetTangent(mid);
                road.transform.rotation = Quaternion.LookRotation(tangent, Vector3.up);
                road.transform.localScale = new Vector3(11f, 0.3f, segmentLength + 1f);

                var centerDash = GameObject.CreatePrimitive(PrimitiveType.Cube);
                centerDash.name = $"LaneDash_{i:00}";
                centerDash.transform.SetParent(roadRoot.transform);
                centerDash.transform.position = racePath.GetPoint(mid) + Vector3.up * 0.015f;
                centerDash.transform.rotation = road.transform.rotation;
                centerDash.transform.localScale = new Vector3(0.09f, 0.025f, Mathf.Min(5.5f, segmentLength * 0.23f));
            }

            BuildRoadsideWorld(racePath, roadRoot.transform, roadSegmentCount, segmentLength);

            var trafficRoot = new GameObject("Traffic");
            for (int i = 0; i < 12; i++)
            {
                float distance = 150f + i * 145f;
                float lane = ((i % 3) - 1) * 3.45f;
                CreateTrafficVehicle(racePath, trafficRoot.transform, i, distance, lane);
            }

            CreateJumpRamp(racePath, roadRoot.transform, 760f, 12f);
            CreateJumpRamp(racePath, roadRoot.transform, 1480f, 9f);
            CreateCheckpointGates(racePath, roadRoot.transform, 4);

            var finish = new GameObject("FinishLine").transform;
            finish.position = racePath.GetPoint(racePath.TotalLength);
            finish.rotation = Quaternion.LookRotation(racePath.GetTangent(racePath.TotalLength), Vector3.up);
            BuildFinishGate(racePath, finish);

            var car = new GameObject("PlayerCar");
            car.transform.position = racePath.GetPoint(12f) + Vector3.up * 0.8f;
            car.transform.rotation = Quaternion.LookRotation(racePath.GetTangent(12f), Vector3.up);
            var body = car.AddComponent<Rigidbody>();
            body.mass = 1420f;
            car.AddComponent<NitroStreetRacing.CarController>();
            car.AddComponent<NitroStreetRacing.VehicleStabilityAssist>();
            car.AddComponent<NitroStreetRacing.CollisionRecovery>();
            car.AddComponent<NitroStreetRacing.PlayerVehicleBootstrap>();
            var chassis = GameObject.CreatePrimitive(PrimitiveType.Cube);
            chassis.name = "VehicleBody";
            chassis.transform.SetParent(car.transform);
            chassis.transform.localPosition = new Vector3(0f, 0.35f, 0f);
            chassis.transform.localScale = new Vector3(1.9f, 0.55f, 4.3f);
            ApplyMaterial(chassis.GetComponent<Renderer>(), new Color(0.08f, 0.28f, 0.82f), 0.82f, 0.18f);

            var cabin = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cabin.name = "CabinGlass";
            cabin.transform.SetParent(car.transform);
            cabin.transform.localPosition = new Vector3(0f, 0.82f, -0.15f);
            cabin.transform.localScale = new Vector3(1.42f, 0.42f, 1.8f);
            ApplyMaterial(cabin.GetComponent<Renderer>(), new Color(0.03f, 0.06f, 0.10f), 0.25f, 0.12f);

            var frontSplitter = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frontSplitter.name = "FrontSplitter";
            frontSplitter.transform.SetParent(car.transform);
            frontSplitter.transform.localPosition = new Vector3(0f, 0.06f, 2.24f);
            frontSplitter.transform.localScale = new Vector3(2.05f, 0.10f, 0.45f);
            ApplyMaterial(frontSplitter.GetComponent<Renderer>(), new Color(0.015f, 0.018f, 0.025f), 0.9f, 0.12f);

            var rearWing = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rearWing.name = "RearWing";
            rearWing.transform.SetParent(car.transform);
            rearWing.transform.localPosition = new Vector3(0f, 0.95f, -1.72f);
            rearWing.transform.localScale = new Vector3(1.95f, 0.11f, 0.48f);
            ApplyMaterial(rearWing.GetComponent<Renderer>(), new Color(0.02f, 0.025f, 0.035f), 0.88f, 0.12f);

            var rearWingBaseL = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rearWingBaseL.name = "WingSupportL";
            rearWingBaseL.transform.SetParent(car.transform);
            rearWingBaseL.transform.localPosition = new Vector3(-0.65f, 0.65f, -1.62f);
            rearWingBaseL.transform.localScale = new Vector3(0.12f, 0.55f, 0.12f);
            ApplyMaterial(rearWingBaseL.GetComponent<Renderer>(), new Color(0.02f, 0.025f, 0.035f), 0.88f, 0.12f);

            var rearWingBaseR = Object.Instantiate(rearWingBaseL, car.transform);
            rearWingBaseR.name = "WingSupportR";
            rearWingBaseR.transform.localPosition = new Vector3(0.65f, 0.65f, -1.62f);

            var headlightL = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            headlightL.name = "HeadlightL";
            headlightL.transform.SetParent(car.transform);
            headlightL.transform.localPosition = new Vector3(-0.62f, 0.42f, 2.12f);
            headlightL.transform.localScale = new Vector3(0.30f, 0.13f, 0.08f);
            ApplyMaterial(headlightL.GetComponent<Renderer>(), new Color(0.72f, 0.88f, 1f), 0.05f, 0.15f, true);

            var headlightR = Object.Instantiate(headlightL, car.transform);
            headlightR.name = "HeadlightR";
            headlightR.transform.localPosition = new Vector3(0.62f, 0.42f, 2.12f);

            var taillightL = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            taillightL.name = "TailLightL";
            taillightL.transform.SetParent(car.transform);
            taillightL.transform.localPosition = new Vector3(-0.68f, 0.43f, -2.12f);
            taillightL.transform.localScale = new Vector3(0.28f, 0.12f, 0.08f);
            ApplyMaterial(taillightL.GetComponent<Renderer>(), new Color(1f, 0.03f, 0.02f), 0.05f, 0.15f, true);

            var taillightR = Object.Instantiate(taillightL, car.transform);
            taillightR.name = "TailLightR";
            taillightR.transform.localPosition = new Vector3(0.68f, 0.43f, -2.12f);

            for (int i = 0; i < 4; i++)
            {
                var wheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                wheel.name = $"Wheel_{i}";
                wheel.transform.SetParent(car.transform);
                wheel.transform.localScale = new Vector3(0.58f, 0.18f, 0.58f);
                wheel.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                float x = i % 2 == 0 ? -0.95f : 0.95f;
                float z = i < 2 ? 1.35f : -1.35f;
                wheel.transform.localPosition = new Vector3(x, 0f, z);
                ApplyMaterial(wheel.GetComponent<Renderer>(), new Color(0.015f, 0.018f, 0.022f), 0.85f, 0.22f);
            }

            var cameraGo = new GameObject("RaceCamera");
            cameraGo.tag = "MainCamera";
            var cam = cameraGo.AddComponent<Camera>();
            cameraGo.AddComponent<NitroStreetRacing.RaceCamera>();
            cameraGo.transform.position = new Vector3(0f, 4f, -10f);

            var lightGo = new GameObject("Sun");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            lightGo.transform.rotation = Quaternion.Euler(45f, -25f, 0f);

            var touch = new GameObject("TouchInput");
            touch.AddComponent<NitroStreetRacing.TouchInput>();

            var canvasGo = new GameObject("HUD");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();
            var hud = canvasGo.AddComponent<NitroStreetRacing.RacePresentationHUD>();
            var stateTextGo = CreateText(canvasGo.transform, "RaceState", new Vector2(0f, 170f), 48);
            var speedTextGo = CreateText(canvasGo.transform, "Speed", new Vector2(0f, 90f), 40);
            var progressTextGo = CreateText(canvasGo.transform, "Progress", new Vector2(0f, 30f), 30);
            var scoreTextGo = CreateText(canvasGo.transform, "Score", new Vector2(0f, -35f), 28);
            var comboTextGo = CreateText(canvasGo.transform, "Combo", new Vector2(0f, -80f), 24);
            var objectiveTextGo = CreateText(canvasGo.transform, "Objective", new Vector2(0f, 400f), 26);
            var objectiveStatusGo = CreateText(canvasGo.transform, "ObjectiveStatus", new Vector2(0f, 345f), 24);
            var routeDirectionGo = CreateText(canvasGo.transform, "RouteDirection", new Vector2(690f, 345f), 28);
            var routeDistanceGo = CreateText(canvasGo.transform, "RouteDistance", new Vector2(690f, 305f), 22);
            var bannerGo = new GameObject("RaceEventBanner");
            bannerGo.transform.SetParent(canvasGo.transform);
            var bannerRect = bannerGo.AddComponent<RectTransform>();
            bannerRect.anchorMin = bannerRect.anchorMax = new Vector2(0.5f, 0.5f);
            bannerRect.anchoredPosition = new Vector2(0f, 245f);
            bannerRect.sizeDelta = new Vector2(1100f, 130f);
            var bannerGroup = bannerGo.AddComponent<CanvasGroup>();
            var bannerText = CreateText(bannerGo.transform, "Event", Vector2.zero, 58);
            bannerText.GetComponent<RectTransform>().sizeDelta = bannerRect.sizeDelta;
            var eventBanner = bannerGo.AddComponent<NitroStreetRacing.RaceEventBanner>();
            SetPrivate(eventBanner, "bannerText", bannerText);
            SetPrivate(eventBanner, "group", bannerGroup);
            var stateText = stateTextGo.GetComponent<Text>();
            var speedText = speedTextGo.GetComponent<Text>();
            var progressText = progressTextGo.GetComponent<Text>();
            var scoreText = scoreTextGo.GetComponent<Text>();
            var comboText = comboTextGo.GetComponent<Text>();
            var objectiveText = objectiveTextGo.GetComponent<Text>();
            var objectiveStatus = objectiveStatusGo.GetComponent<Text>();

            SetPrivate(hud, "flow", systems.GetComponent<NitroStreetRacing.RaceFlowController>());
            SetPrivate(hud, "car", car.GetComponent<NitroStreetRacing.CarController>());
            SetPrivate(hud, "progress", progress);
            SetPrivate(hud, "stateText", stateText);
            SetPrivate(hud, "speedText", speedText);
            SetPrivate(hud, "progressText", progressText);

            var objectiveHud = canvasGo.AddComponent<NitroStreetRacing.RaceObjectiveHUD>();
            SetPrivate(objectiveHud, "objectives", systems.GetComponent<NitroStreetRacing.RaceObjectiveSystem>());
            SetPrivate(objectiveHud, "objectiveText", objectiveText);
            SetPrivate(objectiveHud, "statusText", objectiveStatus);

            var routeHud = canvasGo.AddComponent<NitroStreetRacing.RaceRouteGuidanceHUD>();
            SetPrivate(routeHud, "path", racePath);
            SetPrivate(routeHud, "progress", progress);
            SetPrivate(routeHud, "player", car.transform);
            SetPrivate(routeHud, "directionText", routeDirectionGo.GetComponent<Text>());
            SetPrivate(routeHud, "distanceText", routeDistanceGo.GetComponent<Text>());

            var controls = canvasGo.AddComponent<NitroStreetRacing.MobileControlOverlay>();
            controls.Configure(canvas);
            CreateControlButton(canvasGo.transform, "LEFT", new Vector2(-420f, -260f), NitroStreetRacing.MobileControlButton.ActionType.Left);
            CreateControlButton(canvasGo.transform, "RIGHT", new Vector2(-250f, -260f), NitroStreetRacing.MobileControlButton.ActionType.Right);
            CreateControlButton(canvasGo.transform, "BRAKE", new Vector2(300f, -240f), NitroStreetRacing.MobileControlButton.ActionType.Brake);
            CreateControlButton(canvasGo.transform, "NITRO", new Vector2(470f, -150f), NitroStreetRacing.MobileControlButton.ActionType.Nitro);

            var pauseButton = CreateUIButton(canvasGo.transform, "PAUSE", new Vector2(810f, 465f), new Vector2(180f, 80f));
            pauseButton.onClick.AddListener(pauseController.TogglePause);

            var pausePanel = new GameObject("PausePanel");
            pausePanel.transform.SetParent(canvasGo.transform);
            var panelRect = pausePanel.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            var panelImage = pausePanel.AddComponent<Image>();
            panelImage.color = new Color(0f, 0f, 0f, 0.82f);
            var pausedText = CreateText(pausePanel.transform, "PAUSED", new Vector2(0f, 170f), 68);
            var resumeButton = CreateUIButton(pausePanel.transform, "RESUME", new Vector2(0f, 10f), new Vector2(320f, 90f));
            var restartButton = CreateUIButton(pausePanel.transform, "RESTART", new Vector2(0f, -110f), new Vector2(320f, 90f));
            resumeButton.onClick.AddListener(() => pauseController.SetPaused(false));
            restartButton.onClick.AddListener(restartController.RestartRace);
            pauseController.BindPanel(pausePanel);

            var scoreHud = canvasGo.AddComponent<NitroStreetRacing.RaceScoreHUD>();
            SetPrivate(scoreHud, "combo", systems.GetComponent<NitroStreetRacing.RaceComboSystem>());
            SetPrivate(scoreHud, "scoreText", scoreText);
            SetPrivate(scoreHud, "comboText", comboText);

            var resultsPanel = new GameObject("ResultsPanel");
            resultsPanel.transform.SetParent(canvasGo.transform);
            var resultsRect = resultsPanel.AddComponent<RectTransform>();
            resultsRect.anchorMin = Vector2.zero;
            resultsRect.anchorMax = Vector2.one;
            resultsRect.offsetMin = Vector2.zero;
            resultsRect.offsetMax = Vector2.zero;
            var resultsImage = resultsPanel.AddComponent<Image>();
            resultsImage.color = new Color(0f, 0f, 0f, 0.88f);
            var resultsTitle = CreateText(resultsPanel.transform, "RESULTS", new Vector2(0f, 220f), 66);
            var resultsTime = CreateText(resultsPanel.transform, "00:00.000", new Vector2(0f, 95f), 48);
            var resultsReward = CreateText(resultsPanel.transform, "REWARD  +500", new Vector2(0f, 15f), 32);
            var resultsBest = CreateText(resultsPanel.transform, "BEST  --:--.---", new Vector2(0f, -60f), 28);
            var resultsRestart = CreateUIButton(resultsPanel.transform, "RACE AGAIN", new Vector2(0f, -190f), new Vector2(360f, 90f));
            resultsRestart.onClick.AddListener(restartController.RestartRace);
            var resultsHud = canvasGo.AddComponent<NitroStreetRacing.RaceResultsHUD>();
            SetPrivate(resultsHud, "saveGame", save);
            SetPrivate(resultsHud, "resultsPanel", resultsPanel);
            SetPrivate(resultsHud, "timeText", resultsTime.GetComponent<Text>());
            SetPrivate(resultsHud, "rewardText", resultsReward.GetComponent<Text>());
            SetPrivate(resultsHud, "bestText", resultsBest.GetComponent<Text>());
            resultsPanel.SetActive(false);

            progress.SetPath(racePath);
            progress.SetPlayer(car.transform);
            progress.SetFinish(finish);
            progress.SetRaceDistance(racePath.TotalLength);
            cameraGo.GetComponent<NitroStreetRacing.RaceCamera>().SetTarget(car.transform);

            var flow = systems.GetComponent<NitroStreetRacing.RaceFlowController>();
            SetPrivate(flow, "events", events);
            SetPrivate(flow, "player", car.GetComponent<NitroStreetRacing.CarController>());
            SetPrivate(flow, "progress", progress);

            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
        }



        private static void BuildRoadsideWorld(NitroStreetRacing.RaceSplinePath path, Transform roadRoot, int segmentCount, float segmentLength)
        {
            var worldRoot = new GameObject("RoadsideWorld");
            int buildingSeed = 9321;
            var rng = new System.Random(buildingSeed);

            for (int i = 0; i < segmentCount; i++)
            {
                float d = (i + 0.5f) * segmentLength;
                Vector3 point = path.GetPoint(d);
                Vector3 tangent = path.GetTangent(d);
                Vector3 right = path.GetRight(d);
                Quaternion rotation = Quaternion.LookRotation(tangent, Vector3.up);

                if (i % 2 == 0)
                {
                    CreateBarrier(worldRoot.transform, point + right * 6.15f + Vector3.up * 0.55f, rotation, segmentLength);
                    CreateBarrier(worldRoot.transform, point - right * 6.15f + Vector3.up * 0.55f, rotation, segmentLength);
                }

                if (i % 2 == 0)
                {
                    CreateBuilding(worldRoot.transform, point + right * (13f + rng.NextDouble() * 7f), rotation, 10f + (float)rng.NextDouble() * 18f, 5f + (float)rng.NextDouble() * 8f);
                    CreateBuilding(worldRoot.transform, point - right * (13f + rng.NextDouble() * 7f), rotation, 11f + (float)rng.NextDouble() * 20f, 5f + (float)rng.NextDouble() * 8f);
                }

                if (i % 3 == 0)
                {
                    CreateStreetLight(worldRoot.transform, point + right * 7.8f, rotation);
                    CreateStreetLight(worldRoot.transform, point - right * 7.8f, rotation * Quaternion.Euler(0f, 180f, 0f));
                }

                if (i % 4 == 1)
                {
                    CreateTree(worldRoot.transform, point + right * 10.5f, 4.2f);
                    CreateTree(worldRoot.transform, point - right * 10.5f, 4.8f);
                }
            }
        }

        private static void CreateBarrier(Transform parent, Vector3 position, Quaternion rotation, float length)
        {
            var barrier = GameObject.CreatePrimitive(PrimitiveType.Cube);
            barrier.name = "GuardRail";
            barrier.transform.SetParent(parent);
            barrier.transform.SetPositionAndRotation(position, rotation);
            barrier.transform.localScale = new Vector3(0.28f, 0.95f, length * 0.98f);
            ApplyMaterial(barrier.GetComponent<Renderer>(), new Color(0.32f, 0.36f, 0.41f), 0.7f, 0.28f);
        }

        private static void CreateBuilding(Transform parent, Vector3 position, Quaternion roadRotation, float height, float width)
        {
            var building = GameObject.CreatePrimitive(PrimitiveType.Cube);
            building.name = "CityBuilding";
            building.transform.SetParent(parent);
            building.transform.SetPositionAndRotation(position + Vector3.up * (height * 0.5f), roadRotation);
            building.transform.localScale = new Vector3(width, height, width * 0.72f);
            float tone = Mathf.Clamp01(0.18f + (height % 17f) * 0.018f);
            ApplyMaterial(building.GetComponent<Renderer>(), new Color(tone, tone * 0.95f, tone * 1.08f), 0.05f, 0.55f);

            var windows = GameObject.CreatePrimitive(PrimitiveType.Cube);
            windows.name = "WindowBand";
            windows.transform.SetParent(building.transform);
            windows.transform.localPosition = new Vector3(0f, 0.08f, -0.501f);
            windows.transform.localScale = new Vector3(0.82f, 0.56f, 0.018f);
            ApplyMaterial(windows.GetComponent<Renderer>(), new Color(0.04f, 0.08f, 0.12f), 0.15f, 0.18f);
        }

        private static void CreateStreetLight(Transform parent, Vector3 position, Quaternion rotation)
        {
            var pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pole.name = "StreetLight";
            pole.transform.SetParent(parent);
            pole.transform.SetPositionAndRotation(position + Vector3.up * 2.7f, rotation);
            pole.transform.localScale = new Vector3(0.12f, 2.7f, 0.12f);
            ApplyMaterial(pole.GetComponent<Renderer>(), new Color(0.08f, 0.09f, 0.11f), 0.85f, 0.22f);

            var lamp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            lamp.name = "Lamp";
            lamp.transform.SetParent(pole.transform);
            lamp.transform.localPosition = new Vector3(0f, 1f, 0f);
            lamp.transform.localScale = Vector3.one * 0.32f;
            ApplyMaterial(lamp.GetComponent<Renderer>(), new Color(1f, 0.55f, 0.18f), 0.05f, 0.2f, true);
        }

        private static void CreateTree(Transform parent, Vector3 position, float scale)
        {
            var trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name = "TreeTrunk";
            trunk.transform.SetParent(parent);
            trunk.transform.SetPositionAndRotation(position + Vector3.up * (scale * 0.32f), Quaternion.identity);
            trunk.transform.localScale = new Vector3(0.18f, scale * 0.32f, 0.18f);
            ApplyMaterial(trunk.GetComponent<Renderer>(), new Color(0.16f, 0.09f, 0.05f), 0f, 0.9f);

            var canopy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            canopy.name = "TreeCanopy";
            canopy.transform.SetParent(parent);
            canopy.transform.position = position + Vector3.up * (scale * 0.82f);
            canopy.transform.localScale = Vector3.one * scale;
            ApplyMaterial(canopy.GetComponent<Renderer>(), new Color(0.04f, 0.22f, 0.11f), 0.05f, 0.75f);
        }

        private static void CreateTrafficVehicle(NitroStreetRacing.RaceSplinePath path, Transform parent, int index, float distance, float lane)
        {
            distance = Mathf.Clamp(distance, 30f, Mathf.Max(40f, path.TotalLength - 40f));
            Vector3 point = path.GetPoint(distance);
            Vector3 tangent = path.GetTangent(distance);
            Vector3 right = path.GetRight(distance);

            var car = new GameObject($"Traffic_{index:00}");
            car.transform.SetParent(parent);
            car.transform.position = point + right * lane + Vector3.up * 0.55f;
            car.transform.rotation = Quaternion.LookRotation(tangent, Vector3.up);
            var body = car.AddComponent<Rigidbody>();
            body.mass = 1280f;
            body.linearDamping = 0.2f;
            body.angularDamping = 4.5f;
            car.AddComponent<NitroStreetRacing.TrafficVehicleAI>().SetPath(path, distance, lane);

            var chassis = GameObject.CreatePrimitive(PrimitiveType.Cube);
            chassis.name = "TrafficBody";
            chassis.transform.SetParent(car.transform);
            chassis.transform.localPosition = new Vector3(0f, 0.28f, 0f);
            chassis.transform.localScale = new Vector3(1.78f, 0.52f, 4.05f);
            Color[] paints = {
                new Color(0.88f, 0.12f, 0.08f),
                new Color(0.05f, 0.28f, 0.78f),
                new Color(0.95f, 0.67f, 0.06f),
                new Color(0.76f, 0.78f, 0.82f),
                new Color(0.12f, 0.65f, 0.39f),
                new Color(0.50f, 0.18f, 0.73f)
            };
            ApplyMaterial(chassis.GetComponent<Renderer>(), paints[index % paints.Length], 0.72f, 0.2f);

            var roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roof.name = "TrafficCabin";
            roof.transform.SetParent(car.transform);
            roof.transform.localPosition = new Vector3(0f, 0.64f, -0.2f);
            roof.transform.localScale = new Vector3(1.45f, 0.32f, 1.85f);
            ApplyMaterial(roof.GetComponent<Renderer>(), new Color(0.06f, 0.08f, 0.11f), 0.5f, 0.18f);
        }

        private static void CreateCheckpointGates(NitroStreetRacing.RaceSplinePath path, Transform parent, int count)
        {
            var checkpointRoot = new GameObject("CheckpointGates");
            checkpointRoot.transform.SetParent(parent);
            float spacing = path.TotalLength / (count + 1f);
            for (int i = 1; i <= count; i++)
            {
                float d = spacing * i;
                Vector3 point = path.GetPoint(d);
                Quaternion rot = Quaternion.LookRotation(path.GetTangent(d), Vector3.up);
                Vector3 right = path.GetRight(d);
                CreateGatePart(checkpointRoot.transform, point + right * 5.25f + Vector3.up * 2.25f, rot, new Vector3(0.3f, 4.5f, 0.45f));
                CreateGatePart(checkpointRoot.transform, point - right * 5.25f + Vector3.up * 2.25f, rot, new Vector3(0.3f, 4.5f, 0.45f));
                CreateGatePart(checkpointRoot.transform, point + Vector3.up * 4.45f, rot, new Vector3(10.8f, 0.35f, 0.45f));
            }
        }

        private static void CreateJumpRamp(NitroStreetRacing.RaceSplinePath path, Transform parent, float distance, float width)
        {
            distance = Mathf.Clamp(distance, 40f, Mathf.Max(60f, path.TotalLength - 60f));
            Vector3 point = path.GetPoint(distance) + Vector3.up * 0.18f;
            Vector3 tangent = path.GetTangent(distance);
            var ramp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ramp.name = "StuntRamp";
            ramp.transform.SetParent(parent);
            ramp.transform.position = point + Vector3.up * 0.18f;
            ramp.transform.rotation = Quaternion.LookRotation(tangent, Vector3.up) * Quaternion.Euler(-9f, 0f, 0f);
            ramp.transform.localScale = new Vector3(width, 0.42f, 11f);
            ApplyMaterial(ramp.GetComponent<Renderer>(), new Color(0.07f, 0.08f, 0.1f), 0.22f, 0.32f);
        }

        private static void BuildFinishGate(NitroStreetRacing.RaceSplinePath path, Transform finish)
        {
            Vector3 right = path.GetRight(path.TotalLength);
            Quaternion rot = finish.rotation;
            CreateGatePart(finish.parent, finish.position + right * 5.2f + Vector3.up * 2.2f, rot, new Vector3(0.35f, 4.4f, 0.55f));
            CreateGatePart(finish.parent, finish.position - right * 5.2f + Vector3.up * 2.2f, rot, new Vector3(0.35f, 4.4f, 0.55f));
            CreateGatePart(finish.parent, finish.position + Vector3.up * 4.25f, rot, new Vector3(10.75f, 0.5f, 0.55f));
        }

        private static void CreateGatePart(Transform parent, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            var part = GameObject.CreatePrimitive(PrimitiveType.Cube);
            part.name = "FinishGate";
            part.transform.SetParent(parent);
            part.transform.SetPositionAndRotation(position, rotation);
            part.transform.localScale = scale;
            ApplyMaterial(part.GetComponent<Renderer>(), new Color(0.06f, 0.09f, 0.12f), 0.75f, 0.16f);
        }

        private static void ApplyMaterial(Renderer renderer, Color baseColor, float metallic, float smoothness, bool emission = false)
        {
            if (!renderer) return;
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (!shader) shader = Shader.Find("Standard");
            if (!shader) return;

            var material = new Material(shader);
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", baseColor);
            if (material.HasProperty("_Color")) material.color = baseColor;
            if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", metallic);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);
            if (emission)
            {
                material.EnableKeyword("_EMISSION");
                if (material.HasProperty("_EmissionColor")) material.SetColor("_EmissionColor", baseColor * 4f);
            }
            renderer.sharedMaterial = material;
        }

        private static void CreateControlButton(Transform parent, string label, Vector2 position, NitroStreetRacing.MobileControlButton.ActionType action)
        {
            var go = new GameObject(label + "Button");
            go.transform.SetParent(parent);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0f);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(150f, 100f);
            var image = go.AddComponent<Image>();
            image.color = new Color(0.05f, 0.08f, 0.12f, 0.82f);
            var button = go.AddComponent<Button>();
            button.transition = Selectable.Transition.ColorTint;
            var text = CreateText(go.transform, label, Vector2.zero, 24);
            text.GetComponent<RectTransform>().sizeDelta = rect.sizeDelta;
            go.AddComponent<NitroStreetRacing.MobileControlButton>().Configure(action);
        }
        private static Button CreateUIButton(Transform parent, string label, Vector2 position, Vector2 size)
        {
            var go = new GameObject(label + "Button");
            go.transform.SetParent(parent);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            var image = go.AddComponent<Image>();
            image.color = new Color(0.05f, 0.08f, 0.12f, 0.92f);
            var button = go.AddComponent<Button>();
            var text = CreateText(go.transform, label, Vector2.zero, 26);
            text.GetComponent<RectTransform>().sizeDelta = size;
            return button;
        }

        private static GameObject CreateText(Transform parent, string name, Vector2 position, int size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(700f, 90f);
            var text = go.AddComponent<Text>();
            text.alignment = TextAnchor.MiddleCenter;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = size;
            text.text = name;
            return go;
        }

        private static void SetPrivate(Object target, string field, Object value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(field);
            if (prop != null)
            {
                prop.objectReferenceValue = value;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }
    }
}
#endif