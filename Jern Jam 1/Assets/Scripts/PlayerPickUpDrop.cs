using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerPickUpDrop : MonoBehaviour
{
    [SerializeField] private Transform playerCameraTransform;
    [SerializeField] private Transform objectGrabPointTransform;
    [SerializeField] private LayerMask pickUpLayerMask;
    [SerializeField] private AudioClip pickUpDropSound;

    private PlayerTimeControl _timeControl;
    private AudioSource _audio;

    private ObjectGrabbable _objectGrabbable;

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {
        _timeControl = GetComponent<PlayerTimeControl>();
        _audio = GetComponent<AudioSource>();
    }

    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    void Update()
    {
        // Update controls UI
        float pickUpDistance = 5f;
        if (_objectGrabbable != null) {
            ControlsUI.singleton.ShowGrabbingObject();
            return;
        }
        if (Physics.Raycast(playerCameraTransform.position, playerCameraTransform.forward, out RaycastHit raycastHit, pickUpDistance, pickUpLayerMask, QueryTriggerInteraction.Collide)) {
            ObjectGrabbable target;
            ManualButtonController manualButton;
            TimeControlWatch watch;
            if (raycastHit.transform.TryGetComponent(out target)) {
                ControlsUI.singleton.ShowPickUp();
                return;
            } else if (raycastHit.transform.TryGetComponent(out manualButton)) {
                ControlsUI.singleton.ShowPush();
                return;
            } else if (raycastHit.transform.TryGetComponent(out watch)) {
                if (!watch.IsCollected()) {
                    ControlsUI.singleton.ShowTake();
                    return;
                }
            }
        }
        ControlsUI.singleton.HideE();
    }

    public void OnPickUp() {
        if (_objectGrabbable == null) { // Not holding an object
            float pickUpDistance = 5f;
            if (Physics.Raycast(playerCameraTransform.position, playerCameraTransform.forward, out RaycastHit raycastHit, pickUpDistance, pickUpLayerMask, QueryTriggerInteraction.Collide)) {
                ManualButtonController manualButton;
                TimeControlWatch watch;
                if (raycastHit.transform.TryGetComponent(out _objectGrabbable)) {
                    ObjectTimeControllable objectTimeControllable = _objectGrabbable.GetComponent<ObjectTimeControllable>();
                    if (_timeControl.GetTimeControlledObject() != null) {
                        if (_timeControl.GetTimeControlledObject() != objectTimeControllable) {
                            // Picking up a different object while time controlling
                            _objectGrabbable = null; // Cancel grabbing
                            return;
                        } else if (!_timeControl.ReleaseObject()) {
                            // Failed to release object
                            _objectGrabbable = null; // Cancel grabbing
                            return;
                        }
                    }
                    if (objectTimeControllable.IsBeingControlled()) { // Target object is paused
                        // Release object
                        if (!objectTimeControllable.ReleaseTimeControl()) {
                            _objectGrabbable = null; // Cancel grabbing
                            return;
                        }
                    }
                    if (!_objectGrabbable.Grab(playerCameraTransform, objectGrabPointTransform)) {
                        // Object could not be grabbed
                        _objectGrabbable = null; // Cancel grabbing
                        return;
                    }
                    _audio.clip = pickUpDropSound;
                    _audio.Play();
                } else if (raycastHit.transform.TryGetComponent(out manualButton)) {
                    // Manual Buttons
                    manualButton.Press();
                    _audio.clip = pickUpDropSound;
                    _audio.Play();
                } else if (raycastHit.transform.TryGetComponent(out watch)) {
                    watch.Collect(gameObject);
                }
            }
        } else if (_objectGrabbable != null) { // Holding an object
            DropObject();
        }
    }

    public bool DropObject() {
        // Try to drop the object
        if (!_objectGrabbable.Drop()) return false; // Failed to drop (colliding with player)
        _objectGrabbable = null;
        _audio.clip = pickUpDropSound;
        _audio.Play();
        return true;
    }

    public ObjectGrabbable GetGrabbedObject() {
        return _objectGrabbable;
    }
}
