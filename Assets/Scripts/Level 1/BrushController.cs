using UnityEngine;
using System.Collections;

public class BrushController : MonoBehaviour
{
    private bool isDragging = false;
    
    private Vector3 initialPosition;
    private Animator brushAnimator;
    public Animator boyAnimator;
    public GameObject dropTarget;
    public GameObject teeth;
    public GameObject paste;
    public Sprite brushBackSprite;
    public GameObject Foam;
    public Transform restPosition;

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        brushAnimator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        initialPosition = transform.position;
        brushBackSprite = Resources.Load<Sprite>("Images/brush back");
        CheckComponents();
    }

    void Update()
    {
        if (isDragging)
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0;
            transform.position = mousePosition;
        }

        if ( Input.GetMouseButtonDown(0))
        {
            StartDragging();
        }

        if ( Input.GetMouseButtonUp(0) && isDragging)
        {
            isDragging = false;
            CheckDrop();
        }
    }

    private void StartDragging()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Collider2D hitCollider = Physics2D.OverlapPoint(mousePosition);
        if (hitCollider != null && hitCollider.gameObject == gameObject)
        {
            isDragging = true;
            
            initialPosition = transform.position;
        }
    }

    private void CheckDrop()
    {
        if (dropTarget != null && Vector3.Distance(transform.position, dropTarget.transform.position) < 0.5f)
        {
            Debug.Log("Drop successful.");
            HandleSuccessfulDrop(dropTarget);
        }
        else if (teeth != null && Vector3.Distance(transform.position, teeth.transform.position) < 0.5f)
        {
            Debug.Log("Drop on teeth successful.");
            HandleSuccessfulDrop(teeth);
        }
        else
        {
            Debug.Log("Drop failed, returning to initial position.");
            transform.position = initialPosition;
             
        }
    }

    private void HandleSuccessfulDrop(GameObject target)
    {
        
        transform.position = target.transform.position;
        transform.rotation = Quaternion.Euler(0, 0, 0);

        if (target == dropTarget)
        {
            brushAnimator.SetTrigger("paste_On");
            teeth.SetActive(true);
            Destroy(paste);
            StartCoroutine(HandleAnimationCompletion());
        }
        else if (target == teeth)
        {
            
            Foam.SetActive(true);
            Destroy(teeth);
            StartCoroutine(DeactivateFoamAndSetBrushed());
        }        
    }

    private IEnumerator DeactivateFoamAndSetBrushed()
    {
        yield return new WaitForSeconds(5);  // Wait for 5 seconds
        Foam.SetActive(false);
        GetComponent<Collider2D>().enabled = false; // Deactivate foam

        // Reset brush to the rest position and disable dragging
        transform.position = restPosition.position;
        isDragging = false; // Ensure brush is not draggable
       

        boyAnimator.SetBool("isBrushed", true);  // Set the animator parameter
        StartCoroutine(CheckTeethShineAnimation());
    }

    private IEnumerator CheckTeethShineAnimation()
    {
        yield return new WaitUntil(() => boyAnimator.GetCurrentAnimatorStateInfo(0).IsName("TeethShine") &&
                                         boyAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f);
        // Assuming Scene_Manager script is attached to a GameObject named "SceneManager"
        Scene_Manager sceneManager = GameObject.Find("Scene_Manager").GetComponent<Scene_Manager>();
        if (sceneManager != null)
        {
            sceneManager.LoadLevel("Level 2");
        }
        else
        {
            Debug.LogError("SceneManager not found or Scene_Manager script not attached.");
        }
    }
    private IEnumerator HandleAnimationCompletion()
    {
        yield return new WaitUntil(() => brushAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f && !brushAnimator.IsInTransition(0));
       
    }

    private void CheckComponents()
    {
        if (dropTarget == null || teeth == null || paste == null || GetComponent<Collider2D>() == null || brushBackSprite == null)
        {
            Debug.LogError("One or more required components are missing or not assigned in the inspector.");
        }
    }
}
