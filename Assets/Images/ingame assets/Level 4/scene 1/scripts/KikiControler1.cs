using UnityEngine;
using System.Collections;

public class KikiController : MonoBehaviour
{
    public static int itemsDropped = 0;

    public Transform dropLocation;
    public Transform startPosition;
    public GameObject boy;
    public SpriteRenderer parrotSpriteRenderer;

    public float flySpeed = 2f;

    private Animator birdAnimator;
    private Animator boyAnimator;
    private GameObject currentItem;
    private JojoController jojoController;

    private Vector3 targetPosition;
    private bool isMoving = false;

    private enum BirdState
    {
        Idle,
        FlyingToItem,
        PickingUpItem,
        FlyingToDropLocation,
        DroppingItem,
        ReturningToStart
    }

    private BirdState currentState = BirdState.Idle;

    void Start()
    {
        birdAnimator = GetComponent<Animator>();
        boyAnimator = boy.GetComponent<Animator>();
        transform.position = startPosition.position;

        jojoController = boy.GetComponent<JojoController>();
    }

    void Update()
    {
        switch (currentState)
        {
            case BirdState.Idle:
                CheckStartFlying();
                break;

            case BirdState.FlyingToItem:
            case BirdState.FlyingToDropLocation:
            case BirdState.ReturningToStart:
                MoveToTarget();
                break;

            case BirdState.PickingUpItem:
                CheckPickupCompletion();
                break;

            case BirdState.DroppingItem:
                CheckDropCompletion();
                break;
        }
    }

    private void CheckStartFlying()
    {
        if (birdAnimator.GetBool("startFlying"))
        {
            UpdateItemReferences();
            currentItem = GetCurrentItem();
            if (currentItem != null)
            {
                birdAnimator.SetBool("startFlying", false);
                currentState = BirdState.FlyingToItem;
                targetPosition = currentItem.transform.position;
                isMoving = true;
            }
        }
    }

    private GameObject GetCurrentItem()
    {
        switch (itemsDropped)
        {
            case 0:
                return GameObject.FindGameObjectWithTag("IceCream");
            case 1:
                return GameObject.FindGameObjectWithTag("Cookies");
            case 2:
                return GameObject.FindGameObjectWithTag("Apples");
            default:
                birdAnimator.SetBool("allDone", true);
                return null;
        }
    }

    private void MoveToTarget()
    {
        if (!isMoving) return;

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, flySpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            isMoving = false;
            OnReachTarget();
        }
    }

    private void OnReachTarget()
    {
        switch (currentState)
        {
            case BirdState.FlyingToItem:
                birdAnimator.SetBool("positionReached", true);
                currentState = BirdState.PickingUpItem;
                break;

            case BirdState.FlyingToDropLocation:
                birdAnimator.SetBool("positionReached", true);
                birdAnimator.SetBool("itemPicked", false); // Reset itemPicked before dropping
                currentState = BirdState.DroppingItem;
                break;

            case BirdState.ReturningToStart:
                OnReturnToStart();
                currentState = BirdState.Idle;
                break;
        }
    }

    private void CheckPickupCompletion()
    {
        if (birdAnimator.GetCurrentAnimatorStateInfo(0).IsName("pick up") &&
            birdAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
        {
            currentItem.transform.SetParent(transform);
            birdAnimator.SetBool("itemPicked", true);
            birdAnimator.SetBool("positionReached", false);
            parrotSpriteRenderer.flipX = false;
            currentState = BirdState.FlyingToDropLocation;
            targetPosition = dropLocation.position;
            isMoving = true;
        }
    }

    private void CheckDropCompletion()
    {
        if (birdAnimator.GetCurrentAnimatorStateInfo(0).IsName("drop") &&
            birdAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
        {
            currentItem.transform.SetParent(null);
            currentItem.transform.position = dropLocation.position;
            birdAnimator.SetBool("itemDropped", true);
            birdAnimator.SetBool("positionReached", false);
            currentState = BirdState.ReturningToStart;
            targetPosition = startPosition.position;
            isMoving = true;
        }
    }

    private void OnReturnToStart()
    {
        birdAnimator.SetBool("positionReached", true);
        parrotSpriteRenderer.flipX = true;
        itemsDropped++;
        Debug.Log("itemDropped: " + itemsDropped);

        if (itemsDropped == 3)
        {
            birdAnimator.SetBool("allDone", true);
        }
        else
        {
            boyAnimator.SetTrigger("talk2");
            jojoController.CheckAndSpawnPrefab();
        }
        currentState = BirdState.Idle;
        StartCoroutine(ResetPositionReachedAfterDelay());
    }

    private IEnumerator ResetPositionReachedAfterDelay()
    {
        yield return new WaitForSeconds(0.1f);

        birdAnimator.SetBool("positionReached", false);
        birdAnimator.SetBool("startFlying", false);
        birdAnimator.SetBool("itemPicked", false);
        birdAnimator.SetBool("itemDropped", false);
        jojoController.prefabSpawned = false;
    }

    public void UpdateItemReferences()
    {
        currentItem = null; // Reset currentItem
    }
}
