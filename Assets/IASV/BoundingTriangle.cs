using System.Collections; 
using System.Collections.Generic; 
using UnityEngine; 
using TMPro; 

public class BoundingTriangle:MonoBehaviour {
    
    private float HEAD_MASS = 4.5f; // 4.5kg
private float HAND_MASS = 0.5f; 

    public GameObject mLeftHandObj, mRightHandObj, mHeadObj; 
    private GameObject mBoundingTriangle; 
    private bool isBoundingTriangleVisible; 
    private Vector3 mLeftHandObjLastPos, mRightHandObjLastPos, mHeadObjLastPos; 

    // Rending variables
    private Color32[] mBoundingTriangleColors; 
    private TextMeshProUGUI mEFEnergy, mEFSpatialExtent; 
    private bool isOdd = true; 
    private float mDeltaTime = 0;

    void Start() {
        initialize(); 
        setup(); 
    }

    void initialize() {
        // Init game objects
        mBoundingTriangle = new GameObject("Bounding Triangle"); 
        mBoundingTriangle.AddComponent < MeshFilter > (); 
        mBoundingTriangle.AddComponent < MeshRenderer > (); 

        mEFEnergy = GameObject.FindWithTag("EFEnergy").GetComponent < TextMeshProUGUI > (); 
        mEFSpatialExtent = GameObject.FindWithTag("EFSpatialExtent").GetComponent < TextMeshProUGUI > (); 

        mHeadObjLastPos = new Vector3(0, 0, 0);
        mLeftHandObjLastPos = new Vector3(0, 0, 0);
        mRightHandObjLastPos = new Vector3(0, 0, 0);

        // Init logic variables
        isBoundingTriangleVisible = false; 
                
        // Init bounding triangle color
        Color32[] colors = new Color32[3]; 
        for (int i = 0; i < 3; i++) {
            colors[i] = Color32.Lerp(Color.green, Color.green, 1.0f); 
        }
        mBoundingTriangleColors = colors; 
    }

    void calcEFEnergy(float deltaTime) {
        float eTotal = (HEAD_MASS * Mathf.Pow(auxLimbVelocity(mHeadObj.transform.position, mHeadObjLastPos, deltaTime), 2) + 
                       HAND_MASS * Mathf.Pow(auxLimbVelocity(mLeftHandObj.transform.position, mLeftHandObjLastPos, deltaTime), 2) + 
                       HAND_MASS * Mathf.Pow(auxLimbVelocity(mRightHandObj.transform.position, mRightHandObjLastPos, deltaTime), 2))
                       /2; 

        eTotal *= 10f;
        mEFEnergy.text = eTotal.ToString(); 

        mHeadObjLastPos = mHeadObj.transform.position;
        mLeftHandObjLastPos = mLeftHandObj.transform.position;
        mRightHandObjLastPos = mRightHandObj.transform.position;
    }

    /**
    * Auxiliar method to the Expressive Feature - Energy.
    * Calculates the limb velocity.
    */
    float auxLimbVelocity(Vector3 limbPosition, Vector3 lastPos, float deltaTime) {
        return Mathf.Sqrt(Mathf.Pow((limbPosition.x - lastPos.x) / deltaTime, 2) + 
                Mathf.Pow((limbPosition.y - lastPos.y) / deltaTime, 2) +
                Mathf.Pow((limbPosition.z - lastPos.z) / deltaTime, 2)); 
    }

    void setup() {
        // Set bounding triangle shader
        var doubleSidedShader = Shader.Find("Custom/DoubleSided/Unlit"); 
        mBoundingTriangle.GetComponent < Renderer > ().material.shader = doubleSidedShader; 
    }

    // Update is called once per frame
    void Update() {
       CheckBoundingTriangleVisibility(); 
    }

    // Exactly called 50 times per second
    void FixedUpdate() {
        mDeltaTime += Time.deltaTime;

        if (isOdd){
            calcEFEnergy(mDeltaTime);
            mDeltaTime = 0;
        } 
        isOdd = !isOdd;
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
        
        if (isAllBoundingComponentsVisible) {
            if ( ! isBoundingTriangleVisible) {
                isBoundingTriangleVisible = true; 
            }
            UpdateBoundingTrianglePosition(); 
        }
        else if ( ! isAllBoundingComponentsVisible && isBoundingTriangleVisible) {
            isBoundingTriangleVisible = false; 
            RemoveBoundingTriangle(); 
        }
    }

    /**
	 * Method that draws the bounding triangle accordingly to the current head and hands position
	 */
    void UpdateBoundingTrianglePosition() {
        Mesh mesh = mBoundingTriangle.GetComponent < MeshFilter > ().mesh; 
        mesh.Clear(); 
        mesh.vertices = new Vector3[] {
            mLeftHandObj.transform.position, 
            mRightHandObj.transform.position, 
            mHeadObj.transform.position}; 

        mesh.uv = new Vector2[] {new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1)}; 
        mesh.triangles = new int[] {0, 1, 2 }; 

        mesh.colors32 = mBoundingTriangleColors; 

        float perimeter = Vector3.Distance(mLeftHandObj.transform.position, mRightHandObj.transform.position); 
        perimeter += Vector3.Distance(mRightHandObj.transform.position, mHeadObj.transform.position); 
        perimeter += Vector3.Distance(mHeadObj.transform.position, mLeftHandObj.transform.position); 
        perimeter *= 10f; 

        // Update UI
        mEFSpatialExtent.text = perimeter.ToString(); 
    }

    /**
	 * Method that removes the bounding triangle
	 */
    void RemoveBoundingTriangle() {
        Mesh mesh = mBoundingTriangle.GetComponent < MeshFilter > ().mesh; 
        mesh.Clear(); 

        // Update UI
        mEFSpatialExtent.text = "0"; 
    }

}
