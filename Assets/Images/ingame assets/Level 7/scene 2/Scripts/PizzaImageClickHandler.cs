using UnityEngine;
using UnityEngine.EventSystems;

public class PizzaImageClickHandler : MonoBehaviour, IPointerClickHandler
{
    public PizzaDrag pizzaDrag;  // Reference to the PizzaDrag script

    // Define a callback for handling clicks
    public System.Action OnPointerClickCallback;

    public void OnPointerClick(PointerEventData eventData)
    {
        // Notify PizzaDrag or invoke the callback
        if (OnPointerClickCallback != null)
        {
            OnPointerClickCallback.Invoke();
        }
        else if (pizzaDrag != null)
        {
            pizzaDrag.OnPizzaImageTapped(this.gameObject);  // Fallback to default behavior
        }
    }
}
