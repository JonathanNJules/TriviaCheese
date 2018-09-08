using System;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityStandardAssets.ImageEffects;
using System.Threading;

public class AnalyzeDataScript : MonoBehaviour
{
    // Supplementary Information for Analyzing Data
    private string[] articles = {"of", "and", "the", "a", "an", "you", "your", "my", "his", "her", "at", "this", "that", "in", "by", "I",
            "1", "2", "3", "4", "5", "6", "7", "8", "9",
            "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z"};
    private char[] delimiters = {' ', '-', '/', '+', ','};
    public TextAsset popularWordListText;
    string popularWords;

    // Data Given by GatherDataScript
    public Question currentQuestion;
    public string scrapedQuestionText;
    public string[] scrapedAnswerTexts = new string[3];

    // Raw Numbers for Each Method
    float[][] methodNumbers = new float[3][];
    float[][] sortedMethodNumbers = new float[3][];
    float[] methodMax = new float[3];
    int[] methodIndexes = new int[3];

    // Variables for Final Verdict
    int guess;
    bool usedSuggestedAnswer = false;
    float answerConfidence;

    // Transition UI Elements
    public Transform loadingAnimation;
    private Material loadingAnimationMat;
    private float loadingAnimRef1;
    private Vector3 loadingAnimRef2;
    private BlurOptimized blurScript;

    // Display UI Elements
    public Text questionText;
    public Text[] answerTexts;
    public Image[] answerConfidenceGraphic;
    public Image[] visibleRawNumberGraphics;
    private Image[][] rawNumberGraphics = new Image[3][];
    public Text[] visibleRawNumberText;
    private Text[][] rawNumberText = new Text[3][];
    public Text[] rawNumberTitleText;
    public Text answerConfidenceText;
    public Image[] verdictGraphic;
    public Text verdictText;
    private float[] currentAnswerConfidence = new float[3];
    private float[] answerConfidenceRef = new float[3];
    public float displayAnimationInterval;
    private float animationTimer = 0;
    private float[] verdictRef = new float[3];
    private bool transitionToDisplay, initializedDisplay, readyToDisplay;
    private float[][] displayMethodNumbers = new float[3][];
    private float[][] currentDisplayMethodNumbers = new float[3][];
    private float[][] rawNumbersRef = new float[3][];
    private string[] methodTitles = new string[] {"W E I G H T E D   K E Y P H R A S E   M A T C H",
                                                  "S P L I T   K E Y W O R D   M A T C H",
                                                  "A P P E N D E D   S E A R C H   Q U A N T I T Y"};

    // Question Answering Pipeline: 
    //                                    +--------------+
    // capture question -> gather data -> | ANALYZE DATA |
    //                                    +--------------+

    void Start()
    {
        int i;

        LoadingTextScript.textAsStringArray = new string[] { "A", "N", "A", "L", "Y", "Z", "I", "N", "G" };

        // Initialize Eeeeeverything
        for (i = 0; i < 3; i++)
        {
            methodNumbers[i] = new float[3];
            sortedMethodNumbers[i] = new float[3];
            displayMethodNumbers[i] = new float[3];
            currentDisplayMethodNumbers[i] = new float[3];
            rawNumbersRef[i] = new float[3];
            rawNumberGraphics[i] = new Image[3];
            rawNumberText[i] = new Text[3];
        }
        popularWords = popularWordListText.text;

        loadingAnimationMat = loadingAnimation.GetComponent<Renderer>().material;
        blurScript = GetComponent<BlurOptimized>();

        // The two lines of code below (lines 100-101) is the scrapped multithreading code.
        // All you would have to do is uncomment lines 100-101 and comment out line 102 and the code would be multithreaded.
        // However, while this leads to smoother visuals and less studdering at runtime, it actually executes ~2 seconds slower than
        // simply putting the burden on the main thread, so it's disabled here. The same goes when multithreading GatherDataSctipt.
        // The code remains here for a case in point. If you want to see for yourself, have a go at it! "ANALYZING" will be smooth!

        //Thread myThread = new Thread(GenerateAnswerData);
        //myThread.Start();
        GenerateAnswerData();
    }

