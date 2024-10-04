using UnityEngine;

namespace ResolutionMagic
{
public class BlackBarsScene : MonoBehaviour
{
    public void ToggleBlackBars()
    {
        ResolutionManager.Instance.ToggleBlackBars();
    }
}
}
