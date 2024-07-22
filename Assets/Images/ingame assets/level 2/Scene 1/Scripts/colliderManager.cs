using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class colliderManager : MonoBehaviour
{
    public GameObject card1Front;
    public GameObject card2Front;

    private void Start()
    {
        card1Front.GetComponent<Collider2D>().enabled = true;
        card2Front.GetComponent<Collider2D>().enabled = false;

        ST_AudioManager.Instance.OnCard1PlaybackComplete += EnableCard2FrontCollider;
    }

    private void OnDestroy()
    {
        ST_AudioManager.Instance.OnCard1PlaybackComplete -= EnableCard2FrontCollider;
    }

    private void EnableCard2FrontCollider()
    {
        card2Front.GetComponent<Collider2D>().enabled = true;
    }
}
