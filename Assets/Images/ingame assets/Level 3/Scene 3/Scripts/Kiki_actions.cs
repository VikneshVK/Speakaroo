using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kiki_actions : MonoBehaviour
{
    public Transform birdStopPosition;
    
    public float flyspeed = 2f;
 
    public GameObject toysBasket;
    public GameObject Teddy;
    public GameObject Dino;
    public GameObject Bunny;
    public Transform basketFinalPosition;
    public Transform TeddyFinalPosition;
    public Transform DinoFinalPosition;
    public Transform BunnyFinalPosition;


    private Animator animator;
   
    private SpriteRenderer spriteRenderer;

    private bool isFlying = false;
    private bool isIdleCompleted = false;
   

    void Start()
    {
        animator = GetComponent<Animator>();        
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        HandleIdleCompletion();
        WalkToStopPosition();
        
        
    }

    private void HandleIdleCompletion()
    {
        if (!isIdleCompleted && animator.GetCurrentAnimatorStateInfo(0).IsName("Idle") &&
            animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
        {
            isIdleCompleted = true;
            isFlying = true;
            animator.SetBool("canFly", true);
        }
    }

    private void WalkToStopPosition()
    {
        if (birdStopPosition != null && isFlying)
        {
            Vector3 targetPosition = new Vector3(birdStopPosition.position.x, birdStopPosition.position.y, transform.position.z);
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, flyspeed * Time.deltaTime);
            

            if (Mathf.Abs(birdStopPosition.position.x - transform.position.x) <= 0.1f)
            {
                isFlying = false;
                animator.SetBool("canFly", false);
                LeanTween.move(toysBasket, basketFinalPosition.position, 1f).setEase(LeanTweenType.easeOutBack);
                LeanTween.move(Teddy, TeddyFinalPosition.position, 1f).setEase(LeanTweenType.easeOutBack);
                LeanTween.move(Dino, DinoFinalPosition.position, 1f).setEase(LeanTweenType.easeOutBack);
                LeanTween.move(Bunny, BunnyFinalPosition.position, 1f).setEase(LeanTweenType.easeOutBack);
            }
        }
    }  

   
}