    void Update()
    {
        int i, j;

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Transition from the loading results screen to the displaying results screen
        if (transitionToDisplay)
        {
            loadingAnimationMat.color = new Color(1, 1, 1, Mathf.SmoothDamp(loadingAnimationMat.color.a, 0, ref loadingAnimRef1, .15f));
            loadingAnimation.localScale = Vector3.SmoothDamp(loadingAnimation.localScale, Vector3.one * 10, ref loadingAnimRef2, .15f);
            if (!readyToDisplay && loadingAnimationMat.color.a < 0.1f)
                readyToDisplay = true;
            if (blurScript.enabled)
                blurScript.blurSize -= 10 * Time.deltaTime;
            if (blurScript.blurSize <= 0)
                blurScript.enabled = false;
        }
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Display the actual results
        if (readyToDisplay)
        {
            // One Time Stuff
            if (initializedDisplay == false)
            {
                // Final Verdict text
                verdictText.text = FinalVerdictString(currentQuestion.answers[guess], answerConfidence, usedSuggestedAnswer);
                // Question/Answers Display
                questionText.text = currentQuestion.question;
                for (i = 0; i < 3; i++)
                    answerTexts[i].text = currentQuestion.answers[i];
                // Raw Numbers Text
                for (i = 0; i < 3; i++)
                    rawNumberTitleText[i].text = methodTitles[methodIndexes[i]];

                initializedDisplay = true;
            }

            // Move the animation timer
            animationTimer += Time.deltaTime;

            // Raw Data Display
            for (i = 0; i < 3; i++)
            {
                for (j = 0; j < 3; j++)
                {
                    // Raw Numbers Graphic
                    currentDisplayMethodNumbers[i][j] = Mathf.SmoothDamp(currentDisplayMethodNumbers[i][j], displayMethodNumbers[i][j], ref rawNumbersRef[i][j], 0.15f);
                    rawNumberGraphics[i][j].fillAmount = currentDisplayMethodNumbers[i][j] / methodMax[i];
                    rawNumberText[i][j].text = displayMethodNumbers[i][j].ToString();
                }
            }

            // Answer Confidence Display
            for (i = 0; i < 3; i++)
            {
                if (animationTimer > displayAnimationInterval * i)
                    currentAnswerConfidence[i] = Mathf.SmoothDamp(currentAnswerConfidence[i], answerConfidence, ref answerConfidenceRef[i], 0.15f);
                if (currentAnswerConfidence[i] != answerConfidence && answerConfidence - currentAnswerConfidence[i] < 0.2f)
                    currentAnswerConfidence[i] = answerConfidence;
                answerConfidenceGraphic[i].fillAmount = currentAnswerConfidence[i] / 100;
            }
            answerConfidenceText.text = (int)currentAnswerConfidence[0] + "%";

            // Final Verdict Graphic
            for(i = 0; i < 3; i++)
            {
                if (animationTimer > displayAnimationInterval * i)
                    verdictGraphic[i].fillAmount = Mathf.SmoothDamp(verdictGraphic[i].fillAmount, 1, ref verdictRef[i], 0.15f);
            }
        }
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    }

