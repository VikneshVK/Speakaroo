using System.Collections;
using UnityEngine;

public class SuncreamDragHandler : MonoBehaviour
{
    private Vector3 initialPosition;
    private bool isDragging;
    private Collider2D suncreamCollider;
    
    private Animator LotionAnimator;
    private LVL5Sc1_3JojoController1 jojoController;

    public Sprite sprite1;
    public Sprite sprite2;
    public Sprite sprite3;
    public GameObject glowPrefab;

    private AudioSource SfxAudioSource;
    public AudioClip SfxAudio1;

    private Transform position1;
    private Transform position2;
    private Transform position3;
    private Transform position4;

    private GameObject sunburnLeftArm;
    private GameObject sunburnFace;
    private GameObject sunburnRightArm;
    private Transform focusObject;

    private float angleForPosition2 = -41.74f;
    private float angleForPosition3 = 0f;
    private float angleForPosition4 = 41.74f;

    void Start()
    {
        initialPosition = transform.position;
        suncreamCollider = GetComponent<Collider2D>();
        /*suncreamAnimator = GetComponentInChildren<Animator>();*/
        LotionAnimator = GetComponent<Animator>();

        // Find the positions
        position1 = GameObject.Find("Position1").transform;
        position2 = GameObject.Find("Position2").transform;
        position3 = GameObject.Find("Position3").transform;
        position4 = GameObject.Find("Position4").transform;

        Debug.Log("Position1 recognized: " + (position1 != null));
        Debug.Log("Position2 recognized: " + (position2 != null));
        Debug.Log("Position3 recognized: " + (position3 != null));
        Debug.Log("Position4 recognized: " + (position4 != null));

        // Find Jojo and its components
        GameObject jojo = GameObject.FindGameObjectWithTag("Player");
        jojoController = jojo.GetComponent<LVL5Sc1_3JojoController1>();
        sunburnLeftArm = jojo.transform.Find("sunburn-left-arm").gameObject;
        sunburnFace = jojo.transform.Find("sunburn-face").gameObject;
        sunburnRightArm = jojo.transform.Find("sunburn-right-arm").gameObject;
        focusObject = jojo.transform.Find("FocusObject").gameObject.transform;
        SfxAudioSource = GameObject.FindWithTag("SFXAudioSource").GetComponent<AudioSource>();

        // Enable only position2's collider and sprite renderer initially
        SetPositionState(position1, false);
        SetPositionState(position2, true);
        SetPositionState(position3, false);
        SetPositionState(position4, false);
    }

    void Update()
    {
        if (isDragging)
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.position = new Vector3(mousePosition.x, mousePosition.y, initialPosition.z);
            RotateToTargetAngle();
        }
    }

    void OnMouseDown()
    {
        if (suncreamCollider.enabled)
        {
            isDragging = true;
        }
    }

    void OnMouseUp()
    {
        if (isDragging)
        {
            isDragging = false;
            CheckDropPosition();
        }
    }

    private void RotateToTargetAngle()
    {
        if (focusObject != null)
        {
            float targetAngle = 0f;

            if (Vector3.Distance(transform.position, position2.position) < 0.5f)
            {
                targetAngle = angleForPosition2;
            }
            else if (Vector3.Distance(transform.position, position3.position) < 0.5f)
            {
                targetAngle = angleForPosition3;
            }
            else if (Vector3.Distance(transform.position, position4.position) < 0.5f)
            {
                targetAngle = angleForPosition4;
            }

            transform.rotation = Quaternion.Euler(new Vector3(0, 0, targetAngle));
        }
    }

    private void CheckDropPosition()
    {
        float detectionRadius = 0.2f;

        suncreamCollider.enabled = false; 
        Collider2D hitCollider = Physics2D.OverlapCircle((Vector2)transform.position, detectionRadius);
        suncreamCollider.enabled = true;

        if (hitCollider != null)
        {
            Debug.Log("Hit Collider: " + hitCollider.name);
            if (SfxAudioSource != null)
            {
                SfxAudioSource.loop = false;
                SfxAudioSource.PlayOneShot(SfxAudio1);
            }

            if (hitCollider.transform == position2)
            {
                transform.position = position2.position;
                LotionAnimator.SetTrigger("LeftHand");
               
                SetPositionState(position2, false);
                StartCoroutine(ChangeSpriteAfterAnimation(sunburnLeftArm, sprite1, position2.position, position3));
            }
            else if (hitCollider.transform == position3)
            {
                transform.position = position3.position;
                LotionAnimator.SetTrigger("Head");
                
                SetPositionState(position3, false);
                StartCoroutine(ChangeSpriteAfterAnimation(sunburnFace, sprite2, position3.position, position4));
            }
            else if (hitCollider.transform == position4)
            {
                transform.position = position4.position;
                LotionAnimator.SetTrigger("RightHand");
                
                SetPositionState(position4, false);
                StartCoroutine(ChangeSpriteAfterAnimation(sunburnRightArm, sprite3, position4.position, null));
                
            }
            else
            {
                ResetPosition();
            }
        }
        else
        {
            ResetPosition();
        }
    }

    private void SetPositionState(Transform position, bool state)
    {
        Collider2D collider = position.GetComponent<Collider2D>();
        SpriteRenderer spriteRenderer = position.GetComponent<SpriteRenderer>();

        if (collider != null) collider.enabled = state;
        if (spriteRenderer != null) spriteRenderer.enabled = state;
    }

    private IEnumerator ChangeSpriteAfterAnimation(GameObject sunburnObject, Sprite newSprite, Vector3 resetPosition, Transform nextPosition = null)
    {
        yield return new WaitForSeconds(LotionAnimator.GetCurrentAnimatorStateInfo(0).length);

        SpriteRenderer sunburnSpriteRenderer = sunburnObject.GetComponent<SpriteRenderer>();
        if (sunburnSpriteRenderer != null)
        {
            sunburnSpriteRenderer.sprite = newSprite;
        }

        initialPosition = resetPosition;

        if (sunburnSpriteRenderer != null)
        {
            LeanTween.value(sunburnObject, 1f, 0f, 2f)
                .setOnUpdate((float alpha) =>
                {
                    Color color = sunburnSpriteRenderer.color;
                    color.a = alpha;
                    sunburnSpriteRenderer.color = color;
                })
                .setOnComplete(() =>
                {
                    Destroy(sunburnObject);
                    if (nextPosition != null)
                    {
                        SetPositionState(nextPosition, true); 
                        SpawnGlowAtPosition(nextPosition.position);
                    }
                    else
                    {
                        jojoController.minigameComplete = true;
                        Destroy(gameObject);
                    }
                });
        }
    }

    private void SpawnGlowAtPosition(Vector3 position)
    {
        GameObject glow = Instantiate(glowPrefab, position, Quaternion.identity);
        glow.transform.localScale = Vector3.zero;

        LeanTween.scale(glow, Vector3.one * 4, 0.5f) 
            .setOnComplete(() =>
            {
                StartCoroutine(ScaleDownGlow(glow));
            });
    }

    private IEnumerator ScaleDownGlow(GameObject glow)
    {
        yield return new WaitForSeconds(2f); 

        LeanTween.scale(glow, Vector3.zero, 0.5f).setOnComplete(() =>
        {
            Destroy(glow); 
            suncreamCollider.enabled = true; 
        });
    }

    private void ResetPosition()
    {
        transform.position = initialPosition;
    }
}
