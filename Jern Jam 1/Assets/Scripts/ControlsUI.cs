using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ControlsUI : MonoBehaviour
{
    public static ControlsUI singleton;

    [SerializeField] private GameObject leftClick;
    [SerializeField] private GameObject rightClick;
    [SerializeField] private GameObject pickUp;
    [SerializeField] private GameObject drop;
    [SerializeField] private GameObject push;
    [SerializeField] private GameObject take;
    [SerializeField] private GameObject timeControlSelf;
    [SerializeField] private GameObject timeControlObject;
    [SerializeField] private GameObject pause;
    [SerializeField] private GameObject release;
    [SerializeField] private GameObject timeControlControls;

    private bool _grabbing = false;

    void Awake()
    {
        if (singleton != null) {
            Debug.Log("more than 1 controls UI!");
            Destroy(this);
        }
        else singleton = this;
    }

    public void ShowPickUp() {
        pickUp.SetActive(true);
        drop.SetActive(false);
        push.SetActive(false);
        take.SetActive(false);
        // rightClick.SetActive(true);
        // timeControlSelf.SetActive(true);
        _grabbing = false;
    }

    public void ShowGrabbingObject() {
        pickUp.SetActive(false);
        drop.SetActive(true);
        push.SetActive(false);
        take.SetActive(false);
        rightClick.SetActive(false);
        timeControlSelf.SetActive(false);
        _grabbing = true;
    }

    public void ShowPush() {
        pickUp.SetActive(false);
        drop.SetActive(false);
        push.SetActive(true);
        take.SetActive(false);
        // rightClick.SetActive(true);
        // timeControlSelf.SetActive(true);
        _grabbing = false;
    }

    public void ShowTake() {
        pickUp.SetActive(false);
        drop.SetActive(false);
        push.SetActive(false);
        take.SetActive(true);
        _grabbing = false;
    }

    public void HideE() {
        pickUp.SetActive(false);
        drop.SetActive(false);
        push.SetActive(false);
        take.SetActive(false);
        _grabbing = false;
    }

    public void ShowIdle() {
        leftClick.SetActive(false);
        timeControlObject.SetActive(false);
        rightClick.SetActive(true);
        timeControlSelf.SetActive(true);
        pause.SetActive(false);
        release.SetActive(false);
        timeControlControls.SetActive(false);
    }

    public void ShowCanTimeControlObject() {
        leftClick.SetActive(true);
        timeControlObject.SetActive(true);
        if (!_grabbing) {
            rightClick.SetActive(true);
            timeControlSelf.SetActive(true);
        }
        pause.SetActive(false);
        release.SetActive(false);
        timeControlControls.SetActive(false);
    }

    public void ShowTimeControlling() {
        leftClick.SetActive(true);
        timeControlObject.SetActive(false);
        rightClick.SetActive(true);
        timeControlSelf.SetActive(false);
        pause.SetActive(true);
        release.SetActive(true);
        timeControlControls.SetActive(true);
    }
}
