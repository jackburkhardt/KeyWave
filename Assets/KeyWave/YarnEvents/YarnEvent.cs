using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Interaction;
using Newtonsoft.Json;

namespace YarnEvents {
public class YarnEvent
{

    public readonly string Name;
    public readonly string Node;
    public readonly List<Criteria> AwakeCriteria;
    public readonly List<Criteria> ActivationCriteria;
    public readonly List<Criteria> CompletionCriteria;
    public int ActivationCount;
    public YarnEventState State;

    public YarnEvent(string name, string node, List<Criteria> awakeCriteria, List<Criteria> activationCriteria, List<Criteria> completionCriteria,  YarnEventState state = YarnEventState.Inactive, int activationCount = 0)
    {
        Name = name;
        Node = node;
        State = state;
        AwakeCriteria = awakeCriteria;
        ActivationCriteria = activationCriteria;
        CompletionCriteria = completionCriteria;
        if (activationCriteria.Count >= 0) ToggleListeners(true);
        ActivationCount = activationCount;
    }

    public void Activate()
        {
            if (State is YarnEventState.Completed)
            {
                throw new Exception($"Attempted to activate an assignment ({this.Name}) that was already finished.");
            }

            State = YarnEventState.Active;

            Debug.Log("'" + Name + "' is " + State);

            // this check is to avoid toggling listeners on twice if they were already on for activation checking
            if (ActivationCriteria.Count == 0) ToggleListeners(true);
            GameEvent.StartYarnEvent(this);
        }

    public struct Criteria
        {
            public readonly string Type;
            public readonly string Value;
            public bool Fulfilled;
            public bool History;

            [JsonConstructor]
            public Criteria(string type, string value)
            {
                Type = type;
                Value = value;
                Fulfilled = false;
                History = true;

            }
        }

    public enum YarnEventState
        {
            Dormant,
            Inactive,
            Active,
            Completed, 
            Expired
        }

        private List<(List<Criteria>, List<int>)> FindCriteriaByAction(string type)
        {
            List<(List<Criteria>, List<int>)> foundTuples = new List<(List<Criteria>, List<int>)>();
            List<Criteria> foundCriteria = new List<Criteria>();
            List<int> foundIndexes = new List<int>();


            switch (State)
            {
                case YarnEventState.Dormant:
                    foundCriteria = AwakeCriteria.FindAll(criteria => criteria.Type == type);
                    foundIndexes = foundCriteria.Select(criteria => AwakeCriteria.IndexOf(criteria)).ToList();
                    foundTuples.Add((foundCriteria, foundIndexes));
                    break;

                case YarnEventState.Inactive:
                    foundCriteria = (ActivationCriteria.FindAll(criteria => criteria.Type == type));
                    foundIndexes = foundCriteria.Select(criteria => ActivationCriteria.IndexOf(criteria)).ToList();
                    foundTuples.Add((foundCriteria, foundIndexes));
                    foundCriteria = CompletionCriteria.FindAll(criteria => criteria.Type == type && criteria.Fulfilled == false);
                    foundIndexes = foundCriteria.Select(criteria => CompletionCriteria.IndexOf(criteria)).ToList();
                    foundTuples.Add((foundCriteria, foundIndexes));
                    break;

                case YarnEventState.Active:
                    foundCriteria = CompletionCriteria.FindAll(criteria => criteria.Type == type && criteria.Fulfilled == false);
                    foundIndexes = foundCriteria.Select(criteria => CompletionCriteria.IndexOf(criteria)).ToList();
                    foundTuples.Add((foundCriteria, foundIndexes));
                    break;
            }
            // make a list of all of the indexes of foundCriteria
            

            // return a list of tuple of all the found criteria and their indexes in completioncriteria/activationcriteria
            return (foundTuples);
        }


        private void UpdateCriteria(string action, string value)
        {
            if (State == YarnEventState.Completed) return;
            
            // find all unfulfilled criteria for the given action
            var result = FindCriteriaByAction(action);
            // go through each criteria and if its value is the same as the given value, mark it as fulfilled
            // then update the main completioncriteria list with the fulfilled criteria
            foreach (var tuple in result)
            {

            for (int i = 0; i < tuple.Item1.Count; i++)
            {
                var criteria = tuple.Item1[i];
                if (criteria.Fulfilled && criteria.History) return;
                if (criteria.Value == value) criteria.Fulfilled = true;
                else if (!criteria.Fulfilled) return;
                else criteria.Fulfilled = false;
                Debug.Log("'" + Name + "': " + action + " '" + criteria.Value + "' = " + criteria.Fulfilled);


                switch (State) {
                    case YarnEventState.Dormant:
                        AwakeCriteria[tuple.Item2[i]] = criteria;
                        if (AwakeCriteria.All(criterion => criterion.Fulfilled)) { State = YarnEventState.Inactive; Debug.Log("'" + Name + "' is " + State); }
                            break;

                    case YarnEventState.Inactive:
                            if (result.IndexOf(tuple) == 0) { 
                                ActivationCriteria[tuple.Item2[i]] = criteria;
                                if (ActivationCriteria.All(criterion => criterion.Fulfilled)) { Activate(); }
                            }
                            else if (result.IndexOf(tuple) == 1)
                            {
                                CompletionCriteria[tuple.Item2[i]] = criteria;
                                if (CompletionCriteria.All(criterion => criterion.Fulfilled)) { State = YarnEventState.Completed; Debug.Log("'" + Name + "' is " + State); }
                            }
                            break;

                    case YarnEventState.Active:
                        CompletionCriteria[tuple.Item2[i]] = criteria;
                        break;
                }
                }

            }
        }



    //    private void OnPlayerEnterRoom(Actor actor, Transform destination) { if (GameManager.playerActor == actor)  UpdateCriteria("player_enter", destination.name); }
        private void OnYarnEventActivation(YarnEvent yarnEvent) { if (yarnEvent == this) { ActivationCount++; UpdateCriteria("activation_count", ActivationCount.ToString()); } }

        private void OnYarnEventCompletion(YarnEvent yarnEvent) { UpdateCriteria("yarnevent_complete", yarnEvent.Name); }

        private void OnPlayerInteract(IInteractable interactObj) {  UpdateCriteria("player_interact", interactObj.gameObject.name); }

        private void OnYarnSignal(string signal) { UpdateCriteria("yarn_signal", signal); }

        private void onYarnNodeEnd(string node) { UpdateCriteria("yarn_node_end", node); }

        private void onYarnNodeStart(string node) { UpdateCriteria("yarn_node_start", node); }

        private void ToggleListeners(bool enable)
    {
        if (enable)
        {
            //   GameEvent.OnActorEnterRoom += OnPlayerEnterRoom;
                GameEvent.OnYarnEventActive += OnYarnEventActivation;
                GameEvent.OnInteractionStart += OnPlayerInteract;
                GameEvent.OnYarnEventEnd += OnYarnEventCompletion;
                GameEvent.OnYarnSignal += OnYarnSignal;
                GameEvent.OnYarnNodeEnd += onYarnNodeEnd;
                GameEvent.OnYarnNodeStart += onYarnNodeStart;
            }
        else
        {
           
        }
    }

    }
}
