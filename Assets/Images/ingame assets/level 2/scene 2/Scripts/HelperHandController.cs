using UnityEngine;

public class HelperHandController : MonoBehaviour
{
    public GameObject helperHandPrefab;
    public float helperDelay = 5f;
    public float helperMoveDuration = 1f;

    private GameObject helperHandInstance;
    private PillowDragAndDrop currentPillow;

    private PillowDragAndDrop bigPillowLeft;
    private PillowDragAndDrop bigPillowRight;

    public void InitializePillows(PillowDragAndDrop left, PillowDragAndDrop right)
    {
        bigPillowLeft = left;
        bigPillowRight = right;

        // Start with either big pillow left or big pillow right
        if (bigPillowLeft != null && bigPillowLeft.GetComponent<Collider2D>().enabled)
        {
            ScheduleHelperHand(bigPillowLeft);
        }
        else if (bigPillowRight != null && bigPillowRight.GetComponent<Collider2D>().enabled)
        {
            ScheduleHelperHand(bigPillowRight);
        }
    }

    private void StartHelperHand(PillowDragAndDrop pillow)
    {
        Debug.Log("Starting Helper Hand for: " + pillow.gameObject.name);

        if (helperHandInstance != null)
        {
            Destroy(helperHandInstance);
        }

        currentPillow = pillow;
        helperHandInstance = Instantiate(helperHandPrefab, pillow.transform.position, Quaternion.identity);

        LeanTween.move(helperHandInstance, pillow.targetPosition.position, helperMoveDuration)
            .setOnComplete(() =>
            {
                helperHandInstance.transform.position = pillow.transform.position;
                StartHelperHand(pillow);
            });
    }

    public void ScheduleHelperHand(PillowDragAndDrop pillow)
    {
        Debug.Log("Scheduling Helper Hand for: " + pillow.gameObject.name);

        currentPillow = pillow;
        CancelInvoke(nameof(StartHelperHandInternal));
        Invoke(nameof(StartHelperHandInternal), helperDelay);
    }

    private void StartHelperHandInternal()
    {
        if (currentPillow != null && !currentPillow.HasInteracted)
        {
            Debug.Log("Helper Hand delay ended. Spawning for: " + currentPillow.gameObject.name);
            StartHelperHand(currentPillow);
        }
        else
        {
            Debug.Log("Pillow has already been interacted with or is null.");
        }
    }

    public void StopHelperHand()
    {
        Debug.Log("Stopping Helper Hand");

        if (helperHandInstance != null)
        {
            LeanTween.cancel(helperHandInstance);
            Destroy(helperHandInstance);
        }

        CancelInvoke(nameof(StartHelperHandInternal));
    }

    public void ScheduleNextPillow(PillowDragAndDrop nextPillow)
    {
        ScheduleHelperHand(nextPillow);
    }
}
