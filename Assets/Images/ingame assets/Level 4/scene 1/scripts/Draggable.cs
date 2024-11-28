using UnityEngine;
using System.Collections;

public class Draggable : MonoBehaviour
{
    public Transform dropTarget; 
    public System.Action<GameObject> onDrop; 
    public Vector3 startPosition;
    private bool isDragging = false;
    private bool isDropped = false;
    private LVL4Sc1HelperController helperController; 
    private KikiController1 kikiController;
    private Transform dropLocation;
    private Animator playerAnimator;
    public Vector3 DropTargetPosition { get; private set; }

    private void Start()
    {
        startPosition = transform.position;

        GameObject helperHandObject = GameObject.FindGameObjectWithTag("HelperHand");
        if (helperHandObject != null)
        {
            helperController = helperHandObject.GetComponent<LVL4Sc1HelperController>();
        }

        GameObject birdObject = GameObject.FindGameObjectWithTag("Bird");
        if (birdObject != null)
        {
            kikiController = birdObject.GetComponent<KikiController1>();
            if (kikiController != null)
            {
                dropLocation = kikiController.dropLocation;
            }
        }

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerAnimator = playerObject.GetComponent<Animator>();
        }
    }

    private void OnMouseDown()
    {
        if (isDropped) return;

        helperController?.ResetTimer();

        isDragging = true;

        if (playerAnimator != null)
        {
            playerAnimator.SetBool("openMouth", true);
        }
    }

    private void OnMouseDrag()
    {
        if (!isDragging || isDropped) return;

        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = new Vector3(mousePosition.x, mousePosition.y, 0);
    }

    private void OnMouseUp()
    {
        if (!isDragging || isDropped) return;

        isDragging = false;


        if (CheckDropTargetCollision())
        {
            isDropped = true;
            onDrop?.Invoke(gameObject);
        }
        else if (dropLocation != null)
        {
            transform.position = dropLocation.position;
        }
        playerAnimator.SetBool("openMouth", false);
        /*playerAnimator.SetTrigger("Chew");*/
    }

    private bool CheckDropTargetCollision()
    {
        if (dropTarget == null) return false;

        Collider2D targetCollider = dropTarget.GetComponent<Collider2D>();
        return targetCollider != null && targetCollider.bounds.Contains(transform.position);
    }

    public void SetDropTarget(Transform target)
    {
        dropTarget = target;
        DropTargetPosition = target.position;
    }
}
