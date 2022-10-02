using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using StarterAssets;

public class PlayerTimeControl : MonoBehaviour
{
    
    [SerializeField] private Transform playerCameraTransform;
    [SerializeField] private LayerMask timeControlLayerMask;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private GameObject CinemachineCameraTarget;
    [SerializeField] private TimeControlUI ui;
    [SerializeField] private AudioClip controllingSound;
    [SerializeField] private AudioClip pauseSound;
    [SerializeField] private AudioClip releaseSound;

    private FirstPersonController _playerController;
    private PlayerPickUpDrop _pickUpDrop;
    private AudioSource _audio;

    private ObjectTimeControllable _objectTimeControllable;

    private ObjectTimeControllable _selfTimeControllable;
    private bool _controllingSelf = false;

    private ObjectTimeControllable _targetingObject = null;

    private bool _enabled = false;

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {
        _playerController = GetComponent<FirstPersonController>();
        _pickUpDrop = GetComponent<PlayerPickUpDrop>();
        _audio = GetComponent<AudioSource>();
        _selfTimeControllable = GetComponent<ObjectTimeControllable>();
        ui.DisablePlayerTimelinePause();
        _enabled = this.enabled;
    }

    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    void Update()
    {
        // Get mouse input and apply to time control
		Vector2 look = new Vector2(Mouse.current.delta.x.ReadValue(), -Mouse.current.delta.y.ReadValue()) * Time.smoothDeltaTime;
        if (_objectTimeControllable != null) {
            _objectTimeControllable.TimeControl(look.x);
            ui.SetObjectTimeline(_objectTimeControllable.GetCurrentTimelineScale());
        } else if (_controllingSelf) {
            _selfTimeControllable.TimeControl(look.x);
            ui.SetPlayerTimeline(_selfTimeControllable.GetCurrentTimelineScale());
        }

        // Update crosshair sprite
        float timeControlDistance = Mathf.Infinity;
        if (IsControllingTime()) {
            ui.SetCrosshair(TimeControlUI.CrosshairType.TimeControlling);
            ControlsUI.singleton.ShowTimeControlling();
            return;
        }
        ObjectTimeControllable prevTarget = _targetingObject;
        if (Physics.Raycast(playerCameraTransform.position, playerCameraTransform.forward, out RaycastHit raycastHit, timeControlDistance, timeControlLayerMask)) {
            if (raycastHit.transform.TryGetComponent(out _targetingObject)) {
                if (prevTarget != _targetingObject && prevTarget != null) {
                    if (!prevTarget.IsBeingControlled()) prevTarget.DisableOutline(); // Keep outline on when paused
                    prevTarget.DisablePaths();
                }
                if (!_targetingObject.IsBeingControlled()) {
                    ui.SetCrosshair(TimeControlUI.CrosshairType.TimeControllable);
                    _targetingObject.SetOutline(ObjectTimeControllable.outlineControlling);
                } else {
                    ui.SetCrosshair(TimeControlUI.CrosshairType.Paused);
                    _targetingObject.SetOutline(ObjectTimeControllable.outlinePaused);
                }
                _targetingObject.EnablePaths();
                ui.EnableObjectTimeline();
                ui.SetObjectTimeline(_targetingObject.GetCurrentTimelineScale());
                ControlsUI.singleton.ShowCanTimeControlObject();
                return;
            }
        }
        ui.SetCrosshair(TimeControlUI.CrosshairType.Normal);
        ui.DisableObjectTimeline();
        ControlsUI.singleton.ShowIdle();
        if (prevTarget != null &&
            (_pickUpDrop.GetGrabbedObject() == null || _pickUpDrop.GetGrabbedObject() != prevTarget.GetComponent<ObjectGrabbable>())) // keep outline and paths when holding object
        { 
            if (!prevTarget.IsBeingControlled()) {
                // Keep outline on when paused
                prevTarget.DisableOutline();
            }
            prevTarget.DisablePaths();
        }
    }

    /// <summary>
    /// This function is called every fixed framerate frame, if the MonoBehaviour is enabled.
    /// </summary>
    void FixedUpdate()
    {
        if (_objectTimeControllable != null) {
            // Rotate view to look at controlled object
            Quaternion currentRotation = _playerController.GetCameraRotation();

            // find the vector pointing from our position to the target
            Vector3 direction = (_objectTimeControllable.transform.position - playerCameraTransform.position).normalized;

            // create the rotation we need to be in to look at the target
            Quaternion lookRotation = Quaternion.LookRotation(direction);

            // rotate us over time according to speed until we are in the required rotation
            Quaternion slerpedRotation = Quaternion.Slerp(currentRotation, lookRotation, Time.deltaTime * rotationSpeed);

            // Rotate player and camera
            _playerController.RotateCamera(slerpedRotation);
        }
    }
    
