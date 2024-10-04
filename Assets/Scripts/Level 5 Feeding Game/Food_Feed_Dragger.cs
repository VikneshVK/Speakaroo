//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class Food_Feed_Dragger : MonoBehaviour
//{
//    private Vector3 offset;
//    private bool isDragging = false;
//    private Level5_FeedManager feedManager;
//    private Vector2 originalPos;

//    private void Start()
//    {
//        feedManager = FindObjectOfType<Level5_FeedManager>(); // Reference to manager
//        originalPos = transform.localPosition;
//    }

//    private void OnMouseDown()
//    {
//        isDragging = true;
//        offset = transform.position - GetMousePosition();

//        // Notify the manager that the object is being dragged
//        feedManager.OnFoodDragged(this.gameObject);
//    }

//    private void OnMouseDrag()
//    {
//        if (isDragging)
//        {
//            transform.position = GetMousePosition() + offset;
//        }
//    }

//    private void OnMouseUp()
//    {
//        isDragging = false;

//        // Check if dropped inside the correct zone using manager's method
//        if (feedManager.IsDroppedInCorrectZone(this.gameObject))  // Pass the current gameObject being dragged
//        {
//            // Notify manager of the correct drop
//            feedManager.OnCorrectFoodDropped(this.gameObject);
//        }
//        else
//        {
//            // Notify manager of the incorrect drop
//            feedManager.OnIncorrectFoodDropped(this.gameObject);
//            transform.localPosition = originalPos;
//        }
//    }

//    private Vector3 GetMousePosition()
//    {
//        Vector3 mousePoint = Input.mousePosition;
//        mousePoint.z = Camera.main.WorldToScreenPoint(gameObject.transform.position).z;
//        return Camera.main.ScreenToWorldPoint(mousePoint);
//    }
//}

//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class Food_Feed_Dragger : MonoBehaviour
//{
//    private Vector3 offset;
//    private bool isDragging = false;
//    private Level5_FeedManager feedManager;
//    private Vector2 OriginalPos;

//    private void Start()
//    {
//        // Reference to the manager script
//        feedManager = FindObjectOfType<Level5_FeedManager>();
//        OriginalPos = transform.position; // Save the original position to reset if needed
//    }

//    private void OnMouseDown()
//    {
//        isDragging = true;
//        offset = transform.position - GetMousePosition();

//        // Notify the manager that the food object is being dragged
//        feedManager.OnFoodDragged(this.gameObject);
//    }

//    private void OnMouseDrag()
//    {
//        if (isDragging)
//        {
//            // Move the food object along with the mouse
//            transform.position = GetMousePosition() + offset;
//            feedManager.OnFoodDragged(gameObject);
//        }
//    }

//    private void OnMouseUp()
//    {
//        isDragging = false;

//        // Check if the food is dropped inside the correct zone
//        if (feedManager.IsDroppedInCorrectZone(this.gameObject))
//        {
//            // Notify manager of the correct food drop
//            feedManager.OnCorrectFoodDropped(this.gameObject);
//            Debug.Log("Correct Drop");
//        }
//        else
//        {
//            // Notify manager of the incorrect food drop
//            feedManager.OnIncorrectFoodDropped(this.gameObject);
//            Debug.Log("Wrong Drop");
//        }

//        // Reset the dragged food to its original position
//        transform.position = OriginalPos;
//    }

//    private Vector3 GetMousePosition()
//    {
//        Vector3 mousePoint = Input.mousePosition;
//        mousePoint.z = Camera.main.WorldToScreenPoint(gameObject.transform.position).z;
//        return Camera.main.ScreenToWorldPoint(mousePoint);
//    }
//}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Food_Feed_Dragger : MonoBehaviour
{
    private Vector3 offset;
    private bool isDragging = false;
    private Level5_FeedManager feedManager;
    private Vector2 originalPos;

    private void Start()
    {
        feedManager = FindObjectOfType<Level5_FeedManager>(); // Reference to the manager
        originalPos = transform.position;  // Store the original position of the object
    }

    private void Update()
    {
        // Handle mouse input
        if (Input.GetMouseButtonDown(0))
        {
            OnMouseDown();
        }
        else if (Input.GetMouseButton(0) && isDragging)
        {
            OnMouseDrag();
        }
        else if (Input.GetMouseButtonUp(0) && isDragging)
        {
            OnMouseUp();
        }

        // Handle touch input for mobile devices
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);  // Get the first touch

            if (touch.phase == TouchPhase.Began)
            {
                OnTouchDown(touch);
            }
            else if (touch.phase == TouchPhase.Moved && isDragging)
            {
                OnTouchDrag(touch);
            }
            else if (touch.phase == TouchPhase.Ended && isDragging)
            {
                OnTouchUp();
            }
        }
    }

    // Mouse input handling
    private void OnMouseDown()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

        if (hit.collider != null && hit.collider.gameObject == gameObject)
        {
            isDragging = true;
            offset = transform.position - GetMouseWorldPosition();

            // Notify the manager that the object is being dragged
            feedManager.OnFoodDragged(this.gameObject);
        }
    }

    private void OnMouseDrag()
    {
        if (isDragging)
        {
            transform.position = GetMouseWorldPosition() + offset;
            feedManager.OnFoodDragged(this.gameObject);

           
        }
    }

    private void OnMouseUp()
    {
        isDragging = false;

        // Get the mouse position in the world
        Vector2 mousePosition = GetMouseWorldPosition();

        // Define the ray direction (from the food object to the mouse position)
        Vector2 rayDirection = mousePosition - (Vector2)transform.position;

        // Normalize the direction
        rayDirection.Normalize();

        // Perform a raycast with a specified distance
        RaycastHit2D raycastHit = Physics2D.Raycast(transform.position, rayDirection, 10f);

        // Check if the ray hits the drop zone
        if (raycastHit.collider != null && raycastHit.collider.CompareTag("DropZone_Feeding"))
        {
            feedManager.OnFoodDropped(gameObject);
        }
    }


    // Touch input handling
    private void OnTouchDown(Touch touch)
    {
        Ray ray = Camera.main.ScreenPointToRay(touch.position);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

        if (hit.collider != null && hit.collider.gameObject == gameObject)
        {
            isDragging = true;
            offset = transform.position - GetTouchWorldPosition(touch);

            // Notify the manager that the object is being dragged
            feedManager.OnFoodDragged(this.gameObject);
        }
    }

    private void OnTouchDrag(Touch touch)
    {
        if (isDragging)
        {
            transform.position = GetTouchWorldPosition(touch) + offset;
            feedManager.OnFoodDragged(this.gameObject);
        }
    }

    private void OnTouchUp()
    {
        isDragging = false;

        // Check if dropped inside the correct zone using raycast
        if (feedManager.IsDroppedInCorrectZone(this.gameObject))
        {
            feedManager.OnCorrectFoodDropped(this.gameObject);
            Debug.Log("Correct Drop");
        }
        else
        {
            feedManager.OnIncorrectFoodDropped(this.gameObject);
            Debug.Log("Wrong Drop");
        }
    }

    // Utility functions to get world positions
    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = Camera.main.WorldToScreenPoint(gameObject.transform.position).z;
        return Camera.main.ScreenToWorldPoint(mousePoint);
    }

    private Vector3 GetTouchWorldPosition(Touch touch)
    {
        Vector3 touchPoint = touch.position;
        touchPoint.z = Camera.main.WorldToScreenPoint(gameObject.transform.position).z;
        return Camera.main.ScreenToWorldPoint(touchPoint);
    }
}
