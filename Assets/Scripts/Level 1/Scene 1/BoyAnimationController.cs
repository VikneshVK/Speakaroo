using UnityEngine;
using System.Collections;
using TMPro;

public class BoyAnimationController : MonoBehaviour
{
    private Animator animator;
    private Camera mainCamera;
    private Vector3 targetPosition;
    public float walkDuration = 2f;
    public float zoomSize = 5f;
    public float zoomDuration = 2f;
    public Animator BirdAnimator;
    public TextMeshProUGUI subtitleText;
    public AudioClip audio1;
    public AudioClip audio2;
    public AudioClip audio3;
    public AudioClip audio4;
    public AudioClip audio5;
    public AudioClip FinalDialouge;
    public Lvl1Sc1AudioManager audiomanager;
    public GameObject speechBubblePrefab;
    public Transform speechBubbleContainer;
    private bool isPrefabSpawned;
    private float originalOrthographicSize;
    private SpriteRenderer boySprite;
    private bool isAudio2Played;
    private bool isAudio3Played;
    private bool isAudio4Played;
    private bool isAudio5Played;
    private void Start()
    {
        animator = GetComponent<Animator>();
        mainCamera = Camera.main;
        boySprite = GetComponent<SpriteRenderer>();
        if (animator == null)
        {
            Debug.LogError("Animator component missing on Boy object.");
            return;
        }

        if (speechBubblePrefab == null || speechBubbleContainer == null)
        {
            Debug.LogError("Speech bubble prefab or container is not assigned.");
            return;
        }

        isPrefabSpawned = false;
        isAudio2Played = false;
        isAudio3Played = false;
        isAudio4Played = false;
        originalOrthographicSize = mainCamera.orthographicSize;
        targetPosition = new Vector3(0, transform.position.y, transform.position.z); // Center of the viewport    

        // Start the scene by walking to the center
        StartCoroutine(WalkToCenter());
    }
    private void Update()
    {
        var animatorStateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (animatorStateInfo.IsName("hand reaching") && animatorStateInfo.normalizedTime < 0.1f && !isAudio3Played)
        {
            isAudio3Played = true;
            audiomanager.PlayAudio(audio3);            
        }
        // Play audio at the start of the "Talk" animation
        if (animatorStateInfo.IsName("Talk") && animatorStateInfo.normalizedTime < 0.1f && !isAudio2Played)
        {
            isAudio2Played = true;
            audiomanager.PlayAudio(audio2);
            StartCoroutine(RevealTextWordByWord("Uh..! I can't reach it, Lets ask Kiki", 0.5f));
        }

        // Spawn the speech bubble at the end of the "Talk" animation
        if (animatorStateInfo.IsName("Talk") && animatorStateInfo.normalizedTime >= 0.9f && !isPrefabSpawned)
        {
            isPrefabSpawned = true;
            SpawnSpeechBubble();
        }

        if (BirdAnimator.GetCurrentAnimatorStateInfo(0).IsName("birdKnock") &&
            BirdAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.1f && !isAudio4Played)
        {
            isAudio4Played = true;
            audiomanager.PlayAudio(audio4);
            StartCoroutine(RevealTextWordByWord("Here you go...! JoJo ", 0.5f));
        }
        if (animatorStateInfo.IsName("FinalDialouge") && animatorStateInfo.normalizedTime < 0.1f && !isAudio5Played)
        {
            isAudio5Played = true;
            audiomanager.PlayAudio(FinalDialouge);
            StartCoroutine(RevealTextWordByWord("WOW..! My Teeth looks so Clean, Thank You Kiki and Friend ", 0.5f));
        }

        // Reset flags when the "Talk" animation ends
        if (!animatorStateInfo.IsName("Talk"))
        {
            isAudio2Played = false;
            isAudio3Played = false;
            isPrefabSpawned = false;
        }
    }
    private IEnumerator WalkToCenter()
    {
        boySprite.flipX = true;
        animator.SetBool("isWalking", true);
        Vector3 startPosition = transform.position;
        float elapsedTime = 0;

        while (elapsedTime < walkDuration)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / walkDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;
        boySprite.flipX = false;
        animator.SetBool("isWalking", false);
        BirdAnimator.SetTrigger("GoodMorning");
        audiomanager.PlayAudio(audio5);
        StartCoroutine(RevealTextWordByWord("Hi Friend, Let's help JoJo brush his Teeth ", 0.5f));
        OnTalkAnimationEnd();
    }

    // This method should be called by an Animation Event at the end of the Talk animation
    public void OnTalkAnimationEnd()
    {
        StartCoroutine(HandlePostTalk());
    }

    private IEnumerator HandlePostTalk()
    {
        yield return new WaitForSeconds(6f);
        StartCoroutine(ZoomIn());
    }

    private IEnumerator ZoomIn()
    {
        float startOrthographicSize = mainCamera.orthographicSize;

        float elapsedTime = 0;

        while (elapsedTime < zoomDuration)
        {
            mainCamera.orthographicSize = Mathf.Lerp(startOrthographicSize, zoomSize, elapsedTime / zoomDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        mainCamera.orthographicSize = zoomSize;
        animator.SetBool("showTeeth", true);        
        yield return new WaitForSeconds(3.5f);
        StartCoroutine(ZoomOut());
    }

    private IEnumerator ZoomOut()
    {
        float startOrthographicSize = mainCamera.orthographicSize;
        float elapsedTime = 0;

        while (elapsedTime < zoomDuration)
        {
            mainCamera.orthographicSize = Mathf.Lerp(startOrthographicSize, originalOrthographicSize, elapsedTime / zoomDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        mainCamera.orthographicSize = originalOrthographicSize;

        animator.SetBool("showTeeth", false);
        audiomanager.PlayAudio(audio1);
        StartCoroutine(RevealTextWordByWord("Oh No My Teeth is Yellow..! Let's Brush", 0.5f));
    }

    private void SpawnSpeechBubble()
    {
        if (speechBubblePrefab != null && speechBubbleContainer != null)
        {
            Instantiate(speechBubblePrefab, speechBubbleContainer.position, Quaternion.identity, speechBubbleContainer);
        }
        else
        {
            Debug.LogError("Speech bubble prefab or container is not assigned.");
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
