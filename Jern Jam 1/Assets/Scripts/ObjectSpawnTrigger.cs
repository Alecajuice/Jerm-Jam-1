using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarterAssets;

public class ObjectSpawnTrigger : MonoBehaviour
{
    [SerializeField] private GameObject targetObject;
    [SerializeField] private Vector3 initialVelocity;

    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    void Start()
    {
        targetObject.SetActive(false);
    }

    /// <summary>
    /// OnTriggerEnter is called when the Collider other enters the trigger.
    /// </summary>
    /// <param name="other">The other Collider involved in this collision.</param>
    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<FirstPersonController>() != null) {
            targetObject.SetActive(true);
            Rigidbody rb = targetObject.GetComponent<Rigidbody>();
            if (rb != null) {
                rb.velocity = initialVelocity;
            }
        }
    }
}
