using UnityEngine;

public class ScrollTextureScript : MonoBehaviour {

    private Material mat;
    public Vector2 scroll;

	void Start () {
        mat = GetComponent<Renderer>().material;
	}
	
	void Update () {
        mat.mainTextureOffset += scroll * Time.deltaTime;
	}
}
