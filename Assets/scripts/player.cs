using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class player : MonoBehaviour
{

    public Transform Stone;
    private NavMeshAgent _agent;
    private Animator _animator;
    void Start()
    {
        _animator = GetComponent<Animator>();
        _agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        _animator.SetFloat("Speed",_agent.velocity.magnitude);
        _agent.SetDestination(Stone.position);
        
    }
}
