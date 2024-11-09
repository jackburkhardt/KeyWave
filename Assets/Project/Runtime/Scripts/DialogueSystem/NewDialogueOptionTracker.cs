using System.Collections;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;
using UnityEngine;
using UnityEngine.Events;

namespace Project.Runtime.Scripts.DialogueSystem
{
    public class NewDialogueOptionTracker : MonoBehaviour
    {
        private Dictionary<string, List<(DialogueEntry entry, bool active)>> trackedNodes = new();
        public UnityEvent<DialogueEntry> onNewOptionAvailable;
        private void OnEnable()
        {
            StartCoroutine(BuildTableCoroutine());
        }

        private IEnumerator BuildTableCoroutine()
        {
            while (!DialogueManager.Instance.isInitialized)
            {
                yield return null;
            }
            
            // find all nodes with the "track" field
            foreach (var convo in DialogueManager.MasterDatabase.conversations)
            {
                List<(DialogueEntry, bool)> trackedEntries = new();
                foreach (var entry in convo.dialogueEntries)
                {
                    if (entry.fields.Exists(f => f.title == "Track" && f.value == "True"))
                    {
                        foreach (var childID in entry.outgoingLinks)
                        {
                            var child = convo.GetDialogueEntry(childID.destinationDialogueID);
                            if (!string.IsNullOrEmpty(child.conditionsString))
                            {
                                var luaResult = Lua.Run($"return ({child.conditionsString})");
                                if (!luaResult.asBool)
                                {
                                    trackedEntries.Add((child, false));
                                }
                                yield return null;
                            }
                        }
                    }
                }
                
                string baseConvo = convo.Title.Split('/')[0];
                if (!trackedNodes.TryAdd(baseConvo, trackedEntries))
                {
                    trackedNodes[baseConvo].AddRange(trackedEntries);
                }

                yield return null;
            }
        }

        public void OnConversationLineEnd(Subtitle sub)
        {
            var convoName = DialogueManager.GetConversationTitle(sub.dialogueEntry.conversationID);
            var convoNameBase = convoName.Split('/')[0];
            
            if (trackedNodes.TryGetValue(convoNameBase, out var node))
            {
                for (int i = 0; i < node.Count; i++)
                {
                    var entry = node[i];
                    if (!entry.active)
                    {
                        var luaResult = Lua.Run($"return ({entry.entry.conditionsString})");
                        if (luaResult.asBool)
                        {
                            node[i] = (entry.entry, true);
                            onNewOptionAvailable.Invoke(entry.entry);
                            BroadcastMessage("OnNewOptionAvailable", entry.entry, SendMessageOptions.DontRequireReceiver);
                            Debug.Log("New option unlocked " + entry.entry.currentMenuText);
                            Field.SetValue(entry.entry.fields, "Show Badge", true);
                            
                        }
                    }
                }
            }
        }
    }
}