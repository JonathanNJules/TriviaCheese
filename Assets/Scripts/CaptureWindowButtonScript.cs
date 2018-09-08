using UnityEngine;
using UnityEngine.UI;

public class CaptureWindowButtonScript : MonoBehaviour {

    private bool mouseOver, adjusting;
    private Image i;
    private Text t;
    private Color startColor, lightTintedColor, darkTintedColor;
    public int parameterIndex;
    private int startX, startVal;
    private float scrollSpeed = 1;
    private int ca, xVal;

	void Start () {
        i = GetComponent<Image>();
        t = transform.GetChild(0).GetComponent<Text>();
        startColor = i.color;
        lightTintedColor = startColor * new Color(.8f, .8f, .8f, 1);
        darkTintedColor = startColor * new Color(.6f, .6f, .6f, 1);
        UpdateText();
    }
	
	void Update () {

        // Starting Adjustments
        if (Input.GetMouseButtonDown(0) && mouseOver)
        {
            adjusting = true;
            i.color = darkTintedColor;
            startX = (int)Input.mousePosition.x;

            // We want the user to adjust the values by clicking and dragging the boxes, so how much the value
            // changes should be relative to where we started our dragging motion. Let's grab that here.
            switch (parameterIndex)
            {
                case 0:
                    startVal = CaptureWindowParameters.xPosition[ca];
                    break;
                case 1:
                    startVal = CaptureWindowParameters.yPosition[ca];
                    break;
                case 2:
                    startVal = CaptureWindowParameters.width[ca];
                    break;
                case 3:
                    startVal = CaptureWindowParameters.height[ca];
                    break;
            }
        }

        // Adjusting Values
        if (!adjusting) return;

        xVal = startVal + (int)((Input.mousePosition.x - startX) * scrollSpeed);
        switch (parameterIndex)
        {
            case 0:
                CaptureWindowParameters.xPosition[ca] = xVal;
                break;
            case 1:
                CaptureWindowParameters.yPosition[ca] = xVal;
                break;
            case 2:
                CaptureWindowParameters.width[ca] = xVal;
                break;
            case 3:
                CaptureWindowParameters.height[ca] = xVal;
                break;
        }
        ClampParameters(parameterIndex);
        UpdateText();

        if (Input.GetMouseButtonUp(0))
        {
            adjusting = false;
            i.color = (mouseOver ? lightTintedColor : startColor);
        }
    }

    void ClampParameters(int state)
    {
        // State 0 means adjusting the xPosition, which means you need to reclamp the xPosition AND the width
        // State 1 means adjusting the yPosition, which means you need to reclamp the yPosition AND the height
        // State 2 means adjusting the width, which means you need to reclamp just the width
        // State 3 means adjusting the height, which means you need to reclamp just the height

        // The SetPixels32() function requires the width and the height to be at least 1.
        // A side effect of this is that the xPosition and yPosition should never be at the maximum x or
        // y for the screen, but instead should always leave at least 1 pixel for the width/height to use.

        // Also, since the pixels are indexed, the width and height go up to the screen's pixel width/height - 1
        // (for example, on a 1920x1080 display, the y pixels range from 0-1079. Notice there is no pixel #1080).
        // A side effect of this is that x/y Position must give ANOTHER pixel of breathing room to width/height.

        if (state == 0)
            CaptureWindowParameters.xPosition[ca] = Mathf.Clamp(CaptureWindowParameters.xPosition[ca], 0, Screen.width - 1 - 1);
        if (state == 1)
            CaptureWindowParameters.yPosition[ca] = Mathf.Clamp(CaptureWindowParameters.yPosition[ca], 0, Screen.height - 1 - 1);
        if (state == 0 || state == 2)
            CaptureWindowParameters.width[ca] = Mathf.Clamp(CaptureWindowParameters.width[ca], 1, Screen.width - CaptureWindowParameters.xPosition[ca] - 1);
        if (state == 1 || state == 3)
            CaptureWindowParameters.height[ca] = Mathf.Clamp(CaptureWindowParameters.height[ca], 1, Screen.height - CaptureWindowParameters.yPosition[ca] - 1);
    }

    public void UpdateText()
    {
        // Updates the text in the adjustment UI boxes. Note that this function is NOT called every frame,
        // but instead is only called when it needs to be - when the user changes the values.

        ca = CaptureWindowParameters.setupApp;
        switch (parameterIndex)
        {
            case 0:
                t.text = CaptureWindowParameters.xPosition[ca].ToString();
                return;
            case 1:
                t.text = CaptureWindowParameters.yPosition[ca].ToString();
                return;
            case 2:
                t.text = CaptureWindowParameters.width[ca].ToString();
                return;
            case 3:
                t.text = CaptureWindowParameters.height[ca].ToString();
                return;
        }
    }

    // Updating Mouse State
    private void OnMouseEnter()
    {
        mouseOver = true;
        i.color = lightTintedColor;
    }
    private void OnMouseExit()
    {
        mouseOver = false;
        if (!adjusting)
            i.color = startColor;
    }
}