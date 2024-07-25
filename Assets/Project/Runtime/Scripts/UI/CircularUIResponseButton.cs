using System;
using System.Collections;
using PixelCrushers.DialogueSystem;
using Project.Runtime.Scripts.DialogueSystem;
using Project.Runtime.Scripts.Manager;
using Project.Runtime.Scripts.Utility;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Project.Runtime.Scripts.UI
{
    [RequireComponent(typeof(CircularUIButton))]

    public class CircularUIResponseButton : CustomUIResponseButton
    {
        private CircularUIButton CircularUIButton => GetComponent<CircularUIButton>();
        protected CircularUIMenuPanel CircularPanelContainer => (CircularUIMenuPanel)GetComponentInParent(typeof(CircularUIMenuPanel));

        public Color ButtonColor
        {
            get => CircularUIButton.image.color;
            set => CircularUIButton.image.color = value;
        }

        private Color DisabledColor
        {
            get => UnityButton.colors.disabledColor;
            set
            {
                var block = UnityButton.colors;
                block.disabledColor = value;
                UnityButton.colors = block;
            }
        }

        private Color BaseColor
        {
            get
            {
                if (response?.destinationEntry != null) return DialogueUtility.NodeColor(response?.destinationEntry);
                return Color.white;
            }
        }

        private Color HighlightColor
        {
            get => UnityButton.colors.highlightedColor;
            set
            {
                var block = UnityButton.colors;
                block.highlightedColor = value;
                UnityButton.colors = block;
            }
        }

        private Color _defaultLabelColor;

        private Item? AssociatedQuest => DialogueManager.Instance.masterDatabase.items.Find(item => item.Name == response?.destinationEntry.GetNextDialogueEntry()?.GetConversation().Title);

        private Points.PointsField[] PointsData
        {
            get
            {
                if (response?.destinationEntry != null && response?.destinationEntry.GetNextDialogueEntry() != null)
                {
                    var conversation = response?.destinationEntry.GetNextDialogueEntry()?.GetConversation().Title;
                    return QuestUtility.GetPoints(conversation);
                
                }
                return Array.Empty<Points.PointsField>();
            }
        }

        public string TimeEstimateText
        {
            get
            {
                if (response.destinationEntry.GetNextDialogueEntry() == null) return "";
            
                if (AssociatedQuest == null) return "";
            
            
                if (response.destinationEntry?.GetConversation().Title == AssociatedQuest.Name) return "";
                if (AssociatedQuest.GetQuestState() != QuestState.Active) return "";
            
                var timespan = AssociatedQuest.Timespan("Duration");
                if (timespan <= 0) return "";
                var unit = timespan > 60 ? "minutes" : "seconds";
                var duration = timespan > 60 ? timespan / 60 : timespan;
                return $"{duration} {unit}";
            }
        }

        public override void Refresh()
        {
            base.Refresh();
            label.color = _defaultLabelColor;
        }
        
        public override void Awake()
        {
            base.Awake();
            _defaultLabelColor = label.color;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            Refresh();
            StartCoroutine(SetButtonColor());
        }

        private IEnumerator SetButtonColor()
        {
            yield return new WaitForEndOfFrame();
            Refresh();
            ButtonColor = BaseColor;
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);
            DisabledColor = HighlightColor;
            label.color = Color.Lerp(label.color, Color.clear, 0.25f);
        
            foreach (var customUIResponseButton in SiblingButtons)
            {
                if (customUIResponseButton != this && customUIResponseButton != MenuPanelContainer.buttonTemplate) customUIResponseButton.label.color = Color.clear;
            }
            
            
        }

        public void OnContentChanged()
        {
            label.color = _defaultLabelColor;
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);
            Refresh();
            
            //todo: implement gradient for multiple point types
            if (PointsData.Length > 0)
            {
                var pointData = PointsData[0];
                if (AssociatedQuest?.GetQuestState() == QuestState.Active && pointData.Points > 0)
                {
                    ButtonColor = Points.Color(pointData.Type);
                }

                else ButtonColor = BaseColor;
            }

            CircularPanelContainer.SetPropertiesFromButton(this);
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);
            Refresh();
            ButtonColor = BaseColor;
            StartCoroutine(HoveredButtonCheck());
        }

        private IEnumerator HoveredButtonCheck()
        {
            yield return new WaitForEndOfFrame();
            if (_hoveredButton == this) CircularPanelContainer.SetPropertiesFromButton(null);
        }
    }
}