using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FieldOfViewAsset;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour {

    public List<GameObject> Cameras = new List<GameObject>();
    
    public GameObject Path;

    private int _currentWaypoint;
    private NavMeshAgent _navMeshAgent;

    private FieldOfView _fieldOfView;

    private bool _isDisctractionMode;
    
    private void Start() {
        this._navMeshAgent = GetComponent<NavMeshAgent>();
        this._navMeshAgent.SetDestination(this.Path.transform.GetChild(this._currentWaypoint).position);

        this._fieldOfView = GetComponentInChildren<FieldOfView>();
    }

    private void Update() {
        if (Mathf.Approximately(this._navMeshAgent.remainingDistance, 0)) {
            this._currentWaypoint++;

            if (this._currentWaypoint == this.Path.transform.childCount) {
                this._currentWaypoint = 0;
            }
            
            this._navMeshAgent.SetDestination(this.Path.transform.GetChild(this._currentWaypoint).position);
        }

        if (this._fieldOfView.GetAllVisibleTargets().Count > 0) {
            OnTargetSpot(this._fieldOfView.GetAllVisibleTargets().First());
        }
        
        // check if 'distraction' happends
        int coinLayer = LayerMask.GetMask("DistractionObjects");
        Collider[] hits = Physics.OverlapSphere(transform.position, this._fieldOfView.ViewRadius, coinLayer);

        if (hits.Length > 0) {
            if (!this._isDisctractionMode) {
                foreach (var hit in hits) {
                    Vector3 hitPosition = hit.transform.position;
                    float angleToCoin = Vector3.Angle(transform.forward, hitPosition - transform.position);
                    if (angleToCoin <= 90) {
                        this._isDisctractionMode = true;
                        StartCoroutine(LookAtCoin(hitPosition, 3));
                    }
                    break;
                }
            }
        }
        else {
            this._isDisctractionMode = false;
        }
    }

    private IEnumerator LookAtCoin(Vector3 coinPosition, float duration) {
        this._fieldOfView.LookAt(coinPosition, duration);
        this._fieldOfView.PauseRotation();
        this._navMeshAgent.speed = 0;

        yield return new WaitForSeconds(duration);

        this._fieldOfView.ResumeRotation();
        this._navMeshAgent.speed = 0.5f;
    }
    
    private void OnEnable() {
        foreach (var fovOwner in this.Cameras) {
            FieldOfView fieldOfView = fovOwner.GetComponentInChildren<FieldOfView>();
            fieldOfView.TargetSpotted += OnTargetSpot;
        }
    }

    private void OnTargetSpot(GameObject target) {
        this._navMeshAgent.SetDestination(target.transform.position);
    }
}
