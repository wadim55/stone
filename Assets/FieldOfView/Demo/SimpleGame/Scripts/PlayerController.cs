using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour {

    public delegate void OnPlayerExit(GameObject target);
    public static event OnPlayerExit PlayerExit;
    
    public GameObject NavigationSurface;
    public GameObject Target;
    
    private NavMeshAgent _navMeshAgent;
    private bool _gameFinished;

    private void Start() {
        this._navMeshAgent = GetComponent<NavMeshAgent>();
    }

    void Update() {
        if (this._gameFinished) {
            return;
        }
        
        if (Vector3.Distance(this.Target.transform.position, transform.position) < 2) {
            if (PlayerExit != null) {
                PlayerExit(gameObject);
                this._gameFinished = true;
            }
        }
        
        // mouse click
        if (Input.GetMouseButtonUp(0)) {
            MovePlayerTo(Input.mousePosition);
        }
        // tap on touch screen
        else if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Ended) {
            MovePlayerTo(Input.GetTouch(0).position);
        }
    }

    private void MovePlayerTo(Vector3 position) {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(position);
        if (Physics.Raycast(ray, out hit)) {
            if (hit.collider.gameObject == this.NavigationSurface) {
                this._navMeshAgent.SetDestination(hit.point);
            }
        }
    }
}