using UnityEngine;
using System.Collections;
using TMPro;
using Firebase.Analytics;

public class LVL6Sc2KikiController : MonoBehaviour
{
    [SerializeField] private float tweenDuration = 1.0f;
    [SerializeField] private AudioClip audioClip1; // First tween audio
    private AudioSource audioSource;
    public Lvl6QuestManager Lvl6QuestManager;
    public LVL6Sc2Helperhand helperHandController;
    public TextMeshProUGUI subtitleText;
    private Animator animator;

    private Vector2 outsideViewportPosition = new Vector2(180, -275); // Position outside viewport
    private Vector2 insideViewportPosition = new Vector2(180, 175);   // Position inside viewport

    private void Start()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();

        rectTransform.anchoredPosition = outsideViewportPosition;

        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        StartCoroutine(HandleTweens());
    }

    private IEnumerator HandleTweens()
    {  
        TweenBirdandback("FirstTalk");
        yield return new WaitForSeconds(0.5f);
        audioSource.clip = audioClip1;
        audioSource.Play();
        StartCoroutine(RevealTextWordByWord("Something is under the Sand", 0.5f));
        yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName("FirstTalk") &&
                                        animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.9f);

        yield return new WaitForSeconds(1f);

        Lvl6QuestManager.SpawnItems();
    }

    public void TweenBirdandback(string parameter)
    {
        RectTransform rectTransform = GetComponent<RectTransform>();

        LeanTween.value(gameObject, rectTransform.anchoredPosition.y, insideViewportPosition.y, tweenDuration).setOnUpdate((float val) =>
        {
            rectTransform.anchoredPosition = new Vector2(outsideViewportPosition.x, val);
        }).setOnComplete(() =>
        {
            animator.SetTrigger(parameter);
            StartCoroutine(WaitForAnimationComplete(parameter));
        });

        helperHandController.OnTweenBirdAndBackCalled();
    }

    private IEnumerator WaitForAnimationComplete(string parameter)
    {
        
        float timeout = 3f; // maximum time to wait for the animation
        float elapsed = 0f;

        while (elapsed < timeout && !(animator.GetCurrentAnimatorStateInfo(0).IsName(parameter) &&
                                      animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.9f))
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        RectTransform rectTransform = GetComponent<RectTransform>();
        LeanTween.value(gameObject, rectTransform.anchoredPosition.y, outsideViewportPosition.y, tweenDuration).setOnUpdate((float val) =>
        {
            rectTransform.anchoredPosition = new Vector2(outsideViewportPosition.x, val);
        });
    }

    public void PlayQuestAudio(AudioClip questAudio)
    {
        StartCoroutine(PlayingAudio(questAudio));
    }

    private IEnumerator PlayingAudio(AudioClip questAudio)
    {
        yield return new WaitForSeconds(0.5f);
        audioSource.clip = questAudio;
        audioSource.Play();
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
