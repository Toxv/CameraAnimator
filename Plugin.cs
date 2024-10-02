using BepInEx;
using System;
using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;
using GorillaNetworking;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using System.Threading;
using HarmonyLib;
using System.Text;
using System.Collections;
using UnityEngine.SceneManagement;
using Cam.AnimationFunctions;

namespace Cam
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        // -- Objects and Game State Management
        private GameObject cube;
        private GameObject[] keyframesvisual = new GameObject[0];
        private KeyframePostion[] keyframes = new KeyframePostion[0];
        private int currentFrame = 0;
        private int nextFrame = 1;
        private int keyframeselectednum = -1;

        // -- Timing and Playback
        private float frameRate = 10f;
        private float timePerFrame;
        private float timer = 0f;
        private bool isPlaying = false;
        private KeyframePostion newKeyframe = new KeyframePostion(Vector3.zero, Quaternion.identity, 90.0f);


        // -- GUI Layout
        private Rect editorRect = new Rect(0, Screen.height, 700, 400);

        // -- Lerp Animation
        private float lerpTime = 0f;
        private float lerpDuration = 1f;
        private bool testStarted = false;

        // -- Dragging and Interaction
        private Vector2 dragOffset;
        private bool isDragging = false;
        private Rect dragArea;

        // -- Camera and Movement Settings
        private GameObject CamObject;
        private bool looking = false;
        private bool lockcusor = false;
        private float fov = 90f;
        private float sensitivity = 0.3f;
        private float movementSpeed = 10f;
        private float fastMovementSpeed = 100f;
        private float freeLookSensitivity = 3f;
        private float zoomSensitivity = 1f;
        private float fastZoomSensitivity = 5f;
        private float smoothness = 1.0f;
        private bool loaded = false;
        private bool cameraview = false;
        private bool keyframeselected = false;
        // -- GUI Skin and Asset Bundles
        private GUISkin skin;
        private AssetBundle bundle;

        // -- Audio Management
        public AudioListener playerListener;
        public AudioListener cameraListener;

        // -- Cinemachine Setup
        private GameObject CineCamObject;
        private CinemachineVirtualCamera CineCam;
        private Camera Cam;

        // -- Timeline and Keyframe Management
        private const float maxFrames = 24f;
        private float maxTimeline;
        private bool isDraggingMarker = false;
        private bool loopPlayback = false;
        private Rect timelineRect;
        private Vector2 scrollPosition = Vector2.zero;
        private float zoomFactor = 1f;
        private const float keyframeBaseWidth = 10f;
        private int draggedKeyframeIndex = -1;
        private List<float> keyframePositions = new List<float>();

        // -- Frame Rate and Timing
        private float fps = 24f;
        private float currentTime = 0f;

        // -- GUI Visibility
        bool guiVisible = true;

        // -- Other
        static string cameraPath = Path.Combine(Paths.BepInExRootPath, "CameraAnimationSaves");// wryser stole ur code heheh

        public bool loadScene;
        public string sceneName;

        public bool Forest;
        public bool City;
        public bool Canyons;
        public bool Beach;
        public bool Mountains;
        public bool Clouds;
        public bool Basement;
        public bool Cave;
        public bool Metropolis;
        public bool Rotating;
        private bool loadAllExceptForest = false;
        private GameObject city;
        private GameObject forest;

        // Dear Dev if you are reading this im sorry for my code please forgive me i swear it wasnt me who made this
        // my dog coded it thats why its to messy - Tox
        public void AddKeyframe(Vector3 position, Quaternion rotation, float fov)
        {
            Array.Resize(ref keyframes, keyframes.Length + 1);
            keyframes[keyframes.Length - 1] = new KeyframePostion(position, rotation, fov);
            Debug.Log($"Added Keyframe: Position: {position}, Rotation: {rotation}");
        }
        // if anyone can get importing working il give them nitro dm and if u get it working and show me and send it to me
        public void ExportKeyframes()
        {
            if (keyframes.Length == 0)
            {
                Debug.LogWarning("No keyframes to export.");
                return;
            }

            string keyframeFileName = $"Keyframes_Save_{DateTime.Now:dd-MM-yyyy_HH-mm-ss}.kfc";
            string fullPath = Path.Combine(cameraPath, keyframeFileName);

            StringBuilder jsonBuilder = new StringBuilder();
            jsonBuilder.AppendLine("{");
            jsonBuilder.AppendLine($"    \"frameRate\": {frameRate},");
            jsonBuilder.AppendLine($"    \"fps\": {fps},");
            jsonBuilder.AppendLine($"    \"loopPlayback\": {loopPlayback.ToString().ToLower()},");
            jsonBuilder.AppendLine("    \"keyframes\": [");

            for (int i = 0; i < keyframes.Length; i++)
            {
                jsonBuilder.AppendLine("        {");
                jsonBuilder.AppendLine($"            \"position\": [{keyframes[i].position.x}, {keyframes[i].position.y}, {keyframes[i].position.z}],");
                jsonBuilder.AppendLine($"            \"rotation\": [{keyframes[i].rotation.x}, {keyframes[i].rotation.y}, {keyframes[i].rotation.z}, {keyframes[i].rotation.w}],");
                jsonBuilder.AppendLine($"            \"fov\": {keyframes[i].fov}");
                jsonBuilder.Append("        }");

                if (i < keyframes.Length - 1)
                    jsonBuilder.Append(",");

                jsonBuilder.AppendLine();
            }

            jsonBuilder.AppendLine("    ],");
            jsonBuilder.AppendLine("    \"keyframePositions\": [");

            for (int i = 0; i < keyframePositions.Count; i++)
            {
                jsonBuilder.Append($"        {keyframePositions[i]}");
                if (i < keyframePositions.Count - 1)
                    jsonBuilder.Append(",");

                jsonBuilder.AppendLine();
            }

            jsonBuilder.AppendLine("    ],");
            jsonBuilder.AppendLine("    \"keyframesVisual\": [");

            for (int i = 0; i < keyframesvisual.Length; i++)
            {
                jsonBuilder.Append($"        {{ \"name\": \"{keyframesvisual[i].name}\", " +
                                  $"\"position\": [{keyframesvisual[i].transform.position.x}, {keyframesvisual[i].transform.position.y}, {keyframesvisual[i].transform.position.z}], " +
                                  $"\"rotation\": [{keyframesvisual[i].transform.rotation.x}, {keyframesvisual[i].transform.rotation.y}, {keyframesvisual[i].transform.rotation.z}, {keyframesvisual[i].transform.rotation.w}] }}");

                if (i < keyframesvisual.Length - 1)
                    jsonBuilder.Append(",");

                jsonBuilder.AppendLine();
            }

            jsonBuilder.AppendLine("    ]");
            jsonBuilder.AppendLine("}");
            using (StreamWriter writer = new StreamWriter(fullPath))
            {
                writer.WriteAsync(jsonBuilder.ToString()).GetAwaiter().GetResult();
            }

            Debug.Log($"Keyframes exported to: {fullPath}");
        }
        private void Update()
        {

            if (cameraview && isPlaying)
            {
                CamObject.transform.position = cube.transform.position;
                CamObject.transform.rotation = cube.transform.rotation;
            }
            if (loaded)
            {
                SetupCamera();
                HandleMovement();
                HandleFreeLook();
                HandleCursorLock();
            }
            if (!isPlaying && loaded)
            {
                cube.transform.position = newKeyframe.position;
                cube.transform.rotation = newKeyframe.rotation;
            }
        }
        void HandleMovement()
        {
            Transform transform = CamObject.transform;
            var fastMode = Keyboard.current.pKey.IsPressed();
            var speed = fastMode ? fastMovementSpeed : movementSpeed;
            if (!cameraview)
            {
                transform.gameObject.GetComponent<Camera>().fieldOfView = fov;
            }
            if (Keyboard.current.aKey.IsPressed() || Keyboard.current.leftArrowKey.IsPressed())
            {
                transform.position += -transform.right * speed * Time.deltaTime;
            }

            if (Keyboard.current.dKey.IsPressed() || Keyboard.current.rightArrowKey.IsPressed())
            {
                transform.position += transform.right * speed * Time.deltaTime;
            }

            if (Keyboard.current.wKey.IsPressed() || Keyboard.current.upArrowKey.IsPressed())
            {
                transform.position += transform.forward * speed * Time.deltaTime;
            }

            if (Keyboard.current.sKey.IsPressed() || Keyboard.current.downArrowKey.IsPressed())
            {
                transform.position += -transform.forward * speed * Time.deltaTime;
            }

            if (Keyboard.current.spaceKey.IsPressed())
            {
                transform.position += transform.up * speed * Time.deltaTime;
            }

            if (Keyboard.current.shiftKey.IsPressed())
            {
                transform.position += -transform.up * speed * Time.deltaTime;
            }

            if (Keyboard.current.rKey.IsPressed() || Keyboard.current.pageUpKey.IsPressed())
            {
                transform.position += Vector3.up * speed * Time.deltaTime;
            }

            if (Keyboard.current.fKey.IsPressed() || Keyboard.current.pageDownKey.IsPressed())
            {
                transform.position += -Vector3.up * speed * Time.deltaTime;
            }
            if (Keyboard.current.zKey.wasPressedThisFrame)
            {
                Mathf.Lerp(CamObject.gameObject.GetComponent<Camera>().fieldOfView, fov - 50, 0.4f);
            }
            if (Keyboard.current.zKey.wasReleasedThisFrame)
            {
                Mathf.Lerp(CamObject.gameObject.GetComponent<Camera>().fieldOfView, fov, 0.4f);
            }
            float scrollAxis = Mouse.current.scroll.ReadValue().y;
            if (scrollAxis != 0)
            {
                var zoomSpeed = fastMode ? fastZoomSensitivity : zoomSensitivity;

                Vector3 newPosition = transform.position += transform.forward * scrollAxis * zoomSpeed * Time.deltaTime;
                transform.position = Vector3.Lerp(transform.position, newPosition, smoothness);
            }
        }
        private float mapLoaderStartHeight = 0f;
        private float mapLoaderTargetHeight = 320f;
        private float mapLoaderCurrentHeight;
        private float mapLoaderAnimationDuration = 1f;
        private float mapLoaderTimer = 0f;
        private bool mapLoaderIsAnimating = true;
        public static ZoneData FindZoneData(GTZone zone)
        {
            return (ZoneData)AccessTools.Method(typeof(ZoneManagement), "GetZoneData", null, null).Invoke(ZoneManagement.instance, new object[1] { zone });
        }

        void MapLoader()
        {
            GUI.skin = skin;
            float areaHeight = 400f;
            float areaWidth = 220f;
            GUILayout.BeginArea(new Rect(10f, UnityEngine.Screen.height - areaHeight - 15f, areaWidth, areaHeight));
            if (GUILayout.Button("Load Beach"))
            {
                GTZone zone = GTZone.beach;
                FindZoneData(zone);
                FindZoneData(GTZone.forest);
                GTZone[] zones = new GTZone[] { zone, GTZone.forest };
                ZoneManagement.SetActiveZones(zones);
            }
            if (GUILayout.Button("Load Forest"))
            {
                GTZone zone = GTZone.forest;
                FindZoneData(zone);
                FindZoneData(GTZone.forest);
                GTZone[] zones = new GTZone[] { zone, GTZone.forest };
                ZoneManagement.SetActiveZones(zones);
            }
            if (GUILayout.Button("Load City"))
            {
                GTZone zone = GTZone.city;
                FindZoneData(zone);
                FindZoneData(GTZone.forest);
                GTZone[] zones = new GTZone[] { zone, GTZone.forest };
                ZoneManagement.SetActiveZones(zones);
            }
            if (GUILayout.Button("Load Metropolis"))
            {
                GTZone zone = GTZone.Metropolis;
                FindZoneData(zone);
                FindZoneData(GTZone.forest);
                GTZone[] zones = new GTZone[] { zone, GTZone.forest };
                ZoneManagement.SetActiveZones(zones);
            }
            if (GUILayout.Button("Load Clouds"))
            {
                GTZone zone = GTZone.skyJungle;
                FindZoneData(zone);
                FindZoneData(GTZone.forest);
                GTZone[] zones = new GTZone[] { zone, GTZone.forest };
                ZoneManagement.SetActiveZones(zones);
            }
            if (GUILayout.Button("Load Mountains"))
            {
                GTZone zone = GTZone.mountain;
                FindZoneData(zone);
                FindZoneData(GTZone.forest);
                GTZone[] zones = new GTZone[] { zone, GTZone.forest };
                ZoneManagement.SetActiveZones(zones);
            }

            if (GUILayout.Button("Load Rotating"))
            {
                GTZone zone = GTZone.rotating;
                FindZoneData(zone);
                FindZoneData(GTZone.forest);
                GTZone[] zones = new GTZone[] { zone, GTZone.forest };
                ZoneManagement.SetActiveZones(zones);
            }
            if (GUILayout.Button("Load Cave"))
            {
                GTZone zone = GTZone.cave;
                FindZoneData(zone);
                FindZoneData(GTZone.forest);
                GTZone[] zones = new GTZone[] { zone, GTZone.forest };
                ZoneManagement.SetActiveZones(zones);
            }

            if (GUILayout.Button("Load Canyons"))
            {
                Canyons = !Canyons;
                GTZone zone = GTZone.canyon;
                FindZoneData(zone);
                FindZoneData(GTZone.forest);
                GTZone[] zones = new GTZone[] { zone, GTZone.forest };

                ZoneManagement.SetActiveZones(zones);
            }
        
            GUILayout.EndArea();
        }

        void ToggleSceneLoad(string sceneName, ref bool sceneToggle)
        {
            if (sceneToggle)
            {
                SceneManager.UnloadSceneAsync(sceneName);
                sceneToggle = false;
            }
            else
            {
                SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
                sceneToggle = true;
            }
        }


        private IEnumerator UnloadAndLoadScenes()
        {

            city.SetActive(true);
            if (Canyons) yield return UnloadIfLoaded("Canyon2");
            if (Beach) yield return UnloadIfLoaded("Beach");
            if (Mountains) yield return UnloadIfLoaded("Mountain");
            if (Clouds) yield return UnloadIfLoaded("Skyjungle");
            if (Basement) yield return UnloadIfLoaded("Basement");
            if (Cave) yield return UnloadIfLoaded("Cave");
            if (Metropolis) yield return UnloadIfLoaded("Metropolis");
            if (Rotating) yield return UnloadIfLoaded("Rotating");

            City = true;
            Canyons = true;
            Beach = true;
            Mountains = true;
            Clouds = true;
            Basement = true;
            Cave = true;
            Metropolis = true;
            Rotating = true;

            SceneManager.LoadScene("City", LoadSceneMode.Additive);
            SceneManager.LoadScene("Canyon2", LoadSceneMode.Additive);
            SceneManager.LoadScene("Beach", LoadSceneMode.Additive);
            SceneManager.LoadScene("Mountain", LoadSceneMode.Additive);
            SceneManager.LoadScene("Skyjungle", LoadSceneMode.Additive);
            SceneManager.LoadScene("Basement", LoadSceneMode.Additive);
            SceneManager.LoadScene("Cave", LoadSceneMode.Additive);
            SceneManager.LoadScene("Metropolis", LoadSceneMode.Additive);
            SceneManager.LoadScene("Rotating", LoadSceneMode.Additive);
        }

        private IEnumerator UnloadAllExcept(string exceptScene)
        {
            city.SetActive(false);
            if (exceptScene != "Canyon2") yield return UnloadIfLoaded("Canyon2");
            if (exceptScene != "Beach") yield return UnloadIfLoaded("Beach");
            if (exceptScene != "Mountain") yield return UnloadIfLoaded("Mountain");
            if (exceptScene != "Skyjungle") yield return UnloadIfLoaded("Skyjungle");
            if (exceptScene != "Basement") yield return UnloadIfLoaded("Basement");
            if (exceptScene != "Cave") yield return UnloadIfLoaded("Cave");
            if (exceptScene != "Metropolis") yield return UnloadIfLoaded("Metropolis");
            if (exceptScene != "Rotating") yield return UnloadIfLoaded("Rotating");

            if (exceptScene != "City") City = false;
            if (exceptScene != "Canyon2") Canyons = false;
            if (exceptScene != "Beach") Beach = false;
            if (exceptScene != "Mountain") Mountains = false;
            if (exceptScene != "Skyjungle") Clouds = false;
            if (exceptScene != "Basement") Basement = false;
            if (exceptScene != "Cave") Cave = false;
            if (exceptScene != "Metropolis") Metropolis = false;
            if (exceptScene != "Rotating") Rotating = false;
        }

        private IEnumerator UnloadIfLoaded(string sceneName)
        {
            Scene scene = SceneManager.GetSceneByName(sceneName);
            if (scene.isLoaded)
            {
                AsyncOperation asyncOperation = SceneManager.UnloadSceneAsync(sceneName);
                while (!asyncOperation.isDone)
                {
                    yield return null;
                }
            }
        }
        private void SetupCamera()
        {
            if (GorillaLocomotion.Player.Instance != null)
            {
                GameObject shoulderCamera = GameObject.Find("Player Objects/Third Person Camera/Shoulder Camera");
                if (shoulderCamera == null)
                {
                    // return;
                }
                else
                {

                    CamObject = GameObject.Find("Player Objects/Third Person Camera/Shoulder Camera");
                    if (CamObject != null)
                    {
                        Camera shoulderCamComponent = shoulderCamera.GetComponent<Camera>();
                        if (shoulderCamComponent == null)
                        {
                            //return;
                        }
                        CineCamObject = CamObject.transform.Find("CM vcam1")?.gameObject;
                        Cam = CamObject.GetComponent<Camera>();
                        playerListener = GorillaLocomotion.Player.Instance.GetComponentInChildren<AudioListener>();
                        if (cameraListener == null)
                        {
                            cameraListener = Cam.gameObject.AddComponent<AudioListener>();
                        }
                        if (Cam == null)
                        {
                        }
                        else
                        {
                            Cam.nearClipPlane = 0.01f;
                        }

                        if (CineCamObject != null)
                        {
                            CineCam = CineCamObject.GetComponent<CinemachineVirtualCamera>();
                            if (CineCam != null)
                            {
                                CineCam.enabled = false;
                            }
                            else
                            {
                            }
                        }
                        else
                        {
                        }
                    }
                    else
                    {
                    }
                }
            }
            else
            {
            }

        }
        void HandleFreeLook()
        {
            if (looking)
            {
                Transform transform = CamObject.transform;
                Vector2 mouseDelta = Mouse.current.delta.ReadValue();
                float newRotationY = transform.localEulerAngles.y + mouseDelta.x * freeLookSensitivity * Time.deltaTime;
                float newRotationX = transform.localEulerAngles.x - mouseDelta.y * freeLookSensitivity * Time.deltaTime;
                transform.localEulerAngles = new Vector3(newRotationX, newRotationY, 0f);

            }

            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                if (lockcusor)
                {
                    StartLooking();
                }
            }
            else if (Mouse.current.rightButton.wasReleasedThisFrame)
            {
                if (!lockcusor)
                {
                    StopLooking();
                }
            }
        }
        private Vector2 dragStartMousePosition;
        void HandleCursorLock()
        {
            if (Keyboard.current != null && (Keyboard.current.lKey.wasPressedThisFrame))
            {
                if (UnityEngine.Cursor.lockState == CursorLockMode.Locked)
                {
                    StopLooking();
                    lockcusor = false;
                }
                else if (UnityEngine.Cursor.lockState == CursorLockMode.None)
                {
                    StartLooking();
                    lockcusor = true;
                }
            }
        }
        public void StartLooking()
        {
            looking = true;
            UnityEngine.Cursor.visible = false;
            UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        }
        public void StopLooking()
        {
            looking = false;
            UnityEngine.Cursor.visible = true;
            UnityEngine.Cursor.lockState = CursorLockMode.None;
        }

        string[] options = {
            "Linear",
            "EaseInOut",
            "Constant",
            "EaseIn",
            "EaseOut",
            "Bounce",
            "Exponential",
            "BounceIn",
            "BounceInOut",
            "BounceOut",
            "Decelerate",
            "Ease",
            "EaseIn",
            "EaseInBack",
            "EaseInCirc",
            "EaseInCubic",
            "EaseInExpo",
            "EaseInOut",
            "EaseInOutBack",
            "EaseInOutCirc",
            "EaseInOutCubic",
            "EaseInOutCubicEmphasized",
            "EaseInOutExpo",
            "EaseInOutQuad",
            "EaseInOutQuart",
            "EaseInOutQuint",
            "EaseInOutSine",
            "EaseInQuad",
            "EaseInQuart",
            "EaseInQuint",
            "EaseInSine",
            "EaseInToLinear",
            "EaseOut",
            "EaseOutBack",
            "EaseOutCirc",
            "EaseOutCubic",
            "EaseOutExpo",
            "EaseOutQuad",
            "EaseOutQuart",
            "EaseOutQuint",
            "EaseOutSine",
            "ElasticIn",
            "ElasticOut",
            "FastEaseInToSlowEaseOut",
            "FastLinearToSlowEaseIn",
            "FastOutSlowIn",
            "Linear",
            "LinearToEaseOut",
            "SlowMiddle"
        };
        private int selectedIndex = 0;
        private bool showDropdown = false;
        private void OnGUI()
        {
            if (guiVisible)
            {
                MapLoader();
            }
            GUI.skin = skin;
            if (isPlaying)
            {
                PlayAnimation();
            }

            if (Event.current.type == UnityEngine.EventType.KeyDown && Event.current.keyCode == KeyCode.P)
            {
                isPlaying = !isPlaying;
                if (isPlaying)
                {
                    Console.WriteLine(keyframes.Length);
                    lerpTime = 0f;
                    currentTime = timelineRect.x / zoomFactor;
                }
                else
                {
                    lerpTime = 0f;
                }
            }
            if (Event.current.type == UnityEngine.EventType.KeyDown && Event.current.keyCode == KeyCode.Tab)
            {
                guiVisible = !guiVisible;
            }
            if (Event.current.type == UnityEngine.EventType.KeyDown && Event.current.keyCode == KeyCode.K)
            {
                float fov = CamObject.GetComponent<Camera>().fieldOfView;
                KeyframePostion[] newKeyframes = new KeyframePostion[keyframes.Length + 1];
                for (int i = 0; i < keyframes.Length; i++)
                {
                    newKeyframes[i] = keyframes[i];
                }
                newKeyframes[keyframes.Length] = new KeyframePostion(CamObject.transform.position, CamObject.transform.rotation, fov);
                keyframes = newKeyframes;

                currentFrame = keyframes.Length - 1;
                nextFrame = (currentFrame + 1) % keyframes.Length;
                cube.transform.position = keyframes[currentFrame].position;
                cube.transform.rotation = keyframes[currentFrame].rotation;
                GameObject keyframeVisual = GameObject.CreatePrimitive(PrimitiveType.Cube);
                keyframeVisual.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                keyframeVisual.GetComponent<Renderer>().material.shader = Shader.Find("GorillaTag/UberShader");
                keyframeVisual.GetComponent<Renderer>().material.color = Color.red;
                keyframeVisual.transform.position = CamObject.transform.position;
                keyframeVisual.transform.rotation = CamObject.transform.rotation;

                GameObject[] newKeyframesVisual = new GameObject[keyframes.Length];
                for (int i = 0; i < keyframesvisual.Length; i++)
                {
                    newKeyframesVisual[i] = keyframesvisual[i];
                }
                newKeyframesVisual[keyframes.Length - 1] = keyframeVisual;
                keyframesvisual = newKeyframesVisual;
                AddKeyframe(timelineRect.x + scrollPosition.x);
            }

            if (guiVisible)
            {
                foreach (var visual in keyframesvisual)
                {
                    visual.active = true;
                }
                float guiWidth = 600;
                float guiHeight = 255f;
                float xPos = (Screen.width - guiWidth) / 2;
                float yPos = Screen.height - guiHeight - 20;

                GUI.Box(new Rect(xPos, yPos, guiWidth, guiHeight), "");

                GUILayout.BeginArea(new Rect(xPos + 20, yPos + 15, guiWidth - 40, 30));
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Camera", GUILayout.Width(80)))
                {
                    if (keyframes.Length > 0)
                    {
                        cameraview = !cameraview;
                    }
                }

                if (GUILayout.Button("<", GUILayout.Width(40)))
                {
                    if (keyframes.Length > 0)
                    {
                        currentFrame = (currentFrame - 1 + keyframes.Length) % keyframes.Length;
                        nextFrame = (currentFrame + 1) % keyframes.Length;
                        cube.transform.position = keyframes[currentFrame].position;
                        cube.transform.rotation = keyframes[currentFrame].rotation;
                        newKeyframe = keyframes[currentFrame];
                    }
                }

                if (GUILayout.Button(">", GUILayout.Width(40)))
                {
                    if (keyframes.Length > 0)
                    {
                        currentFrame = (currentFrame + 1) % keyframes.Length;
                        nextFrame = (currentFrame + 1) % keyframes.Length;
                        cube.transform.position = keyframes[currentFrame].position;
                        cube.transform.rotation = keyframes[currentFrame].rotation;
                        newKeyframe = keyframes[currentFrame];
                    }
                }

                if (GUILayout.Button("Reset", GUILayout.Width(60))) { ResetTimeline(); }

                GUILayout.Label("FPS:", GUILayout.Width(40));
                string fpsInput = GUILayout.TextField(fps.ToString(), GUILayout.Width(50));
                if (float.TryParse(fpsInput, out float newFps) && newFps >= 1)
                {
                    fps = newFps;

                }
                GUILayout.Label("FOV:", GUILayout.Width(40));
                string FovInput = GUILayout.TextField(fov.ToString(), GUILayout.Width(50));
                if (float.TryParse(FovInput, out float fovnew) && fovnew >= 1)
                {
                    fov = fovnew;

                }

                if (keyframeselected || isPlaying)
                {
                    int frameplaying;
                    if (isPlaying)
                    {
                        frameplaying = currentFrame;
                    }
                    else
                    {
                        frameplaying = keyframeselectednum;
                    }
                    GUILayout.Label("Key: " + frameplaying, GUILayout.Width(40));
                    if (keyframes.Length > frameplaying)
                    {
                        GUILayout.Label("Fov: " + keyframes[frameplaying].fov, GUILayout.Width(40));
                    }
                }
                GUILayout.EndHorizontal();
                GUILayout.EndArea();

                GUILayout.BeginArea(new Rect(xPos + 20, yPos + 50, guiWidth - 40, 30));
                GUILayout.BeginHorizontal();

                if (GUILayout.Button("Zoom In", GUILayout.Width(100)))
                {
                    zoomFactor += 0.1f;
                    zoomFactor = Mathf.Clamp(zoomFactor, 0.1f, maxFrames / 24f);
                }
                if (GUILayout.Button("Zoom Out", GUILayout.Width(100)))
                {
                    if (zoomFactor > 0.7f)
                    {
                        zoomFactor -= 0.1f;
                        zoomFactor = Mathf.Clamp(zoomFactor, 0.1f, maxFrames / 24f);
                    }
                }

                loopPlayback = GUILayout.Toggle(loopPlayback, " Loop Playback");
                if (keyframeselected)
                {
                    if (GUILayout.Button("Delete Keyframe"))
                    {
                        keyframePositions.RemoveAt(keyframeselectednum);
                        if (keyframesvisual.Length > keyframeselectednum && keyframesvisual[keyframeselectednum] != null)
                        {
                            Destroy(keyframesvisual[keyframeselectednum]);
                        }
                        keyframesvisual = keyframesvisual.Where((_, index) => index != keyframeselectednum).ToArray();
                        if (keyframes.Length > keyframeselectednum)
                        {
                            List<KeyframePostion> keyframesList = keyframes.ToList();
                            keyframesList.RemoveAt(keyframeselectednum);
                            keyframes = keyframesList.ToArray();
                        }
                        if (draggedKeyframeIndex == keyframeselectednum)
                        {
                            draggedKeyframeIndex = -1;
                        }
                        keyframeselected = false;
                        keyframeselectednum = -1;
                    }
                }
                if (!keyframeselected)
                {
                    if (GUILayout.Button("<", GUILayout.Width(50)))
                    {
                        selectedIndex = (selectedIndex - 1 + options.Length) % options.Length;
                        UpdateAnimationCurve();
                    }
                    GUILayout.Label($"{options[selectedIndex]}", GUILayout.Width(100));
                    if (GUILayout.Button(">", GUILayout.Width(50)))
                    {
                        selectedIndex = (selectedIndex + 1) % options.Length;
                        UpdateAnimationCurve();
                    }

                }// sorry not sorry got bored but if u find this dm monkey! and il say something
                GUILayout.EndHorizontal();
                GUILayout.EndArea();

                scrollPosition = GUI.BeginScrollView(new Rect(xPos + 40, yPos + 90, guiWidth - 80, 130), scrollPosition, new Rect(0, 0, maxTimeline * zoomFactor, 80), true, false);

                float labelSpacing = 60f * zoomFactor;
                for (int i = 1; i <= maxFrames; i++)
                {
                    GUI.Label(new Rect(i * labelSpacing, 0, 60, 80), i.ToString());
                }
                if (Event.current.type == UnityEngine.EventType.MouseDown && Event.current.button == 0)
                {
                    float clickedPosition = Event.current.mousePosition.x + scrollPosition.x;
                    timelineRect.x = Mathf.Clamp(clickedPosition - scrollPosition.x, 0, maxTimeline * zoomFactor);
                    currentTime = timelineRect.x / zoomFactor;
                    keyframeselected = false;
                    keyframeselectednum = -1;
                    Event.current.Use();
                }
                float markerScreenX = timelineRect.x;
                GUI.DrawTexture(new Rect(markerScreenX, 0, 2, 130), Texture2D.whiteTexture);
                float dragOffset = 0;

                for (int i = 0; i < keyframePositions.Count; i++)
                {
                    float keyframeWidth = keyframeBaseWidth * zoomFactor;
                    float displayedKeyframeX = keyframePositions[i] * zoomFactor - scrollPosition.x;
                    Rect keyframe = new Rect(displayedKeyframeX, 0, keyframeWidth, 80);
                    if (keyframe.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown && Event.current.button == 0)
                    {
                        draggedKeyframeIndex = i;
                        dragStartMousePosition = Event.current.mousePosition;
                        Event.current.Use();
                    }
                    if (draggedKeyframeIndex == i && Event.current.type == EventType.MouseDrag)
                    {
                        float offset = Event.current.mousePosition.x - dragStartMousePosition.x;
                        float newKeyframeX = keyframePositions[draggedKeyframeIndex] + offset / zoomFactor;
                        keyframePositions[draggedKeyframeIndex] = newKeyframeX;
                        dragStartMousePosition.x = Event.current.mousePosition.x;
                        Event.current.Use();
                    }
                    if (Event.current.type == EventType.MouseUp && draggedKeyframeIndex == i)
                    {
                        float offset = Event.current.mousePosition.x - dragStartMousePosition.x;
                        float newKeyframeX = keyframePositions[draggedKeyframeIndex] + offset / zoomFactor;
                        keyframePositions[draggedKeyframeIndex] = newKeyframeX;
                        dragStartMousePosition.x = Event.current.mousePosition.x;
                        Event.current.Use();

                    }
                    if (keyframe.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown && Event.current.button == 1)
                    {
                        keyframeselected = true;
                        keyframeselectednum = i;

                        Event.current.Use();
                        break;
                    }
                    Color diamondColor = (keyframeselected && keyframeselectednum == i) ? Color.red : Color.white;
                    DrawDiamond(new Vector2(displayedKeyframeX + keyframeWidth / 2, 40), 10, diamondColor);
                }

                GUI.EndScrollView();
                if (GUI.Button(new Rect(xPos + 20, yPos + 230, 140, 20), "Add Keyframe"))
                {

                    float fov = CamObject.GetComponent<Camera>().fieldOfView;
                    KeyframePostion[] newKeyframes = new KeyframePostion[keyframes.Length + 1];
                    for (int i = 0; i < keyframes.Length; i++)
                    {
                        newKeyframes[i] = keyframes[i];
                    }
                    newKeyframes[keyframes.Length] = new KeyframePostion(CamObject.transform.position, CamObject.transform.rotation, fov);
                    keyframes = newKeyframes;
                    currentFrame = keyframes.Length - 1;
                    nextFrame = (currentFrame + 1) % keyframes.Length;
                    cube.transform.position = keyframes[currentFrame].position;
                    cube.transform.rotation = keyframes[currentFrame].rotation;
                    GameObject keyframeVisual = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    keyframeVisual.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                    keyframeVisual.GetComponent<Renderer>().material.shader = Shader.Find("GorillaTag/UberShader");
                    keyframeVisual.GetComponent<Renderer>().material.color = Color.red;
                    keyframeVisual.transform.position = CamObject.transform.position;
                    keyframeVisual.transform.rotation = CamObject.transform.rotation;
                    GameObject[] newKeyframesVisual = new GameObject[keyframes.Length];
                    for (int i = 0; i < keyframesvisual.Length; i++)
                    {
                        newKeyframesVisual[i] = keyframesvisual[i];
                    }
                    newKeyframesVisual[keyframes.Length - 1] = keyframeVisual;
                    keyframesvisual = newKeyframesVisual;
                    AddKeyframe(timelineRect.x + scrollPosition.x);
                    print($"Keyframes: {keyframePositions}");
                    for (int i = 0; i < keyframePositions.Count; i++)
                    {
                        print(keyframePositions[i]);

                    }
                }
                if (GUI.Button(new Rect(xPos + 180, yPos + 230, 140, 20), "Save Animation"))
                {
                    ExportKeyframes();
                }
                if (GUI.Button(new Rect(xPos + 340, yPos + 230, 140, 20), "Load Animation"))
                {
                }
                if (keyframeselected && keyframes.Length > keyframeselectednum)
                {
                    float currentFOV = keyframes[keyframeselectednum].fov;
                    GUI.Label(new Rect(xPos + 500, yPos + 230, 40, 20), "FOV:");
                    string fovinput2 = GUI.TextField(new Rect(xPos + 540, yPos + 230, 50, 20), currentFOV.ToString());
                    if (float.TryParse(fovinput2, out float fovnew2) && fovnew2 >= 1)
                    {
                        keyframes[keyframeselectednum].fov = fovnew2;
                    }
                }



            }
            else
            {
                foreach (var visual in keyframesvisual)
                {
                    visual.active = false;
                }
            }


        }
        private void ResetTimeline()
        {
            keyframes = new KeyframePostion[0];
            currentFrame = 0;
            nextFrame = 0;
            keyframePositions.Clear();
            foreach (var visual in keyframesvisual)
            {
                Destroy(visual);
            }
            keyframesvisual = new GameObject[0];
            currentTime = 0;
            isPlaying = false;
        }
        private void DrawDiamond(Vector2 position, float size, Color color)
        {
            Vector2[] diamondPoints = new Vector2[4];
            diamondPoints[0] = new Vector2(position.x, position.y - size * Mathf.Sqrt(2) / 2);
            diamondPoints[1] = new Vector2(position.x + size * Mathf.Sqrt(2) / 2, position.y);
            diamondPoints[2] = new Vector2(position.x, position.y + size * Mathf.Sqrt(2) / 2);
            diamondPoints[3] = new Vector2(position.x - size * Mathf.Sqrt(2) / 2, position.y);

            GL.PushMatrix();
            GL.Begin(GL.QUADS);
            GL.Color(color);
            for (int i = 0; i < diamondPoints.Length; i++)
            {
                GL.Vertex3(diamondPoints[i].x, diamondPoints[i].y, 0);
            }
            GL.End();
            GL.PopMatrix();
        }
        private void AddKeyframe(float positionX)
        {
            if (!keyframePositions.Contains(positionX))
            {
                keyframePositions.Add(positionX);
            }
        }
        private int efef = 0;
        private float maxDistanceFactor = 1.0f;
        private float minTimePerFrame = 0.05f;
        private float maxTimePerFrame = 0.5f;
        public static float timestart = 0.0f;
        public static float valuestart = 0.0f;
        public static float starttime = 1.0f;
        public static float endtime = 1.0f;
        private AnimationCurve myCurve;
        public AnimationCurve curve = AnimationCurve.Linear(timestart, valuestart, starttime, endtime);
        public Transform start;
        public Transform end;
        public float duration = 1.0f;
        private float t = 0.0f;
        
        void UpdateAnimationCurve()
        {
            switch (selectedIndex)
            {
                case 0: // Linear
                    curve = AnimationCurve.Linear(timestart, valuestart, starttime, endtime);
                    break;
                case 1: // Ease
                    curve = AnimationFunction.CreateEaseCurve();
                    break;
                case 2: // Ease In
                    curve = AnimationFunction.CreateEaseInCurve();
                    break;
                case 3: // Ease Out
                    curve = AnimationFunction.CreateEaseOutCurve();
                    break;
                case 4: // Ease In and Out
                    curve = AnimationCurve.EaseInOut(timestart, valuestart, starttime, endtime);
                    break;
                case 5: // Bounce In
                    curve = AnimationFunction.CreateBounceInCurve();
                    break;
                case 6: // Bounce Out
                    curve = AnimationFunction.CreateBounceOutCurve();
                    break;
                case 7: // Bounce In and Out
                    curve = AnimationFunction.CreateBounceInOutCurve();
                    break;
                case 8: // Decelerate
                    curve = AnimationFunction.CreateDecelerateCurve();
                    break;
                case 9: // Elastic In
                    curve = AnimationFunction.CreateElasticInCurve();
                    break;
                case 10: // Elastic Out
                    curve = AnimationFunction.CreateElasticOutCurve();
                    break;
                case 11: // Ease In Back
                    curve = AnimationFunction.CreateEaseInBackCurve();
                    break;
                case 12: // Ease In Circ
                    curve = AnimationFunction.CreateEaseInCircCurve();
                    break;
                case 13: // Ease In Cubic
                    curve = AnimationFunction.CreateEaseInCubicCurve();
                    break;
                case 14: // Ease In Expo
                    curve = AnimationFunction.CreateEaseInExpoCurve();
                    break;
                case 15: // Ease In Out Back
                    curve = AnimationFunction.CreateEaseInOutBackCurve();
                    break;
                case 16: // Ease In Out Circ
                    curve = AnimationFunction.CreateEaseInOutCircCurve();
                    break;
                case 17: // Ease In Out Cubic
                    curve = AnimationFunction.CreateEaseInOutCubicCurve();
                    break;
                case 18: // Ease In Out Quad
                    curve = AnimationFunction.CreateEaseInOutQuadCurve();
                    break;
                case 19: // Ease In Out Quart
                    curve = AnimationFunction.CreateEaseInOutQuartCurve();
                    break;
                case 20: // Ease In Out Quint
                    curve = AnimationFunction.CreateEaseInOutQuintCurve();
                    break;
                case 21: // Ease In Out Sine
                    curve = AnimationFunction.CreateEaseInOutSineCurve();
                    break;
                case 22: // Ease In Quad
                    curve = AnimationFunction.CreateEaseInQuadCurve();
                    break;
                case 23: // Ease In Quart
                    curve = AnimationFunction.CreateEaseInQuartCurve();
                    break;
                case 24: // Ease In Quint
                    curve = AnimationFunction.CreateEaseInQuintCurve();
                    break;
                case 25: // Ease In Sine
                    curve = AnimationFunction.CreateEaseInSineCurve();
                    break;
                case 26: // Ease Out Back
                    curve = AnimationFunction.CreateEaseOutBackCurve();
                    break;
                case 27: // Ease Out Circ
                    curve = AnimationFunction.CreateEaseOutCircCurve();
                    break;
                case 28: // Ease Out Cubic
                    curve = AnimationFunction.CreateEaseOutCubicCurve();
                    break;
                case 29: // Ease Out Expo
                    curve = AnimationFunction.CreateEaseOutExpoCurve();
                    break;
                case 30: // Ease Out Quad
                    curve = AnimationFunction.CreateEaseOutQuadCurve();
                    break;
                case 31: // Ease Out Quart
                    curve = AnimationFunction.CreateEaseOutQuartCurve();
                    break;
                case 32: // Ease Out Quint
                    curve = AnimationFunction.CreateEaseOutQuintCurve();
                    break;
                case 33: // Ease Out Sine
                    curve = AnimationFunction.CreateEaseOutSineCurve();
                    break;
                case 34: // Fast Ease In to Slow Ease Out
                    curve = AnimationFunction.CreateFastEaseInToSlowEaseOut();
                    break;
                case 35: // Fast Linear to Slow Ease In
                    curve = AnimationFunction.CreateFastLinearToSlowEaseIn();
                    break;
                case 36: // Fast Out Slow In
                    curve = AnimationFunction.CreateFastOutSlowIn();
                    break;
                case 37: // Linear to Ease Out
                    curve = AnimationFunction.CreateLinearToEaseOut();
                    break;
                case 38: // Slow Middle
                    curve = AnimationFunction.CreateSlowMiddle();
                    break;
                default:
                    curve = AnimationCurve.Linear(timestart, valuestart, starttime, endtime);
                    break;
            }
        }

       

        private void CreateKeyframeVisual(KeyframePostion keyframe)
        {
            GameObject keyframeVisual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            keyframeVisual.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
            keyframeVisual.GetComponent<Renderer>().material.shader = Shader.Find("GorillaTag/UberShader");
            keyframeVisual.GetComponent<Renderer>().material.color = Color.red;

            keyframeVisual.transform.position = keyframe.position;
            keyframeVisual.transform.rotation = keyframe.rotation;

            keyframesvisual.AddItem(keyframeVisual);
        }

        public void CreateAllKeyframesVisuals()
        {
            foreach (var visual in keyframesvisual)
            {
                Destroy(visual); 
            }
            keyframesvisual = new GameObject[0]; 

            foreach (var keyframe in keyframes)
            {
                CreateKeyframeVisual(keyframe);
            }
        }
        private void PlayAnimation()
        {
            currentTime += Time.deltaTime;
            float totalDuration = 24f / fps;
            float newMarkerPosition = (currentTime / totalDuration) * maxTimeline * zoomFactor;
            if (keyframes.Length > 0)
            {
                if (isPlaying)
                {
                    float timePerFrame = totalDuration / keyframes.Length;
                    timer += Time.deltaTime;
                    while (timer >= timePerFrame)
                    {
                        timer -= timePerFrame;
                        currentFrame++;

                        if (currentFrame >= keyframes.Length)
                        {
                            if (loopPlayback)
                            {
                                currentFrame = 0;
                                currentTime = 0f;
                                timer = 0f;
                            }
                            else
                            {
                                currentTime = 0f;
                                currentFrame = 0;
                                timer = 0f;
                                efef++;

                                if (efef >= 2)
                                {
                                    currentFrame = keyframes.Length - 1;
                                    isPlaying = false;
                                    efef = 0;
                                }
                            }
                        }
                    }
                    if (currentFrame < keyframes.Length - 1)
                    {
                        KeyframePostion start = keyframes[currentFrame];
                        KeyframePostion end = keyframes[currentFrame + 1];

                        float lerpFactor = timer / timePerFrame;
                        float curveT = curve.Evaluate(lerpFactor);


                        cube.transform.position = Vector3.Lerp(start.position, end.position, curveT);
                        cube.transform.rotation = Quaternion.Slerp(start.rotation, end.rotation, curveT);
                        CamObject.GetComponent<Camera>().fieldOfView = Mathf.Lerp(start.fov, end.fov, curveT);
                    }
                    else
                    {

                        cube.transform.position = keyframes[currentFrame].position;
                        cube.transform.rotation = keyframes[currentFrame].rotation;
                    }
                }
                else
                {

                    cube.transform.position = keyframes[currentFrame].position;
                    cube.transform.rotation = keyframes[currentFrame].rotation;
                }
                timelineRect.x = newMarkerPosition;
            }
        }




        void Start()
        {
            GorillaTagger.OnPlayerSpawned(playerSpawned);
        }
        void playerSpawned()
        {
            city = GameObject.Find("City_WorkingPrefab");
            forest = GameObject.Find("Environment Objects/LocalObjects_Prefab/Forest/");
            bundle = AssetBundle.LoadFromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("Cam.Assets.ui"));
            skin = bundle.LoadAsset<GUISkin>("Skin");
            timelineRect = new Rect(40, 160, 2, 140);
            maxTimeline = maxFrames * 60f;
            if (!Directory.Exists(cameraPath))
            {
                Directory.CreateDirectory(cameraPath);
            }
            cube = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            cube.transform.position = new Vector3(0, 0, 0);
            cube.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            cube.transform.rotation = Quaternion.Euler(90, 0, 0);
            MeshRenderer renderer = cube.GetComponent<MeshRenderer>();
            Material material = new Material(Shader.Find("GorillaTag/UberShader"));
            material.color = Color.blue;
            renderer.material = material;

            if (cube == null)
            {
                Debug.LogError("Cube GameObject not found in the scene.");
                return;
            }
            timePerFrame = 1f / frameRate;
            SetupCamera();
            loaded = true;
        }
    }
}
