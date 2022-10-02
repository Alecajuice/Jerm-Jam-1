using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectGrabbable : MonoBehaviour
{
    [SerializeField] private LayerMask grabPointLayers;
    [SerializeField] private float minRadius;

    public bool canPickUp = true;

    private Rigidbody _rb;
    private ObjectGrabbablePlayerTrigger _playerTrigger;
    private Transform _playerTransform;
    private Transform _objectGrabPointTransform;

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake() {
        _rb = GetComponent<Rigidbody>();
        _playerTrigger = GetComponentInChildren<ObjectGrabbablePlayerTrigger>();
        gameObject.layer = LayerMask.NameToLayer("PickUp");
    }

    public bool Grab(Transform playerTransform, Transform objectGrabPointTransform) {
        if (!canPickUp) return false;
        this._playerTransform = playerTransform;
        this._objectGrabPointTransform = objectGrabPointTransform;
        _rb.useGravity = false;
        gameObject.layer = LayerMask.NameToLayer("PickedUp");
        return true;
    }

    public bool Drop() {
        if (_playerTrigger.triggered) { // Colliding with player
            // TODO: Play error sound
            Debug.Log("Can't drop!");
            return false;
        }
        this._objectGrabPointTransform = null;
        _rb.useGravity = true;
        gameObject.layer = LayerMask.NameToLayer("PickUp");
        return true;
    }

    /// <summary>
    /// This function is called every fixed framerate frame, if the MonoBehaviour is enabled.
    /// </summary>
    void FixedUpdate() {
        if (_objectGrabPointTransform != null) {
            float translateSpeed = 30f;
            float rotateSpeed = 30f;

            // Move towards grab point
            Vector3 direction = _objectGrabPointTransform.position - _playerTransform.position;
            Ray ray = new Ray(_playerTransform.position, direction);
            RaycastHit hit;
            Vector3 targetPosition;
            if (!Physics.Raycast(ray, out hit, direction.magnitude + minRadius, grabPointLayers, QueryTriggerInteraction.Ignore)) { // TODO: make collision layer for this?
                targetPosition = _objectGrabPointTransform.position;
            } else {
                Vector3 targetDirection = hit.point - _playerTransform.position;
                targetPosition = _playerTransform.position + targetDirection - (targetDirection.normalized * minRadius);
            }
            Vector3 newPosition = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * translateSpeed);
            _rb.MovePosition(newPosition);

            // Rotate towards player
            Quaternion newRotation = Quaternion.Lerp(transform.rotation, _objectGrabPointTransform.rotation, Time.deltaTime * rotateSpeed);
            _rb.MoveRotation(newRotation);
        }
    }
}
