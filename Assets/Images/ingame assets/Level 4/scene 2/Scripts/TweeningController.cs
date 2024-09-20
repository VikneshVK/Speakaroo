using UnityEngine;

public class TweeningController : MonoBehaviour
{
    public bool isSecondTime = false;

    [Header("Set 1 Game Objects and Target Positions")]
    public GameObject[] set1Objects;
    public Transform[] set1Targets;

    [Header("Set 2 Game Objects and Target Positions")]
    public GameObject[] set2Objects;
    public Transform[] set2Targets;

    private Vector3[] set1InitialPositions;
    private Vector3[] set2InitialPositions;

    private JuiceManager juiceManager;

    public float tweenDuration = 1f;
    public LeanTweenType easeType = LeanTweenType.easeOutBack;

    private bool previousState = false; // To track the previous state of isSecondTime

    private void Awake()
    {
        LeanTween.init(5000);
    }

    private void Start()
    {
        juiceManager = FindObjectOfType<JuiceManager>();

        // Store initial positions of the game objects
        set1InitialPositions = new Vector3[set1Objects.Length];
        set2InitialPositions = new Vector3[set2Objects.Length];

        for (int i = 0; i < set1Objects.Length; i++)
        {
            if (set1Objects[i] != null) // Check if the game object is not null
            {
                set1InitialPositions[i] = set1Objects[i].transform.position;
            }
        }

        for (int i = 0; i < set2Objects.Length; i++)
        {
            if (set2Objects[i] != null) // Check if the game object is not null
            {
                set2InitialPositions[i] = set2Objects[i].transform.position;
            }
        }

        // Initially, tween Set 1 to target positions
        PerformInitialTweening();
    }

    private void Update()
    {
        if (isSecondTime != previousState)
        {
            previousState = isSecondTime;

            
            PerformTweening();

            
            if (isSecondTime)
            {
                LVL4Sc2HelperHand.Instance.ResetAndStartDelayTimer(); // Only for the second time
            }
            else
            {
                LVL4Sc2HelperHand.Instance.ResetAndStartDelayTimer(); // For the first time
            }
        }
    }

    private void PerformInitialTweening()
    {
        if (isSecondTime)
        {
            // Move set 2 to target positions and set 1 back to initial positions
            TweenSetToInitial(set1Objects, set1InitialPositions);
            TweenSetToTarget(set2Objects, set2Targets);
        }
        else
        {
            // Move set 1 to target positions and set 2 back to initial positions
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

            
            LVL4Sc2HelperHand.Instance.ResetAndStartDelayTimer();

            
            if (juiceManager != null)
            {
                juiceManager.isSecondTime = true;
            }
        }
        else
        {
            TweenSetToTarget(set1Objects, set1Targets);
            TweenSetToInitial(set2Objects, set2InitialPositions);

            
            LVL4Sc2HelperHand.Instance.ResetAndStartDelayTimer();
        }
    }


    private void TweenSetToTarget(GameObject[] objects, Transform[] targets)
    {
        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i] != null && targets[i] != null)
            {
                LeanTween.cancel(objects[i]); // Cancel any existing tweens on the object
                LeanTween.move(objects[i], targets[i].position, 1f).setEase(LeanTweenType.easeOutBack);
            }
        }
    }

    private void TweenSetToInitial(GameObject[] objects, Vector3[] initialPositions)
    {
        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i] != null)
            {
                LeanTween.cancel(objects[i]); // Cancel any existing tweens on the object
                LeanTween.move(objects[i], initialPositions[i], 1f).setEase(LeanTweenType.easeOutBack);
            }
        }
    }
}
