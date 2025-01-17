using UnityEngine;
using System.Collections;

public class DustRemover : MonoBehaviour
{
    [Header("Brush Prefab")]
    [SerializeField] private GameObject brushPrefab;
    [SerializeField] private Transform brushSpawnPoint; // Position outside the viewport
    public LVL6Sc2Helperhand helperhand;
    public GameObject blackoutpanel;
    private GameObject spawnedBrush;
    private AudioSource SfxAudioSource;
    public AudioClip SfxAudio1;
    
    private Lvl6QuestManager questManager;
    private ParticleSystem particleEffect;

    private void Start()
    {
        questManager = FindObjectOfType<Lvl6QuestManager>();
        SfxAudioSource = GameObject.FindWithTag("SFXAudioSource").GetComponent<AudioSource>();
    }

    private void OnMouseDown()
    {
        if (spawnedBrush == null)
        {
            // Spawn and tween brush to this position
            spawnedBrush = Instantiate(brushPrefab, brushSpawnPoint.position, Quaternion.identity);
            helperhand.DestroyAndResetTimer();
            LeanTween.move(spawnedBrush, transform.position, 1.0f).setOnComplete(() =>
            {
                StartCoroutine(BrushRoutine());
            });

            // Disable other colliders when this one is clicked
            questManager.DisableOtherColliders(this.transform);
        }
    }

    private IEnumerator BrushRoutine()
    {
        // Get the sprite renderer of the first child
        SpriteRenderer firstChildSpriteRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();

        if (firstChildSpriteRenderer != null)
        {
            // Tween the opacity of the first child to 0 over 2 seconds
            LeanTween.value(gameObject, 1f, 0f, 2f).setOnUpdate((float alpha) =>
            {
                Color color = firstChildSpriteRenderer.color;
                color.a = alpha;
                firstChildSpriteRenderer.color = color;
            });
        }
        else
        {
            Debug.LogWarning("First child does not have a SpriteRenderer component.");
        }

        // Animate the brush
        Animator brushAnimator = spawnedBrush.GetComponent<Animator>();
        brushAnimator.SetBool("Brush", true);
        if (SfxAudioSource != null)
        {
            SfxAudioSource.loop = false;
            SfxAudioSource.PlayOneShot(SfxAudio1);
        }
        yield return new WaitForSeconds(2f);

        brushAnimator.SetBool("Brush", false);

        yield return new WaitForSeconds(0.5f);

        // Move brush back to spawn point
        LeanTween.move(spawnedBrush, brushSpawnPoint.position, 1.0f).setOnComplete(() =>
        {
            Destroy(spawnedBrush);
            spawnedBrush = null;

            // Perform dust removal on children
            RemoveDust();
        });
    }


    // Assuming this is within DustRemover and calling QuestValidation
    private void RemoveDust()
    {
        if (transform.childCount < 2) return; // Ensure there are at least two children

        // Disable the first child instead of destroying it
        transform.GetChild(0).gameObject.SetActive(false);

        // Tween the second child (spawned prefab) to the center of the viewport
        GameObject spawnedPrefab = transform.GetChild(1).gameObject; // Use the second child
        Vector3 originalScale = spawnedPrefab.transform.localScale;
        Vector3 viewportCenter = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, Camera.main.nearClipPlane)); // Center in viewport

        // Get the sprite renderer and change the order in layer
        SpriteRenderer spriteRenderer = spawnedPrefab.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = 10; // Set order in layer
        }

        // Manually find the particle effect by tag within the spawned prefab's children
        foreach (Transform child in spawnedPrefab.transform)
        {
            if (child.CompareTag("Respawn"))
            {
                particleEffect = child.GetComponent<ParticleSystem>();
                break;
            }
        }

        // Tween to the center of the viewport
        LeanTween.move(spawnedPrefab, viewportCenter, 1.0f);
        LeanTween.scale(spawnedPrefab, originalScale * 2.5f, 1.0f).setOnComplete(() =>
        {
            // Enable particle effect if found
            if (particleEffect != null)
            {
                particleEffect.Play();
                
            }
            blackoutpanel.SetActive(true);
            spawnedPrefab.GetComponent<SpriteRenderer>().sortingOrder = 15;

            // Call QuestValidation after tweening is complete
            // Pass 'gameObject' as 'clickedObject' reference
            questManager.QuestValidation(ItemName(), this.gameObject);
        });
    }





    private string ItemName()
    {
        string name = transform.GetChild(1).gameObject.name;
        return name.Replace("(Clone)", "").Trim();
    }
}
