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
    public TextMeshProUGUI subtitleText;
    public Tween_Toys tweentoys;

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
    public bool audioplayed = false;

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

        helperController = FindObjectOfType<Helper_PointerController>();
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
            StartCoroutine(BlinkAndResetPosition());
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
                                                       kikiAnimator.SetBool("toysWashed", true);
                                                       jojoAnimator.SetBool("toysWashed", true);
                                                       if (!audioplayed)
                                                       {
                                                           tapAudiosource.Play();
                                                           StartCoroutine(RevealTextWordByWord("Super now, let's hang the wet toys", 0.5f));
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

    private IEnumerator RevealTextWordByWord(string fullText, float delayBetweenWords)
    {
        subtitleText.text = "";
        subtitleText.gameObject.SetActive(true);

        string[] words = fullText.Split(' ');

        for (int i = 0; i < words.Length; i++)
        {
            subtitleText.text = string.Join(" ", words, 0, i + 1);
            yield return new WaitForSeconds(delayBetweenWords);
        }
        subtitleText.text = "";
    }
}
