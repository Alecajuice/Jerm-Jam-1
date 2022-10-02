using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EZCameraShake;

public class Bridge : TriggerForwardTarget
{
    [SerializeField] private float triggerTimer;
    [SerializeField] private float initialVelocity;
    [SerializeField] private float gravityMultiplier;
    [SerializeField] private AudioClip triggerSound;
    [SerializeField] private AudioClip fallSound;

    private Rigidbody _rb;
    private AudioSource _audio;

    private bool _triggered = false;
    private bool _triggerApplied = false;
    private float _timer = 0f;
    private float prevVelocity = 0f;

    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _audio = GetComponent<AudioSource>();
    }

    /// <summary>
    /// This function is called every fixed framerate frame, if the MonoBehaviour is enabled.
    /// </summary>
    void FixedUpdate()
    {
        if (_triggered && !_triggerApplied) {
            _timer += Time.fixedDeltaTime;
            if (_timer >= triggerTimer) {
                _rb.isKinematic = false;
                _rb.velocity = new Vector3(0, -initialVelocity, 0);
                _triggerApplied = true;
                _timer = 0f;
            }
        }
        if (_triggerApplied) {
            _rb.AddForce((gravityMultiplier - 1) * Physics.gravity * _rb.mass);
        }
        if (_rb.velocity.y >= -0.1 && prevVelocity < -0.1 && !_rb.isKinematic) {
            _audio.PlayOneShot(fallSound);
        }
        prevVelocity = _rb.velocity.y;
    }

    public override void TriggerForwardEnter(Collider other)
    {
        if (!_triggered) {
            _triggered = true;
            _timer = 0f;
            _audio.PlayOneShot(triggerSound);
            CameraShaker.Instance.ShakeOnce(4f, 4f, 5f, 20f);
        }
    }

    public override void TriggerForwardExit(Collider other)
    {
        
    }
}
