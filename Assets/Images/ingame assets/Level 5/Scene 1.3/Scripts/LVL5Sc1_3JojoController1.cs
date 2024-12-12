using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class LVL5Sc1_3JojoController1 : MonoBehaviour
{
    public Transform stopPosition;
    public GameObject prefab1;
    public GameObject Suncream;
    public Transform prefabSpawnLocation;
    public Transform suncreamSpawnLocation;
    public float walkSpeed = 2f;
    public Image Kiki;
    public GameObject glowPrefab;
    public GameObject Mom;

    public AudioClip Audio1;
    public AudioClip Audio2;
    public AudioClip Audio3;
    public AudioClip Audio4;
    public AudioClip Audio5;
    private AudioSource boyAudioSource;
    public TextMeshProUGUI subtitleText;

    public Transform position1;
    public Transform position2;
    private AudioSource SfxAudioSource;
    public AudioClip SfxAudio1;
    private bool walkingSfxPlayed = false;

    private Animator animator;
    private Animator kikiAnimator;
    private Animator MomAnimator;

    private bool canWalk;
    public bool suncreamSpawned;
    public bool minigameComplete;
    private bool finalDialogueTriggered;
    private GameObject spawnedSuncream;

    void Start()
    {
        animator = GetComponent<Animator>();
        boyAudioSource = GetComponent<AudioSource>();
        kikiAnimator = Kiki.GetComponent<Animator>();
        MomAnimator = Mom.GetComponent<Animator>();
        canWalk = true;
        minigameComplete = false;
        suncreamSpawned = false;
        finalDialogueTriggered = false;
        animator.SetBool("canWalk", true);
        subtitleText.text = "";
        SfxAudioSource = GameObject.FindWithTag("SFXAudioSource").GetComponent<AudioSource>();
    }
    void Update()
    {
        if (canWalk)
        {
            if (!walkingSfxPlayed)
            {
                SfxAudioSource.loop = true;
                walkingSfxPlayed = true;
                SfxAudioSource.clip = SfxAudio1;
                SfxAudioSource.Play();
            }

            WalkToPosition();
        }

        if (suncreamSpawned)
        {
            MomAnimator.SetTrigger("suncream");
            StartCoroutine(GiveSuncreamAnimation());            
            suncreamSpawned = false; // Reset to prevent repeated calls
        }
        if (minigameComplete && !finalDialogueTriggered)
        {
            finalDialogueTriggered = true; // Ensure this block runs only once
            StartCoroutine(PlayFinalDialogue());
        }
    }

    private IEnumerator GiveSuncreamAnimation()
    {
        yield return new WaitForSeconds(1f);
        HandleSuncreamSpawn();
    }

    private void WalkToPosition()
    {
        Vector3 currentPosition = transform.position;
        currentPosition.x = Mathf.MoveTowards(currentPosition.x, stopPosition.position.x, walkSpeed * Time.deltaTime);
        transform.position = currentPosition;


        if (Mathf.Abs(transform.position.x - stopPosition.position.x) <= 0.5f)
        {
            SfxAudioSource.Stop();
            walkingSfxPlayed = false;
            animator.SetBool("canWalk", false);
            canWalk = false;
            TweenBird();
        }
    }

    public void TweenBird()
    {

        RectTransform kikiRectTransform = Kiki.GetComponent<RectTransform>();
        LeanTween.moveY(kikiRectTransform, 150, 1f).setOnComplete(() =>
        {

            kikiAnimator.SetTrigger("canTalk");
            boyAudioSource.clip = Audio1;
            boyAudioSource.Play();
            StartCoroutine(RevealTextWordByWord("Oh No! Its too Hot", 0.5f));
            StartCoroutine(ReturnBirdToStartPosition());
        });
    }

    private IEnumerator ReturnBirdToStartPosition()
    {
        yield return new WaitForSeconds(3f);

        RectTransform kikiRectTransform = Kiki.GetComponent<RectTransform>();
        LeanTween.moveY(kikiRectTransform, -225, 1f).setOnComplete(() =>
        {

            animator.SetTrigger("canTalk");

            boyAudioSource.clip = Audio2;
            boyAudioSource.Play();
            StartCoroutine(RevealTextWordByWord("Let's ask Mom for Sunscreen", 0.5f));
            StartCoroutine(ScaleChildrenAndSpawnPrefab());
        });
    }

    private IEnumerator ScaleChildrenAndSpawnPrefab()
    {
        float animationLength = animator.GetCurrentAnimatorStateInfo(0).length;

        yield return new WaitForSeconds(animationLength);

        Instantiate(prefab1, prefabSpawnLocation.position, Quaternion.identity);
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(true);
        }
    }
    private void HandleSuncreamSpawn()
    {
        Quaternion rotation = Quaternion.Euler(0, 0, 0);
        spawnedSuncream = Instantiate(Suncream, suncreamSpawnLocation.position, rotation);

        LeanTween.move(spawnedSuncream, position1.position, 1f).setOnComplete(() =>
        {
            EnableSpriteRenderer(position2);
            SpawnGlowAtPosition(position2.position);
            TweenBird2();
        });
    }
    private void SpawnGlowAtPosition(Vector3 position)
    {
        GameObject glow = Instantiate(glowPrefab, position, Quaternion.identity);
        glow.transform.localScale = Vector3.zero;

        LeanTween.scale(glow, Vector3.one * 4, 0.5f) // Scale up to 4
            .setOnComplete(() =>
            {
                StartCoroutine(ScaleDownGlow(glow));
            });
    }

    private IEnumerator ScaleDownGlow(GameObject glow)
    {
        yield return new WaitForSeconds(2f); // Wait for 2 seconds

        LeanTween.scale(glow, Vector3.zero, 0.5f).setOnComplete(() =>
        {
            Destroy(glow);
        });
    }

    private void EnableSpriteRenderer(Transform position)
    {
        SpriteRenderer spriteRenderer = position.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }
    }

    public void TweenBird2()
    {
        RectTransform kikiRectTransform = Kiki.GetComponent<RectTransform>();
        LeanTween.moveY(kikiRectTransform, 150, 1f).setOnComplete(() =>
        {
            kikiAnimator.SetTrigger("canTalk2");
            boyAudioSource.clip = Audio3;
            boyAudioSource.Play();
            StartCoroutine(RevealTextWordByWord("Put on the Sunscreen", 0.5f));
            StartCoroutine(ReturnBirdToStartPosition2());
        });
    }

    private IEnumerator ReturnBirdToStartPosition2()
    {
        yield return new WaitForSeconds(3f);

        RectTransform kikiRectTransform = Kiki.GetComponent<RectTransform>();
        LeanTween.moveY(kikiRectTransform, -225, 1f).setOnComplete(() =>
        {
            if (spawnedSuncream != null)
            {
                Collider2D collider = spawnedSuncream.GetComponent<Collider2D>();
                if (collider != null)
                {
                    collider.enabled = true;
                }
            }
        });
    }

    private IEnumerator PlayFinalDialogue()
    {
        animator.SetTrigger("finalDialogue");

        boyAudioSource.clip = Audio4;
        boyAudioSource.Play();
        StartCoroutine(RevealTextWordByWord("Ahh.! Much Better", 0.5f));

        float animationLength = animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(animationLength);

        boyAudioSource.clip = Audio5;
        boyAudioSource.Play();
        StartCoroutine(RevealTextWordByWord("Let's go In...!", 0.5f));
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
