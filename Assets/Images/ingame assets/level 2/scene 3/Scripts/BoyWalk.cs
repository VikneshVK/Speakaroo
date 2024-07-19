using UnityEngine;

public class BoyWalk : MonoBehaviour
{
    public Animator boyAnimator;
    public Animator birdAnimator;
    public Transform stopPosition;
    public float moveSpeed = 2.0f;
    public GameObject Bag;
    public dragManager dragManager;  // Reference to DragManager

    private bool isWalking = false;
    private bool reachedStopPosition = false;
    private bool birdFinishedTalking = false;
    private bool boyFinishedTalking = false;
    private Collider2D bagCollider;

    void Start()
    {
        boyAnimator = GetComponent<Animator>();
        bagCollider = Bag.GetComponent<Collider2D>();
        bagCollider.enabled = false; // Ensure it's disabled initially
    }

    void Update()
    {
        // Check if the boy should start walking
        if (boyAnimator.GetCurrentAnimatorStateInfo(0).IsName("Idle") &&
            boyAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f &&
            !isWalking)
        {
            boyAnimator.SetBool("can Walk", true);
            isWalking = true;
        }

        // Move the boy towards the stop position
        if (isWalking && !reachedStopPosition)
        {
            MoveTowardsStopPosition();
        }

        // Check if the bird has finished talking
        if (reachedStopPosition && birdAnimator.GetCurrentAnimatorStateInfo(0).IsName("Talk") &&
            birdAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.96f &&
            !birdFinishedTalking)
        {
            Debug.Log("boy can talk");
            birdFinishedTalking = true;
            boyAnimator.SetBool("can talk", true);
        }

        // Check if the boy has finished talking
        if (boyAnimator.GetCurrentAnimatorStateInfo(0).IsName("Talk") &&
            boyAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.9f &&
            !boyFinishedTalking)
        {
            Debug.Log("bird will talk bagTalk");
            boyFinishedTalking = true;
            birdAnimator.SetTrigger("bagTalk");
            dragManager.OnTriggerActivated("bagTalk");  // Notify DragManager
            bagCollider.enabled = true;
        }
    }

    void MoveTowardsStopPosition()
    {
        transform.position = Vector2.MoveTowards(transform.position, stopPosition.position, moveSpeed * Time.deltaTime);

        if (Vector2.Distance(transform.position, stopPosition.position) < 0.1f)
        {
            reachedStopPosition = true;
            boyAnimator.SetBool("can Walk", false);
            birdAnimator.SetBool("canTalk", true);
        }
    }
}
