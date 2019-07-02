using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshOperator : MonoBehaviour {
    public const string pointTag = "graph";
    public GameObject target;
    public GameObject point;
    public GameObject navPoint;
    public GameObject line;
    public int pointsAmount = 10; //must be non-negative
    public float duplicateRange = 0.01f;
    public GameObject startPoint;
    public GameObject endPoint;

    public List<GameObject> points;
    public List<GameObject> navPoints;
    private Mesh mesh;
    private MeshRenderer meshRenderer;
    private bool attached = false;
    private bool spawned = false;
    private bool pathsSet = false;
    private bool reached = true;

    void Update() {
        if (!attached && target) { attached = true; }
        if (attached) {
            if (!target) { attached = false; spawned = false; }
            else
            {
                if (!mesh) mesh = target.GetComponent<MeshFilter>().mesh;
                if (!meshRenderer) meshRenderer = target.GetComponent<MeshRenderer>();
            }
            if (!spawned) {
                refreshPoints();
            }
            else if (!pathsSet) {
                
                if (Input.GetKey(KeyCode.Mouse0))
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit, 100))
                        if (hit.collider.gameObject.tag == pointTag) startPoint = hit.collider.gameObject;
                }
                if (Input.GetKey(KeyCode.Mouse1))
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit, 100))
                        if (hit.collider.gameObject.tag == pointTag) endPoint = hit.collider.gameObject;
                }
                
                /*
                for (int i = 0; i < points.Count-1; ++i)
                    for (int j = i+1; j < points.Count; ++j)
                    {
                        startPoint = points[i];
                        endPoint = points[j];
                        startSetPath();
                        points[i].GetComponent<PointPaths>().addConnection(j, true, points[i].GetComponent<PointPrefs>().getNext());
                        points[j].GetComponent<PointPaths>().addConnection(i, false, points[j].GetComponent<PointPrefs>().getPrev());
                    }
                deleteDuplicatedNavPoints();
                pathsSet = true;
                */
            }
        }
    }

    public void clearAllPoints() {
        foreach (GameObject elem in points) Destroy(elem);
        points.Clear();
        foreach (GameObject elem in navPoints) Destroy(elem);
        navPoints.Clear();
        startPoint = null;
        endPoint = null;
        reached = true;
    }

    public void generatePoints() {
        clearAllPoints();
        float minX = mesh.vertices[0].x, minY = mesh.vertices[0].y, minZ = mesh.vertices[0].z,
              maxX = mesh.vertices[0].x, maxY = mesh.vertices[0].y, maxZ = mesh.vertices[0].z;
        for (int i = 1; i < mesh.vertexCount; ++i) {
            if (mesh.vertices[i].x < minX) minX = mesh.vertices[i].x;
            if (mesh.vertices[i].y < minY) minY = mesh.vertices[i].y;
            if (mesh.vertices[i].z < minZ) minZ = mesh.vertices[i].z;
            if (mesh.vertices[i].x > maxX) maxX = mesh.vertices[i].x;
            if (mesh.vertices[i].y > maxY) maxY = mesh.vertices[i].y;
            if (mesh.vertices[i].z > maxZ) maxZ = mesh.vertices[i].z;
        }
        for (int i = 0; i < pointsAmount; ++i) {
            float x = Random.Range(minX, maxX),
                  y = Random.Range(minY, maxY),
                  z = Random.Range(minZ, maxZ);
            GameObject newPoint = Instantiate(point, new Vector3(x, y, z), Quaternion.Euler(0, 0, 0));
            newPoint.name = "Point " + (i + 1).ToString();
            newPoint.transform.position = new Vector3(x, y, z);
            points.Add(newPoint);
        }
    }

    private int getNearestTriangleIndex(Vector3 point) {
        float x, y, z;
        int index = -1;
        float minDistance = -1;
        for (int j = 0; j < mesh.triangles.Length; j += 3)
        {
            x = (mesh.vertices[mesh.triangles[j]].x + mesh.vertices[mesh.triangles[j + 1]].x + mesh.vertices[mesh.triangles[j + 2]].x) / 3;
            y = (mesh.vertices[mesh.triangles[j]].y + mesh.vertices[mesh.triangles[j + 1]].y + mesh.vertices[mesh.triangles[j + 2]].y) / 3;
            z = (mesh.vertices[mesh.triangles[j]].z + mesh.vertices[mesh.triangles[j + 1]].z + mesh.vertices[mesh.triangles[j + 2]].z) / 3;
            float dist = Vector3.SqrMagnitude(point - new Vector3(x, y, z));
            if (minDistance == -1 || dist < minDistance) { minDistance = dist; index = j; }
        }
        return index;
    }

    public void setPoints() {
        for (int i = 0; i < points.Count; ++i) {
            float x, y, z, w;
            int index = getNearestTriangleIndex(points[i].transform.position);
            x = (mesh.vertices[mesh.triangles[index + 1]].y - mesh.vertices[mesh.triangles[index]].y) * (mesh.vertices[mesh.triangles[index + 2]].z - mesh.vertices[mesh.triangles[index]].z) - (mesh.vertices[mesh.triangles[index + 1]].z - mesh.vertices[mesh.triangles[index]].z) * (mesh.vertices[mesh.triangles[index + 2]].y - mesh.vertices[mesh.triangles[index]].y);
            y = (mesh.vertices[mesh.triangles[index + 1]].z - mesh.vertices[mesh.triangles[index]].z) * (mesh.vertices[mesh.triangles[index + 2]].x - mesh.vertices[mesh.triangles[index]].x) - (mesh.vertices[mesh.triangles[index + 1]].x - mesh.vertices[mesh.triangles[index]].x) * (mesh.vertices[mesh.triangles[index + 2]].z - mesh.vertices[mesh.triangles[index]].z);
            z = (mesh.vertices[mesh.triangles[index + 1]].x - mesh.vertices[mesh.triangles[index]].x) * (mesh.vertices[mesh.triangles[index + 2]].y - mesh.vertices[mesh.triangles[index]].y) - (mesh.vertices[mesh.triangles[index + 1]].y - mesh.vertices[mesh.triangles[index]].y) * (mesh.vertices[mesh.triangles[index + 2]].x - mesh.vertices[mesh.triangles[index]].x);
            w = -x * mesh.vertices[mesh.triangles[index]].x - y * mesh.vertices[mesh.triangles[index]].y - z * mesh.vertices[mesh.triangles[index]].z;
            w = -(points[i].transform.position.x * x + points[i].transform.position.y * y + points[i].transform.position.z * z + w) / (x * x + y * y + z * z);
            x = points[i].transform.position.x + x * w;
            y = points[i].transform.position.y + y * w;
            z = points[i].transform.position.z + z * w;
            Vector3 newPos = new Vector3(x, y, z);
            points[i].transform.position = newPos;
            points[i].GetComponent<PointPrefs>().setTriangleIndex(index);
            w = 0;
            x = (Vector3.Magnitude(mesh.vertices[mesh.triangles[index]] - mesh.vertices[mesh.triangles[index + 1]]) + Vector3.Magnitude(mesh.vertices[mesh.triangles[index]] - newPos) + Vector3.Magnitude(newPos - mesh.vertices[mesh.triangles[index + 1]])) / 2;
            w += Mathf.Sqrt(x * (x - Vector3.Magnitude(mesh.vertices[mesh.triangles[index]] - mesh.vertices[mesh.triangles[index + 1]])) * (x - Vector3.Magnitude(mesh.vertices[mesh.triangles[index]] - newPos)) * (x - Vector3.Magnitude(newPos - mesh.vertices[mesh.triangles[index + 1]])));
            x = (Vector3.Magnitude(mesh.vertices[mesh.triangles[index + 2]] - mesh.vertices[mesh.triangles[index + 1]]) + Vector3.Magnitude(mesh.vertices[mesh.triangles[index + 2]] - newPos) + Vector3.Magnitude(newPos - mesh.vertices[mesh.triangles[index + 1]])) / 2;
            w += Mathf.Sqrt(x * (x - Vector3.Magnitude(mesh.vertices[mesh.triangles[index + 2]] - mesh.vertices[mesh.triangles[index + 1]])) * (x - Vector3.Magnitude(mesh.vertices[mesh.triangles[index + 2]] - newPos)) * (x - Vector3.Magnitude(newPos - mesh.vertices[mesh.triangles[index + 1]])));
            x = (Vector3.Magnitude(mesh.vertices[mesh.triangles[index + 2]] - mesh.vertices[mesh.triangles[index]]) + Vector3.Magnitude(mesh.vertices[mesh.triangles[index + 2]] - newPos) + Vector3.Magnitude(newPos - mesh.vertices[mesh.triangles[index]])) / 2;
            w += Mathf.Sqrt(x * (x - Vector3.Magnitude(mesh.vertices[mesh.triangles[index + 2]] - mesh.vertices[mesh.triangles[index]])) * (x - Vector3.Magnitude(mesh.vertices[mesh.triangles[index + 2]] - newPos)) * (x - Vector3.Magnitude(newPos - mesh.vertices[mesh.triangles[index]])));
            x = (Vector3.Magnitude(mesh.vertices[mesh.triangles[index + 2]] - mesh.vertices[mesh.triangles[index]]) + Vector3.Magnitude(mesh.vertices[mesh.triangles[index + 2]] - mesh.vertices[mesh.triangles[index + 1]]) + Vector3.Magnitude(mesh.vertices[mesh.triangles[index + 1]] - mesh.vertices[mesh.triangles[index]])) / 2;
            y = Mathf.Sqrt(x * (x - Vector3.Magnitude(mesh.vertices[mesh.triangles[index + 2]] - mesh.vertices[mesh.triangles[index]])) * (x - Vector3.Magnitude(mesh.vertices[mesh.triangles[index + 2]] - mesh.vertices[mesh.triangles[index + 1]])) * (x - Vector3.Magnitude(mesh.vertices[mesh.triangles[index + 1]] - mesh.vertices[mesh.triangles[index]])));
            if (w - y > 0.0001f) {
                int n = 0;
                for (int j = 1; j < 3; ++j)
                    if (Vector3.Distance(mesh.vertices[mesh.triangles[index + j]], newPos) < Vector3.Distance(mesh.vertices[mesh.triangles[index + n]], newPos)) n = j;
                points[i].transform.position = mesh.vertices[mesh.triangles[index+n]];
            }
        }
    }

    public void refreshPoints() {
        generatePoints();
        setPoints();
        spawned = true;
    }

    public void setPath() {
        if (!startPoint || !endPoint) { print("Set points"); reached = true; return; }
        int indexHome = startPoint.GetComponent<PointPrefs>().getTriangleIndex();
        int indexEnd = endPoint.GetComponent<PointPrefs>().getTriangleIndex();
        int indexCur = indexHome;
        Vector3 pointHome = startPoint.transform.position;
        Vector3 pointEnd = endPoint.transform.position;

        if (indexHome != indexEnd && Vector3.Distance(pointHome, pointEnd) > 0.001f)
        {
            float x, y, z, w;
            int c = 0;
            x = (mesh.vertices[mesh.triangles[indexHome + 1]].y - mesh.vertices[mesh.triangles[indexHome]].y) * (mesh.vertices[mesh.triangles[indexHome + 2]].z - mesh.vertices[mesh.triangles[indexHome]].z) - (mesh.vertices[mesh.triangles[indexHome + 1]].z - mesh.vertices[mesh.triangles[indexHome]].z) * (mesh.vertices[mesh.triangles[indexHome + 2]].y - mesh.vertices[mesh.triangles[indexHome]].y);
            y = (mesh.vertices[mesh.triangles[indexHome + 1]].z - mesh.vertices[mesh.triangles[indexHome]].z) * (mesh.vertices[mesh.triangles[indexHome + 2]].x - mesh.vertices[mesh.triangles[indexHome]].x) - (mesh.vertices[mesh.triangles[indexHome + 1]].x - mesh.vertices[mesh.triangles[indexHome]].x) * (mesh.vertices[mesh.triangles[indexHome + 2]].z - mesh.vertices[mesh.triangles[indexHome]].z);
            z = (mesh.vertices[mesh.triangles[indexHome + 1]].x - mesh.vertices[mesh.triangles[indexHome]].x) * (mesh.vertices[mesh.triangles[indexHome + 2]].y - mesh.vertices[mesh.triangles[indexHome]].y) - (mesh.vertices[mesh.triangles[indexHome + 1]].y - mesh.vertices[mesh.triangles[indexHome]].y) * (mesh.vertices[mesh.triangles[indexHome + 2]].x - mesh.vertices[mesh.triangles[indexHome]].x);
            w = -x * mesh.vertices[mesh.triangles[indexHome]].x - y * mesh.vertices[mesh.triangles[indexHome]].y - z * mesh.vertices[mesh.triangles[indexHome]].z;
            w = -(pointEnd.x * x + pointEnd.y * y + pointEnd.z * z + w) / (x * x + y * y + z * z);
            x = pointEnd.x + x * w;
            y = pointEnd.y + y * w;
            z = pointEnd.z + z * w;
            Vector3 newPos = new Vector3(x, y, z);
            Vector3 cross = newPos;
            Vector3 cross1 = new Vector3(0, 0, 0), cross2 = new Vector3(0, 0, 0);
            for (int i = 0; i < 3; ++i)
            {
                if (Vector3.Distance(pointHome, mesh.vertices[mesh.triangles[indexHome + i]]) + Vector3.Distance(mesh.vertices[mesh.triangles[indexHome + ((i + 1) % 3)]], pointHome) - Vector3.Distance(mesh.vertices[mesh.triangles[indexHome + ((i + 1) % 3)]], mesh.vertices[mesh.triangles[indexHome + i]]) > 0.001f)
                    if (crossed(pointHome, newPos, mesh.vertices[mesh.triangles[indexHome + i]], mesh.vertices[mesh.triangles[indexHome + ((i + 1) % 3)]], ref cross))
                    {
                        cross1 = mesh.vertices[mesh.triangles[indexHome + i]];
                        cross2 = mesh.vertices[mesh.triangles[indexHome + ((i + 1) % 3)]];
                        break;
                    }
            }
            // Special cases
            if (cross == newPos && Vector3.Distance(cross, pointEnd) > 0.001f) // (R) Reindexation
            {
                x = -1;
                c = indexHome;
                for (int j = 0; j < mesh.triangles.Length; j += 3) {
                    for (int i = 0; i < 3; ++i)
                        if (Vector3.Distance(pointHome, mesh.vertices[mesh.triangles[j + i]]) + Vector3.Distance(mesh.vertices[mesh.triangles[j + ((i + 1) % 3)]], pointHome) - Vector3.Distance(mesh.vertices[mesh.triangles[j + ((i + 1) % 3)]], mesh.vertices[mesh.triangles[j + i]]) <= 0.001f)
                        {
                            if (j == indexEnd) {
                                startPoint.GetComponent<PointPrefs>().setNext(endPoint);
                                endPoint.GetComponent<PointPrefs>().setPrev(startPoint);
                                GameObject nLine = Instantiate(line, pointHome, Quaternion.Euler(0, 0, 0));
                                nLine.GetComponent<LineBehavior>().set(this, startPoint, endPoint);
                                deleteDuplicatedNavPoints();
                                reached = true;
                                startPoint = null;
                                endPoint = null;
                                return;
                            }
                            y = Vector3.Distance(mesh.vertices[mesh.triangles[j]], pointEnd) + Vector3.Distance(mesh.vertices[mesh.triangles[j + 1]], pointEnd) + Vector3.Distance(mesh.vertices[mesh.triangles[j + 2]], pointEnd);
                            if ((x == -1 || y < x) && j != c) { indexHome = j; x = y; }
                            break;
                        }
                }
                x = (mesh.vertices[mesh.triangles[indexHome + 1]].y - mesh.vertices[mesh.triangles[indexHome]].y) * (mesh.vertices[mesh.triangles[indexHome + 2]].z - mesh.vertices[mesh.triangles[indexHome]].z) - (mesh.vertices[mesh.triangles[indexHome + 1]].z - mesh.vertices[mesh.triangles[indexHome]].z) * (mesh.vertices[mesh.triangles[indexHome + 2]].y - mesh.vertices[mesh.triangles[indexHome]].y);
                y = (mesh.vertices[mesh.triangles[indexHome + 1]].z - mesh.vertices[mesh.triangles[indexHome]].z) * (mesh.vertices[mesh.triangles[indexHome + 2]].x - mesh.vertices[mesh.triangles[indexHome]].x) - (mesh.vertices[mesh.triangles[indexHome + 1]].x - mesh.vertices[mesh.triangles[indexHome]].x) * (mesh.vertices[mesh.triangles[indexHome + 2]].z - mesh.vertices[mesh.triangles[indexHome]].z);
                z = (mesh.vertices[mesh.triangles[indexHome + 1]].x - mesh.vertices[mesh.triangles[indexHome]].x) * (mesh.vertices[mesh.triangles[indexHome + 2]].y - mesh.vertices[mesh.triangles[indexHome]].y) - (mesh.vertices[mesh.triangles[indexHome + 1]].y - mesh.vertices[mesh.triangles[indexHome]].y) * (mesh.vertices[mesh.triangles[indexHome + 2]].x - mesh.vertices[mesh.triangles[indexHome]].x);
                w = -x * mesh.vertices[mesh.triangles[indexHome]].x - y * mesh.vertices[mesh.triangles[indexHome]].y - z * mesh.vertices[mesh.triangles[indexHome]].z;
                w = -(pointEnd.x * x + pointEnd.y * y + pointEnd.z * z + w) / (x * x + y * y + z * z);
                x = pointEnd.x + x * w;
                y = pointEnd.y + y * w;
                z = pointEnd.z + z * w;
                newPos = new Vector3(x, y, z);
                cross = newPos;
                for (int k = 0; k < 3; ++k)
                {
                    if (Vector3.Distance(pointHome, mesh.vertices[mesh.triangles[indexHome + k]]) + Vector3.Distance(mesh.vertices[mesh.triangles[indexHome + ((k + 1) % 3)]], pointHome) - Vector3.Distance(mesh.vertices[mesh.triangles[indexHome + ((k + 1) % 3)]], mesh.vertices[mesh.triangles[indexHome + k]]) > 0.001f)
                        if (crossed(pointHome, newPos, mesh.vertices[mesh.triangles[indexHome + k]], mesh.vertices[mesh.triangles[indexHome + ((k + 1) % 3)]], ref cross))
                        {
                            cross1 = mesh.vertices[mesh.triangles[indexHome + k]];
                            cross2 = mesh.vertices[mesh.triangles[indexHome + ((k + 1) % 3)]];
                            break;
                        }
                }
                if (cross == newPos && Vector3.Distance(cross, pointEnd) > 0.001f) // (M) Movement through the mesh points
                {
                    c = 0;
                    cross = mesh.vertices[mesh.triangles[indexHome]];
                    for (int i = 1; i < 3; ++i) if (Vector3.Distance(mesh.vertices[mesh.triangles[indexHome + i]], pointEnd) < Vector3.Distance(cross, pointEnd)) { cross = mesh.vertices[mesh.triangles[indexHome + i]]; c = i; }
                    cross1 = cross;
                    if (Vector3.Distance(mesh.vertices[mesh.triangles[indexHome + (c + 1) % 3]], pointEnd) < Vector3.Distance(mesh.vertices[mesh.triangles[indexHome + (c + 2) % 3]], pointEnd))
                        cross2 = mesh.vertices[mesh.triangles[indexHome + (c + 1) % 3]];
                    else cross2 = mesh.vertices[mesh.triangles[indexHome + (c + 2) % 3]];
                } // (M)
            } // (R)

            // Triangle search
            for (int j = 0; j < mesh.triangles.Length; j += 3) {
                c = 0;
                for (int i = 0; i < 3; ++i)
                    if (mesh.vertices[mesh.triangles[j + i]] == cross1 || mesh.vertices[mesh.triangles[j + i]] == cross2) c++;
                if (indexHome != j && c == 2)
                {
                    indexCur = j;
                    break;
                }
            }
            GameObject next = Instantiate(navPoint, cross, Quaternion.Euler(0, 0, 0));
            next.name = indexCur.ToString();
            next.GetComponent<PointPrefs>().setTriangleIndex(indexCur);
            startPoint.GetComponent<PointPrefs>().setNext(next);
            next.GetComponent<PointPrefs>().setPrev(startPoint);
            navPoints.Add(next);
            startPoint = next;
            GameObject newLine = Instantiate(line, pointHome, Quaternion.Euler(0, 0, 0));
            newLine.GetComponent<LineBehavior>().set(this, startPoint, next);
            if (!reached) setPath();
        }
        else
        {
            startPoint.GetComponent<PointPrefs>().setNext(endPoint);
            endPoint.GetComponent<PointPrefs>().setPrev(startPoint);
            GameObject newLine = Instantiate(line, pointHome, Quaternion.Euler(0, 0, 0));
            newLine.GetComponent<LineBehavior>().set(this, startPoint, endPoint);
            deleteDuplicatedNavPoints();
            reached = true;
            startPoint = null;
            endPoint = null;
        }
    }

    private bool crossed(Vector3 m1, Vector3 m2, Vector3 m3, Vector3 m4, ref Vector3 cross) {
        float x = 0, y = 0, z = 0;
        //float w,v,u;
        Vector3 crossPoint;
        //w = (((m2.y - m1.y) * (m4.x - m3.x)) - ((m4.y - m3.y) * (m2.x - m1.x)));//xy
        //v = (((m2.z - m1.z) * (m4.y - m3.y)) - ((m4.z - m3.z) * (m2.y - m1.y)));//yz
        //u = (((m2.x - m1.x) * (m4.z - m3.z)) - ((m4.x - m3.x) * (m2.z - m1.z)));//zx
        x = (((m1.x - m2.x) * ((m4.y * m3.x) - (m3.y * m4.x))) + ((m3.x - m4.x) * ((m1.y * m2.x) - (m2.y * m1.x)))) / (((m2.y - m1.y) * (m4.x - m3.x)) - ((m4.y - m3.y) * (m2.x - m1.x)));//xy
        y = (((m1.y - m2.y) * ((m4.z * m3.y) - (m3.z * m4.y))) + ((m3.y - m4.y) * ((m1.z * m2.y) - (m2.z * m1.y)))) / (((m2.z - m1.z) * (m4.y - m3.y)) - ((m4.z - m3.z) * (m2.y - m1.y)));//yz
        z = (((m1.z - m2.z) * ((m4.x * m3.z) - (m3.x * m4.z))) + ((m3.z - m4.z) * ((m1.x * m2.z) - (m2.x * m1.z)))) / (((m2.x - m1.x) * (m4.z - m3.z)) - ((m4.x - m3.x) * (m2.z - m1.z)));//zx
        crossPoint = new Vector3(x, y, z);
        // Usable to check deviations
        //print(crossPoint+" <"+ (Vector3.Distance(crossPoint , m3) + Vector3.Distance(m4 , crossPoint) - Vector3.Distance(m4 , m3)).ToString()+"> <"+(Vector3.Distance(crossPoint , m1) + Vector3.Distance(m2 , crossPoint) - Vector3.Distance(m2 , m1)).ToString()+"> "+m1+m2+m3+m4+" "+w+" "+v+" "+u);
        if (Vector3.Distance(crossPoint, m3) + Vector3.Distance(m4, crossPoint) - Vector3.Distance(m4, m3) < 0.001f && Vector3.Distance(crossPoint, m1) + Vector3.Distance(m2, crossPoint) - Vector3.Distance(m2, m1) < 0.001f)
        {
            cross = crossPoint;
            return true;
        }
        else
        {
            return false;
        }
    }

    private void deleteDuplicatedNavPoints() {
        for (int i = 0; i < navPoints.Count;) {
            bool col = false;
            if (Vector3.Distance(navPoints[i].transform.position, navPoints[i].GetComponent<PointPrefs>().getNext().transform.position) <= duplicateRange) {
                col = true;
                GameObject target = navPoints[i];
                navPoints.Remove(target);
                target.GetComponent<PointPrefs>().dissolve();
                Destroy(target);
            }
            if (!col) i++;
        }
    }

    public void startSetPath() { reached = false; setPath(); }
    public bool getReached() { return reached; }
}