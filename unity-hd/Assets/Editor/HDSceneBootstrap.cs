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

            var finish = new GameObject("FinishLine").transform;
            finish.position = racePath.GetPoint(racePath.TotalLength);
            finish.rotation = Quaternion.LookRotation(racePath.GetTangent(racePath.TotalLength), Vector3.up);

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