    public void OnFire() {
        if (!_enabled) return;
        if (_objectTimeControllable == null && !_controllingSelf) { // Not time controlling an object and not controlling self
            // Try time controlling an object
            float timeControlDistance = Mathf.Infinity;
            if (Physics.Raycast(playerCameraTransform.position, playerCameraTransform.forward, out RaycastHit raycastHit, timeControlDistance, timeControlLayerMask)) {
                if (raycastHit.transform.TryGetComponent(out _objectTimeControllable)) {
                    if (_pickUpDrop.GetGrabbedObject() != null) {
                        if (_pickUpDrop.GetGrabbedObject() != _objectTimeControllable.GetComponent<ObjectGrabbable>()) {
                            // Time controlling a different object from the one we're holding, cancel time control
                            _objectTimeControllable = null;
                            return;
                        }
                        if (!_pickUpDrop.DropObject()) {
                            // Failed to drop object, cancel time control
                            _objectTimeControllable = null;
                            return;
                        }
                    }
                    if (!_objectTimeControllable.IsBeingControlled()) {
                        // Start controlling for the first time, else resuming
                        _objectTimeControllable.StartTimeControl();
                    }
                    _objectTimeControllable.SetOutline(ObjectTimeControllable.outlineControlling);
                    _objectTimeControllable.EnablePaths();
                    _playerController.EnableLook(false);
                    ui.EnableObjectTimeline();
                    ui.SetObjectTimeline(_objectTimeControllable.GetCurrentTimelineScale());
                    _audio.clip = controllingSound;
                    _audio.loop = true;
                    _audio.Play();
                }
            }
        } else if (_objectTimeControllable != null) { // Time controlling an object
            // Pause
            _objectTimeControllable.SetOutline(ObjectTimeControllable.outlinePaused);
            _playerController.EnableLook(true);
            _objectTimeControllable = null;
            ui.DisableObjectTimeline();
            _audio.clip = pauseSound;
            _audio.loop = false;
            _audio.Play();
        } else if (_controllingSelf) { // Time controlling self
            // Pause self
            _controllingSelf = false;
            _playerController.EnableLook(true);
            ui.EnablePlayerTimelinePause();
            _audio.clip = pauseSound;
            _audio.loop = false;
            _audio.Play();
        }
    }

    public void OnAltFire() {
        if (!_enabled) return;
        if (_objectTimeControllable == null && !_controllingSelf && _pickUpDrop.GetGrabbedObject() == null) {
            // Time control player
            if (!_selfTimeControllable.IsBeingControlled()) _selfTimeControllable.StartTimeControl(); // Start controlling for the first time, else resuming
            _selfTimeControllable.EnablePaths();
            _playerController.EnableLook(false);
            _playerController.EnableMove(false);
            _playerController.gameObject.layer = LayerMask.NameToLayer("CharacterNoCollisions");
            _controllingSelf = true;
            ui.SetPlayerTimeline(_selfTimeControllable.GetCurrentTimelineScale());
            ui.DisablePlayerTimelinePause();
            _audio.clip = controllingSound;
            _audio.loop = true;
            _audio.Play();
        } else if (_objectTimeControllable != null) {
            // Release time controlled object
            ReleaseObject();
        } else if (_controllingSelf) {
            // Release time control on player
            if (!_selfTimeControllable.ReleaseTimeControl()) return;
            _selfTimeControllable.DisablePaths();
            _controllingSelf = false;
            _playerController.EnableLook(true);
            _playerController.EnableMove(true);
            _playerController.gameObject.layer = LayerMask.NameToLayer("Character");
            ui.SetPlayerTimeline(_selfTimeControllable.GetCurrentTimelineScale());
            _audio.clip = releaseSound;
            _audio.loop = false;
            _audio.Play();
        }
    }

    // public void OnLook(InputValue value) {
    //     if (!_enabled) return;
    //     Vector2 look = value.Get<Vector2>();
    //     if (_objectTimeControllable != null) {
    //         _objectTimeControllable.TimeControl(look.x);
    //         ui.SetObjectTimeline(_objectTimeControllable.GetCurrentTimelineScale());
    //     } else if (_controllingSelf) {
    //         _selfTimeControllable.TimeControl(look.x);
    //         ui.SetPlayerTimeline(_selfTimeControllable.GetCurrentTimelineScale());
    //     }
    // }

    public bool ReleaseObject() {
        if (!_enabled) return false;
        if (!_objectTimeControllable.ReleaseTimeControl()) return false;
        _objectTimeControllable.DisableOutline();
        _objectTimeControllable.DisablePaths();
        _playerController.EnableLook(true);
        _objectTimeControllable = null;
        ui.DisableObjectTimeline();
        _audio.clip = releaseSound;
        _audio.loop = false;
        _audio.Play();
        return true;
    }

    public ObjectTimeControllable GetTimeControlledObject() {
        return _objectTimeControllable;
    }

    public bool IsControllingTime() {
        return _objectTimeControllable != null || _controllingSelf;
    }

    public void SetEnabled(bool enabled) {
        _enabled = enabled;
        this.enabled = enabled;
        if (enabled) {
            ui.EnablePlayerTimeline();
        }
    }
}
