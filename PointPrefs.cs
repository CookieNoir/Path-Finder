using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointPrefs : MonoBehaviour {
    private GameObject next;
    private GameObject prev;
    private GameObject lineToNext;
    private float valueToNext;
    private float valueToPrev;
    public int triangleIndex;

    private float countValue(Vector3 point) {
        return 0;
    }

    public void setPrev(GameObject target) {
        prev = target;
        valueToPrev = countValue(prev.transform.position);
    }

    public void setNext(GameObject target) {
        next = target;
        valueToNext = countValue(next.transform.position);
    }

    public GameObject getNext() {
        return next;
    }

    public GameObject getPrev() {
        return prev;
    }

    public void setTriangleIndex(int index) {
        triangleIndex = index;
    }
    public int getTriangleIndex() {
        return triangleIndex;
    }

    public void dissolve()
    {
        if (prev) prev.GetComponent<PointPrefs>().setNext(next);
        if (next) next.GetComponent<PointPrefs>().setPrev(prev);
    }

    public void setLineToNext(GameObject line) {
        lineToNext = line;
    }
    public GameObject getLineToNext() {
        return lineToNext;
    }
}
