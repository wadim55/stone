using UnityEngine;

public class PathVisualizer : MonoBehaviour {

    public Color PathColor = Color.red;

    private void OnDrawGizmos() {
        Gizmos.color = PathColor;

        Transform previousWaypoint = null;

        foreach (Transform waypoint in transform) {
            Gizmos.DrawSphere(waypoint.position, 0.1f);
            Transform currentWaypoint = waypoint;

            if (previousWaypoint != null) {
                Gizmos.DrawLine(previousWaypoint.position, currentWaypoint.position);
            }

            previousWaypoint = waypoint;
        }

        Gizmos.DrawLine(previousWaypoint.position, transform.GetChild(0).position);
    }

}