using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Scratch_Card : MonoBehaviour
{
    public GameObject maskPrefab; // Assign the mask prefab in the Inspector
    private HashSet<Vector3> instantiatedPositions = new HashSet<Vector3>();
    private float positionTolerance = 0.1f;
    private bool isSpawning = false;
    private bool timerStarted = false;
    public GameObject retryButton;
    private AudioSource audioSource;
    private Image buttonImage;
    private Image ringImage;
    private void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void Start()
    {
        buttonImage = retryButton.GetComponent<Image>();
        ringImage = retryButton.transform.Find("Ring").GetComponent<Image>();
    }
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);
            if (hit.collider != null && hit.collider.transform == transform)
            {
                if (!isSpawning)
                {
                    StartCoroutine(SpawnPrefabs());
                }

                if (!timerStarted)
                {
                    StartCoroutine(DestroyCardAfterDelay(2f));
                    timerStarted = true;
                }
            }
        }
        else
        {
            isSpawning = false;
        }
    }

    private IEnumerator SpawnPrefabs()
    {
        isSpawning = true;
        retryButton.GetComponent<Button>().interactable = false; // Disable interaction
        
        SetAlpha(buttonImage, 20);
        SetAlpha(ringImage, 20);
        
        ST_AudioManager.Instance.PlayScratchAudio();

        while (isSpawning)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);
            if (hit.collider != null && hit.collider.transform == transform)
            {
                Vector3 spawnPos = new Vector3(hit.point.x, hit.point.y, hit.collider.transform.position.z);
                if (!AlreadyHasMaskAtPoint(spawnPos))
                {
                    GameObject instantiatedMask = Instantiate(maskPrefab, spawnPos, Quaternion.identity);
                    instantiatedMask.transform.SetParent(transform);
                    instantiatedPositions.Add(spawnPos);
                }
            }

            yield return null;
        }
    }

    private bool AlreadyHasMaskAtPoint(Vector3 point)
    {
        foreach (Vector3 pos in instantiatedPositions)
        {
            if (Vector3.Distance(pos, point) < positionTolerance)
            {
                return true;
            }
        }
        return false;
    }

    private IEnumerator DestroyCardAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        Transform parentTransform = transform.parent; // Get the parent transform before destroying
        Destroy(gameObject); // Destroy this gameObject (card_1_front or card_2_front)

        if (parentTransform != null)
        {
            ST_AudioManager.Instance.PlayRevealAudio(parentTransform.tag); // Play reveal audio after destruction
        }
    }

    private void SetAlpha(Image image, float alphaValue)
    {
        Color color = image.color;
        color.a = alphaValue / 255f;
        image.color = color;
    }
}
