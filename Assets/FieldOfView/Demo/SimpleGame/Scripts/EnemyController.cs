using System.Collections.Generic;
using System.Linq;
using FieldOfViewAsset;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour {

    public delegate void OnTargetCaught(GameObject target);
    public static event OnTargetCaught TargetCaught;
    
    public Transform Path;

    private FieldOfView _fieldOfView;

    private NavMeshAgent _navMeshAgent;
    private int _nextWaypointIndex;

    private bool _continueSearch = true;

    private float _initialRadius;
    private int _initialAngle;

    private void Awake() {
        this._fieldOfView = GetComponentInChildren<FieldOfView>();

        this._initialRadius = this._fieldOfView.ViewRadius;
        this._initialAngle = this._fieldOfView.ViewAngle;
    }

    private void OnEnable() {
        this._fieldOfView.TargetSpotted += TargetSpotted;
        this._fieldOfView.TargetDetected += TargetDetected;
        this._fieldOfView.TargetLost += TargetLost;
    }

    private void OnDisable() {
        this._fieldOfView.TargetSpotted -= TargetSpotted;
        this._fieldOfView.TargetDetected -= TargetDetected;
        this._fieldOfView.TargetLost -= TargetLost;
    }

    private void Start() {
        this._navMeshAgent = GetComponent<NavMeshAgent>();
        this._navMeshAgent.destination = this.Path.GetChild(this._nextWaypointIndex).transform.position;
    }

    private void Update() {
        if (!this._continueSearch) {
            return;
        }
        
        List<GameObject> visibleTargets = this._fieldOfView.GetAllVisibleTargets();
        if (visibleTargets.Count == 0) {
            this._fieldOfView.ViewRadius = this._initialRadius; // return to previous radius in calm mode
            this._fieldOfView.ViewAngle = this._initialAngle; // return to previous width in calm mode
            
            this._navMeshAgent.destination = this.Path.GetChild(this._nextWaypointIndex).transform.position;
        }
        else {
            this._fieldOfView.ViewRadius = this._initialRadius + 1; // make enemy see further in alarm mode
            this._fieldOfView.ViewAngle = this._initialAngle + 20; // make enemy see wider in alarm mode
            
            GameObject closestTarget = visibleTargets
                    .OrderBy(target => Vector3.Distance(target.transform.position, transform.position)).First();

            if (Vector3.Distance(closestTarget.transform.position, transform.position) < 1.5) {
                if (TargetCaught != null) {
                    TargetCaught(closestTarget);
                }
                
                this._continueSearch = false;
            }
            
            this._navMeshAgent.destination = visibleTargets.First().transform.position;
        }
        
        if (visibleTargets.Count == 0 && Mathf.Approximately(this._navMeshAgent.remainingDistance, 0) && !this._navMeshAgent.pathPending) {
            this._nextWaypointIndex++;

            if (this._nextWaypointIndex == this.Path.childCount) {
                this._nextWaypointIndex = 0;
            }
        }
        
        // mouse click
        if (Input.GetMouseButtonUp(0)) {
            CheckToggle(Input.mousePosition);
        }
        // tap on touch screen
        else if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Ended) {
            CheckToggle(Input.GetTouch(0).position);
        }
    }

    private void CheckToggle(Vector3 position) {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(position);
        if (Physics.Raycast(ray, out hit)) {
            if (hit.collider == gameObject.GetComponent<BoxCollider>()) {
                if (this._fieldOfView.IsVisibilityRuleEnabled(FieldOfView.VisibilityRules.AlwaysVisible)) {
                    this._fieldOfView.DisableVisibilityRule(FieldOfView.VisibilityRules.AlwaysVisible);
                }
                else {
                    this._fieldOfView.EnableVisibilityRule(FieldOfView.VisibilityRules.AlwaysVisible);
                }
            }
        }
    }

    private void TargetSpotted(GameObject target) {
        Debug.Log("I guess I spot " + target.name);
    }

    private void TargetDetected(GameObject target) {
        Debug.Log("Now I can see " + target.name);
    }

    private void TargetLost(GameObject target) {
        Debug.Log("I cannot see " + target.name + " anymore");
    }
}