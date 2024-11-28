using UnityEngine;
using System.Collections;

public class Brush_Controller : MonoBehaviour
{
    public GameObject teeth;
    public GameObject SfxAudio;
    public AudioClip SfxAudio1;
    public Transform initialPosition;
    public Animator boyAnimator;
    public LayerMask interactionLayers;
    public GameObject Paste;

    private Camera mainCamera;
    private Animator animator;
    private AudioSource SfxAudioSouce;

    private bool isDragging = false;

    void Start()
    {
        mainCamera = Camera.main;
        animator = GetComponent<Animator>();
        if (SfxAudio != null)
        {
            SfxAudioSouce = SfxAudio.GetComponent<AudioSource>();
        }
    }

    void Update()
    {
        if (isDragging)
        {
            Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            transform.position = mousePosition;

            RaycastHit2D[] hits = Physics2D.RaycastAll(mousePosition, Vector2.zero, 0f, interactionLayers);
            CheckRaycastHits(hits);
        }
    }

    void OnMouseDown()
    {
        isDragging = true;
    }

    void OnMouseUp()
    {
        isDragging = false;
    }

    void CheckRaycastHits(RaycastHit2D[] hits)
    {
        foreach (var hit in hits)
        {
            if (hit.collider.gameObject.CompareTag("DropPoint"))
            {
                transform.position = hit.collider.transform.position;
                transform.rotation = Quaternion.identity;
                /*GetComponent<SpriteRenderer>().flipX = false;*/
                GetComponent<Collider2D>().enabled = false;
                if (!animator.GetBool("paste_On"))
                {
                    animator.SetBool("paste_On", true);
                    if (SfxAudioSouce != null)
                    {
                        SfxAudioSouce.clip = SfxAudio1;
                        SfxAudioSouce.Play();
                    }
                    boyAnimator.SetTrigger("dirtyTeeth");
                    hit.collider.enabled = false;
                    Destroy(Paste);
                }
            }
        }

        // If dragging ended and no relevant object was found, re-enable the collider
        if (!isDragging)
        {
            GetComponent<Collider2D>().enabled = true;
        }
    }
}
