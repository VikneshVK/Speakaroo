using UnityEngine;
using System.Collections;

public class LeafDragAndDrop : MonoBehaviour
{
    public enum LeafType { Leaf1, Leaf2 }

    public LeafType leafType;
    public Vector3 dropOffset;
    public GameObject bin;
    public GameObject spawnPrefab;
    public Transform spawnLocation;
    public GameObject spareLeaves;
    public GameObject trashCanSmoke;
    public GameObject Boy;
    public GameObject Parrot;
    public AudioSource Audio1;
    public AudioSource Audio2;
    public AudioClip SfxAudio1;
    public AudioClip SfxAudio2;
    public HP_HelperpointerController helperPointerController;
    public GameObject glowPrefab;

    private Animator boyAnimator;
    private Animator parrotAnimator;
    private Animator leavesAnimator;
    private Animator smokeAnimator;
    private Animator binAnimator;
    private AudioSource SfxAudioSource;
    private bool SfxaudioPlaying = false;
    private AudioSource instructionAudio;
    public bool dragging = false; // Changed to public


    private Vector3 offset;
    private Vector3 startPosition;

    public Collider2D leaves1Collider; // Reference to leaves1 Collider
    public Collider2D leaves2Collider; // Reference to leaves2 Collider

    private AnchorGameObject anchorGameObjectScript;

    private void Start()
    {
        startPosition = transform.position;
        binAnimator = bin.GetComponent<Animator>();
        leavesAnimator = spareLeaves.GetComponent<Animator>();
        smokeAnimator = trashCanSmoke.GetComponent<Animator>();
        boyAnimator = Boy.GetComponent<Animator>();
        parrotAnimator = Parrot.GetComponent<Animator>();
        anchorGameObjectScript = GetComponent<AnchorGameObject>();
        SfxAudioSource = GameObject.FindWithTag("SFXAudioSource").GetComponent<AudioSource>();
        instructionAudio = helperPointerController.gameObject.GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (dragging)
        {
            binAnimator.SetBool("binOpen", true);
            Vector3 mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10);
            transform.position = Camera.main.ScreenToWorldPoint(mousePosition) + offset;
            if (SfxAudioSource != null && !SfxaudioPlaying)
            {
                SfxaudioPlaying = true;
                /*SfxAudioSource.loop = true;*/
                SfxAudioSource.clip = SfxAudio1;
                SfxAudioSource.Play();
            }
            if (leafType == LeafType.Leaf1)
            {
                helperPointerController.OnLeaf1Interacted();
            }
            else if (leafType == LeafType.Leaf2)
            {
                helperPointerController.OnLeaf2Interacted();
            }
        }
    }

    private void OnMouseDown()
    {
        if (!dragging)
        {
            Vector3 mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10);
            offset = transform.position - Camera.main.ScreenToWorldPoint(mousePosition);
            dragging = true;

            if (anchorGameObjectScript != null)
            {
                anchorGameObjectScript.enabled = false;
            }
        }
    }

    private void OnMouseUp()
    {
        dragging = false;
        binAnimator.SetBool("binOpen", false);
        if (SfxAudioSource != null)
        {
            SfxAudioSource.loop = false;
            SfxAudioSource.clip = SfxAudio2;
            SfxAudioSource.Play();
        }
        SfxaudioPlaying = false;

        // If the leaf is dropped incorrectly
        if (bin && !gameObject.GetComponent<Collider2D>().bounds.Intersects(bin.GetComponent<Collider2D>().bounds))
        {
            boyAnimator.SetTrigger("WrongDrop");
            parrotAnimator.SetTrigger("WrongDrop");
            Audio2.Play();
            transform.position = startPosition;
            StartCoroutine(Instruction());
        }
        else
        {
            // If the leaf is dropped correctly in the bin
            transform.position += dropOffset;
            boyAnimator.SetTrigger("RightDrop");
            parrotAnimator.SetTrigger("RightDrop");
            Audio1.Play();
            leavesAnimator.SetTrigger("onDust");
            smokeAnimator.SetTrigger("onDust");

            // Disable colliders for both leaves
            DisableColliders();

            // Disable the current leaf's object
            DisableObject();

            StartCoroutine(DelayedInstantiate(1.5f)); // Delay instantiation by 1.5 seconds
        }
    }


    // Disables both leaves1 and leaves2 colliders when a correct drop occurs
    private void DisableColliders()
    {
        Debug.Log("Colliders Disabled");
        if (leaves1Collider != null)
        {
            leaves1Collider.enabled = false;
        }

        if (leaves2Collider != null)
        {
            leaves2Collider.enabled = false;
        }
    }

    private void DisableObject()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }

        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
        }
    }

    private IEnumerator DelayedInstantiate(float delay)
    {
        yield return new WaitForSeconds(delay); // Wait for the specified delay
        if (spawnPrefab != null && spawnLocation != null)
        {
            Instantiate(spawnPrefab, spawnLocation.position, Quaternion.identity);
        }
    }

    private IEnumerator Instruction()
    {
        yield return new WaitForSeconds(2.5f);
        parrotAnimator.SetTrigger("onceMore");
        instructionAudio.Play();
        SpawnGlowEffect();
    }

    private void SpawnGlowEffect()
    {
        if (glowPrefab != null)
        {
            // Instantiate the glow prefab on the current game object
            GameObject glow = Instantiate(glowPrefab, transform.position, Quaternion.identity);
            glow.transform.localScale = Vector3.zero; // Start with scale 0

            // Tween the glow's scale to 8, and then scale down and destroy after 2 seconds
            LeanTween.scale(glow, new Vector3(8, 8, 1), 0.5f).setOnComplete(() =>
            {
                StartCoroutine(ScaleDownAndDestroy(glow));
            });
        }
    }

    private IEnumerator ScaleDownAndDestroy(GameObject glow)
    {
        yield return new WaitForSeconds(2f); // Wait for 2 seconds
        LeanTween.scale(glow, Vector3.zero, 0.5f).setOnComplete(() =>
        {
            Destroy(glow); // Destroy the glow object after scaling down
            helperPointerController.StartInactivityTimer();
        });
    }
}
