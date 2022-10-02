using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonController : MonoBehaviour
{
    [SerializeField] private DoorController[] targets;
    [SerializeField] private AudioClip buttonPressSound;
    [SerializeField] private AudioClip buttonUnpressSound;

    private Animator _animator;
    private AudioSource _audio;

    private uint _objectCount = 0;

    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    void Start()
    {
        _animator = GetComponent<Animator>();
        _audio = GetComponent<AudioSource>();
    }

    public void AddObject() {
        _objectCount++;
        _animator.SetBool("Pressed", true);
        if (_objectCount == 1) {
            foreach (DoorController target in targets) {
                target.Activate();
            }
            _audio.clip = buttonPressSound;
            _audio.Play();
        }
    }

    public void RemoveObject() {
        if (_objectCount > 0) _objectCount--;
        if (_objectCount == 0) {
            _animator.SetBool("Pressed", false);
            foreach (DoorController target in targets) {
                target.Deactivate();
            }
            _audio.clip = buttonUnpressSound;
            _audio.Play();
        }
    }

    /// <summary>
    /// OnTriggerEnter is called when the Collider other enters the trigger.
    /// </summary>
    /// <param name="other">The other Collider involved in this collision.</param>
    void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger) return;
        AddObject();
    }

    /// <summary>
    /// OnTriggerExit is called when the Collider other has stopped touching the trigger.
    /// </summary>
    /// <param name="other">The other Collider involved in this collision.</param>
    void OnTriggerExit(Collider other)
    {
        if (other.isTrigger) return;
        RemoveObject();
    }
}
