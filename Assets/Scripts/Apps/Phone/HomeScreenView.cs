using System.Collections;
using System.Collections.Generic;
using Apps.Phone;
using UnityEngine;
using UnityEngine.UI;

public class HomeScreenView : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var buttons = GetComponentsInChildren<Button>();
        buttons[0].onClick.AddListener(() => Phone.Instance.UISwitchScreen("Contacts"));
        buttons[1].onClick.AddListener(() => Phone.Instance.UISwitchScreen("Emails"));
        buttons[2].onClick.AddListener(() => Phone.Instance.UISwitchScreen("Messages"));
    }

}
