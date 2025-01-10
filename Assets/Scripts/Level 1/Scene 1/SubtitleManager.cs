using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class SubtitleManager : MonoBehaviour
{
    public Image jojoBanner;
    public Image kikiBanner;
    public TextMeshProUGUI subtitleText;
    public float fadeDuration; // Fade duration for both fade-in and fade-out

    private void Start()
    {
        // Ensure banners and subtitle text are initially hidden
        if (jojoBanner != null) jojoBanner.color = new Color(1, 1, 1, 0);
        if (kikiBanner != null) kikiBanner.color = new Color(1, 1, 1, 0);
        if (subtitleText != null) subtitleText.gameObject.SetActive(false);
    }

    public void DisplaySubtitle(string fullText, string dialogueType, AudioClip audioClip)
    {
        StartCoroutine(ShowSubtitle(fullText, dialogueType, audioClip));
    }


    private IEnumerator ShowSubtitle(string fullText, string dialogueType, AudioClip audioClip)
    {
        subtitleText.gameObject.SetActive(true);
        Image activeBanner = null;

        // Determine which banner to fade in based on the dialogue type
        if (dialogueType == "JoJo")
        {
            activeBanner = jojoBanner;
        }
        else if (dialogueType == "Kiki")
        {
            activeBanner = kikiBanner;
        }

        if (activeBanner != null)
        {
            // Fade in the banner
            for (float t = 0; t <= fadeDuration; t += Time.deltaTime)
            {
                float alpha = t / fadeDuration;
                activeBanner.color = new Color(1, 1, 1, alpha);
                yield return null;
            }
            activeBanner.color = new Color(1, 1, 1, 1);
        }

        // Calculate display time for subtitles
        float totalDuration = audioClip != null ? audioClip.length : 3f; // Default to 3 seconds if no audio
        float subtitleDisplayTime = totalDuration - (2 * fadeDuration); // Remaining time after accounting for fade durations
        subtitleDisplayTime = Mathf.Max(0, subtitleDisplayTime); // Ensure non-negative time

        string[] sentences = SplitTextIntoSentences(fullText);
        float durationPerSentence = subtitleDisplayTime / sentences.Length;

        foreach (string sentence in sentences)
        {
            subtitleText.text = sentence;

            // Wait for the calculated duration per sentence
            yield return new WaitForSeconds(durationPerSentence);
        }

        // Hide the subtitle text
        subtitleText.text = "";
        subtitleText.gameObject.SetActive(false);

        // Fade out the banner
        if (activeBanner != null)
        {
            for (float t = 0; t <= fadeDuration; t += Time.deltaTime)
            {
                float alpha = 1 - (t / fadeDuration);
                activeBanner.color = new Color(1, 1, 1, alpha);
                yield return null;
            }
            activeBanner.color = new Color(1, 1, 1, 0);
        }
    }

    private string[] SplitTextIntoSentences(string text)
    {
        // Split the text by sentences or character limit
        const int maxCharsPerSentence = 60; // Adjust based on your UI
        if (text.Length <= maxCharsPerSentence)
        {
            return new string[] { text };
        }

        // Break the text into smaller parts
        string[] words = text.Split(' ');
        string currentSentence = "";
        List<string> sentences = new List<string>();

        foreach (string word in words)
        {
            if (currentSentence.Length + word.Length + 1 <= maxCharsPerSentence)
            {
                currentSentence += (currentSentence.Length > 0 ? " " : "") + word;
            }
            else
            {
                sentences.Add(currentSentence);
                currentSentence = word;
            }
        }

        if (!string.IsNullOrEmpty(currentSentence))
        {
            sentences.Add(currentSentence);
        }

        return sentences.ToArray();
    }
}
