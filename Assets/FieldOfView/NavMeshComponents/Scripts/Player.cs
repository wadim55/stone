using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class Player : MonoBehaviour
{

    public Camera cam;
    private NavMeshAgent _agent;
   


    private void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
    }
    
    private void Update()
    {
        if (Input.GetMouseButton(0))   // целеуказание для игрока куда идти
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                _agent.SetDestination(hit.point);
            }
        }

      
    } 
    
}
