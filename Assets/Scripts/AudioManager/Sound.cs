using UnityEngine;

[System.Serializable]
public class Music
{
    public string name;
    public AudioClip clip;
    [Range(0f,1f)]
    public float volume;
    public bool loop;
    [HideInInspector]
    public AudioSource source;
}

[System.Serializable]
public class SFX
{
    public string name;
    public AudioClip clip;
    [Range(0f,1f)]
    public float volume;
    [Range(0f,3f)]
    public float pitch;
    [HideInInspector]
    public AudioSource source;
}