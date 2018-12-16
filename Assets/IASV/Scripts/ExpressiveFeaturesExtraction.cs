using System.Collections; 
using System.Collections.Generic; 
using UnityEngine; 
using TMPro; 

public class ExpressiveFeaturesExtraction:MonoBehaviour {
    private GiftWrappingAlgorithm mGiftWrapping;

    private float HEAD_MASS = 4.5f; // 4.5kg
    private float HAND_MASS = 0.5f; 

    private int SLIDING_WINDOW = 50; // Unit is number of frames. In this case, 2 seconds sliding window (25fps).

    public GameObject mLeftHandObj, mRightHandObj, mHeadObj; 
    private GameObject mBoundingTriangle; 
    private bool isBoundingTriangleVisible; 
    private Vector3 mLeftHandObjLastPos, mRightHandObjLastPos, mHeadObjLastPos; 
    private Vector3 mLeftHandObjLastVelocity, mRightHandObjLastVelocity;
    private Vector3[] mLeftHandPositions, mRightHandPositions; // TODO: Change this to ARRAY
    
    // Rending variables
    private Color32[] mBoundingTriangleColors; 
    private TextMeshProUGUI mEFEnergy, mEFSpatialExtent, mEFLeftCurvature, mEFRightCurvature, mSISpatial, mSISpread, mHeadLeaning; 
    private bool isOdd = true;
    private float mDeltaTime = 0;
    private int mCurrentFrame = 0;
    private float mETotal = 0.0f, mTotalLeftCurvature = 0.0f, mTotalRightCurvature = 0.0f, mSymmetrySpatial = 0.0f, mTriangleSpatialExtent = 0.0f, mTotalHeadLeaning = 0.0f;

    void Start() {
        initialize(); 
        setup(); 
    }

    void initialize() {
        mGiftWrapping = new GiftWrappingAlgorithm();
        mGiftWrapping.Start();

        // Init game objects
        mBoundingTriangle = new GameObject("Bounding Triangle"); 
        mBoundingTriangle.AddComponent < MeshFilter > (); 
        mBoundingTriangle.AddComponent < MeshRenderer > (); 

        mEFEnergy = GameObject.FindWithTag("EFEnergy").GetComponent < TextMeshProUGUI > (); 
        mEFSpatialExtent = GameObject.FindWithTag("EFSpatialExtent").GetComponent < TextMeshProUGUI > (); 
        mEFRightCurvature = GameObject.FindWithTag("EFSmoothnessRight").GetComponent < TextMeshProUGUI > (); 
        mEFLeftCurvature = GameObject.FindWithTag("EFSmoothnessLeft").GetComponent < TextMeshProUGUI > (); 
        mSISpatial = GameObject.FindWithTag("EFSISpatial").GetComponent < TextMeshProUGUI > (); 
        mSISpread = GameObject.FindWithTag("EFSISpread").GetComponent < TextMeshProUGUI > (); 
        mHeadLeaning = GameObject.FindWithTag("EFHeadLeaning").GetComponent < TextMeshProUGUI > (); 

        // Variables to calculate Energy
        mHeadObjLastPos = new Vector3(0, 0, 0);
        mLeftHandObjLastPos = new Vector3(0, 0, 0);
        mRightHandObjLastPos = new Vector3(0, 0, 0);

        // Variables to calculate Smoothness
        mLeftHandObjLastVelocity = new Vector3(0, 0, 0);
        mRightHandObjLastVelocity = new Vector3(0, 0, 0);

        mLeftHandPositions = new Vector3[SLIDING_WINDOW];
        mRightHandPositions = new Vector3[SLIDING_WINDOW];

        // Init logic variables
        isBoundingTriangleVisible = false; 
                
        // Init bounding triangle color
        Color32[] colors = new Color32[3]; 
        for (int i = 0; i < 3; i++) {
            colors[i] = Color32.Lerp(Color.green, Color.green, 1.0f); 
        }
        mBoundingTriangleColors = colors; 
    }

    void setup() {
        // Set bounding triangle shader
        var doubleSidedShader = Shader.Find("Custom/DoubleSided/Unlit"); 
        mBoundingTriangle.GetComponent < Renderer > ().material.shader = doubleSidedShader; 
    }

    // Update is called once per frame
    void Update() {
        mGiftWrapping.Update();
        CheckBoundingTriangleVisibility(); 
    }

