using UnityEngine;

public class ScrubberController : MonoBehaviour
{
    public GameObject maskPrefab;

    private void Update()
    {
        HandleScrubbing();
    }

    private void HandleScrubbing()
    {
        if (Input.GetMouseButton(0))
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.position = mousePosition;

            RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);
            if (hit.collider != null && hit.collider.GetComponent<DishController>() != null)
            {
                hit.collider.GetComponent<DishController>().StartScrubbing();
            }
        }
    }
}
