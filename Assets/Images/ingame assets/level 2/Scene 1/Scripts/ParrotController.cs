using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;

public class ParrotController : MonoBehaviour
{
    public GameObject mainBus;
    public GameObject mainWhale;
    public GameObject mainBuilding;
    public Transform finalPositionWhale;
    public Transform finalPositionBus;
    public Transform finalPositionBuilding;
    public Boolean cleaningCompleted;
    public float speed = 5.0f;

    private AnchorGameObject anchor;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Vector3 startPosition;
    private GameObject pushedGameObject;
    private bool isReturning = false;
    private List<string> pushedObjects = new List<string>();
    private Dictionary<string, GameObject> mainObjects;
    private List<string> requiredObjects = new List<string> { "bus S", "whale s", "building blocks s" };


    void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        startPosition = transform.position;
        anchor = GetComponent<AnchorGameObject>();
        cleaningCompleted = false;
        
        mainObjects = new Dictionary<string, GameObject>
        {
            {"bus S", mainBus},
            {"whale s", mainWhale},
            {"building blocks s", mainBuilding}
        };
        foreach (var obj in mainObjects.Values)
        {
            obj.GetComponent<Collider2D>().enabled = false;
        }
    }

    void Update()
    {
        if (animator.GetBool("startWalking") && !isReturning)
        {
            anchor.enabled = false;
            transform.Translate(speed * Time.deltaTime, 0, 0); // Move the parrot to the right
        }

        if (isReturning)
        {
            ReturnToStart();
        }
        if (pushedObjects.Count == requiredObjects.Count && !pushedObjects.Except(requiredObjects).Any())
        {
            animator.SetBool("cleaningDone", true); // All required objects have been pushed
            cleaningCompleted = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Pushable") && !pushedObjects.Contains(other.gameObject.name))
        {
            animator.SetBool("startWalking", false);
            animator.SetTrigger("canKnock");
            pushedGameObject = other.gameObject;
            other.GetComponent<Collider2D>().enabled = false;
            pushedObjects.Add(pushedGameObject.name);
            StartCoroutine(WaitForKnockToComplete());
            UpdateColliderStatus(pushedGameObject.name);
        }
    }

    IEnumerator WaitForKnockToComplete()
    {
        // Wait until the "Bird Knock" animation is being played
        yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName("Bird Knock"));
        // Wait until the "Bird Knock" animation is complete
        yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f);

        MovePushedObject();
    }

    private void MovePushedObject()
    {
        Transform finalPosition = null;
        if (pushedGameObject.name == "bus S")
            finalPosition = finalPositionBus;
        else if (pushedGameObject.name == "whale s")
            finalPosition = finalPositionWhale;
        else if (pushedGameObject.name == "building blocks s")
            finalPosition = finalPositionBuilding;

        if (finalPosition != null)
        {
            LeanTween.move(pushedGameObject, finalPosition.position, 1f).setOnComplete(OnCompleteMove);
        }
        else
        {
            Debug.LogError("No matching final position found for: " + pushedGameObject.name);
        }
    }

    private void OnCompleteMove()
    {
        spriteRenderer.flipX = false; // Prepare sprite for walk back
        animator.SetBool("walkBack", true);
        isReturning = true;
    }

    private void ReturnToStart()
    {
        transform.position = Vector3.MoveTowards(transform.position, startPosition, speed * Time.deltaTime);
        if (transform.position == startPosition)
        {
            ResetAnimatorBooleans();
            spriteRenderer.flipX = true;
            anchor.enabled = true;
            isReturning = false;
        }
    }

    private void ResetAnimatorBooleans()
    {
        animator.SetBool("resetPosition", true);
        animator.SetBool("walkBack", false);
        animator.SetBool("startWalking", false);
        animator.SetBool("canKnock", false);
        animator.SetBool("cleaningDone", false);
    }

    private void UpdateColliderStatus(string pushedName)
    {
        var otherKeys = mainObjects.Keys.Where(k => k != pushedName).ToList();

        foreach (var key in otherKeys)
        {
            if (!pushedObjects.Contains(key))
            {
                mainObjects[key].GetComponent<Collider2D>().enabled = true;
            }
        }
    }
}
