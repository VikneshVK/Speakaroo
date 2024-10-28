using TMPro;
using UnityEngine;
using System.Collections;

public class HP_HelperpointerController : MonoBehaviour
{
    public GameObject leaves1;
    public GameObject leaves2;
    public GameObject helperPointerPrefab;
    public GameObject glowPrefab; // Glow effect prefab
    public Collider2D trashCanCollider; // Reference to the trash can's collider
    public Transform endposition;
    public float inactivityTime = 10f; // Time before the pointer spawns, adjustable in Inspector
    public float tweenDuration = 2f; // Duration of the tween to trash can
    public TextMeshProUGUI subtitleText;

    private bool isTimerActive = false;
    private bool leaves1Interacted = false;
    private bool leaves2Interacted = false;
    private bool glowSpawned = false; // To track if glow has been spawned at half time
    private float leaves1Timer = 0f;
    private float leaves2Timer = 0f;
    private GameObject helperPointerInstance;

    private LeafDragAndDrop leaves1DragScript;
    private LeafDragAndDrop leaves2DragScript;

    private Collider2D leaves1Collider;
    private Collider2D leaves2Collider;

    private SpriteRenderer leaves1SpriteRenderer;
    private SpriteRenderer leaves2SpriteRenderer;

    private AudioSource audioSource; // Reference to the AudioSource component

    void Start()
    {
        leaves1DragScript = leaves1.GetComponent<LeafDragAndDrop>();
        leaves2DragScript = leaves2.GetComponent<LeafDragAndDrop>();

        leaves1Collider = leaves1.GetComponent<Collider2D>();
        leaves2Collider = leaves2.GetComponent<Collider2D>();

        leaves1SpriteRenderer = leaves1.GetComponent<SpriteRenderer>();
        leaves2SpriteRenderer = leaves2.GetComponent<SpriteRenderer>();

        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (isTimerActive)
        {
            // Only check for inactivity if neither leaf has been interacted with
            if (!leaves1Interacted && !leaves2Interacted)
            {
                CheckInactivity(leaves1, ref leaves1Timer, ref leaves1Interacted);
                CheckInactivity(leaves2, ref leaves2Timer, ref leaves2Interacted);
            }

            // Stop helper pointer if either leaf is being dragged
            if ((leaves1DragScript.dragging || leaves2DragScript.dragging) && helperPointerInstance != null)
            {
                Destroy(helperPointerInstance);
                StopInactivityTimer(); // Stop the timer to prevent further helper pointer spawning
            }
        }
    }

    private void CheckInactivity(GameObject leaf, ref float timer, ref bool interacted)
    {
        if (!interacted && leaf.GetComponent<Collider2D>().enabled && leaf.GetComponent<SpriteRenderer>().enabled)
        {
            timer += Time.deltaTime;

            // Spawn glow effect at half inactivity time
            if (timer >= inactivityTime / 2 && !glowSpawned)
            {
                glowSpawned = true;
                SpawnGlowEffect(leaf.transform.position);
            }

            // Spawn helper pointer at full inactivity time
            if (timer >= inactivityTime && helperPointerInstance == null)
            {
                SpawnHelperPointer(leaf.transform.position);
            }
        }
    }

    public void StartInactivityTimer()
    {
        isTimerActive = true;
        leaves1Timer = 0f;
        leaves2Timer = 0f;
        glowSpawned = false;
    }

    private void SpawnGlowEffect(Vector3 position)
    {
        GameObject glowInstance = Instantiate(glowPrefab, position, Quaternion.identity);

        // Tween scale up, wait, then tween back down
        LeanTween.scale(glowInstance, Vector3.one * 8, 0.5f)
            .setOnComplete(() =>
            {
                LeanTween.scale(glowInstance, Vector3.zero, 0.5f)
                    .setDelay(1f)
                    .setOnComplete(() => Destroy(glowInstance));
            });
    }

    private void SpawnHelperPointer(Vector3 startPosition)
    {
        helperPointerInstance = Instantiate(helperPointerPrefab, startPosition, Quaternion.identity);

        if (audioSource != null)
        {
            audioSource.Play();
            StartCoroutine(RevealTextWordByWord("Put the Dry Leaves inside the Bin", 0.5f));
        }

        Vector3 trashCanCenter = endposition.position;
        LeanTween.move(helperPointerInstance, trashCanCenter, tweenDuration).setLoopClamp();
    }

    public void StopInactivityTimer()
    {
        isTimerActive = false;
        ResetTimers();
    }

    private void ResetTimers()
    {
        leaves1Timer = 0f;
        leaves2Timer = 0f;
        leaves1Interacted = false;
        leaves2Interacted = false;
        glowSpawned = false;
    }

    public void OnLeaf1Interacted()
    {
        leaves1Interacted = true;
        ResetHelperPointerIfDragging();
    }

    public void OnLeaf2Interacted()
    {
        leaves2Interacted = true;
        ResetHelperPointerIfDragging();
    }

    private void ResetHelperPointerIfDragging()
    {
        if (helperPointerInstance != null)
        {
            Destroy(helperPointerInstance);
        }
        StopInactivityTimer(); // Stop timer to prevent further helper pointer spawning
    }

    private IEnumerator RevealTextWordByWord(string fullText, float delayBetweenWords)
    {
        subtitleText.text = "";  // Clear the text before starting
        subtitleText.gameObject.SetActive(true);  // Ensure the subtitle text is active

        string[] words = fullText.Split(' ');  // Split the full text into individual words

        // Reveal words one by one
        for (int i = 0; i < words.Length; i++)
        {
            subtitleText.text = string.Join(" ", words, 0, i + 1);  // Show only the words up to the current index
            yield return new WaitForSeconds(delayBetweenWords);  // Wait before revealing the next word
        }
        subtitleText.gameObject.SetActive(false);
    }
}
