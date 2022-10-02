using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManualButtonController : MonoBehaviour
{
    [SerializeField] private CubeHatch[] targets;

    private Animator _animator;

    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    void Start()
    {
        _animator = GetComponent<Animator>();
    }

    public void Press() {
        _animator.SetTrigger("Press");
        foreach (CubeHatch target in targets) {
            target.Activate();
        }
    }
}
