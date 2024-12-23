using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class Lvl8Sc1TweenManager : MonoBehaviour
{
    public bool speechTherapyCompleted = false;
    private Animator BoyAnimator;
    private Lvl8Sc1Manager managerScript;
    private bool isRetryClicked = false;
    private GameObject buttonToActivate;
    private LVL5Sc12Jojocontroller jojoController;
    /*private List<GameObject> spriteMasks = new List<GameObject>();*/
    private GameObject stCanvas;
    public AudioMixer audioMixer;
    private const string musicVolumeParam = "MusicVolume";
    private const string AmbientVolumeParam = "AmbientVolume";

    private void Start()
    {
        ST_AudioManager.Instance.OnPlaybackComplete += HandlePlaybackComplete;
        ST_AudioManager.Instance.OnRetryClicked += ResetTimer;
        stCanvas = GameObject.FindGameObjectWithTag("STCanvas");
        GameObject Boy = GameObject.FindGameObjectWithTag("Player");
        if (Boy != null)
        {
            BoyAnimator = Boy.GetComponent<Animator>();
        }
        GameObject manager = GameObject.FindGameObjectWithTag("lvl8Manager");
        if (manager != null)
        {
            managerScript = manager.GetComponent<Lvl8Sc1Manager>();
        }

        GameObject uiCanvas = GameObject.FindGameObjectWithTag("UiPanel");
        if (uiCanvas != null)
        {
            buttonToActivate = uiCanvas.transform.Find("RetryButton").gameObject;
        }

        if (buttonToActivate == null)
        {
            Debug.LogWarning("Button not found inside the UICanvas.");
        }

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
        SetMusicVolume(-25f);
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
        managerScript.jojoCanTalk = true;

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
