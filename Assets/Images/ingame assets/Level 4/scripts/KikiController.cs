using UnityEngine;

public class KikiController : MonoBehaviour
{
    public static int itemsDropped = 0;

    public Transform dropLocation; // Reference to the drop location
    public Transform startPosition; // Reference to the start position

    public float flySpeed = 2f; // Speed at which Kiki flies

    private Animator birdAnimator;
    private GameObject currentItem;
    private GameObject iceCream;
    private GameObject cookies;
    private GameObject apples;
    private int itemIndex = 0; // To track which item to handle next

    private Vector3 targetPosition;
    private bool isMoving = false; // Indicates whether the character is currently moving
    private System.Action onComplete;

    void Start()
    {
        birdAnimator = GetComponent<Animator>();

        // Find items by their tags
        iceCream = GameObject.FindGameObjectWithTag("IceCream");
        cookies = GameObject.FindGameObjectWithTag("Cookies");
        apples = GameObject.FindGameObjectWithTag("Apples");

        transform.position = startPosition.position; // Initialize to start position
    }

    void Update()
    {
        if (isMoving)
        {
            MoveToTarget();
        }
        else
        {
            HandleFlying();
            HandlePositionReached();
            HandleItemPicked();
            HandleItemDropped();
        }
    }

    private void HandleFlying()
    {
        if (birdAnimator.GetBool("startFlying"))
        {
            currentItem = GetCurrentItem();
            if (currentItem != null)
            {
                birdAnimator.SetBool("startFlying", false);
                targetPosition = currentItem.transform.position;
                onComplete = OnReachItemPosition;
                isMoving = true; // Start moving towards the item
            }
        }
    }

    private GameObject GetCurrentItem()
    {
        switch (itemIndex)
        {
            case 0:
                return iceCream;
            case 1:
                return cookies;
            case 2:
                return apples;
            default:
                birdAnimator.SetBool("allDone", true);
                return null;
        }
    }

    private void HandlePositionReached()
    {
        if (birdAnimator.GetBool("positionReached"))
        {
            PickupItem();
        }
    }

    private void PickupItem()
    {
        if (currentItem != null && birdAnimator.GetBool("positionReached"))
        {
            currentItem.transform.SetParent(transform);
            birdAnimator.SetBool("itemPicked", true);
            birdAnimator.SetBool("positionReached", false);
            targetPosition = dropLocation.position;
            onComplete = OnReachDropLocation;
            isMoving = true; // Start moving towards the drop location
        }
    }

    private void HandleItemPicked()
    {
        // This function is kept for logical separation, but the movement is handled in HandleFlying and PickupItem
    }

    private void DropItem()
    {
        if (currentItem != null)
        {
            currentItem.transform.SetParent(null);
            currentItem.transform.position = dropLocation.position;
            currentItem = null;
            birdAnimator.SetBool("itemDropped", true);
            birdAnimator.SetBool("positionReached", false);
        }
    }

    private void HandleItemDropped()
    {
        if (birdAnimator.GetBool("itemDropped"))
        {
            itemsDropped++;
            itemIndex++;
            birdAnimator.SetBool("itemDropped", false);
            if (itemIndex < 3)
            {
                birdAnimator.SetBool("startFlying", true);
            }
            else
            {
                birdAnimator.SetBool("allDone", true);
            }

            targetPosition = startPosition.position;
            onComplete = OnReturnToStart;
            isMoving = true; // Start moving back to start position
        }
    }

    private void MoveToTarget()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, flySpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            onComplete?.Invoke();
            onComplete = null; // Reset the callback
            isMoving = false; // Stop moving
        }
    }

    private void OnReachItemPosition()
    {
        birdAnimator.SetBool("positionReached", true);
    }

    private void OnReachDropLocation()
    {
        DropItem();
        birdAnimator.SetBool("positionReached", true);
    }

    private void OnReturnToStart()
    {
        // Logic when the parrot returns to the start position
        // This can be used to trigger any additional behaviors
    }
}
