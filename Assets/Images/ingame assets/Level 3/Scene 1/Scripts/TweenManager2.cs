using UnityEngine;
using System.Collections;

public class TweenManager2 : MonoBehaviour
{
    private Animator birdAnimator;
    private Bird_Controller birdController;
    private bool isRetryClicked = false;
    private bool speechTherapyCompleted = false;
    

    private void Start()
    {
        ST_AudioManager.Instance.OnPlaybackComplete += HandlePlaybackComplete;
        ST_AudioManager.Instance.OnRetryClicked += ResetTimer;

        // Get the reference to the Animator and Bird_Controller component of the Bird game object
        GameObject bird = GameObject.FindGameObjectWithTag("Bird");
        if (bird != null)
        {
            birdAnimator = bird.GetComponent<Animator>();
            birdController = bird.GetComponent<Bird_Controller>();
        }
    }
    private void OnDestroy()
    {
        ST_AudioManager.Instance.OnPlaybackComplete -= HandlePlaybackComplete;
        ST_AudioManager.Instance.OnRetryClicked -= ResetTimer;
    }

    private void HandlePlaybackComplete()
    {
        // Start the timer after playback is complete
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

        // Tween all children to scale 0
        foreach (Transform child in transform)
        {
            LeanTween.scale(child.gameObject, Vector3.zero, 0.5f).setEase(LeanTweenType.easeInOutBack);
        }

        // Wait for the children to finish tweening
        LeanTween.delayedCall(0.5f, () =>
        {
            speechTherapyCompleted = true;
            birdController.OnBigLeafDropped();
            LeanTween.scale(gameObject, Vector3.zero, 0.5f).setEase(LeanTweenType.easeInOutBack).setOnComplete(() =>
            {
                Destroy(gameObject);
            });
        });
    }
}
