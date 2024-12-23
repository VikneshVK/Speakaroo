using UnityEngine;
using System.Collections;
using UnityEngine.Audio;

public class TweenManager2 : MonoBehaviour
{
    private Animator birdAnimator;
    private Bird_Controller birdController;
    private bool isRetryClicked = false;
    private bool speechTherapyCompleted = false;
    private GameObject stCanvas;
    public AudioMixer audioMixer;
    private const string musicVolumeParam = "MusicVolume";
    private const string AmbientVolumeParam = "AmbientVolume";

    private void Start()
    {
        ST_AudioManager.Instance.OnPlaybackComplete += HandlePlaybackComplete;
        ST_AudioManager.Instance.OnRetryClicked += ResetTimer;
        stCanvas = GameObject.FindGameObjectWithTag("STCanvas");
        // Get the reference to the Animator and Bird_Controller component of the Bird game object
        GameObject bird = GameObject.FindGameObjectWithTag("Kiki");
        if (bird != null)
        {
            birdAnimator = bird.GetComponent<Animator>();
            birdController = bird.GetComponent<Bird_Controller>();
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
        birdController.OnBigLeafDropped(); // Trigger bird's action for dropping the leaf
        birdAnimator.SetTrigger("jump");        

        LeanTween.scale(gameObject, Vector3.zero, 0.5f).setEase(LeanTweenType.easeInOutBack).setOnComplete(() =>
        {
            Destroy(gameObject); // Destroy this game object after completing the tween
        });
    }

    public void SkipButton()
    {
        StartCoroutine(Timer(0f));
        speechTherapyCompleted = true;
    }
}
