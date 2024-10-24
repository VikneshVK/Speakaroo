using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeakerDragHandler : MonoBehaviour
{
    private bool isDragging = false;
    private Vector3 initialPosition;
    public Collider2D dropZoneCollider; // Reference to the drop zone collider
    private bool isDropped;
    public bool isBeakerDropped;

    [SerializeField] private Transform testTubeStand;
    [SerializeField] private Transform finalStandPosition;
    [SerializeField] private float tweenTime = 1f;
    [SerializeField] private float beakerTweenTime = 0.5f;

    private void Start()
    {
        isDropped = false;
        isBeakerDropped = false;
        initialPosition = transform.position;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);

            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                isDragging = true;
            }
        }

        if (Input.GetMouseButtonUp(0) && !isDropped)
        {
            isDragging = false;

            if (dropZoneCollider != null && dropZoneCollider.OverlapPoint(transform.position) )
            {
                isDropped = true;

                LeanTween.move(gameObject, dropZoneCollider.transform.position, beakerTweenTime).setEase(LeanTweenType.easeInOutQuad)
                .setOnComplete(() =>
                {
                    LeanTween.move(testTubeStand.gameObject, finalStandPosition, tweenTime).setEase(LeanTweenType.easeInOutQuad);
                });

                isBeakerDropped = true;
                dropZoneCollider.enabled = false;
            }
            else
            {
                LeanTween.move(gameObject, initialPosition, beakerTweenTime).setEase(LeanTweenType.easeInOutQuad);
            }
        }

        if (isDragging && !isDropped)
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.position = new Vector2(mousePosition.x, mousePosition.y);
        }
    }

}
