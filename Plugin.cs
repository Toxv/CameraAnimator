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

namespace Cam
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        // -- Objects and Game State Management
        private GameObject cube;
        private GameObject[] keyframesvisual = new GameObject[0];
        private Keyframe[] keyframes = new Keyframe[0];

        private int currentFrame = 0;
        private int nextFrame = 1;
        private int keyframeselectednum = -1;

        // -- Timing and Playback
        private float frameRate = 10f;
        private float timePerFrame;
        private float timer = 0f;
        private bool isPlaying = false;
        private Keyframe newKeyframe = new Keyframe(Vector3.zero, Quaternion.identity, float.NaN);

        // -- GUI Layout
        private Rect editorRect = new Rect(0, Screen.height, 700, 400);

        // -- Lerp Animation
        private float lerpTime = 0f;
        private float lerpDuration = 4f;
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

        public void AddKeyframe(Vector3 position, Quaternion rotation, float fov)
        {
            Array.Resize(ref keyframes, keyframes.Length + 1);
            keyframes[keyframes.Length - 1] = new Keyframe(position, rotation, fov);
            Debug.Log($"Added Keyframe: Position: {position}, Rotation: {rotation}");
        }

        public void ExportKeyframes(/*string filePath*/)
        {
            if (keyframes.Length == 0)
            {
                Debug.LogWarning("No keyframes to export.");
                return;
            }
            string json = "{\n";
            json += $"    \"frameRate\": {frameRate},\n";
            json += $"    \"fps\": {fps},\n";
            json += $"    \"loopPlayback\": {loopPlayback.ToString().ToLower()},\n";
            json += "    \"keyframes\": [\n";

            for (int i = 0; i < keyframes.Length; i++)
            {
                json += "        {\n";
                json += $"            \"position\": [{keyframes[i].position.x}, {keyframes[i].position.y}, {keyframes[i].position.z}],\n";
                json += $"            \"rotation\": [{keyframes[i].rotation.x}, {keyframes[i].rotation.y}, {keyframes[i].rotation.z}, {keyframes[i].rotation.w}]\n";
                json += "        }";

                if (i < keyframes.Length - 1)
                    json += ",";

                json += "\n";
            }

            json += "    ]\n";
            json += "}";
            Debug.Log(json);
        }
        private void Update()
        {
            if (!isPlaying)
            {
                cube.transform.position = newKeyframe.position;
                cube.transform.rotation = newKeyframe.rotation;
            }
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

                        PhotonNetworkController.Instance.disableAFKKick = true;
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
        private void OnGUI()
        {
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
                Keyframe[] newKeyframes = new Keyframe[keyframes.Length + 1];
                for (int i = 0; i < keyframes.Length; i++)
                {
                    newKeyframes[i] = keyframes[i];
                }
                newKeyframes[keyframes.Length] = new Keyframe(CamObject.transform.position, CamObject.transform.rotation,fov);
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

                Console.WriteLine("added keyframe");
                Debug.Log("Keyframes count: " + keyframes.Length);
                Debug.Log("Visual Keyframes count: " + keyframesvisual.Length);
                AddKeyframe(timelineRect.x + scrollPosition.x);
            }

            if (guiVisible)
            {
                foreach (var visual in keyframesvisual)
                {
                    visual.active = true;
                }
                float guiWidth = 600f;
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

                    // Determine the frame being played
                    if (isPlaying)
                    {
                        frameplaying = currentFrame;
                    }
                    else
                    {
                        frameplaying = keyframeselectednum; // Use the selected keyframe index if not playing
                    }

                    // Display keyframe information
                    GUILayout.Label("Key: " + frameplaying, GUILayout.Width(40));

                    // Ensure we don't access an out-of-bounds index
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
                            List<Keyframe> keyframesList = keyframes.ToList();
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
                    if (keyframe.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown && Event.current.button == 0) // Left-click
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
                    if (keyframe.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown && Event.current.button == 1) // Right-click
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
                    Keyframe[] newKeyframes = new Keyframe[keyframes.Length + 1];
                    for (int i = 0; i < keyframes.Length; i++)
                    {
                        newKeyframes[i] = keyframes[i];
                    }
                    newKeyframes[keyframes.Length] = new Keyframe(CamObject.transform.position, CamObject.transform.rotation, fov);
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

                    Console.WriteLine("added keyframe");
                    Debug.Log("Keyframes count: " + keyframes.Length);
                    Debug.Log("Visual Keyframes count: " + keyframesvisual.Length);
                    AddKeyframe(timelineRect.x + scrollPosition.x);
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
            keyframes = new Keyframe[0];
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

        private void PlayAnimation()
        {
            float totalDuration = 24f / fps; 
            currentTime += Time.deltaTime; 
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
                                ++efef;
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
                        
                        Keyframe start = keyframes[currentFrame];
                        Keyframe end = keyframes[currentFrame + 1];

                        nextFrame = currentFrame + 1;
                        float lerpFactor = timer / timePerFrame;
                        cube.transform.position = Vector3.Lerp(keyframes[currentFrame].position, keyframes[nextFrame].position, lerpFactor);
                        cube.transform.rotation = Quaternion.Slerp(keyframes[currentFrame].rotation, keyframes[nextFrame].rotation, lerpFactor);
                        CamObject.GetComponent<Camera>().fieldOfView = Mathf.Lerp(start.fov, end.fov, lerpFactor);
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


                        bundle = AssetBundle.LoadFromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("Cam.Assets.ui"));
                        skin = bundle.LoadAsset<GUISkin>("Skin");
                        timelineRect = new Rect(40, 160, 2, 140);
                        maxTimeline = maxFrames * 60f;

                        cube = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        cube.transform.position = new Vector3(0, 0, 0); // Set position
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
                        GorillaTagger.OnPlayerSpawned(playerSpawned);
             }
        void playerSpawned()
        {
                        SetupCamera();
                        loaded = true;
                    }
                }
            }
