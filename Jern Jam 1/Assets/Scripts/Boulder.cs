using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EZCameraShake;

public class Boulder : TriggerForwardTarget
{
    [SerializeField] private float triggerTimer;
    [SerializeField] private Vector3 triggerVelocity;
    [SerializeField] private Vector3 triggerAngularVelocity;
    [SerializeField] private float kinematicTimer;
    [SerializeField] private Vector3 triggerFriendVelocity;
    [SerializeField] private Rigidbody[] friends;
    [SerializeField] private AudioClip triggerSound;

    private Rigidbody _rb;
    private AudioSource _audio;

    private bool _triggered = false;
    private bool _triggerApplied = false;
    private float _timer = 0f;
    private bool _madeKinematic = false;

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
                _rb.velocity = triggerVelocity;
                _rb.angularVelocity = triggerAngularVelocity;
                foreach (Rigidbody friend in friends) {
                    friend.gameObject.SetActive(true);
                    friend.velocity = triggerFriendVelocity;
                }
                _triggerApplied = true;
                _timer = 0f;
            }
        }
        if (_triggerApplied && !_madeKinematic) {
            _timer += Time.fixedDeltaTime;
            if (_timer >= kinematicTimer) {
                _rb.isKinematic = true;
                _madeKinematic = true;
            }
        }
    }

    public override void TriggerForwardEnter(Collider other)
    {
        if (!_triggered) {
            _triggered = true;
            _timer = 0f;
            _audio.clip = triggerSound;
            _audio.Play();
            CameraShaker.Instance.ShakeOnce(4f, 4f, 5f, 20f);
        }
    }

    public override void TriggerForwardExit(Collider other)
    {

    }
}