    void GenerateAnswerData()
    {
        int i, j, k;
        float answerSplitWordTotal;
        int articleCount;
        bool foundArticle;
        string[] answerSplitWords;
        int methodToUse;
        float[] methodConfidence = new float[3];
        float guessNum;
        string suggestedAnswer;
        int suggestedAnswerGuess;

        // There's a small "gotcha" with this entire process that could ruin everything:
        // Google may block this program from scraping it's data. This is more or less
        // guaranteed to happen if you use the program heavily in a short span of time.
        // If/when this situation arises, we want to fail gracefully.
        if (WasBlocked())
        {
            Fail();
            return;
        }

        // Suggested Answer
        suggestedAnswerGuess = -1;
        suggestedAnswer = SearchForSuggestedAnswer(scrapedQuestionText);
        if (suggestedAnswer != "Not Available")
        {
            for (i = 0; i < 3; i++)
            {
                if (String.Equals(suggestedAnswer, currentQuestion.answers[i], StringComparison.OrdinalIgnoreCase))
                    suggestedAnswerGuess = i;
            }
        }

        // For All 3 Answer Choices, Get...
        for (i = 0; i < 3; i++)
        {
            // Weighted Keyphrase Match
            methodNumbers[0][i] = WordMatchCount(scrapedQuestionText, currentQuestion.answers[i], 2);
            sortedMethodNumbers[0][i] = methodNumbers[0][i];

            // Split Keyword Match
            answerSplitWordTotal = articleCount = 0;
            answerSplitWords = currentQuestion.answers[i].Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
            for (j = 0; j < answerSplitWords.Length; j++)
            {
                foundArticle = false;
                for (k = 0; k < articles.Length; k++)
                {
                    if (String.Equals(answerSplitWords[j], articles[k], StringComparison.OrdinalIgnoreCase))
                    {
                        foundArticle = true;
                        articleCount++;
                        break;
                    }
                }
                if (!foundArticle)
                    answerSplitWordTotal += WordMatchCount(scrapedQuestionText, answerSplitWords[j].Replace(" ", ""), 0);
            }
            answerSplitWordTotal = (articleCount != answerSplitWords.Length) ? answerSplitWordTotal / (answerSplitWords.Length - articleCount) : 0;

            methodNumbers[1][i] = answerSplitWordTotal;
            sortedMethodNumbers[1][i] = methodNumbers[1][i];

            // Appended Search Quantity
            methodNumbers[2][i] = GetAnswerQuantity(scrapedAnswerTexts[i]);
            sortedMethodNumbers[2][i] = methodNumbers[2][i];
        }

        for (i = 0; i < 3; i++)
            Array.Sort(sortedMethodNumbers[i]);

        // Determine Final Guess
        // Start by trying method 1
        methodToUse = 0;

        // If there is hardly a difference between the guess and the runnerup, switch to method 2
        if (!currentQuestion.invert && sortedMethodNumbers[0][2] - sortedMethodNumbers[0][1] <= 1 ||
             currentQuestion.invert && sortedMethodNumbers[0][1] - sortedMethodNumbers[0][0] <= 1)
            methodToUse = 1;
        else
        {
            // If we're still using method 1, and there is a certain number of words (on average) in the answers, switch to method 2
            if (((currentQuestion.answers[0].Count(c => c == ' ') + 1) +
                  (currentQuestion.answers[1].Count(c => c == ' ') + 1) +
                  (currentQuestion.answers[2].Count(c => c == ' ') + 1)) / 3.0f > 2.7f)
                methodToUse = 1;
        }

        for(i = 0; i < 3; i++)
            methodConfidence[i] = AnswerConfidence(methodNumbers[i], i, currentQuestion);

        // If you're trying method 1, and method 3 is VERY confident, switch to method 3
        if (methodToUse == 0)
        {
            if (methodConfidence[0] < methodConfidence[2] && methodConfidence[2] >= 80)
                methodToUse = 2;
        }

        // If you're trying method 2, and method 3 is VERY confident, or method 2 couldn't find anything, switch to method 3
        else if (methodToUse == 1)
        {
            if (methodConfidence[1] < methodConfidence[2] && methodConfidence[2] >= 80 || 
                !currentQuestion.invert && sortedMethodNumbers[1][2] == 0 ||
                 currentQuestion.invert && sortedMethodNumbers[1][1] == 0)
                methodToUse = 2;
        }

        if (suggestedAnswerGuess != -1)
        {
            guess = suggestedAnswerGuess;
            usedSuggestedAnswer = true;
        }
        else
        {
            i = (!currentQuestion.invert) ? 2 : 0;
            guessNum = sortedMethodNumbers[methodToUse][i];
            for (i = 0; i < 3; i++)
            {
                if (Mathf.Abs(guessNum - methodNumbers[methodToUse][i]) < 0.01f)
                    guess = i;
            }
        }

        // Set final answer confidence
        answerConfidence = usedSuggestedAnswer ? 99 : methodConfidence[methodToUse];

        // Determine Order to Display Methods
        methodIndexes[0] = methodToUse;
        if(methodIndexes[0] == 0)
        {
            if (sortedMethodNumbers[1][2] > 0)
            {
                methodIndexes[1] = 1;
                methodIndexes[2] = 2;
            }
            else
            {
                methodIndexes[1] = 2;
                methodIndexes[2] = 1;
            }
        }
        else if (methodIndexes[0] == 1)
        {
            if (sortedMethodNumbers[0][2] > 0)
            {
                methodIndexes[1] = 0;
                methodIndexes[2] = 2;
            }
            else
            {
                methodIndexes[1] = 2;
                methodIndexes[2] = 0;
            }
        }
        else // (methodIndexes[0] == 2)
        {
            if (sortedMethodNumbers[0][2] > 0)
            {
                methodIndexes[1] = 0;
                methodIndexes[2] = 1;
            }
            else
            {
                methodIndexes[1] = 1;
                methodIndexes[2] = 0;
            }
        }

        // Finally, take care of all the setup and display the results
        for (i = 0; i < 3; i++)
        {
            displayMethodNumbers[i] = methodNumbers[methodIndexes[i]];
            methodMax[i] = sortedMethodNumbers[methodIndexes[i]][2];
            for (j = 0; j < 3; j++)
            {
                rawNumberGraphics[i][j] = visibleRawNumberGraphics[i * 3 + j];
                rawNumberText[i][j] = visibleRawNumberText[i * 3 + j];
            }
        }
        transitionToDisplay = true;
    }

    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    int WordMatchCount(string corpus, string key, int weightMode)
    {
        // In this function we count the number of times the answer "key" pops up in the scraped Google text "corpus".
        // We also implement a techique I call a "Top 5 Weighted" search, where, in the Google results, the 1st, 2nd, 
        // and 3rd results are actually counted as three matches, the 4th and 5th results are counted as two matches,
        // and every match after that is counted as a single match as usual. 

        // I did a comparison on my sample of 600 questions with the count being Unweighted, Top 3 Weighted (where the
        // first 3 results are worth two matches, and everything else is one), Top 5 Weighted, and Cascade Weighted
        // (where ALL results are weighted based on how early in the Google results they appear). Top 5 Weighted
        // yielded the best results, so that's the technique I use for methods 1 and 2 here.

        // Please note that while the results in method 1 AND 2 are Top 5 Weighted, I only put "weighted" in the name of 
        // method 1 simply becuase the name of method 2 would be too long if it had weighted included in it's name.

        int foundIndex = 0;
        int foundCount = 0;
        int startIndex = 0;

        int[] resultDividers = new int[6];
        int currentD = 0, i = 0;
        int currentMatchWeight = 1;

        int len = key.Length;
        int corLen = corpus.Length;

        // In Google's HTML, "&nbsp;..." usually denotes the beginning of the next search result. This means a match in the 5th
        // result would appear somewhere after the 5th "&nbsp;..." and before the 6th "&nbsp;...". Consequently, we need to grab
        // the indices of the first SIX "&nbsp;..." strings to determine whether or not a match is in the first five results.
        currentD = corpus.IndexOf("&nbsp;...", currentD, StringComparison.OrdinalIgnoreCase);
        while (currentD != -1 && i < 6)
        {
            resultDividers[i] = currentD;
            currentD++; i++;
            currentD = corpus.IndexOf("&nbsp;...", currentD, StringComparison.OrdinalIgnoreCase);
        }
        // If we didn't have enough results to fill the array, put the remaining dividers at the end of the corpus
        for( ; i < 6; i++)
            resultDividers[i] = corLen;

        foundIndex = corpus.IndexOf(key, startIndex, StringComparison.OrdinalIgnoreCase);
        while (foundIndex != -1)
        {
            if (foundIndex < resultDividers[3])
                currentMatchWeight = 3;
            else if (foundIndex < resultDividers[5])
                currentMatchWeight = 2;
            else
                currentMatchWeight = 1;
            foundCount += currentMatchWeight;

            // If there is an alphanumeric character directly before or after the match, then it probably isn't a legitimate match
            // found in the actual search results (but is probably a lucky HTML code match), so remove it from our running total
            if (Char.IsLetterOrDigit(corpus[foundIndex - 1]) || Char.IsLetterOrDigit(corpus[foundIndex + len]))
                foundCount -= currentMatchWeight;

            startIndex = foundIndex + 1;
            foundIndex = corpus.IndexOf(key, startIndex, StringComparison.OrdinalIgnoreCase);
        }

        return foundCount;
    }

