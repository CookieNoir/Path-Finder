using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct connection {
    public int pointNumber;
    public bool direction; // TRUE if it's the start, FALSE if it's the end
    public GameObject navPoint;
    public float pathLength; // -1 if the length isn't calculated
}

public class PointPaths : MonoBehaviour {
    public List<connection> connections;

    public void addConnection(int pointNum, bool dir, GameObject target) {
        connection newConnection = new connection();
        newConnection.pointNumber = pointNum;
        newConnection.direction = dir;
        newConnection.navPoint = target;
        newConnection.pathLength = -1;
        connections.Add(newConnection);
    }
}
