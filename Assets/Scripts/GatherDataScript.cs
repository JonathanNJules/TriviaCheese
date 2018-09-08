using System.Collections;
using UnityEngine;
using TessLib;

public class GatherDataScript : MonoBehaviour {

    private TessLibClass tessLibClass = new TessLibClass();
    private Question gatheredQuestion = new Question();
    private AnalyzeDataScript ads;
    private HandleErrorScript hes;

    // Question Answering Pipeline: 
    //                     +-------------+
    // capture question -> | GATHER DATA | -> analyze data
    //                     +-------------+

    void Start () {
        // Every so often we stumble upon an annoying "sharing violation" error when trying to capture the question
        // I'm still looking for a solution to this pesky bug, but for the time being we can at least fail gracefully
        Application.logMessageReceived += HandleLog;
        hes = GetComponent<HandleErrorScript>();
        ads = GetComponent<AnalyzeDataScript>();
        LoadingTextScript.textAsStringArray = new string[] { "R", "E", "A", "D", "I", "N", "G", " ", "T", "E", "X", "T" };
        ExecuteTesseract();
    }

    void ExecuteTesseract()
    {
        string extractedText;
        int extractedTextLen;
        int currentStartI = 0;

        // Take in the Image and Extract Text from it
        extractedText = tessLibClass.OCRTranslate(Application.dataPath + "/QuestionPic.png", Application.dataPath + "/StreamingAssets/tesseract-master.1153", "eng");
        extractedText = extractedText.Replace('\n', '~');
        extractedTextLen = extractedText.Length;

        // Store Question
        currentStartI = extractedText.IndexOf("?") + 1;
        if (currentStartI == -1) Fail();
        gatheredQuestion.question = extractedText.Substring(0, currentStartI);
        gatheredQuestion.question = gatheredQuestion.question.Replace('~', ' ');
        gatheredQuestion.question = gatheredQuestion.question.Replace("&", "and");

        // Store 3 Answer Choices
        for(int i = 0; i < 3; i++)
        {
            while (currentStartI < extractedTextLen && extractedText[currentStartI] == '~') currentStartI++;
            // If we failed to fully parse the text, abandon ship
            if (currentStartI == extractedTextLen)
            {
                Fail();
                return;
            }
            gatheredQuestion.answers[i] = extractedText.Substring(currentStartI, extractedText.IndexOf('~', currentStartI + 1) - currentStartI);
            currentStartI = extractedText.IndexOf('~', currentStartI + 1);
            gatheredQuestion.answers[i] = gatheredQuestion.answers[i].Replace("&", "and");
        }

        // Check for Inverted Questions
        gatheredQuestion.invert = (gatheredQuestion.question.Contains("NOT") || gatheredQuestion.question.Contains("NEVER"));      

        // Send Result to Analyze Data Script
        ads.currentQuestion = gatheredQuestion;

        // Kick Off the Next Part, Searching for The Question
        StartCoroutine(ScrapeWebpages(gatheredQuestion));
    }

    IEnumerator ScrapeWebpages(Question q)
    {
        int i;
        WWW tempSite;

        string adjustedQuestion = q.question;

        LoadingTextScript.textAsStringArray = new string[] { "S", "E", "A", "R", "C", "H", "I", "N", "G"};

        // When searching for inverted questions, we want to search the question as if it's not inverted and pick the answer with the least
        // results. This is why the inverted keywords (NOT and NEVER) are removed here. This methodology certainly has room for improvement
        adjustedQuestion.Replace("NOT", "");
        adjustedQuestion.Replace("NEVER", "");

        // Search for the question (downloading the HTML with 75 results), WAIT until the download completes, THEN store the result in our 
        // AnalyzeDataScript for later use. Repeat this process for all three answer choices' appended searches (getting only 1 result)
        tempSite = new WWW("https://www.google.com/search?q=" + adjustedQuestion + "&num=75&filter=0&hl=en");
        yield return tempSite;
        ads.scrapedQuestionText = tempSite.text;
        for (i = 0; i < 3; i++)
        {
            tempSite = new WWW("https://www.google.com/search?q=" + adjustedQuestion + " \"" + q.answers[i] + "\"" + "&num=1&hl=en");
            yield return tempSite;
            ads.scrapedAnswerTexts[i] = tempSite.text;
        }

        // Now that we have collected all the data we need for analysis, kick off AnalyzeDataScript
        ads.enabled = true;
        Application.logMessageReceived -= HandleLog;
    }

    // If we fail to parse the text extracted by Tesseract (most likely due to a bad capture window), we want to cut the process 
    // short here and fail gracefully, telling the user what happened, and allowing them to try the process again with ease. 
    // So if that's the case, disable our script, and let HandleErrorScript do it's job.
    void Fail()
    {
        GetComponent<HandleErrorScript>().errorType = 1;
        enabled = false;
    }

    // I'd much rather catch the problem before it happens rather than let the error occur and react to it, and for the most part
    // i've been good about doing the former and not the latter. For this case, however, I haven't bothered finding the percise
    // source of the sharing violation, and since it's an uncommon occurence anyways, i'll make an exception here. Whatever.
    void HandleLog(string logString, string stackTrace, LogType type)
    {
        if (type == LogType.Exception && logString.Contains("Sharing violation"))
        {
            hes.errorType = 3;
            if(this != null)
                enabled = false;
        }
    }
}

//////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////

[System.Serializable]
public class Question
{
    public string question;
    public string[] answers;
    public int correctA;
    public bool invert;

    public Question()
    {
        answers = new string[3];
    }
}
