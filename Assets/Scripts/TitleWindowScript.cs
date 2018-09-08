using UnityEngine;

public class TitleWindowScript : MonoBehaviour {

    // STATES:
    // 0 = Instructions Panel
    // 1 = Credits Panel
    // 2 = Setup Panel
    // 3 = Off

    public int state = 3;
    private bool switchState;
    public Transform offPosition, onPosition;
    public GameObject[] menus;

    private CanvasGroup cg;
    private float alphaRef;
    private Vector3 positionRef;
    private int i;

	void Start () {
        cg = GetComponent<CanvasGroup>();
	}
	
	void Update () {
        // Animate the window in/out depending on whether or not the window is set to off (i.e. state = 3)
        transform.position = Vector3.SmoothDamp(transform.position, ((state == 3) ? offPosition : onPosition).position, ref positionRef, 0.15f);
        cg.alpha = Mathf.SmoothDamp(cg.alpha, (state == 3) ? 0 : 1, ref alphaRef, 0.08f);

        // Don't adjust the panel every frame - only reassign stuff when the state changes
        if (switchState)
        {
            for(i = 0; i < 3; i++)
                menus[i].SetActive(state == i);
            switchState = false;
        }
    }

    // Function called by the Unity buttons to update the state
    public void ChangeWindow(int newState)
    {
        state = newState;
        switchState = true;
    }
}