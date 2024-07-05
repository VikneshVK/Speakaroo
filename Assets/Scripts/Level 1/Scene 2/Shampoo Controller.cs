using UnityEngine;

public class ShampooController : MonoBehaviour
{
    public GameObject foamPrefab; // Reference to the foam prefab
    public Transform foamSpawnLocation; // Reference to the spawn location
    public Transform shampooFinalPosition;
    public Vector3 initialPosition; // Initial position of the shampoo
    public int maxFoamCount = 6; // Maximum number of foams to spawn
    public float spawnCooldown = 0.5f; // Cooldown time in seconds between spawns
    public ShowerController showerController; // Reference to the ShowerController

    private bool isDragging = false;
    private Vector3 offset;
    private Camera mainCamera;
    private int foamCount = 0; // Current count of spawned foams
    private bool isDraggable = true;
    private float nextSpawnTime = 0f; // Time when the next foam can be spawned
    private Collider2D shampooCollider;

    private void Start()
    {
        mainCamera = Camera.main;
        initialPosition = transform.position; // Store the initial position
        shampooCollider = GetComponent<Collider2D>(); // Get the shampoo's collider
    }

    private void OnMouseDown()
    {
        if (isDraggable)
        {
            isDragging = true;
            offset = transform.position - GetMouseWorldPos();
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
            transform.position = new Vector3(mousePos.x, mousePos.y, initialPosition.z); // Keep the original z position

            if (Time.time >= nextSpawnTime)
            {
                RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos, Vector2.zero);
                foreach (var hit in hits)
                {
                    if (hit.collider.CompareTag("Hair"))
                    {
                        if (foamCount < maxFoamCount)
                        {
                            Vector3 randomPosition = foamSpawnLocation.position + new Vector3(
                                Random.Range(-foamSpawnLocation.localScale.x / 2, foamSpawnLocation.localScale.x / 2),
                                Random.Range(-foamSpawnLocation.localScale.y / 2, foamSpawnLocation.localScale.y / 2),
                                0
                            );

                          /*  Quaternion randomRotation = Quaternion.Euler(0, 0, Random.Range(0, 360));*/

                            GameObject foamInstance = Instantiate(foamPrefab, randomPosition, Quaternion.identity);
                            foamInstance.transform.localScale = Vector3.zero;
                            LeanTween.scale(foamInstance, new Vector3(1, 1, 1), 0.5f); // Tween the scale from 0 to 1 over 0.5 seconds
                            foamCount++;
                            Debug.Log("Foam spawned: " + foamCount);

                            showerController.AddFoamObject(foamInstance); // Add the foam instance to the shower controller

                            nextSpawnTime = Time.time + spawnCooldown; // Set the next spawn time
                        }
                        if (foamCount >= maxFoamCount)
                        {
                            ResetPosition();
                            return;
                        }
                        break;
                    }
                }
            }
        }
    }

    private Vector3 GetMouseWorldPos()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = mainCamera.nearClipPlane;
        return mainCamera.ScreenToWorldPoint(mousePoint);
    }

    private void ResetPosition()
    {
        transform.SetLocalPositionAndRotation(shampooFinalPosition.position, Quaternion.identity);// Reset the position to the initial position

        isDragging = false; // Stop dragging
        isDraggable = false; // Make the shampoo non-draggable
        shampooCollider.enabled = false; // Deactivate the collider
    }
}
