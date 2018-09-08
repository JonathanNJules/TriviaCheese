using UnityEngine;
using UnityEngine.UI;

public class HandleErrorScript : MonoBehaviour {

    [HideInInspector]
    public short errorType = 0; // 0 = No Error | 1 = Error Reading Image | 2 = Error Reading HTML | 3 = Sharing Violation
    private bool setErrorMessage;
    public Image errorMessageImage;
    public Text errorMessageText;
    private float errorMessageImageRef;

    public RectTransform retryButton;
    private Vector3 retryButtonPos;
    private Vector3 retryButtonRef;

    public Transform loadingAnimation;
    private Material loadingAnimationMat;
    private float loadingAnimRef1;
    private Vector3 loadingAnimRef2;

    void Start () {
        retryButtonPos = new Vector3(0, -76, -98);
        loadingAnimationMat = loadingAnimation.GetComponent<Renderer>().material;
    }

    void Update () {
        if (errorType == 0) return;

        if (!setErrorMessage) {
            if (errorType == 1)
                errorMessageText.text = "Uh oh, it looks like we had trouble reading the image from your capture window!";
            else if (errorType == 2)
                errorMessageText.text = "Uh oh, it looks like we had trouble searching for the answer! Google may have blocked us from using their results!";
            else //if (errorType == 3)
                errorMessageText.text = "Uh oh, it looks like we ran into a sharing violation. Give it another go, it should work if you try again.";
            setErrorMessage = true;
        }

        retryButton.anchoredPosition3D = Vector3.SmoothDamp(retryButton.anchoredPosition3D, retryButtonPos, ref retryButtonRef, 0.18f);
        errorMessageImage.fillAmount = Mathf.SmoothDamp(errorMessageImage.fillAmount, 1, ref errorMessageImageRef, 0.15f);

        loadingAnimationMat.color = new Color(1, 1, 1, Mathf.SmoothDamp(loadingAnimationMat.color.a, 0, ref loadingAnimRef1, .15f));
        loadingAnimation.localScale = Vector3.SmoothDamp(loadingAnimation.localScale, Vector3.one * 10, ref loadingAnimRef2, .15f);
    }
}
