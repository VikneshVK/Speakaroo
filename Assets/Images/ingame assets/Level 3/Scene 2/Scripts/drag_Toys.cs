using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class drag_Toys : MonoBehaviour
{
    public ParticleSystem waterParticleSystem;
    public float hitDuration = 2f;
    public Transform targetTransform;
    public float moveTime = 1f;
    public float scaleTime = 0.5f;
    public float scaleDownFactor = 0.85f;
    public static int completedTweens;
    public GameObject kiki;
    public GameObject jojo;
    public bool isDragging = false;
    public AudioSource tapAudiosource;

    public static bool isTeddyInteracted = false;
    public static bool isDinoInteracted = false;
    public static bool isBunnyInteracted = false;

    private Vector3 offset;
    private float hitTimer = 0f;
    private bool spriteChanged = false;
    private SpriteRenderer spriteRenderer;
    private Collider2D toysCollider;
    private Animator kikiAnimator;
    private Animator jojoAnimator;
    public bool audioplayed = false;


    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        toysCollider = GetComponent<Collider2D>();
        kikiAnimator = kiki.GetComponent<Animator>();
        jojoAnimator = jojo.GetComponent<Animator>();   
        completedTweens = 0;
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
            Vector3 mousePosition = Input.mousePosition;
            mousePosition.z = Camera.main.WorldToScreenPoint(transform.position).z;
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);

            offset = transform.position - worldPosition;
            isDragging = true;

            Helper_PointerController helperController = FindObjectOfType<Helper_PointerController>();
            if (helperController != null)
            {
                helperController.ResetHelperHand();
                helperController.ResetAndRestartTimer(isToyInteraction: true);
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
    }

    private void OnParticleCollision(GameObject other)
    {
        if (other.gameObject == waterParticleSystem.gameObject)
        {
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
                     .setEase(LeanTweenType.easeOutBack)
                     .setOnComplete(() =>
                     {
                         LeanTween.scale(gameObject, transform.localScale * scaleDownFactor, scaleTime)
                                  .setEase(LeanTweenType.easeOutBack)
                                  .setOnComplete(() =>
                                  {
                                      LeanTween.scale(gameObject, transform.localScale, scaleTime)
                                               .setEase(LeanTweenType.easeOutBack)
                                               .setOnComplete(() =>
                                               {
                                                   Helper_PointerController helperController = FindObjectOfType<Helper_PointerController>();
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
                                                           audioplayed = true;
                                                       }

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
