using System;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime.Misc;
using Apps;
using Interaction;
using JetBrains.Annotations;
using Unity.VisualScripting.Antlr3.Runtime;

namespace Assignments
{
    public class Assignment
    {
        public readonly string Name;
        public readonly string Descriptor;
        public readonly List<Criteria> CompletionCriteria;
        public readonly List<Criteria> ActivationCriteria;
        public readonly AssignmentType Type;
        public AssignmentState State;
        public readonly TimeSpan ReleaseTime;
        public readonly TimeSpan DueTime;

        public Assignment(string name, string descriptor, List<Criteria> completionCriteria, List<Criteria> activationCriteria, 
            AssignmentType type = AssignmentType.General, AssignmentState state = AssignmentState.Inactive, 
            TimeSpan releaseTime = default, TimeSpan dueTime = default)
        {
            Name = name;
            Descriptor = descriptor;
            CompletionCriteria = completionCriteria;
            ActivationCriteria = activationCriteria;
            Type = type;
            ReleaseTime = releaseTime;
            DueTime = dueTime;
            State = state;
            if (activationCriteria.Count >= 0) ToggleListeners(true);
        }
        
        public void Activate()
        {
            if (State is AssignmentState.Completed or AssignmentState.Failed)
            {
                throw new Exception("Attempted to activate an assignment that was already finished.");
            }
            
            State = AssignmentState.Active;
            // this check is to avoid toggling listeners on twice if they were already on for activation checking
            if (ActivationCriteria.Count == 0) ToggleListeners(true);
            GameEvent.StartAssignment(this);
        }
        
        public void Complete()
        {
            if (State is not AssignmentState.Active)
            {
                throw new Exception("Attempted to complete an assignment that was not active.");
            }
            
            State = AssignmentState.Completed;
            ToggleListeners(false);
            GameEvent.CompleteAssignment(this);
        }

        public void Fail()
        {
            if (State is not AssignmentState.Active)
            {
                throw new Exception("Attempted to complete an assignment that was not active.");
            }
            
            State = AssignmentState.Failed;
            ToggleListeners(false);
            GameEvent.FailAssignment(this);
        }
        
        private void OnTimeChange(TimeSpan time)
        {
            if (!IsTimed || Over) return;
            
            if (time >= DueTime)
            {
                Fail();
            }
        }
        
        private void OnEmailOpen(EmailBackend.Email email) => UpdateCriteria("open_email", email.Subject);
        private void OnInteractionStart(IInteractable interactable) => UpdateCriteria("interact", interactable.name);
        private void OnUnlockPC() => UpdateCriteria("unlock_pc", "");
        private void OnAssignmentComplete(Assignment assignment) => UpdateCriteria("complete_assignment", assignment.Name);
        private void OnSearch(string query, bool success) => UpdateCriteria("do_search", query);
        private void OnConversationStart(Character character) => UpdateCriteria("converse", character.name);
        private void OnPCScreenChange(string screen) => UpdateCriteria("view_screen_pc", screen);
        private void OnPhoneScreenChange(string screen) => UpdateCriteria("view_screen_phone", screen);

        private void ToggleListeners(bool enable)
        {
            if (enable)
            {
                if (IsTimed) GameEvent.OnTimeChange += OnTimeChange;
                GameEvent.OnEmailOpen += OnEmailOpen;
                GameEvent.OnInteractionStart += OnInteractionStart;
                GameEvent.OnAssignmentComplete += OnAssignmentComplete;
                GameEvent.OnSearch += OnSearch;
                GameEvent.OnPCUnlock += OnUnlockPC;
                GameEvent.OnPCScreenChange += OnPCScreenChange;
                GameEvent.OnPhoneScreenChange += OnPhoneScreenChange;
            }
            else
            {
                if (IsTimed) GameEvent.OnTimeChange -= OnTimeChange;
                GameEvent.OnEmailOpen -= OnEmailOpen;
                GameEvent.OnInteractionStart -= OnInteractionStart;
                GameEvent.OnAssignmentComplete -= OnAssignmentComplete;
                GameEvent.OnSearch -= OnSearch;
                GameEvent.OnPCUnlock -= OnUnlockPC;
                GameEvent.OnPCScreenChange -= OnPCScreenChange;
                GameEvent.OnPhoneScreenChange -= OnPhoneScreenChange;
            }
        }
        
        private (List<Criteria>, List<int>) FindCriteriaByAction(string type)
        {
            List<Criteria> foundCriteria = new List<Criteria>();
            
            // if this assignment is active, we are looking for COMPLETION criteria matching the action type
            // if this assignment is inactive, we are looking for ACTIVATION criteria matching the action type
            if (State is AssignmentState.Active)
            {
                foundCriteria = CompletionCriteria.FindAll(criteria => criteria.Type == type && criteria.Fulfilled == false);
            } else if (State is AssignmentState.Inactive)
            {
                foundCriteria = ActivationCriteria.FindAll(criteria => criteria.Type == type && criteria.Fulfilled == false);
            }

            // make a list of all of the indexes of foundCriteria
            var foundIndexes = foundCriteria.Select(criteria => CompletionCriteria.IndexOf(criteria)).ToList();
            
            // return a tuple of all the found criteria and their indexes in completioncriteria/activationcriteria
            return (foundCriteria, foundIndexes);
        }

        private void UpdateCriteria(string action, string value)
        {
            // find all unfulfilled criteria for the given action
            var result = FindCriteriaByAction(action);
            if (result.Item1.Count == 0) return;

            // go through each unfulfilled criteria and if its value is the same as the given value, mark it as fulfilled
            // then update the main completioncriteria list with the fulfilled criteria
            for (int i = 0; i < result.Item1.Count; i++)
            {
                var criteria = result.Item1[i];
                if (criteria.Value == value)
                {
                    criteria.Fulfilled = true;
                    if (State is AssignmentState.Active)
                    {
                        CompletionCriteria[result.Item2[i]] = criteria;
                        // check if all criteria have been fulfilled and complete the assignment if they have
                        if (CompletionCriteria.All(criterion => criterion.Fulfilled)) Complete();
                    } else if (State is AssignmentState.Inactive)
                    {
                        ActivationCriteria[result.Item2[i]] = criteria;
                        // check if all criteria have been fulfilled and activate the assignment if they have
                        if (ActivationCriteria.All(criterion => criterion.Fulfilled)) Activate();
                    }
                    
                }
            }
        }

        public bool IsTimed => this.Type is AssignmentType.Timed or AssignmentType.PlayerTimed
            or AssignmentType.Emergency or AssignmentType.PlayerEmergency;

        public bool Over => this.State is AssignmentState.Completed or AssignmentState.Inactive or AssignmentState.Failed;

        public bool Completed => this.State == AssignmentState.Completed;
        
        public bool IsActive => this.State == AssignmentState.Active;
        
        public bool CanDelegate => this.Type is not AssignmentType.PlayerTimed or AssignmentType.PlayerEmergency or AssignmentType.PlayerOnly;
        
        public enum AssignmentType
        {
            General,
            Locked,
            PlayerOnly,
            Timed,
            Emergency,
            PlayerTimed,
            PlayerEmergency
        }

        public enum AssignmentState
        {
            Inactive,
            Active,
            Completed,
            Failed
        }
        
        public struct Criteria
        {
            public readonly string Type;
            public readonly string Value;
            public bool Fulfilled;
            
            public Criteria(string type, string value)
            {
                Type = type;
                Value = value;
                Fulfilled = false;
            }
            
        }
    }
}