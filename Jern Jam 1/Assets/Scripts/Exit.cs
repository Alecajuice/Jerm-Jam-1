using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarterAssets;
using UnityEngine.SceneManagement;

public class Exit : MonoBehaviour
{
    [SerializeField] private Animator fadeToWhiteUI;
    [SerializeField] private string nextScene;
    [SerializeField] private AudioClip exitSound;

    private AudioSource _audio;

    private bool exiting = false;

    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    void Start()
    {
        _audio = GetComponent<AudioSource>();
    }

    /// <summary>
    /// OnTriggerEnter is called when the Collider other enters the trigger.
    /// </summary>
    /// <param name="other">The other Collider involved in this collision.</param>
    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<FirstPersonController>() != null) {
            fadeToWhiteUI.SetBool("FadeOut", true);
            exiting = true;
            _audio.clip = exitSound;
            _audio.Play();
            MusicPlayer.singleton.StopOminous();
        }
    }

    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    void Update()
    {
        if (exiting) {
            if (fadeToWhiteUI.GetCurrentAnimatorStateInfo(0).IsName("Finished")) {
                SceneManager.LoadScene(nextScene);
            }
        }
    }
}