    string SearchForSuggestedAnswer(string corpus)
    {
        // Sometimes you Google a question, and the answer appears rather brazenly at the top of the page

        // For an example, search "what is the capital of Canada" or "who is Tom Brady's wife" in Google

        // While trivia games (HQ in particular) are good about avoiding this type of question, it does pop 
        // up once every blue moon. In our sample of 600 questions, it occured 3 times. That's an appearance
        // rate of 0.5%! It also appeared once slighly after making this bot in a savage question (that the
        // bot actually got right!). When this does happen, I call it a "suggested answer"

        int suggestedIndex = 0;

        suggestedIndex = corpus.IndexOf("\"mrH1y\">");

        if (suggestedIndex == -1)
        {
            suggestedIndex = corpus.IndexOf("Mkc1Te\">");
            if (suggestedIndex == -1)
                return "Not Available";
        }

        suggestedIndex += 8;

        return corpus.Substring(suggestedIndex, corpus.IndexOf("<", suggestedIndex) - suggestedIndex);
    }

    long GetAnswerQuantity(string corpus)
    {
        // Google always provides us with the number of results with every seach we perform. In all honesty the results can be a bit
        // fickle, and are easily diluded with a lack of balance in the answer choices' popularity. Despite this, it is a tactic we 
        // can virtually always use to get some sort of answer, so as a last resort (method 3) it will serve us well if used right

        int startIndex = 0, i = 0;
        char[] resultCharArray = new char[1024];
        string resultString;

        startIndex = corpus.IndexOf("\"resultStats\"");

        if (startIndex == -1) return 0;

        // In the rare but totally possible case that we get exactly one result, we have to start at a slightly different index
        startIndex += (corpus[startIndex + 14] == 'A') ? 20 : 14;

        while (corpus[startIndex] != ' ')
        {
            resultCharArray[i] = corpus[startIndex];
            startIndex++; i++;
        }
        resultString = new string(resultCharArray);

        resultString = resultString.Replace(",", "");
        try { return Convert.ToInt64(resultString); }
        catch { return -1; }
    }

