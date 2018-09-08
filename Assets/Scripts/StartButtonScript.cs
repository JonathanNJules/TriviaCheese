using UnityEngine;
using UnityEngine.UI;

public class StartButtonScript : MonoBehaviour {

    public CaptureQuestionScript cqs;
    private bool fadeOut;
    private CanvasGroup cg;
    private float alphaRef;
    private Vector3 transformRef;

    void Start () {
        cg = GetComponent<CanvasGroup>();
	}
	
	void Update () {

        // When the start button is pressed, blow up and fade out. When the animation finishes, destroy yourself
        if (fadeOut)
        {
            cg.alpha = Mathf.SmoothDamp(cg.alpha, 0, ref alphaRef, 0.08f);
            transform.localScale = Vector3.SmoothDamp(transform.localScale, Vector3.one * 1.6f, ref transformRef, 0.15f);
            if (cg.alpha < 0.01f)
                Destroy(gameObject);
        }
	}

    // The public function used by the start button to kick off the answer finding process
    public void StartLoading()
    {
        cqs.ready = true;
        fadeOut = true;
        GetComponent<Button>().enabled = false;
    }
}
