using UnityEngine;

public class GameSceneManager : MonoBehaviour
{
    public Animator boyAnimator;
    public GameObject[] dryClothes;
    public GameObject[] wetToys;
    public GameObject[] clotheslinePositions;
    public Sprite[] dryToWetSprites;
    public GameObject basket; // Reference to the clothes basket
    public GameObject toysBasket; // Reference to the toys basket

    private int clothesMovedCount = 0;
    private int toysMovedCount = 0;

    public void ClothMoved()
    {
        clothesMovedCount++;
        if (clothesMovedCount == dryClothes.Length)
        {
            boyAnimator.SetTrigger("clothsRemoved");
            Invoke(nameof(EnableToys), boyAnimator.GetCurrentAnimatorStateInfo(0).length);
        }
    }

    private void EnableToys()
    {
        foreach (var toy in wetToys)
        {
            toy.GetComponent<Collider2D>().enabled = true;
        }
    }

    public void ToyMoved()
    {
        toysMovedCount++;
        if (toysMovedCount == wetToys.Length)
        {
            Invoke(nameof(ChangeToWetSprites), 10f);
        }
    }

    private void ChangeToWetSprites()
    {
        for (int i = 0; i < dryClothes.Length; i++)
        {
            dryClothes[i].GetComponent<SpriteRenderer>().sprite = dryToWetSprites[i];
        }

        for (int i = 0; i < wetToys.Length; i++)
        {
            wetToys[i].GetComponent<SpriteRenderer>().sprite = dryToWetSprites[dryClothes.Length + i];
        }
    }
}
