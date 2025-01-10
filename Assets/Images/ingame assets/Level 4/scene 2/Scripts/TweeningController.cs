using UnityEngine;
using System.Collections;
using TMPro;

public class TweeningController : MonoBehaviour
{
    public bool isSecondTime = false;
    public LVL4Sc2AudioManager audioManager;
    public AudioClip Audio1;
    public SubtitleManager subtitleManager;
    [Header("Set 1 Game Objects and Target Positions")]
    public GameObject[] set1Objects;
    public Transform[] set1Targets;
    public bool JuiceTweenCompleted;
    [Header("Set 2 Game Objects and Target Positions")]
    public GameObject[] set2Objects;
    public Transform[] set2Targets;

    private Vector3[] set1InitialPositions;
    private Vector3[] set2InitialPositions;

    private JuiceManager juiceManager;

    public float tweenDuration = 1f;
    public LeanTweenType easeType = LeanTweenType.easeOutBack;
    public bool TweenComplete;
    private bool previousState = false; // To track the previous state of isSecondTime

    // Bird-related variables
    private GameObject bird;
    public Transform birdEndPosition;
    public LVL4Sc2HelperHand helperhand;
    private Vector3 birdInitialPosition;
    private Animator birdAnimator;

    private void Awake()
    {
        LeanTween.init(5000);
    }

    private void Start()
    {
        juiceManager = FindObjectOfType<JuiceManager>();
        JuiceTweenCompleted = false;
        // Store initial positions of the game objects
        set1InitialPositions = new Vector3[set1Objects.Length];
        set2InitialPositions = new Vector3[set2Objects.Length];

        for (int i = 0; i < set1Objects.Length; i++)
        {
            if (set1Objects[i] != null)
            {
                set1InitialPositions[i] = set1Objects[i].transform.position;
            }
        }

        for (int i = 0; i < set2Objects.Length; i++)
        {
            if (set2Objects[i] != null)
            {
                set2InitialPositions[i] = set2Objects[i].transform.position;
            }
        }

        // Set initial tweening
        PerformInitialTweening();

        // Get bird reference and its initial position
        bird = GameObject.FindGameObjectWithTag("Bird");
        if (bird != null)
        {
            birdInitialPosition = bird.transform.position;
            birdAnimator = bird.GetComponent<Animator>();
        }
    }

    private void Update()
    {
        if (isSecondTime != previousState)
        {
            previousState = isSecondTime;

            PerformTweening();

            helperhand.ResetAndStartDelayTimer();

            if (isSecondTime && juiceManager != null)
            {
                juiceManager.isSecondTime = true;
            }
        }
    }

    private void PerformInitialTweening()
    {
        if (isSecondTime)
        {
            TweenSetToInitial(set1Objects, set1InitialPositions);
            TweenSetToTarget(set2Objects, set2Targets);
            CallBirdTween(); // Call bird tweening when set2 is moved to target positions on the second time
        }
        else
        {
            TweenSetToTarget(set1Objects, set1Targets);
            TweenSetToInitial(set2Objects, set2InitialPositions);
        }
    }

    private void PerformTweening()
    {
        if (isSecondTime)
        {
            TweenSetToInitial(set1Objects, set1InitialPositions);
            TweenSetToTarget(set2Objects, set2Targets);            
            CallBirdTween(); // Call bird tweening here when isSecondTime is true
        }
        else
        {
            TweenSetToTarget(set1Objects, set1Targets);
            TweenSetToInitial(set2Objects, set2InitialPositions);
        }
    }

    private void CallBirdTween()
    {
        // Get the RectTransform component of the bird
        RectTransform birdRectTransform = bird.GetComponent<RectTransform>();
        if (birdRectTransform != null)
        {
            // Set initial position (outside the viewport)
            birdRectTransform.anchoredPosition = new Vector2(-1300, 20);

            // Tween bird to final position (inside the viewport)
            LeanTween.value(bird, -1300, -790, tweenDuration).setEase(easeType).setOnUpdate((float x) =>
            {
                birdRectTransform.anchoredPosition = new Vector2(x, 20);
            }).setOnComplete(() =>
            {
                StartCoroutine(BirdAnimationSequence(birdRectTransform));
            });
        }
    }

    private IEnumerator BirdAnimationSequence(RectTransform birdRectTransform)
    {
        if (birdAnimator != null)
        {
            // Trigger "Intro2" animation
            birdAnimator.SetTrigger("Intro2");
            audioManager.PlayAudio(Audio1);
            subtitleManager.DisplaySubtitle("Now, Let's make juice", "Kiki", Audio1);
        }

        yield return new WaitForSeconds(3f); // Wait for the animation and text reveal to complete

        // Tween bird back to initial position (outside the viewport)
        LeanTween.value(bird, -790, -1300, tweenDuration).setEase(easeType).setOnUpdate((float x) =>
        {
            birdRectTransform.anchoredPosition = new Vector2(x, 21);
        });

        TweenComplete = true;
        JuiceTweenCompleted = true;
    }

    private void TweenSetToTarget(GameObject[] objects, Transform[] targets)
    {
        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i] != null && targets[i] != null)
            {
                LeanTween.cancel(objects[i]);
                LeanTween.move(objects[i], targets[i].position, tweenDuration).setEase(easeType);
            }
        }
    }

    private void TweenSetToInitial(GameObject[] objects, Vector3[] initialPositions)
    {
        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i] != null)
            {
                LeanTween.cancel(objects[i]);
                LeanTween.move(objects[i], initialPositions[i], tweenDuration).setEase(easeType);
            }
        }
    }
    
}
