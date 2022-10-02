using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarterAssets;

public class DoorController : MonoBehaviour
{
    [SerializeField] private bool automatic = false;
    [SerializeField] private float autoSenseDistance;
    [SerializeField] private bool inverted;
    [SerializeField] private uint numRequiredInputs = 1;
    [SerializeField] private AudioClip doorSound;

    private Animator _animator;
    private AudioSource _audio;
    private FirstPersonController _player;

    private bool _open;
    private uint _numInputs = 0;

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {
        _animator = GetComponent<Animator>();
        _audio = GetComponent<AudioSource>();
        if (automatic) {
            _player = FindObjectOfType<FirstPersonController>();
        }
        _animator.SetBool("character_nearby", inverted);
        _open = inverted;
    }

    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    void Update()
    {
        if (automatic) {
            if (Vector3.Distance(_player.transform.position, transform.position) < autoSenseDistance) {
                if (_numInputs == 0) Activate();
            } else {
                if (_numInputs > 0) Deactivate();
            }
        }
    }

    public void Activate() {
        _numInputs++;
        if (_numInputs >= numRequiredInputs) {
            _animator.SetBool("character_nearby", !inverted);
            _open = !inverted;
            _audio.clip = doorSound;
            _audio.Play();
        }
    }
    
    public void Deactivate() {
        if (_numInputs > 0) _numInputs--;
        if (_numInputs < numRequiredInputs) {
            _animator.SetBool("character_nearby", inverted);
            _open = inverted;
            _audio.clip = doorSound;
            _audio.Play();
        }
    }
}
