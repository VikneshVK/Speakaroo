using UnityEngine;

public class DressDragDrop : MonoBehaviour
{
    private Vector3 initialPosition;
    private bool isDragging = false;
    public string dressType; // "School", "Summer", "Winter"
    public GameObject boyCharacter;
    public GameObject summerDress;
    public GameObject winterDress;
    public GameObject schoolDress;
    public Animator boyAnimator;

    void Start()
    {
        initialPosition = transform.position;
    }

    void Update()
    {
        if (isDragging)
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0f; // Ensure the z position is 0 for 2D games
            transform.position = mousePosition;
        }
        Collider2D collider = GetComponent<Collider2D>();
        if (boyAnimator.GetCurrentAnimatorStateInfo(0).IsName("Idle 0") &&
           boyAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
        {
            collider.enabled = true;
        }
    }

    void OnMouseDown()
    {
        isDragging = true;
    }

    void OnMouseUp()
    {
        isDragging = false;

        if (IsDroppedOnBoy())
        {
            HandleDressChange();
        }
        else
        {
            transform.position = initialPosition; // Reset to initial position if not dropped on boy
        }
    }

    private bool IsDroppedOnBoy()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos, Vector2.zero);

        foreach (var hit in hits)
        {
            Debug.Log("Hit object: " + hit.collider.gameObject.name);
            if (hit.collider.gameObject == boyCharacter)
            {
                Debug.Log("Collider check is good");
                return true;
            }
        }
        Debug.Log("Collider check is not good");
        return false;
    }

    private void HandleDressChange()
    {
        Animator animator = boyCharacter.GetComponent<Animator>();
        if (animator != null)
        {
            switch (dressType)
            {
                case "School":
                    animator.Play("SchoolDress");
                    EnableDisableDresses(schoolDress, summerDress, winterDress);
                    DisableAllDresses(); // Disable dragging and colliders on all dresses
                    break;
                case "Summer":
                    animator.Play("red dress Sad Face Hand Movements");
                    EnableDisableDresses(summerDress, schoolDress, winterDress);
                    break;
                case "Winter":
                    animator.Play("Blue dress Sad Face Hand Movements");
                    EnableDisableDresses(winterDress, summerDress, schoolDress);
                    break;
            }
        }
        transform.position = initialPosition; // Reset position after handling the dress change
    }

    private void EnableDisableDresses(GameObject activeDress, GameObject firstInactiveDress, GameObject secondInactiveDress)
    {
        activeDress.SetActive(false);
        firstInactiveDress.SetActive(true);
        secondInactiveDress.SetActive(true);
    }

    private void DisableAllDresses()
    {
        DressDragDrop[] allDresses = FindObjectsOfType<DressDragDrop>();
        foreach (var dress in allDresses)
        {
            dress.enabled = false; // Disable dragging
            Collider2D collider = dress.GetComponent<Collider2D>();
            if (collider != null)
            {
                collider.enabled = false; // Disable collider
            }
        }
    }
}
