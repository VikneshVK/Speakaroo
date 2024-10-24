using UnityEngine;
using System.Collections;

public class LVL6Sc2KikiController : MonoBehaviour
{
    [SerializeField] private GameObject finalPosition;
    [SerializeField] private float tweenDuration = 1.0f;
    [SerializeField] private AudioClip audioClip1; // First tween audio
    private AudioSource audioSource;
    public Lvl6QuestManager Lvl6QuestManager;
    public LVL6Sc2Helperhand helperHandController;
    private Animator animator;
    private Vector3 originalPosition;

    private void Start()
    {
        originalPosition = transform.position;
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        StartCoroutine(HandleTweens());
    }

    private IEnumerator HandleTweens()
    {
        // Set and play the first tween audio
        audioSource.clip = audioClip1;
        audioSource.Play();

        // Call the first tween and wait for it to complete
        TweenBirdandback("FirstTalk");
        yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName("FirstTalk") &&
                                        animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.9f);

        yield return new WaitForSeconds(1f);

        // Now, call SpawnItems() which triggers the second tween
        Lvl6QuestManager.SpawnItems();
    }

    public void TweenBirdandback(string parameter)
    {
        LeanTween.move(gameObject, finalPosition.transform.position, tweenDuration).setOnComplete(() =>
        {
            animator.SetTrigger(parameter);
            StartCoroutine(WaitForAnimationComplete(parameter));
        });
        helperHandController.OnTweenBirdAndBackCalled();
    }

    private IEnumerator WaitForAnimationComplete(string parameter)
    {
        yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName(parameter) &&
                                        animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.9f);
        LeanTween.move(gameObject, originalPosition, tweenDuration);
    }

    // Called when quest starts to play the appropriate audio clip based on the quest
    public void PlayQuestAudio(AudioClip questAudio)
    {
        audioSource.clip = questAudio;
        audioSource.Play();
    }
}
