using System;
using UnityEngine;

[System.Serializable]
public class Keyframe
{
    public Vector3 position;
    public Quaternion rotation;
    public float fov;

    public Keyframe(Vector3 pos, Quaternion rot, float fieldOfView)
    {
        position = pos;
        rotation = rot;
        fov = fieldOfView;
    }
}

[Serializable]
public class SerializableKeyframe
{
    public Vector3 position;
    public Quaternion rotation;

    public SerializableKeyframe(Vector3 position, Quaternion rotation)
    {
        this.position = position;
        this.rotation = rotation;
    }
}
[System.Serializable]
public class KeyframeData
{
    public Vector3 position;
    public Quaternion rotation;
}

[System.Serializable]
public class AnimationData
{
    public float frameRate;
    public float fps;
    public bool loopPlayback;
    public KeyframeData[] keyframes;
}
