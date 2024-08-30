using System.Collections;
using UnityEngine;

public class LV4DragManager : MonoBehaviour
{
    public GameObject dirtyDishes; // Reference to the Dirty Dishes GameObject
    public GameObject foam; // Reference to the Foam GameObject
    public GameObject yourPrefab; // The prefab to spawn after the Dirty Dishes are scaled down
    public Vector3 spawnPosition; // Position to spawn the prefab
    public Animator birdAnimator; // Reference to the Bird Animator
    public string canTalkParam = "canTalk"; // Name of the animation parameter

    private Vector3 offset;
    private bool isDragging = false;

    void Start()
    {
        birdAnimator.SetTrigger(canTalkParam);
        StartCoroutine(WaitForTalkAnimation());
    }

    void Update()
    {
        HandleMouseInput();
    }

    void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);

            if (hit.collider != null && hit.collider.gameObject == dirtyDishes)
            {
                isDragging = true;
                offset = dirtyDishes.transform.position - (Vector3)mousePosition;
            }
        }
        else if (Input.GetMouseButton(0) && isDragging)
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            dirtyDishes.transform.position = new Vector3(mousePosition.x, mousePosition.y, dirtyDishes.transform.position.z) + offset;
        }
        else if (Input.GetMouseButtonUp(0) && isDragging)
        {
            OnDirtyDishesDropped();
            isDragging = false;
        }
    }

    IEnumerator WaitForTalkAnimation()
    {
        yield return new WaitUntil(() => birdAnimator.GetCurrentAnimatorStateInfo(0).IsName("Talk") == false);
        dirtyDishes.GetComponent<Collider2D>().enabled = true;
    }

    void OnDirtyDishesDropped()
    {
        if (IsDroppedOnFoam(dirtyDishes))
        {
            LeanTween.scale(dirtyDishes, Vector3.zero, 0.5f).setOnComplete(SpawnPrefab);
        }
    }

    bool IsDroppedOnFoam(GameObject droppedObject)
    {
        Collider2D foamCollider = foam.GetComponent<Collider2D>();
        Collider2D droppedCollider = droppedObject.GetComponent<Collider2D>();
        return foamCollider.bounds.Intersects(droppedCollider.bounds);
    }

    void SpawnPrefab()
    {
        GameObject spawnedObject = Instantiate(yourPrefab, spawnPosition, Quaternion.identity);

        
        Transform floor = spawnedObject.transform.Find("floor");
        Transform sink = spawnedObject.transform.Find("sink");

       
        Vector3 floorOriginalScale = floor.localScale;
        Vector3 sinkOriginalScale = sink.localScale;

       
        floor.localScale = Vector3.zero;
        sink.localScale = Vector3.zero;

        
        LeanTween.scale(floor.gameObject, floorOriginalScale, 0.5f).setEase(LeanTweenType.easeOutBounce).setOnComplete(() =>
        {
           
            LeanTween.scale(sink.gameObject, sinkOriginalScale, 0.5f).setEase(LeanTweenType.easeOutBounce).setOnComplete(() =>
            {
               
                foreach (Transform dish in sink)
                {
                    Vector3 dishOriginalScale = dish.localScale; // Store dish's original scale
                    dish.localScale = Vector3.zero; // Set initial scale of the dish to zero
                    LeanTween.scale(dish.gameObject, dishOriginalScale, 0.5f).setEase(LeanTweenType.easeOutBounce);
                }
            });
        });
    }
}