    bool IsRelativeQuestion(string question)
    {
        // My research data has suggested that the chances of getting a question right plummet about 20% when the
        // question is a relative question, i.e. it compares the answer choices to each other

        // Here is an example: Which of these sports balls is the heaviest? | Baseball | Softball | Racquetball |

        // Although this question behavior is hard to counter, it demonstrates fairly obvious patterns, and we try
        // to pick out these patterns here to assist us in our answer confidence prediction

        int wotIndex;

        wotIndex = question.IndexOf("which of these", 0, StringComparison.OrdinalIgnoreCase);

        if (wotIndex == -1) return false;

        if (question.IndexOf("most", wotIndex, StringComparison.OrdinalIgnoreCase) != -1 ||
            question.IndexOf("est ", wotIndex, StringComparison.OrdinalIgnoreCase) != -1 ||
            question.IndexOf("est?", wotIndex, StringComparison.OrdinalIgnoreCase) != -1 ||
            question.IndexOf("first", wotIndex, StringComparison.OrdinalIgnoreCase) != -1)
        {
            // Avoid "est" false positives by ensuring it didn't come from the words best and test
            if (question.IndexOf("best", wotIndex, StringComparison.OrdinalIgnoreCase) == -1 && 
                question.IndexOf("test", wotIndex, StringComparison.OrdinalIgnoreCase) == -1)
                return true;
        }

        return false;
    }