    // Exactly called SLIDING_WINDOW times per second
    void FixedUpdate() {
        mDeltaTime += Time.deltaTime;

        if (isOdd){
            Vector3 currentLeftHandPosition = mLeftHandObj.transform.position;
            Vector3 currentRightHandPosition = mRightHandObj.transform.position;

            mCurrentFrame += 1; // Increment frame count

            calcEFEnergy(mDeltaTime);

            CalcSISpatial();

            CalcBoundingTriangleSpatialExtent();

            calcHeadLeaning(mDeltaTime);

            if(mCurrentFrame == SLIDING_WINDOW){ // 2 Seconds reached 
                // Energy ammount
                mEFEnergy.text = (mETotal / 50.0f).ToString(); 

                // Symmetry Spatial
                mSISpatial.text = (mSymmetrySpatial / 50.0f).ToString();

                // Hands smoothness
                CalcEFCurvature();
                mEFLeftCurvature.text = mTotalLeftCurvature.ToString();
                mEFRightCurvature.text = mTotalRightCurvature.ToString();

                // Bounding Triangle Perimeter
                mEFSpatialExtent.text = (mTriangleSpatialExtent / 50.0f).ToString();

                // Symmetry Spread 
                float symmetrySpread = calcSISpread(mLeftHandPositions, mRightHandPositions);
                mSISpread.text = HasValue(symmetrySpread) ? symmetrySpread.ToString() : "0";

                // Head Leaning
                mHeadLeaning.text = (mTotalHeadLeaning / 50.0f).ToString();

                // Reset variables
                mETotal = 0.0f;
                mTotalLeftCurvature = 0.0f;
                mTotalRightCurvature = 0.0f;
                mSymmetrySpatial = 0.0f;
                mTriangleSpatialExtent = 0.0f;
                mTotalHeadLeaning = 0.0f;

                mCurrentFrame = 0;
            } else{
                mLeftHandPositions[mCurrentFrame] = currentLeftHandPosition;
                mRightHandPositions[mCurrentFrame] = currentRightHandPosition;
            }

            mDeltaTime = 0;

            mHeadObjLastPos = mHeadObj.transform.position;
            mLeftHandObjLastPos = mLeftHandObj.transform.position;
            mRightHandObjLastPos = mRightHandObj.transform.position;
        }

        isOdd = !isOdd;
    }

    void CalcBoundingTriangleSpatialExtent(){
        if(!isBoundingTriangleVisible)
            return;

        float perimeter = Vector3.Distance(mLeftHandObj.transform.position, mRightHandObj.transform.position); 
        perimeter += Vector3.Distance(mRightHandObj.transform.position, mHeadObj.transform.position); 
        perimeter += Vector3.Distance(mHeadObj.transform.position, mLeftHandObj.transform.position); 

        // Perimeter Spatial Extent
        mTriangleSpatialExtent += perimeter;
    }

    void calcHeadLeaning(float deltaTime){
        float headLeaning = (mHeadObj.transform.position.z - mHeadObjLastPos.z) / deltaTime;
        mTotalHeadLeaning += headLeaning;
    }

    float calcSISpread(Vector3[] leftHandPos, Vector3[] rightHandPos){
        float hLeftHand = calcGeometricEntropy(leftHandPos);
        float hRightHand = calcGeometricEntropy(rightHandPos);

        float spreadSI = hLeftHand / hRightHand;

        return spreadSI;
    }

    float calcGeometricEntropy(Vector3[] handPositions){
        float distance = 0;
        for(int i = 1; i < SLIDING_WINDOW; i++){
            distance += Vector3.Distance(handPositions[i-1], handPositions[i]); 
        }

        List<Vector3> positions = new List<Vector3>();
        for(int i = 0; i < handPositions.Length; i++)
            positions.Add(handPositions[i]);

        List<Vector3> convexHull = mGiftWrapping.UpdatePoints(positions);

        float perimeterAroundConvexHull = 0;
        Vector3 lastVertex = new Vector3(0, 0, 0);
        bool firstIteration = true;
        foreach(Vector3 vertex in convexHull){
            if(firstIteration){
                firstIteration = false;
                lastVertex = vertex;
            } else {
                perimeterAroundConvexHull += Vector3.Distance(lastVertex, vertex);
            }
        }

        float h = Mathf.Log((2*distance) / perimeterAroundConvexHull);

        return h;
    }

