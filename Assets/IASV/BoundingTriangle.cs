using System.Collections; 
using System.Collections.Generic; 
using UnityEngine; 

public class BoundingTriangle:MonoBehaviour {
    public GameObject mLeftHandObj, mRightHandObj, mHeadObj; 
    private GameObject mBoundingTriangle; 
    private bool isBoundingTriangleVisible; 

    void Start() {
        init(); 
    }

    void init() {
        // Initialize game objects
        mBoundingTriangle = new GameObject("Bounding Triangle"); 
        mBoundingTriangle.AddComponent < MeshFilter > (); 
        mBoundingTriangle.AddComponent < MeshRenderer > (); 

        // Initialize logic variables
        isBoundingTriangleVisible = false; 
    }

    // Update is called once per frame
    void Update() {
       CheckBoundingTriangleVisibility(); 
       
    }



    /**
	 * Method that prints the objs world coordinates to the console
	 */
    void PrintWorldPos(GameObject obj) {
        Debug.Log(obj.transform.position.ToString("F4")); 
    }

    /**
    * Method that checks if the required components are visible (both hands and head) and updates the view.
    */
    void CheckBoundingTriangleVisibility() {
        bool isAllBoundingComponentsVisible = mLeftHandObj.activeInHierarchy && mRightHandObj.activeInHierarchy && mHeadObj.activeInHierarchy; 
        
        if (isAllBoundingComponentsVisible &&  ! isBoundingTriangleVisible) {
            isBoundingTriangleVisible = true; 
            DrawBoundingTriangle(); 
        }
        else if ( ! isAllBoundingComponentsVisible && isBoundingTriangleVisible) {
            isBoundingTriangleVisible = false; 
            mBoundingTriangle.GetComponent < MeshFilter > ().mesh.Clear(); 
        }
    }

    /**
	 * Method that draws a bounding triangle 
	 */
    void DrawBoundingTriangle() {
        Mesh mesh = mBoundingTriangle.GetComponent < MeshFilter > ().mesh; 
        mesh.Clear(); 
        mesh.vertices = new Vector3[] {new Vector3(0, 0, 0), new Vector3(0, 1, 0), new Vector3(1, 1, 0)}; 
        mesh.uv = new Vector2[] {new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1)}; 
        mesh.triangles = new int[] {0, 1, 2 }; 
    }

    /**
	 * Method that removes the bounding triangle
	 */
    void RemoveBoundingTriangle() {
        Mesh mesh = mBoundingTriangle.GetComponent < MeshFilter > ().mesh; 
        mesh.Clear(); 
    }

}
