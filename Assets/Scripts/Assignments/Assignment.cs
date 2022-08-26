using System;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime.Misc;
using Apps;
using JetBrains.Annotations;
using Unity.VisualScripting.Antlr3.Runtime;

namespace Assignments
{
    public class Assignment
    {
        public readonly string Name;
        public readonly string Descriptor;
        public List<Criteria> CompletionCriteria;
        public List<Criteria> ActivationCriteria;
        public readonly AssignmentType Type;
        public AssignmentState State;
        public readonly TimeSpan ReleaseTime;
        public readonly TimeSpan DueTime;

        private List<string> eventActions;

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
        }
        
        public void Activate()
        {
            if (State is AssignmentState.Completed or AssignmentState.Failed)
            {
                throw new Exception("Attempted to activate an assignment that was already finished.");
            }
            
            State = AssignmentState.Active;
            ToggleListeners(true);
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
        
        private void OnEmailOpen(EmailBackend.Email email)
        {
            UpdateCriteria("send_email", email.Subject);
        }
        
        private void ToggleListeners(bool enable)
        {
            eventActions = CompletionCriteria.Select(criterion => criterion.Type).ToList();
            eventActions = eventActions.Distinct().ToList();
            if (enable)
            {
                if (IsTimed) GameEvent.OnTimeChange += OnTimeChange;
                GameEvent.OnEmailOpen += OnEmailOpen;
            }
            else
            {
                if (IsTimed) GameEvent.OnTimeChange -= OnTimeChange;
                GameEvent.OnEmailOpen -= OnEmailOpen;
            }
        }
        
        private List<Criteria> FindCriteriaIndex(string type)
        {
            return CompletionCriteria.FindAll(criteria => criteria.Type == type && criteria.Fulfilled == false);
        }

        private void UpdateCriteria(string action, string value)
        {
            // find all unfulfilled criteria for the given action
            // disregard if 
            var list = FindCriteriaIndex(action);
            if (list.Count == 0) return;

            for (int i = 0; i < list.Count; i++)
            {
                var criteria = CompletionCriteria[c];
                criteria.Fulfilled = true;

                CompletionCriteria[c] = criteria;
            }

            if (CompletionCriteria.All(criterion => criterion.Fulfilled)) Complete();
        }

        public bool IsTimed => this.Type is AssignmentType.Timed or AssignmentType.PlayerTimed
            or AssignmentType.Emergency or AssignmentType.PlayerEmergency;

        public bool Over => this.State is AssignmentState.Completed or AssignmentState.Inactive or AssignmentState.Failed;

        public bool Completed => this.State == AssignmentState.Completed;
        
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
            public string Type;
            public string Value;
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