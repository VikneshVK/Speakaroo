using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ResolutionMagic
{
public class SpaceScene : MonoBehaviour
{
    public void ChangeZoonType()
   {
       if(ResolutionManager.Instance.ZoomTo == ResolutionManager.ZoomType.AlwaysDisplayedArea)
       {
           ResolutionManager.Instance.ZoomTo = ResolutionManager.ZoomType.MaximumBounds;
       }
       else
       {
           ResolutionManager.Instance.ZoomTo = ResolutionManager.ZoomType.AlwaysDisplayedArea;
       }
       ResolutionManager.Instance.RefreshResolution();
   }
}
}
