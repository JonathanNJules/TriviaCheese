using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class CaptureWindowParameters : MonoBehaviour {

    public static int currentApp, setupApp; // 0 = HQ Trivia | 1 = Swag IQ | 2 = Cash Show
    public static int[] xPosition, yPosition, width, height;
    private static string newVersiontText = "";

    private int[] defaultXPos, defaultYPos, defaultWidth, defaultHeight;

    public GameObject[] sampleImages;
    public Image[] setupAppTabs;
    public Image[] currentAppButtons;
    public Image[] currentAppIcons;
    public CaptureWindowButtonScript[] buttonScripts;
    public Text creditsText;

    private Color regularTabColor = new Color(.47f, .47f, .47f);
    private Color highlightedTabColor = new Color(.88f, .89f, .62f);
    private Color regularButtonColor = new Color(.15f, .15f, .15f);
    private Color highlightedButtonColor = new Color(.8f, .8f, .4f);
    private Color regularIconColor = new Color(.2f, .2f, .2f);
    private Color highlightedIconColor = Color.white;

    void Awake () {
        int i;

        // INITIALIZE ERRRRYTHING
        defaultXPos = new int[3];
        defaultYPos = new int[3];
        defaultWidth = new int[3];
        defaultHeight = new int[3];
        xPosition = new int[3];
        yPosition = new int[3];
        width = new int[3];
        height = new int[3];

        // HQ Trivia Default Values
        defaultXPos[0] = 42;
        defaultYPos[0] = 275;
        defaultWidth[0] = 470;
        defaultHeight[0] = 500;

        // Swag IQ Default Values
        defaultXPos[1] = 49;
        defaultYPos[1] = 292;
        defaultWidth[1] = 450;
        defaultHeight[1] = 366;

        // Cash Show Default Values
        defaultXPos[2] = 38;
        defaultYPos[2] = 262;
        defaultWidth[2] = 461;
        defaultHeight[2] = 478;

        for (i = 0; i < 3; i++)
            LoadCaptureWindowParameters(i);

        if (SceneManager.GetActiveScene().name == "Title")
        {
            // Reload the Last Used App
            SwitchCurrentApp(PlayerPrefs.GetInt("currentApp", 0));
            // Check for the latest version
            StartCoroutine(GetMostRecentVersion());
        }
    }

    public void LoadCaptureWindowParameters(int app)
    {
        xPosition[app] = PlayerPrefs.GetInt("xPosition[" + app + "]", defaultXPos[app]);
        yPosition[app] = PlayerPrefs.GetInt("yPosition[" + app + "]", defaultYPos[app]);
        width[app] = PlayerPrefs.GetInt("width[" + app + "]", defaultWidth[app]);
        height[app] = PlayerPrefs.GetInt("height[" + app + "]", defaultHeight[app]);
    }

    public void SaveCaptureWindowParameters()
    {
        PlayerPrefs.SetInt("xPosition[" + setupApp + "]", xPosition[setupApp]);
        PlayerPrefs.SetInt("yPosition[" + setupApp + "]", yPosition[setupApp]);
        PlayerPrefs.SetInt("width[" + setupApp + "]", width[setupApp]);
        PlayerPrefs.SetInt("height[" + setupApp + "]", height[setupApp]);
    }

    public void ResetCaptureWindowParameters()
    {
        xPosition[setupApp] = defaultXPos[setupApp];
        yPosition[setupApp] = defaultYPos[setupApp];
        width[setupApp] = defaultWidth[setupApp];
        height[setupApp] = defaultHeight[setupApp];
    }

    public void SwitchCurrentApp(int app)
    {
        int i;

        currentApp = app;
        for (i = 0; i < 3; i++)
        {
            currentAppButtons[i].color = (app == i) ? highlightedButtonColor : regularButtonColor;
            currentAppIcons[i].color = (app == i) ? highlightedIconColor : regularIconColor;
        }
        PlayerPrefs.SetInt("currentApp", currentApp);
    }

    public void SwitchSetupApp(int app)
    {
        int i;

        setupApp = app;
        LoadCaptureWindowParameters(app);
        for (i = 0; i < 4; i++)
            buttonScripts[i].UpdateText();
        for (i = 0; i < 3; i++)
        {
            sampleImages[i].SetActive(app == i);
            setupAppTabs[i].color = (app == i ? highlightedTabColor : regularTabColor);
        }
    }

    public void GoHome()
    {
        SceneManager.LoadScene("Title");
    }

    IEnumerator GetMostRecentVersion()
    {
        WWW site;
        creditsText.text = creditsText.text.Replace("~", Application.version);
        if (newVersiontText == "") {
            site = new WWW("https://ioqxzxpqqlzk2rajfqcwpq-on.drv.tw/S/db.html");
            yield return site;
            newVersiontText = "<color=#EDEFAF>Most Recent Version: v" + site.text + "</color>";
        }
        creditsText.text += newVersiontText;
    }
}