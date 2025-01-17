using System.Collections;
using TMPro;
using UnityEngine;

public class drag_Toys : MonoBehaviour
{
    public ParticleSystem waterParticleSystem;
    public float hitDuration = 2f;
    public Transform targetTransform;
    public float moveTime = 1f;
    public float scaleTime = 0.25f;
    public float scaleDownFactor = 0.85f;
    public static int completedTweens;
    public GameObject kiki;
    public GameObject jojo;
    public bool isDragging = false;
    public AudioSource tapAudiosource;
    public SubtitleManager subtitleManager;
    public Tween_Toys tweentoys;
    public AudioClip SfxAudio1;
    public AudioSource SfxAudioSource;
    public AudioSource SfxAudioSource1;
    public GameObject glowPrefab;
    public int currentToyIndex;

    public static bool isTeddyInteracted = false;
    public static bool isDinoInteracted = false;
    public static bool isBunnyInteracted = false;

    private bool colliderHit;
    private Vector3 offset;
    private float hitTimer = 0f;
    private bool spriteChanged = false;
    private SpriteRenderer spriteRenderer;
    private Collider2D toysCollider;
    private Animator kikiAnimator;
    private Animator jojoAnimator;
    private AudioClip audioClip2;
    public bool audioplayed = false;
    private AudioSource helperAudiosource;
    private Vector3 initialPosition; // Store the initial position

