using UnityEngine;

public class PopupPanelScript : MonoBehaviour {

    [HideInInspector]
    public bool active;
    public float fadeSpeed, scaleSpeed;
    public float inactiveScale;
    private CanvasGroup cg;

    private float ref1;
    private Vector3 ref2;
    private bool mouseOver;
    public AnalyzeDataScript ads;
    public GameObject whiteFade;

	void Start () {
        cg = GetComponent<CanvasGroup>();
	}
	
	void Update () {
        // Transitions
        cg.alpha = Mathf.SmoothDamp(cg.alpha, (active ? 1:0), ref ref1, fadeSpeed);
        transform.localScale = Vector3.SmoothDamp(transform.localScale, Vector3.one * (active ? 1:inactiveScale), ref ref2, scaleSpeed);

        // Click Reactions
        if (Input.GetMouseButtonUp(0) && ads.enabled)
        {
            if (!active)
                active = true;
            else if (!mouseOver)
                active = false;
        }
    }

    // Updating Mouse State
    private void OnMouseEnter()
    {
        mouseOver = true;
    }
    private void OnMouseExit()
    {
        mouseOver = false;
    }

    public void EnableWhiteFade()
    {
        // I'm putting this in side a seperate button function so that I can check
        // to see if the panel is actually open before triggering the button action
        whiteFade.SetActive(active);
    }
}
