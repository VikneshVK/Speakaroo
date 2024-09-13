using UnityEngine;
using System.Collections;

public class KikiController1 : MonoBehaviour
{
    public static int itemsDropped = 0;

    public Transform dropLocation;
    public Transform startPosition;
    public GameObject boy;
    public GameObject itemHolder;

    public float flySpeed = 2f;
    public bool isLocked = false;

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
        if (!isLocked)
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
    }

    private void CheckStartFlying()
    {
        if (birdAnimator.GetBool("startFlying"))
        {
            UpdateItemReferences();
            currentItem = GetCurrentItem();
            Debug.Log("Current Item is " + currentItem);
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

        // Debug the movement step
        Debug.Log($"Moving from {transform.position} to {targetPosition}");

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
            // Attach the picked-up item to the specified parent (itemHolder)
            if (itemHolder != null)
            {
                currentItem.transform.SetParent(itemHolder.transform);

                // Set the local position of the item to zero, so it aligns with the parent (itemHolder)
                currentItem.transform.localPosition = Vector3.zero;
            }
            else
            {
                Debug.LogWarning("Item Holder is not assigned!");
            }

            birdAnimator.SetBool("itemPicked", true);
            birdAnimator.SetBool("positionReached", false);

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

        itemsDropped++;
        Debug.Log("Items Dropped: " + itemsDropped);

        if (itemsDropped == 3)
        {
            birdAnimator.SetBool("allDone", true);
        }
        else
        {
            boyAnimator.SetTrigger("talk2");
            jojoController.CheckAndSpawnPrefab();
        }

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
