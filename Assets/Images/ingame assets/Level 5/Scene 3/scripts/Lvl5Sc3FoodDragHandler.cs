using UnityEngine;
using System.Collections;

public class Lvl5Sc3FoodDragHandler : MonoBehaviour
{
    private Vector3 startPosition;
    private bool isDragging = false;
    private bool isRightFood;
    private Transform dropTarget;
    private Collider2D dropTargetCollider;
    private Lvl5Sc3FeedingManager feedingManager;
    private Vector3 offset;
    public GameObject glowPrefab2;
    public LVL1helperhandController helperController;
    public Animator kikiAnimator;

    private void Start()
    {
        startPosition = transform.position;
        feedingManager = FindObjectOfType<Lvl5Sc3FeedingManager>();

        isRightFood = gameObject.name.Contains("RightFood");

        dropTarget = transform.parent.Find("DropTarget");
        dropTargetCollider = dropTarget.GetComponent<Collider2D>();

    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);
            helperController.ResetTimer();
            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                isDragging = true;

                offset = transform.position - (Vector3)mousePos;

            }

        }

        if (isDragging)
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.position = mousePos + (Vector2)offset;
        }

        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            isDragging = false;

            if (dropTargetCollider.OverlapPoint(transform.position))
            {
                feedingManager.OnFoodDropped(transform, isRightFood);
            }
            else
            {
                // Reset position and play instruction animation
                transform.position = startPosition;
                kikiAnimator.SetTrigger("instruction");
                feedingManager.PlayAudioBasedOnAnimal(transform.parent.name);

                // Find sibling objects for RightFood and WrongFood
                Transform rightFood = transform.parent.Find("RightFood");
                Transform wrongFood = transform.parent.Find("WrongFood");

                // Spawn glow at the position of both foods
                if (rightFood != null)
                {
                    SpawnAndAnimateGlow(rightFood.position);
                }

                if (wrongFood != null)
                {
                    SpawnAndAnimateGlow(wrongFood.position);
                }
            }
        }

    }
    private void SpawnAndAnimateGlow(Vector3 position)
    {
        GameObject glow = Instantiate(glowPrefab2, position, Quaternion.identity);
        glow.transform.localScale = Vector3.zero;

        // Tween the scale of the glow prefab
        LeanTween.scale(glow, Vector3.one * 10f, 0.5f).setEase(LeanTweenType.easeOutExpo).setOnComplete(() =>
        {
            StartCoroutine(FadeOutAndDestroy(glow, 2f));
        });
    }   
    private IEnumerator FadeOutAndDestroy(GameObject glow, float fadeDuration)
    {
        SpriteRenderer sr = glow.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color initialColor = sr.color;
            float time = 0;

            while (time < fadeDuration)
            {
                time += Time.deltaTime;
                float alpha = Mathf.Lerp(initialColor.a, 0f, time / fadeDuration);
                sr.color = new Color(initialColor.r, initialColor.g, initialColor.b, alpha);
                yield return null;
            }

            sr.color = new Color(initialColor.r, initialColor.g, initialColor.b, 0f);
        }

        Destroy(glow);
    }
}
