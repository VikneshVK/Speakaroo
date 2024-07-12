using UnityEngine;

public class ShampooController : MonoBehaviour
{
    public GameObject foamPrefab; // Reference to the foam prefab
    public Transform foamSpawnLocation; // Reference to the spawn location
    public Transform shampooFinalPosition; // Position to reset the shampoo to
    public Vector3 initialPosition; // Initial position of the shampoo
    public int maxFoamCount = 6; // Maximum number of foams to spawn
    public float spawnCooldown = 0.5f; // Cooldown time in seconds between spawns
    public GameObject boyGameObject; // Reference to the boy GameObject
    public GameObject showerGameObject; // Reference to the shower GameObject

    private bool isDragging = false;
    private Vector3 offset;
    private Camera mainCamera;
    private int foamCount = 0; // Current count of spawned foams
    private bool isDraggable = true;
    private float nextSpawnTime = 0f; // Time when the next foam can be spawned
    private Collider2D shampooCollider; // Collider of the shampoo

    private ShowerMechanics showerMechanics; // ShowerMechanics script reference
    private ShowerController showerController; // ShowerController script reference

    private void Start()
    {
        mainCamera = Camera.main;
        initialPosition = transform.position;
        shampooCollider = GetComponent<Collider2D>();

        // Ensure the shower GameObject is active before fetching components
        if (showerGameObject.activeSelf)
        {
            showerMechanics = boyGameObject.GetComponent<ShowerMechanics>();
            showerController = showerGameObject.GetComponent<ShowerController>();
        }

        if (showerMechanics == null)
            Debug.LogError("ShowerMechanics script not found on " + showerGameObject.name);
        if (showerController == null)
            Debug.LogError("ShowerController script not found on " + showerGameObject.name);

    }

    private void OnMouseDown()
    {
        if (isDraggable)
        {
            isDragging = true;
            offset = transform.position - GetMouseWorldPos(); // Calculate offset
        }
    }

    private void OnMouseUp()
    {
        isDragging = false;
    }

    private void Update()
    {
        if (isDragging && isDraggable)
        {
            Vector3 mousePos = GetMouseWorldPos() + offset;
            transform.position = new Vector3(mousePos.x, mousePos.y, initialPosition.z); // Dragging movement

            if (Time.time >= nextSpawnTime)
            {
                RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos, Vector2.zero);
                foreach (var hit in hits)
                {
                    if (hit.collider.CompareTag("Hair"))
                    {
                        if (foamCount < maxFoamCount)
                        {
                            SpawnFoam();
                            if (foamCount == 1) // Trigger script switch on first foam
                            {
                                showerMechanics.enabled = false;
                                showerController.enabled = true;
                            }
                        }
                        else
                        {
                            ResetPosition(); // Reset position and disable interactions
                            return;
                        }
                        break;
                    }
                }
            }
        }
    }

    private void SpawnFoam()
    {
        Vector3 randomPosition = foamSpawnLocation.position + new Vector3(
            Random.Range(-foamSpawnLocation.localScale.x / 2, foamSpawnLocation.localScale.x / 2),
            Random.Range(-foamSpawnLocation.localScale.y / 2, foamSpawnLocation.localScale.y / 2),
            0
        );
        GameObject foamInstance = Instantiate(foamPrefab, randomPosition, Quaternion.identity);
        foamInstance.transform.localScale = Vector3.zero;
        LeanTween.scale(foamInstance, new Vector3(1, 1, 1), 0.5f); // Animate foam appearance
        foamCount++;
        showerController.AddFoamObject(foamInstance); // Add foam to shower controller
        nextSpawnTime = Time.time + spawnCooldown; // Update next spawn time
    }

    private Vector3 GetMouseWorldPos()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = mainCamera.nearClipPlane;
        return mainCamera.ScreenToWorldPoint(mousePoint);
    }

    private void ResetPosition()
    {
        transform.position = shampooFinalPosition.position; // Reset the position
        transform.rotation = Quaternion.identity; // Reset the rotation

        isDragging = false; // Stop dragging
        isDraggable = false; // Make non-draggable
        shampooCollider.enabled = false; // Deactivate the collider
    }
}
