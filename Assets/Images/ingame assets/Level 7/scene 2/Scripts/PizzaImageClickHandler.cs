using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PizzaImageClickHandler : MonoBehaviour, IPointerClickHandler
{
    public PizzaDrag pizzaDrag;  // Reference to the PizzaDrag script

    public void OnPointerClick(PointerEventData eventData)
    {
        // Notify PizzaDrag that this image was clicked
        if (pizzaDrag != null)
        {
            pizzaDrag.OnPizzaImageTapped(this.gameObject);  // Pass the clicked image
        }
    }
}
