using PixelCrushers;
using UnityEngine;

public class DebugUI : MonoBehaviour
{
    [SerializeField] private UITextField luaTime;
    [SerializeField] private UITextField gameStateTime;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        luaTime.text = Clock.CurrentTime;
    }
}
