using System.Collections;
using System.Collections.Generic;
using Apps.Phone;
using UnityEngine;
using UnityEngine.UI;

public class HomeScreenView : MonoBehaviour
{
    // just links the app buttons on homescreen with the "apps" (screens) they open
    void Start()
    {
        var buttons = GetComponentsInChildren<Button>();
        buttons[0].onClick.AddListener(() => Phone.Instance.UISwitchScreen("Contacts"));
        buttons[1].onClick.AddListener(() => Phone.Instance.UISwitchScreen("Emails"));
        buttons[2].onClick.AddListener(() => Phone.Instance.UISwitchScreen("Messages"));
    }

}
