using System.Collections;
using TMPro;
using UnityEngine;

namespace Apps.PC
{
    public class LockScreen : MonoBehaviour
    {
        [SerializeField] private string _password;
        [SerializeField] private TMP_InputField _inputField;
        [SerializeField] private TMP_Text _incorrectPassText;
        [SerializeField] private TMP_Text _correctPassText;
        private float textAnimateDelay = 0.2f;
        private int textAnimateCycles = 3;

        public void AttemptLogon()
        {
            if (_inputField.text == _password)
            {
                if (_incorrectPassText.enabled) _incorrectPassText.enabled = false;
                StartCoroutine(DoCorrectLogon());
            }
            else
            {
                _incorrectPassText.enabled = true;
            }
        }

        private IEnumerator DoCorrectLogon()
        {
            _correctPassText.enabled = true;
            _correctPassText.text = "Welcome, Ava. Logging in";
            
            while (textAnimateCycles > 0)
            {
                // could throw this in a loop too but i'm lazy
                yield return new WaitForSeconds(textAnimateDelay);
                _correctPassText.text += ".";
                yield return new WaitForSeconds(textAnimateDelay);
                _correctPassText.text += ".";
                yield return new WaitForSeconds(textAnimateDelay);
                _correctPassText.text += ".";
                yield return new WaitForSeconds(textAnimateDelay);
                _correctPassText.text = "Welcome, Ava. Logging in";
                textAnimateCycles--;
            }

            PC.Instance.SwitchScreen("Search");
            Destroy(this.gameObject);
        }
    }
}