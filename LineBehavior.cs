using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineBehavior : MonoBehaviour {
    private MeshOperator mops;
    private GameObject start;
    private GameObject destination;
    private float length = 0;
    private bool prepared = false;

    void Update() {
        if (!start) Destroy(gameObject);
        if (prepared) { transform.localScale = new Vector3(1, 1, length); prepared = false; }
        if (!destination) Destroy(gameObject);
    }

    public void set(MeshOperator launcher, GameObject st, GameObject dest) {
        mops = launcher;
        start = st;
        destination = dest;
        transform.LookAt(dest.transform.position);
        length = Vector3.Magnitude(dest.transform.position - transform.position);
        prepared = true;
        if (length <= 0.001f) Destroy(gameObject);
    }

    public void updateDestination(GameObject newDestination) {
        set(mops, start, newDestination);
    }
}
