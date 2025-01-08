using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Lvl1Sc2HelperFunction : MonoBehaviour
{
    [Header("Settings")]
    public float delayTimer; // Total timer duration
    public GameObject glowPrefab; // Prefab for the glow effect
    public GameObject helperPointerPrefab; // Prefab for the helper pointer
    public GameObject helperPointerPrefab2;

    [Header("Positions")]
    public Transform tapPosition; // Position for the tap
    public Transform tapoffScreenPosition;
    public Transform shampooPosition; // Position for the shampoo
    public Transform headPosition; // Position for the head (for shampoo pointer)

    [Header("References")]
    public ShowerController showerController; // Reference to ShowerController script
    public ShowerMechanics showerMechanics; // Reference to ShowerMechanics script
    public Animator birdAnimator; // Reference to the bird animator

    [Header("Audio")]
    public AudioClip audio1;
    public AudioClip audio2;
    public AudioClip audio3;
    public Lvl1Sc1AudioManager audiomanager;
    public TextMeshProUGUI subtitleText;

    private float currentTimer = 0f; // Tracks timer progress
    private bool isTimerRunning = false; // Indicates if the timer is running
    private GameObject activeGlow; // Reference to the current glow prefab
    private GameObject activePointer; // Reference to the current helper pointer
    private bool isForTap = false; // Indicates whether the timer is for the tap
    private bool hasGlowSpawned = false;

    void Update()
    {
        if (isTimerRunning)
        {
            currentTimer += Time.deltaTime;

            // When timer reaches half of delayTimer
            if (currentTimer >= delayTimer / 2 && !hasGlowSpawned)
            {
                PauseTimer();
                SpawnAndAnimateGlow(isForTap ? tapPosition : shampooPosition);
                hasGlowSpawned = true; // Mark glow as spawned
            }

            // When timer reaches delayTimer
            if (currentTimer >= delayTimer)
            {
                isTimerRunning = false;
                SpawnHelperPointer();
            }
        }
    }

    public void StartTimer(bool forTap)
    {
        Debug.Log("TimerStarted");
        if (isTimerRunning)
        {
            Debug.LogWarning("Timer is already running!");
            return;
        }

        // Start the timer for either tap or shampoo
        isForTap = forTap;
        currentTimer = 0f;
        isTimerRunning = true;
    }

    public void ResetTimer()
    {
        // Stop the timer and reset
        currentTimer = 0f;
        isTimerRunning = false;

        // Destroy any active glow or pointer objects
        if (activeGlow != null)
        {
            Destroy(activeGlow);
            activeGlow = null;
        }

        if (activePointer != null)
        {
            LeanTween.cancel(activePointer); // Stop all tweens on the pointer
            Destroy(activePointer);
            activePointer = null;
        }
    }

    private void PauseTimer()
    {
        isTimerRunning = false;
    }

    private void ResumeTimer()
    {
        isTimerRunning = true;
    }

    private void SpawnAndAnimateGlow(Transform targetPosition)
    {
        activeGlow = Instantiate(glowPrefab, targetPosition.position, Quaternion.identity);

        // Tween glow scale to 8, wait for 2 seconds, then scale back to 0
        LeanTween.scale(activeGlow, Vector3.one * 8f, 0.5f).setOnComplete(() =>
        {
            LeanTween.delayedCall(2f, () =>
            {
                LeanTween.scale(activeGlow, Vector3.zero, 0.5f).setOnComplete(() =>
                {
                    Destroy(activeGlow);
                    ResumeTimer();
                });
            });
        });
    }

    private void SpawnHelperPointer()
    {
        if (isForTap)
        {
            // Spawn pointer outside viewport and tween to tap position
            activePointer = Instantiate(helperPointerPrefab2, tapoffScreenPosition.position, Quaternion.identity);
           /* LeanTween.move(activePointer, tapPosition.position, 1f).setLoopClamp();*/

            // Trigger appropriate parameter in bird animator based on tap states
            if (showerMechanics != null && showerController != null && birdAnimator != null)
            {
                if (!showerMechanics.hotTapOn && !showerController.tapsOn)
                {
                    birdAnimator.SetTrigger("open tap");
                    audiomanager.PlayAudio(audio1);
                    StartCoroutine(RevealTextWordByWord("Open the Tap", 0.5f));
                }
                else
                {
                    birdAnimator.SetTrigger("close tap");
                    audiomanager.PlayAudio(audio2);
                    StartCoroutine(RevealTextWordByWord("Close the Tap", 0.5f));
                }
            }
        }
        else
        {
            // Spawn pointer at shampoo position and tween to head position
            activePointer = Instantiate(helperPointerPrefab, shampooPosition.position, Quaternion.identity);
            LeanTween.move(activePointer, headPosition.position, 1f).setLoopClamp();
            if (birdAnimator != null)
            {
                birdAnimator.SetTrigger("shampoo");
                audiomanager.PlayAudio(audio3);
                StartCoroutine(RevealTextWordByWord("Put the Shampoo", 0.5f));
            }
        }
    }

    private IEnumerator RevealTextWordByWord(string fullText, float delayBetweenWords)
    {
        subtitleText.text = "";
        subtitleText.gameObject.SetActive(true);

        string[] words = fullText.Split(' ');

        for (int i = 0; i < words.Length; i++)
        {
            subtitleText.text = string.Join(" ", words, 0, i + 1);
            yield return new WaitForSeconds(delayBetweenWords);
        }
        subtitleText.text = "";
    }
}
