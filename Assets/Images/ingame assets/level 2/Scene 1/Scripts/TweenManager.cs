using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;

public class TweenManager : MonoBehaviour
{
    public bool speechTherapyCompleted = false;
    private Animator birdAnimator;
    private bool isRetryClicked = false;
    private GameObject buttonToActivate;
    private List<GameObject> spriteMasks = new List<GameObject>();
    private AudioSource feedbackAudiosource;
    private AudioClip dialouge3;
    public AudioMixer audioMixer;
    private const string musicVolumeParam = "MusicVolume";
    private const string AmbientVolumeParam = "AmbientVolume";
    private GameObject stCanvas;
    private GameObject SubtitlemanagerGameobject;
    private SubtitleManager subtitleManager;
    private void Start()
    {
        ST_AudioManager.Instance.OnPlaybackComplete += HandlePlaybackComplete;
        ST_AudioManager.Instance.OnRetryClicked += ResetTimer;
        feedbackAudiosource = GameObject.FindGameObjectWithTag("FeedbackAudio").GetComponent<AudioSource>();
        stCanvas = GameObject.FindGameObjectWithTag("STCanvas");
        dialouge3 = Resources.Load<AudioClip>("Audio/FeedbackAudio/Dialouge3");
        // Get the reference to the Animator component of the Bird game object
        GameObject bird = GameObject.FindGameObjectWithTag("Bird");
        SubtitlemanagerGameobject = GameObject.FindGameObjectWithTag("SubtitleManager");
        subtitleManager = SubtitlemanagerGameobject.GetComponent<SubtitleManager>();

        if (bird != null)
        {
            birdAnimator = bird.GetComponent<Animator>();
        }

        // Find the UICanvas and the Button within it
        GameObject uiCanvas = GameObject.FindGameObjectWithTag("UiPanel");
        if (uiCanvas != null)
        {
            buttonToActivate = uiCanvas.transform.Find("RetryButton").gameObject; // Replace "ButtonName" with the actual name of your button
        }

        if (buttonToActivate == null)
        {
            Debug.LogWarning("Button not found inside the UICanvas.");
        }
        GameObject[] masks = GameObject.FindGameObjectsWithTag("SpriteMask");
        spriteMasks.AddRange(masks);
        Debug.Log("Number of sprite masks found: " + spriteMasks.Count);
    }

    private void SetMusicVolume(float volume)
    {
        if (BGAudioManager_Final.Instance != null && BGAudioManager_Final.Instance.IsVolumeEnabled())
        {
            if (audioMixer != null)
            {
                bool result = audioMixer.SetFloat(musicVolumeParam, volume);
                if (!result)
                {
                    Debug.LogError($"Failed to set MusicVolume to {volume}. Is the parameter exposed?");
                }
            }
            else
            {
                Debug.LogError("AudioMixer is not assigned in the Inspector.");
            }
        }
    }

    private void SetAmbientVolume(float volume)
    {
        if (audioMixer != null)
        {
            bool result = audioMixer.SetFloat(AmbientVolumeParam, volume);
            if (!result)
            {
                Debug.LogError($"Failed to set MusicVolume to {volume}. Is the parameter exposed?");
            }
        }
        else
        {
            Debug.LogError("AudioMixer is not assigned in the Inspector.");
        }
    }
    private void OnDestroy()
    {
        ST_AudioManager.Instance.OnPlaybackComplete -= HandlePlaybackComplete;
        ST_AudioManager.Instance.OnRetryClicked -= ResetTimer;
    }

    private void HandlePlaybackComplete()
    {
        StartCoroutine(Timer(1f));
    }

    private void ResetTimer()
    {
        isRetryClicked = true;
        StopAllCoroutines();
    }

    private IEnumerator Timer(float time)
    {
        if (BGAudioManager_Final.Instance != null && BGAudioManager_Final.Instance.IsVolumeEnabled())
        {
            SetMusicVolume(-25f);
        }
        SetAmbientVolume(-10f);
        float counter = 0;
        isRetryClicked = false;

        while (counter < time)
        {
            if (isRetryClicked)
            {
                yield break;
            }

            counter += Time.deltaTime;
            yield return null;
        }

        // Reactivate the button
        if (buttonToActivate != null)
        {
            buttonToActivate.SetActive(true);
        }

        // Tween all children to scale 0
        foreach (Transform child in transform)
        {
            LeanTween.scale(child.gameObject, Vector3.zero, 0.5f).setEase(LeanTweenType.easeInOutBack);
        }

        // Wait for the children to finish tweening
        LeanTween.delayedCall(0.5f, () =>
        {
            foreach (Transform child in stCanvas.transform)
            {
                LeanTween.scale(child.gameObject, Vector3.zero, 0.5f);
            }

            LeanTween.scale(gameObject, Vector3.zero, 0.5f).setEase(LeanTweenType.easeInOutBack).setOnComplete(OnChildScaled);
                       
        });
    }

    private void OnChildScaled()
    {
       
        speechTherapyCompleted = true;
        // Set the animator parameters for the bird
        if (birdAnimator != null)
        {
            birdAnimator.SetBool("startWalking", true);
            birdAnimator.SetBool("resetPosition", false);
            feedbackAudiosource.clip = dialouge3;
            feedbackAudiosource.Play();
            subtitleManager.DisplaySubtitle("Ok! I will put the small toys on the Shelf.","Kiki",feedbackAudiosource.clip);
        }

        // Tween the parent to scale 0
        LeanTween.scale(gameObject, Vector3.zero, 0.5f).setEase(LeanTweenType.easeInOutBack).setOnComplete(() =>
        {
            Destroy(gameObject);
        });
    }
    public void SkipButton()
    {
        StartCoroutine(Timer(0f));
        speechTherapyCompleted = true;
    }
}
