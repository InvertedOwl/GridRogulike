using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class RandomPitchSound : MonoBehaviour
{

    [Serializable]
    public struct KeySoundPair
    {
        public string key;
        public AudioClip sound;
    }

    public AudioSource audioSource;
    public List<KeySoundPair> keySoundPairs = new List<KeySoundPair>();

    public float minPitch = 0.9f;
    public float maxPitch = 1.1f;

    public float volumescaleind = 1f;

    public void PlaySound(string key, float volumescale = 1.0f)
    {
        PlaySoundInternal(key, volumescale);
    }

    public void PlaySound(string key)
    {
        Debug.Log("Playing " + key);
        PlaySoundInternal(key, volumescaleind);
    }

    private void PlaySoundInternal(string key, float volumeScale)
    {
        if (audioSource == null)
            return;

        if (keySoundPairs == null)
            return;

        audioSource.pitch = Random.Range(minPitch, maxPitch);
        AudioClip clip = keySoundPairs.Find(keyvalue =>
        {
            return keyvalue.key == key;
        }).sound;

        if (clip == null)
        {
            Debug.LogWarning("Missing sound for key: " + key);
            return;
        }

        audioSource.PlayOneShot(clip, volumeScale);
    }
}
