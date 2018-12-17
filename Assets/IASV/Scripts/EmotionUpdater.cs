using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmotionUpdater : MonoBehaviour {
   	public Texture[] textures;
    public float changeInterval = 0.33F;
    public Renderer rend;

    void Start()
    {
        rend = GetComponent<Renderer>();
    }

	public void SetEmotion(int index){
		if (textures.Length == 0)
            return;
        rend.material.mainTexture = textures[index];
	}
}
