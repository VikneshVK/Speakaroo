using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class Lvl5Sc12TweenManager : MonoBehaviour
{
    public bool speechTherapyCompleted = false;
    private Animator BoyAnimator;
    private bool isRetryClicked = false;
    private GameObject buttonToActivate;
    private LVL5Sc12Jojocontroller jojoController;
    /*private List<GameObject> spriteMasks = new List<GameObject>();*/
    public AudioMixer audioMixer;
    private const string musicVolumeParam = "MusicVolume";

    private void Start()
    {
        ST_AudioManager.Instance.OnPlaybackComplete += HandlePlaybackComplete;
        ST_AudioManager.Instance.OnRetryClicked += ResetTimer;

        // Get the reference to the Animator component of the Bird game object
        GameObject Boy = GameObject.FindGameObjectWithTag("Player");
        if (Boy != null)
        {
            BoyAnimator = Boy.GetComponent<Animator>();
            jojoController = Boy.GetComponent<LVL5Sc12Jojocontroller>();
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
        /*GameObject[] masks = GameObject.FindGameObjectsWithTag("SpriteMask");
        spriteMasks.AddRange(masks);
        Debug.Log("Number of sprite masks found: " + spriteMasks.Count);*/
    }

    private void SetMusicVolume(float volume)
    {
        if (audioMixer != null)
        {
            bool result = audioMixer.SetFloat(musicVolumeParam, volume); // "MusicVolume" should match the exposed parameter name
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
        SetMusicVolume(0f);
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
            speechTherapyCompleted = true;
            jojoController.canTalk2 = true;
            
            /*if (BoyAnimator != null)
            {
                BoyAnimator.SetBool("canTalk2", true);                
            }*/

            LeanTween.scale(gameObject, Vector3.zero, 0.5f).setEase(LeanTweenType.easeInOutBack).setOnComplete(() =>
            {
                Destroy(gameObject);
            });
        });
    }

    public void SkipButton()
    {
        StartCoroutine(Timer(0f));
        speechTherapyCompleted = true;
    }
}
