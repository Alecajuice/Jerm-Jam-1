using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeControlUI : MonoBehaviour
{
    [SerializeField] private Image crosshairImage;
    [SerializeField] private Sprite normalCrosshair;
    [SerializeField] private Sprite timeControllableCrosshair;
    [SerializeField] private Sprite pausedCrosshair;
    [SerializeField] private Sprite timeControllingCrosshair;

    [SerializeField] private Image objectTimeline;
    [SerializeField] private Image objectTimelineBar;
    [SerializeField] private Image playerTimeline;
    [SerializeField] private Image playerTimelineBar;
    [SerializeField] private Image playerTimelinePause;

    private CrosshairType crosshairType;

    public void SetCrosshair(CrosshairType type) {
        crosshairType = type;
        switch (crosshairType) {
        case CrosshairType.Normal:
            crosshairImage.sprite = normalCrosshair;
            break;
        case CrosshairType.TimeControllable:
            crosshairImage.sprite = timeControllableCrosshair;
            break;
        case CrosshairType.Paused:
            crosshairImage.sprite = pausedCrosshair;
            break;
        case CrosshairType.TimeControlling:
            crosshairImage.sprite = timeControllingCrosshair;
            break;
        }
    }

    public void EnableObjectTimeline() {
        // objectTimeline.gameObject.SetActive(true);
    }

    public void DisableObjectTimeline() {
        objectTimeline.gameObject.SetActive(false);
    }

    public void SetObjectTimeline(float scale) { // scale is 0 to 1
        objectTimelineBar.rectTransform.localScale = new Vector3(scale, 1, 1);
    }

    public void SetPlayerTimeline(float scale) { // scale is 0 to 1
        playerTimelineBar.rectTransform.localScale = new Vector3(scale, 1, 1);
    }

    public void EnablePlayerTimeline() {
        playerTimeline.gameObject.SetActive(true);
    }

    public void EnablePlayerTimelinePause() {
        playerTimelinePause.gameObject.SetActive(true);
    }

    public void DisablePlayerTimelinePause() {
        playerTimelinePause.gameObject.SetActive(false);
    }

    public enum CrosshairType {
        Normal,
        TimeControllable,
        Paused,
        TimeControlling
    }
}
