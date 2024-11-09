using PixelCrushers.DialogueSystem;
using Project;
using Project.Runtime.Scripts.UI;
using Project.Runtime.Scripts.Utility;
using UnityEngine;
using UnityEngine.UI;

public class CharacterResponseButton : CustomUIResponseButton
{
    
    
    
    [ActorPopup(true)]
    public string characterName;
    private static CharacterMenuPanel _characterMenuPanel;
    private DialogueActor _currentActor;
    [GetComponent]
    [SerializeField] protected Image characterImage;
    [GetComponent]
    [SerializeField] protected LayoutElement layoutElement;
    

    public void OnValidate()
    {
        button ??= GetComponent<Button>();
        //showPopupBadge = false;
        _characterMenuPanel ??= MenuPanelContainer as CharacterMenuPanel ?? FindObjectOfType<CharacterMenuPanel>();

        if (_characterMenuPanel == null) return;

        _currentActor = _characterMenuPanel.GetActor(characterName);

        if (_currentActor != null)
        {
            var image = _currentActor.GetComponent<Image>();
            var layout = _currentActor.GetComponent<LayoutElement>();
            characterImage.sprite = image.sprite;
            layoutElement.preferredHeight = layout.preferredHeight;
            layoutElement.preferredWidth = layout.preferredWidth;
        }
    }

    public override void Refresh()
    {
       RefreshLayoutGroups.Refresh(_characterMenuPanel.gameObject);
    }

    public void OnContentChange()
    {
        if (response != null)
        {
            if (Field.FieldExists(response.destinationEntry.fields, _characterMenuPanel.responseField))
            {
                var actor = Field.LookupValue(response.destinationEntry.fields, _characterMenuPanel.responseField);
                characterName = actor;
            }
        }
    }
}
