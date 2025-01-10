using UnityEngine;
using System.Collections;
using TMPro;


public class LVL6Sc2KikiController : MonoBehaviour
{
    [SerializeField] private float tweenDuration = 1.0f;
    [SerializeField] private AudioClip audioClip1; // First tween audio
    private AudioSource audioSource;
    public Lvl6QuestManager Lvl6QuestManager;
    public LVL6Sc2Helperhand helperHandController;
    public TextMeshProUGUI subtitleText;
    public SubtitleManager subtitleManager;
    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        StartCoroutine(HandleTweens());
    }

    private IEnumerator HandleTweens()
    {
        TriggerAnimation("FirstTalk");
        yield return new WaitForSeconds(0.5f);
        audioSource.clip = audioClip1;
        audioSource.Play();
        
        subtitleManager.DisplaySubtitle("Something is under the sand", "Kiki", audioClip1);
        yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName("FirstTalk") &&
                                        animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.9f);

        yield return new WaitForSeconds(1f);

        Lvl6QuestManager.SpawnItems();
    }

    public void TriggerAnimation(string parameter)
    {
        animator.SetTrigger(parameter);
        StartCoroutine(WaitForAnimationComplete(parameter));
        helperHandController.OnTweenBirdAndBackCalled();
    }

    private IEnumerator WaitForAnimationComplete(string parameter)
    {
        float timeout = 3f; // Maximum time to wait for the animation
        float elapsed = 0f;

        while (elapsed < timeout && !(animator.GetCurrentAnimatorStateInfo(0).IsName(parameter) &&
                                      animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.9f))
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
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

    
}
