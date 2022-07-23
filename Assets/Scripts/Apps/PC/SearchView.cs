using UnityEngine;
using UnityEngine.UI;

namespace Apps.PC
{
    public class SearchView : MonoBehaviour
    {
        [SerializeField] private Texture2D notFoundPage;
        [SerializeField] private Object popupPrefab;
        
        public void TrySearch(string query)
        {
            var result = SearchBackend.Search(query);
            OpenSearchResult(result ? result : notFoundPage);
        }

        private void OpenSearchResult(Texture2D tex)
        {
            var newPopup = Instantiate(popupPrefab, transform) as GameObject;
            newPopup.GetComponentInChildren<RawImage>().texture = tex;
        }

        public void Close()
        {
            Destroy(this.gameObject);
        }
    }
}