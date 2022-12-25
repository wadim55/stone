using UnityEngine;
using UnityEngine.AI;

public class Player : MonoBehaviour {

    public GameObject Target;
    public GameObject NavigationSurface;
    
    private NavMeshAgent _navMeshAgent;
    private GameObject _coin;
    private bool _hasCoin;
    
    private void Start() {
        this._navMeshAgent = GetComponent<NavMeshAgent>();
        this._coin = transform.Find("Coin").gameObject;
        this._hasCoin = true;
    }

    private void Update() {
        // left click => move
        if (Input.GetMouseButtonUp(0)) {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit)) {
                if (hit.collider.gameObject.transform.parent.gameObject == this.NavigationSurface) {
                    this._navMeshAgent.SetDestination(hit.point);
                }
            }
        }
        
        // right click => throw a coin
        if (Input.GetMouseButtonUp(1) && this._hasCoin) {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit)) {
                Vector3 direction = hit.point - transform.position;
                direction = new Vector3(direction.x, 0, direction.z);
                
                this._coin.transform.parent = this.NavigationSurface.transform;
                this._coin.transform.Translate(direction.normalized * 0.2f, Space.World);
                
                this._coin.SetActive(true);
                this._coin.GetComponent<Rigidbody>().velocity = direction * 3.5f;
                this._coin.GetComponent<Rigidbody>().useGravity = true;
                
                this._hasCoin = false;
            }
        }
    }

    // pick a coin
    private void OnCollisionEnter(Collision other) {
        if (other.transform.name == "Coin") {
            other.gameObject.GetComponent<Rigidbody>().useGravity = false;
            
            other.gameObject.transform.parent = transform;
            other.gameObject.transform.localPosition = Vector3.zero;
            other.gameObject.transform.localEulerAngles = Vector3.zero;
            
            other.gameObject.SetActive(false);
            this._hasCoin = true;
        }
    }
}
