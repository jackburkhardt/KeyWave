using PixelCrushers.DialogueSystem;
using UnityEngine;
using UnityEngine.UI;

public class InfoPanelObjective : MonoBehaviour
{
    [SerializeField] private Text _objectiveText;
    
    public void SetObjectiveText(string text)
    {
        _objectiveText.text = DialogueLua.GetQuestField(text, "State").asString == "unassigned" ? "???" : FormattedText.Parse(QuestLog.GetQuestTitle(text), DialogueManager.masterDatabase.emphasisSettings).text;
    }
    
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
