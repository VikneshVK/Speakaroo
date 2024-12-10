using UnityEngine;
using System.Collections;
using TMPro;

public class Bird_Controller : MonoBehaviour
{
    public Transform initialPosition;
    public Transform target1;
    public Transform target2;
    public Collider2D dustbinCollider;
    public Animator birdAnimator;
    public Animator trashBinAnimator;
    public GameObject boyController;
    public GameObject helperpointer;
    public TextMeshProUGUI subtitleText;

    public Collider2D bigLeaf1Collider;
    public Collider2D bigLeaf2Collider;
    public bool LevelComplete;

    private HP_HelperpointerController helpercontroller;
    private AudioSource instructionAudio;
    private static int totalLeaves;
    private Transform currentTarget;
    private bool IsleavesDropped;
    private Vector3 moveTarget;
    private bool isMoving;
    private bool inactivityTimerStarted;
    private bool triggerresetted;
    private bool colliderEnabled;
    // Speed variables
    private float moveSpeed = 1.05f;
    private float moveSpeed2 = 4f;
    public float returnMoveSpeed = 3f; // Unique speed for returning to initial position

    private void Awake()
    {
        LeanTween.init(2400);
    }

    void Start()
    {
        totalLeaves = 2;
        birdAnimator.SetBool("is_Flying", true);
        moveTarget = initialPosition.position;
        isMoving = true;
        instructionAudio = helperpointer.GetComponent<AudioSource>();
        helpercontroller = helperpointer.GetComponent<HP_HelperpointerController>();
        inactivityTimerStarted = false;
        LevelComplete = false;
        triggerresetted = false;
        colliderEnabled = false;
    }

    void Update()
    {
        HandleAnimationStateTransitions();

        if (isMoving)
        {
            // Adjust speed based on the target
            float speed = (moveTarget == initialPosition.position) ? returnMoveSpeed : moveSpeed2;

            transform.position = Vector3.MoveTowards(transform.position, moveTarget, speed * Time.deltaTime);

            if (transform.position == moveTarget)
            {
                isMoving = false;
                birdAnimator.SetBool("is_Flying", false);
            }
        }
    }

    void HandleAnimationStateTransitions()
    {
        AnimatorStateInfo stateInfo = birdAnimator.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.IsName("Talk") && stateInfo.normalizedTime >= 0.95f)
        {
            birdAnimator.SetTrigger("instruction");
            instructionAudio.Play();
            StartCoroutine(RevealTextWordByWord("Put the Dry Leaves inside the Bin", 0.5f));
        }

        if (stateInfo.IsName("Jump") && stateInfo.normalizedTime < 0.1f)
        {
            StartTweenToLeafPosition();
        }

        if (stateInfo.IsName("Jump") && stateInfo.normalizedTime >= 1f)
        {
            StartCoroutine(DelayedOnJumpComplete(0.5f)); // Add a slight delay before calling OnJumpComplete
        }

        if (stateInfo.IsName("grab fly and drop") && stateInfo.normalizedTime < 0.1f)
        {
            StartMovingToDustbin();
        }

        if (stateInfo.IsName("grab fly and drop") && stateInfo.normalizedTime >= 0.95f)
        {
            StartCoroutine(DelayedOnFlyAndDropComplete(0.5f)); // Add a slight delay before calling OnFlyAndDropComplete
        }

        if (stateInfo.IsName("back to rest") && stateInfo.normalizedTime < 0.1f)
        {
            StartMovingToInitialPosition();
        }

        if (stateInfo.IsName("back to rest") && stateInfo.normalizedTime >= 0.95f)
        {
            OnReturnToInitialPositionComplete(); // Add a slight delay before calling OnReturnToInitialPositionComplete
        }