    private Helper_PointerController helperController;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        toysCollider = GetComponent<Collider2D>();
        kikiAnimator = kiki.GetComponent<Animator>();
        jojoAnimator = jojo.GetComponent<Animator>();
        completedTweens = 0;
        colliderHit = false;
        audioClip2 = Resources.Load<AudioClip>("audio/Lvl3sc2/Now show your toys under the water");
        helperController = FindObjectOfType<Helper_PointerController>();
        helperAudiosource = helperController.gameObject.GetComponent<AudioSource>();
    }

    void Update()
    {
        if (hitTimer > 0 && !IsBeingHitByParticles())
        {
            hitTimer = 0f;
        }
    }

    private void OnMouseDown()
    {
        if (!spriteChanged)
        {
            initialPosition = transform.position;
            Vector3 mousePosition = Input.mousePosition;
            mousePosition.z = Camera.main.WorldToScreenPoint(transform.position).z;
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);

            offset = transform.position - worldPosition;
            isDragging = true;

            if (helperController != null)
            {
                helperController.ResetHelperHand();
                helperController.ResetAndRestartTimer();
            }
        }
    }

    private void OnMouseDrag()
    {
        if (isDragging)
        {
            Vector3 mousePosition = Input.mousePosition;
            mousePosition.z = Camera.main.WorldToScreenPoint(transform.position).z;
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition) + offset;

            transform.position = worldPosition;
        }
    }

    private void OnMouseUp()
    {
        isDragging = false;
        if (!colliderHit)
        {
            StartCoroutine(HandleMouseUpEffects());
        }
    }
    private IEnumerator HandleMouseUpEffects()
    {
        yield return BlinkAndResetPosition();
        SpawnGlow(transform.position);
        kikiAnimator.SetTrigger("helper");
        helperAudiosource.clip = audioClip2;
        helperAudiosource.Play();
    }
    private void SpawnGlow(Vector3 position)
    {
        GameObject glow = Instantiate(glowPrefab, position, Quaternion.identity);
        LeanTween.scale(glow, Vector3.one * 8, 1f).setEase(LeanTweenType.easeOutQuad);
        StartCoroutine(FadeOutAndDestroy(glow, 2f));
    }

    private IEnumerator FadeOutAndDestroy(GameObject glow, float delay)
    {
        yield return new WaitForSeconds(delay);
        SpriteRenderer sr = glow.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            LeanTween.alpha(glow, 0f, 1f).setEase(LeanTweenType.easeOutQuad).setOnComplete(() =>
            {
                Destroy(glow);
                helperController.EnableHelperHandForToy(currentToyIndex);
            });
        }
        else
        {
            Destroy(glow);
        }
    }

    private void OnParticleCollision(GameObject other)
    {
        if (other.gameObject == waterParticleSystem.gameObject)
        {
            colliderHit = true;
            hitTimer += Time.deltaTime;

            if (hitTimer >= hitDuration && !spriteChanged)
            {
                ChangeSprite();
                if (SfxAudioSource != null)
                {
                    SfxAudioSource.clip = SfxAudio1;
                    SfxAudioSource.loop = false;
                    SfxAudioSource.Play();
                }
            }
        }
    }

    private bool IsBeingHitByParticles()
    {
        return hitTimer > 0;
    }

    private void ResetPositionIfNotCollided()
    {
        if (hitTimer < hitDuration && !spriteChanged)
        {
            StartCoroutine(BlinkAndResetPosition());
        }
    }

    private IEnumerator BlinkAndResetPosition()
    {
        Color originalColor = spriteRenderer.color;
        Color blinkColor = Color.red;

        int blinkCount = 2;
        float blinkDuration = 0.2f; // Duration for each blink

        for (int i = 0; i < blinkCount; i++)
        {
            spriteRenderer.color = blinkColor;
            yield return new WaitForSeconds(blinkDuration);

            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(blinkDuration);
        }
        transform.position = initialPosition;
    }

    public bool IsInteracted()
    {
        return spriteChanged;
    }

    private void ChangeSprite()
    {
        string spriteName = "";
        switch (gameObject.tag)
        {
            case "Teddy":
                spriteName = "wet kuma";
                isTeddyInteracted = true;
                break;
            case "Dino":
                spriteName = "wet dino";
                isDinoInteracted = true;
                break;
            case "Bunny":
                spriteName = "wet bunny";
                isBunnyInteracted = true;
                break;
        }

        string path = $"Images/Lvl 3/Scene 2/{spriteName}";
        Sprite newSprite = Resources.Load<Sprite>(path);
        if (newSprite != null)
        {
            spriteRenderer.sprite = newSprite;
            spriteChanged = true;
            isDragging = false;
            toysCollider.enabled = false;

            LeanTween.move(gameObject, targetTransform.position, moveTime)
                     .setEase(LeanTweenType.easeInOutQuad)
                     .setOnComplete(() =>
                     {
                         LeanTween.scale(gameObject, transform.localScale * scaleDownFactor, scaleTime)
                                  .setEase(LeanTweenType.easeInOutQuad)
                                  .setOnComplete(() =>
                                  {
                                      LeanTween.scale(gameObject, transform.localScale, scaleTime)
                                               .setEase(LeanTweenType.easeInOutQuad)
                                               .setOnComplete(() =>
                                               {
                                                   if (helperController != null)
                                                   {
                                                       helperController.ResetAndRestartTimer();
                                                   }

                                                   completedTweens++;
                                                   if (completedTweens >= 3)
                                                   {
                                                       waterParticleSystem.Stop();
                                                       SfxAudioSource1.Stop();
                                                       kikiAnimator.SetBool("toysWashed", true);
                                                       jojoAnimator.SetBool("toysWashed", true);
                                                       if (!audioplayed)
                                                       {
                                                           tapAudiosource.Play();
                                                           subtitleManager.DisplaySubtitle("Super now, let's hang the wet toys.", "Kiki", tapAudiosource.clip);                                                           
                                                           audioplayed = true;
                                                       }
                                                   }
                                                   else
                                                   {
                                                       tweentoys.MoveToNextToy();
                                                   }
                                               });
                                  });
                     });

            LeanTween.rotate(gameObject, targetTransform.eulerAngles, moveTime).setEase(LeanTweenType.easeOutBack);
        }
        else
        {
            Debug.LogError("Sprite not found for tag: " + gameObject.tag);
        }
    }
   
}
