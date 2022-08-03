using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Apps.PC
{
    public class SearchView : MonoBehaviour
    {
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private Texture2D notFoundPage;

        [SerializeField] private Button emailButton;
        [SerializeField] private Button financeButton;
        [SerializeField] private Button filesButton;

        private void OnEnable()
        {
            emailButton.onClick.AddListener(() => PC.Instance.SwitchScreen("PCInbox"));
        }

        public void TrySearch()
        {
            var result = SearchBackend.Search(inputField.text);
            OpenSearchResult(result ? result : notFoundPage);
        }

        private void OpenSearchResult(Texture2D tex)
        {
            var resultScreen = PC.Instance.SwitchScreen("SearchResult");
            resultScreen.GetComponentInChildren<RawImage>().texture = tex;
            resultScreen.GetComponentInChildren<Text>().text = $"Search result for: \"{inputField.text}\"";
            resultScreen.GetComponent<Button>().onClick.AddListener(() => PC.Instance.GoBack());
        }
    }
}