using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraLvl5Sc1_3TweenManager : MonoBehaviour
{
    public bool speechTherapyCompleted = false;
    private Animator BoyAnimator;
    private bool isRetryClicked = false;
    private GameObject buttonToActivate;
    private LVL5Sc1_3JojoController jojoController;
    

    private void Start()
    {
        ST_AudioManager.Instance.OnPlaybackComplete += HandlePlaybackComplete;
        ST_AudioManager.Instance.OnRetryClicked += ResetTimer;

        // Get the reference to the Animator component of the Bird game object
        GameObject Boy = GameObject.FindGameObjectWithTag("Player");
        if (Boy != null)
        {
            BoyAnimator = Boy.GetComponent<Animator>();
            jojoController = Boy.GetComponent<LVL5Sc1_3JojoController>();
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
            
            if (BoyAnimator != null)
            {
                BoyAnimator.SetBool("canTalk2", true);
            }

            // Tween the parent to scale 0
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
