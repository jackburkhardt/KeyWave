using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PixelCrushers.DialogueSystem;
using Project.Runtime.Scripts.Manager;
using Project.Runtime.Scripts.UI;
using Project.Runtime.Scripts.Utility;
using UnityEngine;

public class PhoneMenuPanel : ItemResponsePanel
{
    protected override bool ItemIsValid(Item item)
    {
        return item.IsContact && DialogueLua.GetItemField(item.Name, "Available").asBool;
    }

    protected override void SetDestinationDialogueEntryFields(Item item, Subtitle originSubtitle, ref DialogueEntry dialogueEntry)
    {
        dialogueEntry.MenuText = GetAssetDisplayName(item);
        dialogueEntry.fields.Add(new Field("Contact", item.id.ToString(), FieldType.Number));
        dialogueEntry.ActorID = originSubtitle.dialogueEntry.ActorID;
    }
}
