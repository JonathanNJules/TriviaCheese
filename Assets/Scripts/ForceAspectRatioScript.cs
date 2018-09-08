using UnityEngine;

public class ForceAspectRatioScript : MonoBehaviour {

    // As I don't feel like configuring the UI to keep up with all different types of aspect ratios,
    // this script should serve as a quick fix to force a particular ratio of your choice
    // Credit to Adrian Lopez, as this script is heavily based on his implementation

    public float aspectRatioWidth, aspectRatioHeight;
    private float targetAspectRatio, currentAspectRatio;
    private float asFactor;
    private Camera cam;
    private Rect rect;

	void Start () {
        targetAspectRatio = aspectRatioWidth / aspectRatioHeight;
        cam = GetComponent<Camera>();
	}
	
	void Update () {
        rect = cam.rect;
        currentAspectRatio = Screen.width / (float)Screen.height;
        asFactor = currentAspectRatio / targetAspectRatio;
        if (asFactor > 1)
        {
            rect.width = 1 / asFactor;
            rect.x = (1 - (1 / asFactor)) / 2;
            rect.height = 1;
            rect.y = 0;
        }
        else if (asFactor < 1)
        {
            rect.height = 1 * asFactor;
            rect.y = (1 - (1 * asFactor)) / 2;
            rect.width = 1; 
            rect.x = 0; 
        }
        else
        {
            rect.width = rect.height = 1;
            rect.x = rect.y = 0;
        }
        cam.rect = rect;
    }
}
