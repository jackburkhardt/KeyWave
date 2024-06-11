using PixelCrushers.DialogueSystem;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Runtime.Scripts.UI.Map
{
    public class InfoPanelObjective : MonoBehaviour
    {
        [SerializeField] private Text _objectiveText;


        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        public void SetObjectiveText(string text)
        {
            _objectiveText.text = DialogueLua.GetQuestField(text, "State").asString == "unassigned" ? "???" : FormattedText.Parse(QuestLog.GetQuestTitle(text), DialogueManager.masterDatabase.emphasisSettings).text;
        }
    }
}