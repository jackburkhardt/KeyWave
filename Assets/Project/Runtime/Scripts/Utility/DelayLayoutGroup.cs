using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Runtime.Scripts.Utility
{
    public class DelayLayoutGroup : MonoBehaviour
    {
        private VerticalLayoutGroup _verticalLayoutGroup;

        // Start is called before the first frame update
        void Awake()
        {
            _verticalLayoutGroup = GetComponent<VerticalLayoutGroup>();
        }


        public void EnableLayoutGroup() => StartCoroutine(LayoutGroup());

        IEnumerator LayoutGroup()
        {
            yield return null;
            _verticalLayoutGroup.enabled = false;
            yield return new WaitForEndOfFrame();
            _verticalLayoutGroup.enabled = true;
        }
    }
}