    int WordPopularity(string word)
    {
        // We search our list of popular words, graciously provided by Josh Kaufman (the author, not the artist), which was, in
        // his words, "determined by n-gram frequency analysis of the Google's Trillion Word Corpus." To learn more, check out
        // https://github.com/first20hours/google-10000-english

        // This list contains of the top 10,000 words in the english language ordered by popularity. Each word gets it's own line,
        // and the file has been modified with |n| before each word, with n being the line number, to make it easier to return the
        // popularity of a word. With this system, popular words have a smaller n, and infrequent words have a larger n.

        // All words not found in the list are simply given a popularity rating of 10,001

        int foundIndex;
        int ratingNumStart, ratingNumEnd;

        foundIndex = popularWords.IndexOf(" " + word + "\r", 0, StringComparison.OrdinalIgnoreCase);

        if (foundIndex == -1)
            return 10001;
        foundIndex -= 3;
        ratingNumEnd = foundIndex + 2;
        while (popularWords[foundIndex] != '|')
            foundIndex--;
        ratingNumStart = foundIndex + 1;

        return Convert.ToInt32(popularWords.Substring(ratingNumStart, ratingNumEnd - ratingNumStart));
    }

    float AnswerConfidence(float[] nums, int method, Question q)
    {
        // Use the Research Data to Estimate a Level of Confidence in the Answer

        double x, y;
        bool relativeQuestion = false;
        int highestWordCount = 1;
        int runnerupMinusAnswerPop = 0;
        int i, answerIndex = 0, runnerupIndex = 0;

        // Preliminary Data Needed for Calculations
        relativeQuestion = IsRelativeQuestion(q.question);

        // These last two peices of preliminary data aren't needed for calculations using method 3 (appended
        // search quantity), so if we are using method 3 we can save time by simply not calculating them
        if (method != 2)
        {
            for (i = 0; i < 3; i++)
            {
                if (nums[i] == Mathf.Max(nums))
                    answerIndex = i;
                if (nums[i] <= nums[(i + 1) % 3] && nums[i] >= nums[(i + 2) % 3] ||
                    nums[i] >= nums[(i + 1) % 3] && nums[i] <= nums[(i + 2) % 3])
                    runnerupIndex = i;
            }
            runnerupMinusAnswerPop = Mathf.Abs(WordPopularity(q.answers[answerIndex]) - WordPopularity(q.answers[runnerupIndex]));

            highestWordCount = Mathf.Max(q.answers[0].Count(c => c == ' ') + 1,
                                          q.answers[1].Count(c => c == ' ') + 1,
                                          q.answers[2].Count(c => c == ' ') + 1);
        }

        // For methods 1/2, x should be the second largest answer minus the largest answer
        // For method 3, x should be the amount for the runnerup

        if (method != 2)
        {
            x = nums[answerIndex] - nums[runnerupIndex];
            y = 94.47588 + (49.99422 - 94.47588) / (1d + Math.Pow( (x / 4.906658), 1.078272) );
            if (relativeQuestion) y *= 0.8;
            if (highestWordCount == 2) y *= .95;
            if (highestWordCount >= 3) y *= .85;
            if (runnerupMinusAnswerPop < 2500) y *= .88;
        }
        else //if (method == 2)
        {
            x = nums[runnerupIndex];
            y = 19.25425 + (90.28311 - 19.25425) / (1d + Math.Pow( (x / 23610.98) , 0.3365215) );
            if (relativeQuestion) y *= .85f;
        }

        return (float)y;
    }

