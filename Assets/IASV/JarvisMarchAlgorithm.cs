using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
public static class JarvisMarchAlgorithm
{
    public static List<Vector3> GetConvexHull(List<Vector3> points)
    {
        //If we have just 3 points, then they are the convex hull, so return those
        if (points.Count == 3)
        {
            //These might not be ccw, and they may also be colinear
            return points;
        }

        //If fewer points, then we cant create a convex hull
        if (points.Count < 3)
        {
            return null;
        }



        //The list with points on the convex hull
        List<Vector3> convexHull = new List<Vector3>();

        //Step 1. Find the vertex with the smallest x coordinate
        //If several have the same x coordinate, find the one with the smallest z
        Vector3 startPos = points[0];

        for (int i = 1; i < points.Count; i++)
        {
            Vector3 testPos = points[i];

            //Because of precision issues, we use Mathf.Approximately to test if the x positions are the same
            if (testPos.x < startPos.x || (Mathf.Approximately(testPos.x, startPos.x) && testPos.z < startPos.z))
            {
                startPos = points[i];
            }
        }

        //This vertex is always on the convex hull
        convexHull.Add(startPos);

        points.Remove(startPos);

      

        //Step 2. Loop to generate the convex hull
        Vector3 currentPoint = convexHull[0];

        //Store colinear points here - better to create this list once than each loop
        List<Vector3> colinearPoints = new List<Vector3>();

        int counter = 0;

        while (true)
        {
            //After 2 iterations we have to add the start position again so we can terminate the algorithm
            //Cant use convexhull.count because of colinear points, so we need a counter
            if (counter == 2)
            {            
                points.Add(convexHull[0]);
            }
        
            //Pick next point randomly
            Vector3 nextPoint = points[Random.Range(0, points.Count)];

            //To 2d space so we can see if a point is to the left is the vector ab
            Vector2 a = new Vector2(currentPoint.x, currentPoint.z);

            Vector2 b = new Vector2(nextPoint.x, nextPoint.z);

            //Test if there's a point to the right of ab, if so then it's the new b
            for (int i = 0; i < points.Count; i++)
            {
                //Dont test the point we picked randomly
                if (points[i].Equals(nextPoint))
                {
                    continue;
                }
            
                Vector2 c = new Vector2(points[i].x, points[i].z);

                //Where is c in relation to a-b
                // < 0 -> to the right
                // = 0 -> on the line
                // > 0 -> to the left
				float relation = Mathf.Sign((b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x));

				
                
                //Colinear points
                //Cant use exactly 0 because of floating point precision issues
                //This accuracy is smallest possible, if smaller points will be missed if we are testing with a plane
                float accuracy = 0.00001f;

                if (relation < accuracy && relation > -accuracy)
                {
                    colinearPoints.Add(points[i]);
                }
                //To the right = better point, so pick it as next point on the convex hull
                else if (relation < 0f)
                {
                    nextPoint = points[i];

                    b = new Vector2(nextPoint.x, nextPoint.z);

                    //Clear colinear points
                    colinearPoints.Clear();
                }
                //To the left = worse point so do nothing
            }

        

            //If we have colinear points
            if (colinearPoints.Count > 0)
            {
                colinearPoints.Add(nextPoint);

                //Sort this list, so we can add the colinear points in correct order
                colinearPoints = colinearPoints.OrderBy(n => Vector3.SqrMagnitude(n - currentPoint)).ToList();

                convexHull.AddRange(colinearPoints);

                currentPoint = colinearPoints[colinearPoints.Count - 1];

                //Remove the points that are now on the convex hull
                for (int i = 0; i < colinearPoints.Count; i++)
                {
                    points.Remove(colinearPoints[i]);
                }

                colinearPoints.Clear();
            }
            else
            {
                convexHull.Add(nextPoint);
            
                points.Remove(nextPoint);

                currentPoint = nextPoint;
            }

            //Have we found the first point on the hull? If so we have completed the hull
            if (currentPoint.Equals(convexHull[0]))
            {
                //Then remove it because it is the same as the first point, and we want a convex hull with no duplicates
                convexHull.RemoveAt(convexHull.Count - 1);

                break;
            }

            counter += 1;
        }

        return convexHull;
    }
}