    void CalcSISpatial(){
        if(!isBoundingTriangleVisible)
            return;

        Mesh mesh = mBoundingTriangle.GetComponent < MeshFilter > ().mesh; 
        if(mesh == null || mesh.vertices.Length <= 0)
            return;

        Vector3 barycenter = getTriangleCentroid(mesh.vertices[0], mesh.vertices[1], mesh.vertices[2]); 
        
        float horizSI = CalcSISpatialAux(mesh.vertices[2].x, mesh.vertices[0].x, mesh.vertices[1].x);
        float vertSI = CalcSISpatialAux(mesh.vertices[2].x, mesh.vertices[0].y, mesh.vertices[1].y);

        float spatialSI = horizSI / vertSI;

        mSymmetrySpatial += spatialSI;
    }

    float CalcSISpatialAux(float b, float l, float r){
        return Mathf.Abs(Mathf.Abs(b - l) - Mathf.Abs(b - r) ) / Mathf.Abs(r - l);
    }

    void CalcEFCurvature(){
        float leftCurvature = 0.0f;
        for(int i = 2; i < mLeftHandPositions.Length; i+= 3){
            leftCurvature += CalcCurvatureAux(mLeftHandPositions[i-1], mLeftHandPositions[i], mLeftHandPositions[i+1]);
        }
        mTotalLeftCurvature = leftCurvature;

        float rightCurvature = 0.0f;
        for(int i = 2; i < mRightHandPositions.Length; i+= 3){
            rightCurvature += CalcCurvatureAux(mRightHandPositions[i-1], mRightHandPositions[i], mRightHandPositions[i+1]);
        }
        mTotalRightCurvature = rightCurvature;    
    }

    float CalcCurvatureAux(Vector3 xMinus, Vector3 x, Vector3 xPlus){
        Vector3 velVec = (xPlus - xMinus) * 2.0f;
        float vel = velVec.magnitude;

        Vector3 accVec = (xPlus + xMinus - (2*x));
        float acc = accVec.magnitude;

        float curvature = Mathf.Abs((acc * Vector3.Dot(velVec, velVec)) - (Vector3.Dot(accVec, velVec)*vel)) / 
            (Vector3.Dot(velVec, velVec) * Vector3.Dot(velVec, velVec));

        return curvature;
    }

    // void calcEFSmoothness(Vector3 leftHandVel, Vector3 leftHandLastVel, Vector3 rightHandVel, Vector3 rightHandLastVel){
    //     float leftCurvature = auxCurvatureValue(leftHandVel, leftHandLastVel);
    //     float rightCurvature = auxCurvatureValue(rightHandVel, rightHandLastVel);

    //     mTotalLeftCurvature += leftCurvature;
    //     mTotalRightCurvature += rightCurvature;
    // }

    // float auxCurvatureValue(Vector3 handVel, Vector3 lastHandVel) {
    //     float xCurrentAcc = (handVel.x - lastHandVel.x);
    //     float yCurrentAcc = (handVel.y - lastHandVel.y);

    //     float top = (handVel.x * yCurrentAcc) - (handVel.y * xCurrentAcc);
    //     float bottom = Mathf.Pow(handVel.x, 2) + Mathf.Pow(handVel.y, 2);
    //     float bottomPow = Mathf.Pow(bottom, (3f/2f));

    //     // Limit decimal cases (for example, all x1000). TODO: Find a better alternative
    //     float curv = top/bottomPow;
    //     // curv = curv / 1000f;
        
    //     return curv;
    // }

    void calcEFEnergy(float deltaTime) {
        float eTotal = (HEAD_MASS * Mathf.Pow(auxLimbVelocity(mHeadObj.transform.position, mHeadObjLastPos, deltaTime), 2) + 
                       HAND_MASS * Mathf.Pow(auxLimbVelocity(mLeftHandObj.transform.position, mLeftHandObjLastPos, deltaTime), 2) + 
                       HAND_MASS * Mathf.Pow(auxLimbVelocity(mRightHandObj.transform.position, mRightHandObjLastPos, deltaTime), 2))
                       /2; 

        mETotal += eTotal;
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
        
    }

    /**
	 * Method that removes the bounding triangle
	 */
    void RemoveBoundingTriangle() {
        Mesh mesh = mBoundingTriangle.GetComponent < MeshFilter > ().mesh; 
        mesh.Clear(); 
    }

    // Or IsNanOrInfinity
    public static bool HasValue(float value){
        return !float.IsNaN(value) && !float.IsInfinity(value);
    }
    
    Vector3 getTriangleCentroid(Vector3 p1, Vector3 p2, Vector3 p3){
        return new Vector3((p1.x + p2.x + p3.x)/3, (p1.y + p2.y + p3.y)/3, (p1.z + p2.z + p3.z)/3);
    }
}
