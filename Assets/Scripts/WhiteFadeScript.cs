using UnityEngine;
using UnityEngine.SceneManagement;

public class WhiteFadeScript : MonoBehaviour {

    public FadeCondition fadeCondition;
    private CanvasGroup cg;

	void Start () {
        cg = GetComponent<CanvasGroup>();
        if (fadeCondition == FadeCondition.FadeFromWhite)
            cg.alpha = 1;
	}
	
	void Update () {
        if (fadeCondition == FadeCondition.FadeToWhite)
        {
            if (cg.alpha < 1)
                cg.alpha += 3 * Time.deltaTime;
            else
                SceneManager.LoadScene("Main");
        }
        else //if (fadeCondition == FadeCondition.FadeFromWhite)
        {
            if(cg.alpha > 0)
                cg.alpha -= 3 * Time.deltaTime;
            else
                Destroy(gameObject);
        }
    }

    public enum FadeCondition { FadeFromWhite, FadeToWhite };
}
