using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using PixelCrushers.DialogueSystem;
using UnityEngine;

public static class KeyWaveUtility
{ 
    
    
    
    public static Conversation GenerateConversation(Asset asset, bool repeatEntries = false)
    {
        var template = Template.FromDefault();
                
        var dialogueEntries = new List<DialogueEntry>();
        var conversation = template.CreateConversation( Template.FromDefault().GetNextConversationID(DialogueManager.masterDatabase), $"/GENERATED/{asset.Name}");
        var entryActorID = asset.IsFieldAssigned("Entry Actor")
            ? int.Parse(asset.LookupValue("Entry Actor"))
            : -1;
                
        var entryFieldLabelStart = repeatEntries && asset.FieldExists("Repeat Entry Count") ? "Repeat Entry" : "Entry";
        var entryCount = asset.LookupInt($"{entryFieldLabelStart} Count");


        var startNode = template.CreateDialogueEntry( 0, conversation.id, "START");
            
        startNode.ActorID = entryActorID;
        startNode.Sequence = "None()";
        startNode.outgoingLinks = new List<Link> { new Link( conversation.id, 0, conversation.id, 1) };
        dialogueEntries.Add(startNode);
                
        int musicEntry = Field.FieldExists( asset.fields, $"Music {entryFieldLabelStart}") ? asset.LookupInt($"Music {entryFieldLabelStart}") : 0;
            

        for (int i = 1; i < entryCount + 1; i++)
        {
            var menuText = asset.LookupValue($"{entryFieldLabelStart} {i} Menu Text");
            var dialogueText = asset.LookupValue($"{entryFieldLabelStart} {i} Dialogue Text");
            var duration = asset.LookupInt($"{entryFieldLabelStart} {i} Duration");
                    
            var newDialogueEntry = template.CreateDialogueEntry( i, conversation.id, string.Empty);
            newDialogueEntry.MenuText = menuText;
            newDialogueEntry.DialogueText = dialogueText;
                
            newDialogueEntry.ActorID = entryActorID;
                    
            newDialogueEntry.fields.Add(new Field("Duration", duration.ToString(CultureInfo.InvariantCulture), FieldType.Number));

            if (i == musicEntry)
            {
                var musicPath = asset.LookupValue("Music");
                newDialogueEntry.userScript += $"PlayClipLooped(\"{musicPath}\")";
            }
                
            if (i < entryCount) newDialogueEntry.outgoingLinks = new List<Link>() { new Link( conversation.id, i, conversation.id, i + 1) };
                
            dialogueEntries.Add(newDialogueEntry);
        };
                
                
        conversation.dialogueEntries = dialogueEntries;
                
        DialogueManager.masterDatabase.conversations.Add(conversation);
        return conversation;
        
    }
    
    
    static Canvas faderCanvas = null;
    static UnityEngine.UI.Image faderImage = null;
        
    public static IEnumerator Fade( string direction, float duration = 1f, Color color = default)
        {
            
            bool stay;
            bool unstay;
            bool fadeIn;
            
            stay = string.Equals(direction, "stay", System.StringComparison.OrdinalIgnoreCase);
            unstay = string.Equals(direction, "unstay", System.StringComparison.OrdinalIgnoreCase);
            fadeIn = unstay || string.Equals(direction, "in", System.StringComparison.OrdinalIgnoreCase);
            
            if (color == default) color = Color.black;
            
            IEnumerator FadeHandler()
            {
                float SmoothMoveCutoff = 0.05f;
                int FaderCanvasSortOrder = 32760;
                
               
                float startTime;
                float endTime;
                
                
                if (faderCanvas == null)
                {
                    faderCanvas = new GameObject("Canvas (Fader)", typeof(Canvas)).GetComponent<Canvas>();
                    faderCanvas.transform.SetParent(DialogueManager.instance.transform);
                    faderCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    faderCanvas.sortingOrder = FaderCanvasSortOrder;
                }
                if (faderImage == null)
                {
                    faderImage = new GameObject("Fader Image", typeof(UnityEngine.UI.Image)).GetComponent<UnityEngine.UI.Image>();
                    faderImage.transform.SetParent(faderCanvas.transform, false);
                    faderImage.rectTransform.anchorMin = Vector2.zero;
                    faderImage.rectTransform.anchorMax = Vector2.one;
                    faderImage.sprite = null;
                    var initializeAlpha = (fadeIn || unstay) ? 1 : 0;
                    faderImage.color = new Color(color.r, color.g, color.b, initializeAlpha);
                }

                if (unstay && faderImage != null && Mathf.Approximately(0, faderImage.color.a))
                {
                    yield break;
                }
                else if (duration > SmoothMoveCutoff)
                {
                    faderCanvas.gameObject.SetActive(true);
                    faderImage.gameObject.SetActive(true);

                    // Set up duration:
                    startTime = Time.time;
                    endTime = startTime + duration;

                    // If fade in or out, start from 1 or 0. Otherwise start from current alpha.
                    var startingAlpha = fadeIn ? 1
                        : (stay || unstay) ? faderImage.color.a
                        : 0;
                    faderImage.color = new Color(color.r, color.g, color.b, startingAlpha);

                }
                else
                {

                    yield break;
                }
                
                while (Time.time < endTime)
                {
                    var t = (Time.time - startTime) / duration;
                    var a = fadeIn ? 1 - t : t;
                    faderImage.color = new Color(color.r, color.g, color.b, a);
                    yield return null;
                }
            }
            
            
            yield return FadeHandler();
            if (fadeIn) Object.Destroy( faderCanvas.gameObject);
            if (fadeIn) Object.Destroy( faderImage.gameObject);
                
        }
    
    
}
