using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarterAssets;

public class MovingPlatform : TriggerForwardTarget
{
    [SerializeField] private Transform platform;
    [SerializeField] private Transform targetPoint;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float stayTime;
    [SerializeField] private bool movingTowardTarget = true;

    private float timer = 0f;

    /// <summary>
    /// This function is called every fixed framerate frame, if the MonoBehaviour is enabled.
    /// </summary>
    void FixedUpdate()
    {
        if (movingTowardTarget) {
            platform.position = Vector3.MoveTowards(platform.position, targetPoint.position, moveSpeed * Time.fixedDeltaTime);
            if (platform.position == targetPoint.position) {
                timer += Time.deltaTime;
                if (timer >= stayTime) {
                    movingTowardTarget = false;
                    timer = 0f;
                }
            }
        } else {
            platform.localPosition = Vector3.MoveTowards(platform.localPosition, new Vector3(0, 0, 0), moveSpeed * Time.fixedDeltaTime);
            if (platform.localPosition == new Vector3(0, 0, 0)) {
                timer += Time.deltaTime;
                if (timer >= stayTime) {
                    movingTowardTarget = true;
                    timer = 0f;
                }
            }
        }
    }

    public override void TriggerForwardEnter(Collider other) {
        other.transform.SetParent(platform.transform);
    }

    public override void TriggerForwardExit(Collider other) {
        other.transform.SetParent(null);
    }
    // private FirstPersonController _ridingPlayer = null;
    // private Vector3 lastPosition;

    // /// <summary>
    // /// Start is called on the frame when a script is enabled just before
    // /// any of the Update methods is called the first time.
    // /// </summary>
    // void Start()
    // {
    //     lastPosition = transform.position;
    // }

    // /// <summary>
    // /// This function is called every fixed framerate frame, if the MonoBehaviour is enabled.
    // /// </summary>
    // void FixedUpdate()
    // {
    //     if (_ridingPlayer != null) {
    //         Vector3 deltaPos = transform.position - lastPosition;
    //         Vector3 playerPos = _ridingPlayer.transform.position;
    //         playerPos.x += deltaPos.x;
    //         playerPos.z += deltaPos.z;
    //         _ridingPlayer.MoveToPosition(playerPos);
    //     }
    //     lastPosition = transform.position;
    // }

    // /// <summary>
    // /// OnCollisionEnter is called when this collider/rigidbody has begun
    // /// touching another rigidbody/collider.
    // /// </summary>
    // /// <param name="other">The Collision data associated with this collision.</param>
    // void OnTriggerEnter(Collider other)
    // {
    //     FirstPersonController player = other.gameObject.GetComponentInParent<FirstPersonController>();
    //     Debug.Log("contact");
    //     if (player != null) {
    //         Debug.Log("player contact");
    //         foreach (ContactPoint contact in other.contacts) {
    //             Debug.Log(Vector3.Angle(contact.normal, Vector3.down));
    //             if (Vector3.Angle(contact.normal, Vector3.down) < 45) { // Coliding with top of platform
    //                 _ridingPlayer = player;
    //                 Debug.Log("riding");
    //             }
    //         }
    //     }
    // }

    // /// <summary>
    // /// OnCollisionStay is called once per frame for every collider/rigidbody
    // /// that is touching rigidbody/collider.
    // /// </summary>
    // /// <param name="other">The Collision data associated with this collision.</param>
    // void OnTriggerStay(Collider other)
    // {
    //     FirstPersonController player = other.gameObject.GetComponentInParent<FirstPersonController>();
    //     if (player != null) {
    //         foreach (ContactPoint contact in other.contacts) {
    //             Debug.Log(Vector3.Angle(contact.normal, Vector3.down));
    //             if (Vector3.Angle(contact.normal, Vector3.down) < 45) { // Coliding with top of platform
    //                 _ridingPlayer = player;
    //                 Debug.Log("riding");
    //                 return;
    //             }
    //         }
    //         _ridingPlayer = null;
    //         Debug.Log("not riding");
    //     }
    // }

    // /// <summary>
    // /// OnCollisionExit is called when this collider/rigidbody has
    // /// stopped touching another rigidbody/collider.
    // /// </summary>
    // /// <param name="other">The Collision data associated with this collision.</param>
    // void OnTriggerExit(Collider other)
    // {
    //     FirstPersonController player = other.gameObject.GetComponentInParent<FirstPersonController>();
    //     if (player != null) {
    //         _ridingPlayer = null;
    //         Debug.Log("not riding");
    //     }
    // }
}
