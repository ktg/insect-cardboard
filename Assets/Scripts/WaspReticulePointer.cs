

using UnityEngine;
using UnityEngine.EventSystems;

public class WaspReticulePointer : GvrReticlePointer
{
    //public GameObject target; 
    
    public override void OnPointerEnter(RaycastResult raycastResultResult, bool isInteractive)
    {
        base.OnPointerEnter(raycastResultResult, isInteractive);
        // if (isInteractive)
        // {
        //     Debug.Log(raycastResultResult.gameObject);
        //     target = raycastResultResult.gameObject;
        // }
        // else
        // {
        //     target = null;
        // }
    }

    // Start is called before the first frame update
    public override void OnPointerClickUp()
    {
        // Debug.Log("Clicked Out!! " + target);
        // if (target == null)
        // {
        //     EventSystem.current.SetSelectedGameObject(null);
        // }
    }
}
