using UnityEngine;
using System.Collections; // Include to use Coroutines

public class LeafDragAndDrop : MonoBehaviour
{
    public Vector3 dropOffset;
    public GameObject bin;
    public GameObject spawnPrefab;
    public Transform spawnLocation;
    public GameObject spareLeaves;
    public GameObject trashCanSmoke;

    private Animator leavesAnimator;
    private Animator smokeAnimator;
    private Animator binAnimator;
    private bool dragging = false;
    private Vector3 offset;
    private Vector3 startPosition;

    private void Start()
    {
        startPosition = transform.position;
        binAnimator = bin.GetComponent<Animator>();
        leavesAnimator = spareLeaves.GetComponent<Animator>();
        smokeAnimator = trashCanSmoke.GetComponent<Animator>();
    }

    private void Update()
    {
        if (dragging)
        {
            binAnimator.SetBool("binOpen", true);
            Vector3 mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10);
            transform.position = Camera.main.ScreenToWorldPoint(mousePosition) + offset;
        }
    }

    private void OnMouseDown()
    {
        if (!dragging)
        {
            Vector3 mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10);
            offset = transform.position - Camera.main.ScreenToWorldPoint(mousePosition);
            dragging = true;
        }
    }

    private void OnMouseUp()
    {
        dragging = false;
        binAnimator.SetBool("binOpen", false);

        if (bin && gameObject.GetComponent<Collider2D>().bounds.Intersects(bin.GetComponent<Collider2D>().bounds))
        {
            transform.position += dropOffset;
            leavesAnimator.SetTrigger("onDust");
            smokeAnimator.SetTrigger("onDust");
            DisableObject();

            StartCoroutine(DelayedInstantiate(1.5f)); // Delay instantiation by 1.5 seconds
        }
        else
        {
            transform.position = startPosition;
        }
    }

    private void DisableObject()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            spriteRenderer.enabled = false;

        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
            collider.enabled = false;
    }

    private IEnumerator DelayedInstantiate(float delay)
    {
        yield return new WaitForSeconds(delay); // Wait for specified delay
        if (spawnPrefab != null && spawnLocation != null)
        {
            Instantiate(spawnPrefab, spawnLocation.position, Quaternion.identity);
        }
    }
}
