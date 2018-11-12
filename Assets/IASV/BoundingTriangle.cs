using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundingTriangle : MonoBehaviour {
	public GameObject leftHandObj, rightHandObj, headObj;
	private GameObject mGoTriangle;

	Vector3[] newVertices;
	Vector2[] newUV;
	int[] newTriangles;

	void Start(){
		Vector2[] vertices = new Vector2[] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1)};
		ushort[] triangles = new ushort[] { 0, 1, 2 };

		DrawPolygon2D(vertices, triangles, Color.red);
	}

	void DrawPolygon2D(Vector2[] vertices, ushort[] triangles, Color color) {
		GameObject polygon = new GameObject("Bounding Triangle"); //create a new game object
		SpriteRenderer sr = polygon.AddComponent<SpriteRenderer>(); // add a sprite renderer
		Texture2D texture = new Texture2D(1025, 1025); // create a texture larger than your maximum polygon size

		// create an array and fill the texture with your color
		List<Color> cols = new List<Color>(); 
		for (int i = 0; i < (texture.width * texture.height); i++)
			cols.Add(color);
		texture.SetPixels(cols.ToArray());
		texture.Apply();

		sr.color = color; //you can also add that color to the sprite renderer

		sr.sprite = Sprite.Create(texture, new Rect(0, 0, 1024, 1024), Vector2.zero, 1); //create a sprite with the texture we just created and colored in

		//convert coordinates to local space
		float lx = Mathf.Infinity, ly = Mathf.Infinity;
		foreach (Vector2 vi in vertices)
		{
			if (vi.x < lx)
				lx = vi.x;
			if (vi.y < ly)
				ly = vi.y;
		}
		Vector2[] localv = new Vector2[vertices.Length];
		for (int i = 0; i < vertices.Length; i++)
		{
			localv[i] = vertices[i] - new Vector2(lx, ly);
		}

		sr.sprite.OverrideGeometry(localv, triangles); // set the vertices and triangles

		polygon.transform.position = (Vector2)transform.InverseTransformPoint(transform.position) + new Vector2(lx, ly); // return to world space
	}

	// Update is called once per frame
	void Update () {
		printWorldPos (leftHandObj);
		printWorldPos (rightHandObj);
		printWorldPos (headObj);

	}

	void printWorldPos(GameObject obj){
		Debug.Log (obj.transform.position.ToString("F4"));
	}

	void drawBoundingTriangle(){
		mGoTriangle.AddComponent<MeshFilter>();
		mGoTriangle.AddComponent<MeshRenderer>();
		Mesh meshTriangle = mGoTriangle.GetComponent<MeshFilter>().mesh;
		meshTriangle.Clear();
		meshTriangle.vertices = new Vector3[] { new Vector3(0, 0, 0), new Vector3(0, 50f, 0), new Vector3(50f, 50f, 0) };
		meshTriangle.uv = new Vector2[] { new Vector2(0, 0), new Vector2(0, 50f), new Vector2(50f, 50f) };
		meshTriangle.triangles = new int[] { 0, 1, 2 };
	}
}
