using System.Collections.Generic;
using FieldOfViewAsset;
using UnityEngine;

public class GameStateManager : MonoBehaviour {

    public GameObject Player;
    public GameObject Target;
    
    public List<GameObject> fovOwners = new List<GameObject>();

    private void Update() {
        if (Vector3.Distance(this.Player.transform.position, this.Target.transform.position) < 0.25) {
            Time.timeScale = 0;
            Debug.Log("You win!");
            enabled = false;
        }
    }

    private void OnEnable() {
        foreach (var fovOwner in this.fovOwners) {
            FieldOfView fieldOfView = fovOwner.GetComponentInChildren<FieldOfView>();
            fieldOfView.TargetDetected += TargetDetected;
        }
    }

    private void TargetDetected(GameObject target) {
        Time.timeScale = 0;
        Debug.Log("Game Over!");
    }
}