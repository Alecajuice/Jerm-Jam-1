using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerForward : MonoBehaviour
{
    [SerializeReference] private TriggerForwardTarget target;

    /// <summary>
    /// OnTriggerEnter is called when the Collider other enters the trigger.
    /// </summary>
    /// <param name="other">The other Collider involved in this collision.</param>
    void OnTriggerEnter(Collider other)
    {
        target.TriggerForwardEnter(other);
    }

    /// <summary>
    /// OnTriggerExit is called when the Collider other has stopped touching the trigger.
    /// </summary>
    /// <param name="other">The other Collider involved in this collision.</param>
    void OnTriggerExit(Collider other)
    {
        target.TriggerForwardExit(other);
    }
}

public abstract class TriggerForwardTarget : MonoBehaviour
{
    public abstract void TriggerForwardEnter(Collider other);
    public abstract void TriggerForwardExit(Collider other);
}