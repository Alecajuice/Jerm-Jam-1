using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicPlayer : TriggerForwardTarget
{
    public static MusicPlayer singleton;

    [SerializeField] private AudioClip ambience;
    [SerializeField] private AudioClip ominous;

    private AudioSource _audio;

    void Awake()
    {
        _audio = GetComponent<AudioSource>();

        DontDestroyOnLoad(gameObject);

        if (singleton != null) {
            Debug.Log("more than 1 controls UI!");
            Destroy(this);
        }
        else singleton = this;
    }

    private static IEnumerator FadeOut(AudioSource audioSource, float FadeTime, AudioClip nextClip) {
        float startVolume = audioSource.volume;
 
        while (audioSource.volume > 0) {
            audioSource.volume -= startVolume * Time.deltaTime / FadeTime;
 
            yield return null;
        }
 
        audioSource.Stop();
        audioSource.volume = startVolume;
        audioSource.clip = nextClip;
        audioSource.Play();
    }

    public void PlayOminous() {
        if (_audio.clip != ominous) StartCoroutine(FadeOut(_audio, 2f, ominous));
    }

    public void StopOminous() {
        if (_audio.clip != ambience) StartCoroutine(FadeOut(_audio, 4f, ambience));
    }

    public override void TriggerForwardEnter(Collider other)
    {
        PlayOminous();
    }

    public override void TriggerForwardExit(Collider other)
    {

    }
}
