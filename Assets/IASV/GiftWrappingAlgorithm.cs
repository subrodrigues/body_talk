using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//Generate a counter-clockwise convex hull with the jarvis march algorithm (gift wrapping)
//The algorithm is O(n*n) but is often faster if the number of points on the hull is fewer than all points
//In that case the algorithm will be O(h * n)
//Is more robust than other algorithms because it will handle colinear points with ease
//The algorithm will fail if we have more than 3 colinear points
//But this is a special case, which will take time to test, so make sure they are NOT colinear!!!
public class GiftWrappingAlgorithm : MonoBehaviour{

    // Number of polygon points
    private int k_iNumPoints = 0;

    // Source points received
    private List<Vector3> m_points;

    // Convex hull
    private List<Vector3> m_convexHull;

    // Use this for initialization
    public void Start(){
        m_points = new List<Vector3>();
        m_convexHull = new List<Vector3>();
    }

    public List<Vector3> UpdatePoints(List<Vector3> points){
        k_iNumPoints = points.Count;
        m_points.Clear();
        m_convexHull.Clear();

        // Random position
        m_points = points;

        // Detecting convex hull
        return ConvexHullDetection();
    }

    // Update is called once per frame
    public void Update()
    {

        // Render source points
        for (int i = 0; i < k_iNumPoints; i++){
            Vector3 pt = m_points[i];

            Debug.DrawLine(new Vector3(pt.x - 0.05f, pt.y, 0), new Vector3(pt.x + 0.05f, pt.y, 0), Color.green, 0, false);
            Debug.DrawLine(new Vector3(pt.x, pt.y - 0.05f, 0), new Vector3(pt.x, pt.y + 0.05f, 0), Color.green, 0, false);
        }

        // Render convex hull
        if (m_convexHull.Count != 0)
        {
            for (int i = 0; i < m_convexHull.Count - 1; i++)
            {
                Debug.DrawLine(m_convexHull[i], m_convexHull[i + 1], Color.red, 0, false);
            }

            Debug.DrawLine(m_convexHull[m_convexHull.Count - 1], m_convexHull[0], Color.red, 0, false);
        }
    }

    /* Implements the theta function from Sedgewick: Algorithms in XXX, chapter 24 */
    /* z-axis is ignored */
    float theta(Vector3 p1, Vector3 p2)
    {
        float dx = p2.x - p1.x;
        float dy = p2.y - p1.y;
        float ax = Mathf.Abs(dx);
        float ay = Mathf.Abs(dy);
        float t = (ax + ay == 0) ? 0 : dy / (ax + ay);

        if (dx < 0)
        {
            t = 2 - t;
        }
        else if (dy < 0)
        {
            t = 4 + t;
        }

        return t * 90.0f;
   }

    /* Implements Gift wrapping algorithm */
    /* http://en.wikipedia.org/wiki/Convex_hull_algorithms */
    List<Vector3> ConvexHullDetection() {
        int nTotalPts = m_points.Count;
        Vector3[] convexHull = new Vector3[nTotalPts + 1];
        int min = 0;
        int nConvexHullPts = 0;
        float v = 0.0f;

        for (int i = 0; i < nTotalPts; i++)
        {
            convexHull[i] = m_points[i];

            if (convexHull[i].y < convexHull[min].y)
            {
                min = i;
            }
        }

        convexHull[nTotalPts] = convexHull[min];
        Swap(ref convexHull[0], ref convexHull[min]);

        while (min != nTotalPts)
        {
            float minAngle = 360.0f;

            for (int i = nConvexHullPts + 1; i < nTotalPts + 1; i++)
            {
                float angle = theta(new Vector2(convexHull[nConvexHullPts].x, convexHull[nConvexHullPts].y), new Vector2(convexHull[i].x, convexHull[i].y));

                if (angle > v && angle < minAngle)
                {
                    minAngle = angle;
                    min = i;
                }
            }

            v = minAngle;
            nConvexHullPts++;
            // Debug.Log("k_iNumPoints: " + k_iNumPoints + " "+ "nConvexHullPts: " + nConvexHullPts + " " + "min: " + min);

            if(nConvexHullPts >= convexHull.Length)
                break;

            Swap(ref convexHull[nConvexHullPts], ref convexHull[min]);
        }

        for (int i = 0; i < nConvexHullPts; i++)
        {
            m_convexHull.Add(convexHull[i]);
        }

        return m_convexHull;
    }

    void Swap(ref Vector3 v1, ref Vector3 v2)
    {
        Vector3 tmp = v1;
        v1 = v2;
        v2 = tmp;
    }
}
