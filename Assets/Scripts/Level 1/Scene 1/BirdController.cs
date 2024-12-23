using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdController : MonoBehaviour
{
    public float speed = 5.0f;
    public GameObject SfxAudio;
    public AudioClip SfxAudio1;
    private SpriteRenderer Sprite;
    private Animator animator;
    private bool isFlying = false;
    private Rigidbody2D rb;
    private AudioSource SfxAudioSouce;
    public Transform brushHolder; // Reference to the brush holder
    public Transform brushContainer; // Reference to the final position of the brush
    public Collider2D brushCollider; // Reference to the brush's collider
    public Transform brush_Transform;
    public Transform droplocationTransform;
    public LVL1helperhandController helperhand;

    void Start()
    {
        animator = GetComponent<Animator>();
       
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D component not found on the GameObject");
            return; // Stop further execution to avoid accessing null reference
        }
        rb.isKinematic = true;
        rb.gravityScale = 0;
        Sprite = GetComponent<SpriteRenderer>();
        if (brushHolder == null || brushContainer == null || brushCollider == null)
        {
            Debug.LogError("Brush holder, brush container or brush collider not assigned in the inspector");
        }

        if (SfxAudio != null)
        {
            SfxAudioSouce = SfxAudio.GetComponent<AudioSource>();
        }
    }

    void Update()
    {
        if (animator.GetBool("isFlying"))
        {
            if (!isFlying)
            {
                StartFlying();
            }
            rb.velocity = Vector2.left * speed;
        }
        
    }

    public void StartFlying()
    {
        isFlying = true;
        animator.SetBool("isFlying", true);
    }

    public void StopFlying()
    {
        isFlying = false;
        animator.SetBool("isFlying", false);
        rb.velocity = Vector2.zero;
        animator.SetBool("knock", true);
        if (SfxAudioSouce != null) 
        {
            SfxAudioSouce.clip = SfxAudio1;
            SfxAudioSouce.Play();
        }        
        StartCoroutine(FlipKiki());
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("BrushHolder"))
        {
            other.GetComponent<Collider2D>().enabled = false;
            StopFlying();
            StartCoroutine(TriggerBrushKnockAtMidpoint());
        }
    }
    private IEnumerator FlipKiki()
    {
        yield return new WaitForSeconds(2.5f);
        Sprite.flipX = false;
    }
    private IEnumerator TriggerBrushKnockAtMidpoint()
    {
        /*animator.SetTrigger("knockBrush");*/
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        float knockBrushDuration = stateInfo.length;
        yield return new WaitForSeconds(knockBrushDuration / 2);

        if (brushHolder != null && brushHolder.childCount > 0)
        {
            Transform brushTransform = brushHolder.GetChild(0);
            brushTransform.SetParent(null);

            if (brushContainer != null)
            {
                LeanTween.rotateZ(brushHolder.gameObject, 0f, 0.5f);
                var tween = LeanTween.move(brushTransform.gameObject, brushContainer.position, 1.0f).setEase(LeanTweenType.easeInOutQuad);
                LeanTween.rotateZ(brushTransform.gameObject, 0f, 1.0f);

                // Wait for the tween to complete before activating the collider
                yield return new WaitForSeconds(tween.time);
                brushCollider.enabled = true; // Activate the collider
                helperhand.StartTimer(brush_Transform.position, droplocationTransform.position);
            }
            else
            {
                Debug.LogError("Brush container not assigned in the inspector.");
            }
        }
    }
}
