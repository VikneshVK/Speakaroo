using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace ResolutionMagic
{
    public class SportsScene : MonoBehaviour
    {
        [SerializeField] Transform focusObject;
        [SerializeField] Text text;

        void Start()
        {
            text.text = "Focusing on the sports court";
        }

        public void FlipGravity()
        {
            Physics2D.gravity *= -1;
        }

        public void ChangeZoonType()
        {
            if (ResolutionManager.Instance.ZoomTo == ResolutionManager.ZoomType.AlwaysDisplayedArea)
            {
                ResolutionManager.Instance.ZoomTo = ResolutionManager.ZoomType.MaximumBounds;
                text.text = "Showing maximum area";
            }
            else
            {
                ResolutionManager.Instance.ZoomTo = ResolutionManager.ZoomType.AlwaysDisplayedArea;
                text.text = "Focusing on the sports court";
            }
            ResolutionManager.Instance.RefreshResolution();
        }

        public void FocusOnGoal()
        {
            StartCoroutine(ActivateFocusObject());
            Vector2 size = focusObject.GetComponent<SpriteRenderer>().bounds.size;
            if (ResolutionManager.Instance.ZoomTo == ResolutionManager.ZoomType.AlwaysDisplayedArea)
            {
                ResolutionManager.Instance.FocusOnObject(focusObject,  5f);
            }
            else
            {
                ResolutionManager.Instance.FocusOnObject( focusObject, 5f);
            }
        }

        IEnumerator ActivateFocusObject()
        {
            focusObject.gameObject.SetActive(true);
            yield return new WaitForSeconds(5f);
            focusObject.gameObject.SetActive(false);
        }
    }
}