    string FinalVerdictString(string answer, float confidence, bool suggestedAswer)
    {
        // Relay a Random Final Verdict Message Depending on the Confidence

        string[] fMessages = new string[] 
        {
            "Perhaps ~, but that's really just a shot in the dark!" ,
            "I don't know really, maybe pick ~"                            ,
            "Uuuum, ~?"                                            ,
            "~? Yeah sure, let's go with that I guess"             ,
            "I'll tell you ~, but i'm honestly not very confident about that answer"
        };
        string[] dMessages = new string[]
        {
            "It's honestly tough to say. Try ~"              ,
            "Perhaps ~...I think..."                         ,
            "I suppose ~, but I could be wrong"              ,
            "That's a tough one for me, but i'll go with ~" ,
            "Can't say with certainty, but my best guess is ~"
        };
        string[] cMessages = new string[]
        {
            "The answer is probably ~"            ,
            "There's a good chance it's ~"        ,
            "I'm fairly sure it's ~"              ,
            "I'd go with ~"                       ,
            "I would say ~, and I have a decent amount of confidence in that answer"
        };
        string[] bMessages = new string[]
        {
            "It looks like the answer is ~" ,
            "Go with ~"                     ,
            "I'm pretty sure it's ~"        ,
            "I'm pretty confident in ~"     ,
            "~ seems like the best answer"
        };
        string[] aMessages = new string[]
        {
            "I'm positive the answer is ~"         ,
            "It's ~. For sure"                     ,
            "I'm very confident the answer is ~!"  ,
            "There's a VERY high chance it's ~"    ,
            "The answer is ~, and i'm very confident about that"
        };
        string[] messagesToUse;
        string finalMessage;

        if (confidence >= 90)
            messagesToUse = aMessages;
        else if (confidence >= 80)
            messagesToUse = bMessages;
        else if (confidence >= 70)
            messagesToUse = cMessages;
        else if (confidence >= 60)
            messagesToUse = dMessages;
        else //(confidence < 60)
            messagesToUse = fMessages;

        if (suggestedAswer)
            finalMessage = "<i><b>SUGGESTED ANSWER!:</b></i> ~";
        else
            finalMessage = messagesToUse[UnityEngine.Random.Range(0, messagesToUse.Length - 1)];

        finalMessage = finalMessage.Replace("~", "<color=#FF5B4E><b>" + answer + "</b></color>");

        return finalMessage;
    }

    bool WasBlocked()
    {
        // One important thing to note is that while we're doing this, the user is still
        // waiting on our results! This is why making this check as quickly as possible is
        // so important: we don't want to slow down the entire process over a fringe case.

        // We have to search for a specific string to detect whether or not they blocked us.
        // We scrape Google in the order Question-> Appended Answer 0-> 1-> 2. Also if they
        // block one search, they block every subsequent seach as well, so the quickest way
        // to know with certainty that we've been blocked is to check just Appended Answer 2

        // Also, when scraping the appended answers, we only grabbed one search result. This
        // still serves our purpose the same (all we do is get the result count at the top),
        // it downloads faster, and it helps us with this function, since, if we didn't get 
        // blocked, we can fly through the shorter HTML and move on as quickly as possible.

        return (scrapedAnswerTexts[2].Contains("Our systems have detected unusual traffic from your computer network"));
    }

    void Fail()
    {
        GetComponent<HandleErrorScript>().errorType = 2;
        enabled = false;
    }
}
