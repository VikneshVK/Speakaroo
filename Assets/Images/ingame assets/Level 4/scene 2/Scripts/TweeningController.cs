using UnityEngine;

public class TweeningController : MonoBehaviour
{
    [SerializeField] private bool isSecondTime = false;

    [Header("Set 1 Game Objects and Target Positions")]
    public GameObject[] set1Objects;
    public Transform[] set1Targets;

    [Header("Set 2 Game Objects and Target Positions")]
    public GameObject[] set2Objects;
    public Transform[] set2Targets;

    private Vector3[] set1InitialPositions;
    private Vector3[] set2InitialPositions;

    private void Awake()
    {
        LeanTween.init(5000);
    }

    private void Start()
    {
        // Store initial positions of the game objects
        set1InitialPositions = new Vector3[set1Objects.Length];
        set2InitialPositions = new Vector3[set2Objects.Length];

        for (int i = 0; i < set1Objects.Length; i++)
        {
            set1InitialPositions[i] = set1Objects[i].transform.position;
        }

        for (int i = 0; i < set2Objects.Length; i++)
        {
            set2InitialPositions[i] = set2Objects[i].transform.position;
        }

        // Initially, tween Set 1 to target positions
        TweenSetToTarget(set1Objects, set1Targets);
    }

    private void Update()
    {
        // Continuously check the boolean value and tween objects accordingly
        if (isSecondTime)
        {
            TweenSetToInitial(set1Objects, set1InitialPositions);
            TweenSetToTarget(set2Objects, set2Targets);
        }
        else
        {
            TweenSetToTarget(set1Objects, set1Targets);
            TweenSetToInitial(set2Objects, set2InitialPositions);
        }
    }

    private void TweenSetToTarget(GameObject[] objects, Transform[] targets)
    {
        for (int i = 0; i < objects.Length; i++)
        {
            LeanTween.cancel(objects[i]); // Cancel any existing tweens on the object
            LeanTween.move(objects[i], targets[i].position, 1f).setEase(LeanTweenType.easeOutBack);
        }
    }

    private void TweenSetToInitial(GameObject[] objects, Vector3[] initialPositions)
    {
        for (int i = 0; i < objects.Length; i++)
        {
            LeanTween.cancel(objects[i]); // Cancel any existing tweens on the object
            LeanTween.move(objects[i], initialPositions[i], 1f).setEase(LeanTweenType.easeOutBack);
        }
    }
}
