using UnityEngine;

public class Coin : MonoBehaviour {

    public bool CanBePicked { get; private set; }

    private void OnCollisionEnter(Collision other) {
        if (other.transform.name == "Player") {
            CanBePicked = false;
        }

        if (other.transform.parent.name == "Floor") {
            CanBePicked = true;
        }
    }
}