        if (stateInfo.IsName("Idle1") && !colliderEnabled)
        {
            colliderEnabled = true;
            OnIdle1State();            
        }
    }

    IEnumerator DelayedOnJumpComplete(float delay)
    {
        yield return new WaitForSeconds(delay);
        OnJumpComplete();
    }

    IEnumerator DelayedOnFlyAndDropComplete(float delay)
    {
        yield return new WaitForSeconds(delay);
        OnFlyAndDropComplete();
    }

    void StartTweenToLeafPosition()
    {
        currentTarget = totalLeaves == 2 ? target1 : target2;
        moveTarget = currentTarget.position;
        isMoving = true;
    }

    void StartMovingToDustbin()
    {
        moveTarget = dustbinCollider.bounds.center;
        isMoving = true;
    }

    void StartMovingToInitialPosition()
    {
        moveTarget = initialPosition.position;
        isMoving = true;
    }

    void OnJumpComplete()
    {
        currentTarget.SetParent(transform);
        currentTarget.localPosition = Vector3.zero;
        birdAnimator.SetTrigger("flyanddrop");
        trashBinAnimator.SetBool("binOpen", true);
        triggerresetted = false;
    }

    void OnFlyAndDropComplete()
    {
        if (currentTarget != null)
        {
            currentTarget.SetParent(null);
            currentTarget.GetComponent<SpriteRenderer>().enabled = false;
        }

        if (!IsleavesDropped)
        {
            IsleavesDropped = true;
            totalLeaves--;
        }
        colliderEnabled = false;
        birdAnimator.SetTrigger("backtoRest");
        trashBinAnimator.SetBool("binOpen", false);
    }

    void OnReturnToInitialPositionComplete()
    {
        IsleavesDropped = false;

        if (totalLeaves == 0)
        {
            LevelComplete = true;
            birdAnimator.SetBool("levelComplete", true);
            birdAnimator.SetTrigger("leavesDropped"); // Trigger "leavesDropped" to transition directly to Idle1 state
        }
        else
        {
            birdAnimator.SetTrigger("canTalk2");
            instructionAudio.Play();
            inactivityTimerStarted = false;
            StartCoroutine(RevealTextWordByWord("Put the Leaves inside the Bin", 0.5f));
        }
    }

    public void OnBigLeafDropped()
    {
        birdAnimator.SetTrigger("jump");
        IsleavesDropped = false;
        DetermineNextTarget();
    }

    void DetermineNextTarget()
    {
        currentTarget = totalLeaves == 2 ? target1 : target2;
        StartTweenToLeafPosition();
    }

    void EnableBigLeavesColliders()
    {
        if (bigLeaf1Collider && bigLeaf2Collider)
        {
            bigLeaf1Collider.enabled = true;
            bigLeaf2Collider.enabled = true;
        }
    }

    void OnIdle1State()
    {
        if (!triggerresetted)
        {
            birdAnimator.ResetTrigger("flyanddrop");
            birdAnimator.ResetTrigger("backtoRest");
            birdAnimator.ResetTrigger("canTalk2");
            birdAnimator.ResetTrigger("jump");
            birdAnimator.ResetTrigger("instruction");
            triggerresetted = true;
        }

        if (totalLeaves == 0)
        {
            LevelComplete = true;
            birdAnimator.SetBool("levelComplete", true);
        }
        else
        {
            EnableBigLeavesColliders();
            if (!inactivityTimerStarted)
            {
                helpercontroller.StartInactivityTimer();
                inactivityTimerStarted = true;
            }
        }
    }

    private IEnumerator RevealTextWordByWord(string fullText, float delayBetweenWords)
    {
        subtitleText.text = "";  // Clear the text before starting
        subtitleText.gameObject.SetActive(true);  // Ensure the subtitle text is active

        string[] words = fullText.Split(' ');  // Split the full text into individual words

        // Reveal words one by one
        for (int i = 0; i < words.Length; i++)
        {
            subtitleText.text = string.Join(" ", words, 0, i + 1);  // Show only the words up to the current index
            yield return new WaitForSeconds(delayBetweenWords);  // Wait before revealing the next word
        }
        subtitleText.gameObject.SetActive(false);
    }
}
