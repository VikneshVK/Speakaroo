using System.Collections;
using UnityEngine;

public class SmoothSpriteAnimation : MonoBehaviour
{
    public Sprite[] sprites;
    public float framesPerSecond = 12f;

    private SpriteRenderer spriteRenderer;
    private int currentFrame;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        StartCoroutine(AnimateSprite());
    }

    IEnumerator AnimateSprite()
    {
        while (true)
        {
            spriteRenderer.sprite = sprites[currentFrame];
            currentFrame = (currentFrame + 1) % sprites.Length;
            yield return new WaitForSeconds(1f / framesPerSecond);
        }
    }
}
