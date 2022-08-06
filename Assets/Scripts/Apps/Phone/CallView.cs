using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Apps.Phone
{
    public class CallView : MonoBehaviour
    {
        [SerializeField] private GameObject pickUpButton;
        [SerializeField] private GameObject hangUpButton;

        [SerializeField] private Image callerImage;
        [SerializeField] private TMP_Text callerName;
        [SerializeField] private TMP_Text callTimeText;

        private void OnEnable()
        {
            callTimeText.text = "CALL INCOMING";
        }
    }
}