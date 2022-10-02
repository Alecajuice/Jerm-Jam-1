using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeControlWatch : MonoBehaviour
{
    [SerializeField] private CutsceneController cutscene;

    private bool _collected = false;

    public void Collect(GameObject player) {
        cutscene.StartCutscene(player);
        _collected = true;
    }

    public bool IsCollected() {
        return _collected;
    }
}
