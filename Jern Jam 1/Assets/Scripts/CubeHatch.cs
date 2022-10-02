using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeHatch : MonoBehaviour
{
    [SerializeField] private GameObject cubePrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private DoorController hatchDoor;
    [SerializeField] private float openTime;

    private GameObject currentCube;
    private bool spawning = false;
    private float timer = 0f;

    /// <summary>
    /// This function is called every fixed framerate frame, if the MonoBehaviour is enabled.
    /// </summary>
    void FixedUpdate()
    {
        if (spawning) {
            timer += Time.deltaTime;
            if (timer >= openTime) {
                hatchDoor.Deactivate();
                spawning = false;
            }
        }
    }

    public void Activate() {
        if (!spawning) {
            spawning = true;
            // TODO: destroying audio
            // TODO: destroying particle effect
            Destroy(currentCube);
            currentCube = Instantiate(cubePrefab, spawnPoint.position, spawnPoint.rotation);
            hatchDoor.Activate();
            timer = 0f;
        }
    }
}
