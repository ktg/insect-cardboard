using UnityEngine;

public class GazeTracking : MonoBehaviour
{
    public Player player;
    private GvrBasePointer pointer;
    
    // Start is called before the first frame update
    void Start()
    {
        pointer = GetComponent<GvrBasePointer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (pointer.TouchDown || pointer.TriggerDown)
        {
            var obj = pointer.CurrentRaycastResult.gameObject;
            player.SelectItem(obj);
        }
    }
}
