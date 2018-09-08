using System;
using UnityEngine;
using UnityEngine.UI;

public class LoadingTextScript : MonoBehaviour {

    // This script controls the text below the loading animation to let the user know what step of the answering process
    // is currently underway. Other scripts, when beginning a new process, will change this text (from their script) to 
    // reflect this to the user. It's also great for debugging; if the program infinitely hangs on a particular line of 
    // text, then we know the problem lies with whatever script/function gave the LoadingTextScript the text that hung.

    // Also, we procedurally animate the text with a neat scrolling light effect

    private Text t;
    private int lightCharStart;
    private float currentTimer;
    public float lightIntervals;

    // We have to start somewhere, so the starting text is simply "loading", but this text
    // quickly gets replaced with whatever action the bot is currently performing. We make
    // an array of strings, instead of one string, to make the animation process easier
    public static string[] textAsStringArray = new string[] { "L", "O", "A", "D", "I", "N", "G" };


    void Start () {
        t = GetComponent<Text>();
        lightCharStart = 2;
	}
	
	void Update () {

        // Don't update the colors every frame; update the timer, and when the timer hits the
        // threshold, signifying its time to change the colors, only then do we update colors
        currentTimer += Time.deltaTime;
        if(currentTimer > lightIntervals)
        {
            UpdateColors();
            lightCharStart++;
            currentTimer = 0;
        }
	}

    void UpdateColors()
    {
        int i;
        int len = textAsStringArray.Length;
        string colorString;

        t.text = "";

        // lightCharStart represents which letter in our string is currently "lit up".
        // There are 4 colors in the string: the regular, unlit color that the majority of characters adopt, the fully lit color
        // that a single character adopts, and the two transition colors taken on by the two characters behind the lit character

        for (i = 0; i < len; i++)
        {
            if      (i == (lightCharStart - 0) % len)
                colorString = "EADCB8";
            else if (i == (lightCharStart - 1) % len)
                colorString = "D2C5A3";
            else if (i == (lightCharStart - 2) % len)
                colorString = "E3C988";
            else
                colorString = "A49262";

            t.text += String.Format("<color=#{0}>{1}</color>", colorString, textAsStringArray[i]);
            if (i != len - 1) t.text += " ";
        }
    }
}