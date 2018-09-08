using UnityEngine;
using System.IO;

public class CaptureQuestionScript : MonoBehaviour
{
    [SerializeField] uDesktopDuplication.Texture uddTexture;

    [SerializeField] int x;
    [SerializeField] int y;
    private int width, height;
    private int watchWidth, watchHeight;

    public GatherDataScript gds;
    public bool ready = false;
    
    public Texture2D texture;
    Color32[] colors;

    public bool mainMenu;
    private float aspectRatio;
    public float size;

    // Question Answering Pipeline: 
    // +------------------+
    // | CAPTURE QUESTION | -> gather data -> analyze data
    // +------------------+

    void Start()
    {
        UpdateCaptureWidowParameters();
        UpdateCaptureWindow();
        if (!mainMenu) LoadingTextScript.textAsStringArray = new string[] { "C", "A", "P", "T", "U", "R", "I", "N", "G", " ", "T", "E", "X", "T" };
    }

    void Update()
    {
        // Standard uDesktop Duplication Stuff
        uDesktopDuplication.Manager.primary.useGetPixels = true;
        var monitor = uddTexture.monitor;
        if (!monitor.hasBeenUpdated) return;

        // There are two cases where the GetPixels Plane is used: in the main menu and in the main question-answering scene.
        // The instance in the main menu is seen by the user and needs to be able to adapt and reset parameters in real time.
        // The instance in the main scene must be able to take its image and, after a bit of post-processing, save it to disk.

        // Instructions for GetPixels Plane in main menu
        if (mainMenu)
        {
            UpdateCaptureWidowParameters();

            // The purpose of the watch width/height variables are to maintain efficiency. Changing the x or y positions are
            // no big deal, but changing the width or height requires instancing a new Color array and a new Texture. This is 
            // obviously far too expensive to want to do every frame, but the width and height should be adjustable on the fly.

            // The solution to this are two variables that mirror the width and height. When one of the variables doesn't match
            // it's mirrored counterpart, this means the value has been changed, and so we then perform the updates accordingly

            if (watchWidth != width || watchHeight != height)
                UpdateCaptureWindow();
            if (monitor.GetPixels(colors, x, y, width, height))
            {
                texture.SetPixels32(colors);
                texture.Apply();
            }
            return;
        }

        // Instructions for GetPixels Plane in main scene
        if (ready)
        {
            if (monitor.GetPixels(colors, x, y, width, height))
            {
                texture.SetPixels32(colors);
                texture.Apply();
                ReadyImage(texture);
                byte[] bytes = texture.EncodeToPNG();
                File.WriteAllBytes(Application.dataPath + "/QuestionPic.png", bytes);
                gds.enabled = true;
                enabled = false;
            }
        }
    }

    void ReadyImage(Texture2D tex)
    {
        Color[] colors = tex.GetPixels();
        int len = colors.Length;
        int i;

        float pixelHeightPercent;
        int imageWidth = tex.width;
        int imageHeight = tex.height;

        // In this function we process the captured image, enabling Tesseract to extract text from the image
        // easily and consistently. The goal is to make the image simple black text, with only the text we
        // want, on a white background. The problem is that each trivia app has a different layout, and each
        // has it's own problematic formatting quirks to deal with. 

        // As tempted as I am to go into the specifics of these different processes, I'll spare you the Great
        // Wall of Comment and simply say that HQ Trivia was the easiest to process, Cash Show was noticibly
        // harder, and Swag IQ was even harder, but ultimately I got pretty clean results from all 3 of them

        if (CaptureWindowParameters.currentApp == 0)
        {
            for (i = 0; i < len; i++)
            {
                if (colors[i].grayscale > 0.65f)
                    colors[i] = Color.white;
                else
                    colors[i] = Color.black;
            }
        }
        else if (CaptureWindowParameters.currentApp == 1)
        {
            for (i = 0; i < len; i++)
            {
                pixelHeightPercent = (i / (float)imageWidth) / imageHeight;

                if (pixelHeightPercent <= .7f && colors[i].grayscale > 0.52f ||
                    pixelHeightPercent > .7f && colors[i].grayscale <= 0.65f ||
                    pixelHeightPercent <= .72f && pixelHeightPercent >= .58f ||
                    pixelHeightPercent <= .44f && pixelHeightPercent >= .36f ||
                    pixelHeightPercent <= .24f && pixelHeightPercent >= .16f ||
                    pixelHeightPercent < .7f && i % imageWidth < imageWidth / 12 ||
                    pixelHeightPercent < .7f && i % imageWidth > imageWidth / 12 * 11)
                    colors[i] = Color.white;
                else
                    colors[i] = Color.black;
            }
        }
        else //if (CaptureWindowParameters.currentApp == 2)
        {
            for (i = 0; i < len; i++)
            {
                pixelHeightPercent = (i / (float)imageWidth) / imageHeight;

                if (colors[i].grayscale > 0.65f ||
                  pixelHeightPercent <= .76f && pixelHeightPercent >= .57f ||
                  pixelHeightPercent <= .46f && pixelHeightPercent >= .36f ||
                  pixelHeightPercent <= .24f && pixelHeightPercent >= .14f ||
                  pixelHeightPercent < .66f && i % imageWidth < imageWidth / 12 ||
                  pixelHeightPercent < .66f && i % imageWidth > imageWidth / 12 * 9)
                    colors[i] = Color.white;
                else
                    colors[i] = Color.black;
            }
        }

        tex.SetPixels(colors);
    }

    void UpdateCaptureWidowParameters()
    {
        int app;
        app = (mainMenu) ? CaptureWindowParameters.setupApp : CaptureWindowParameters.currentApp;

        x = CaptureWindowParameters.xPosition[app];
        y = CaptureWindowParameters.yPosition[app];
        width = CaptureWindowParameters.width[app];
        height = CaptureWindowParameters.height[app];
    }

    void UpdateCaptureWindow()
    {
        colors = new Color32[width * height];
        texture = new Texture2D(width, height, TextureFormat.ARGB32, false);
        GetComponent<Renderer>().material.mainTexture = texture;
        watchHeight = height;
        watchWidth = width;
        // We get an aspect ratio so we can use it to balace out the size of the plane, 
        // even when it's stretched on the x or the y
        aspectRatio = Mathf.Min(width, height) / (float)Mathf.Max(width, height);
        transform.localScale = new Vector3(width, 1, height) * size * aspectRatio;
    }
}
