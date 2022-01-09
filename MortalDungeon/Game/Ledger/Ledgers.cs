﻿using MortalDungeon.Game.Units;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using MortalDungeon.Game.Serializers;
using MortalDungeon.Game.LuaHandling;

namespace MortalDungeon.Game.Ledger
{
    public enum LedgerUpdateType
    {
        Dialogue,
        Feature,
        Quest,
        GeneralState,
        Unit
    }

    public static class Ledgers
    {
        public static List<StateSubscriber> StateSubscribers = new List<StateSubscriber>();

        public static void LedgerUpdated(StateIDValuePair stateValue)
        {
            List<StateSubscriber> currentSubscribers = new List<StateSubscriber>(StateSubscribers);

            for(int i = currentSubscribers.Count - 1; i >= 0; i--)
            {
                if(currentSubscribers[i].TriggerValue.Type == stateValue.Type && currentSubscribers[i].TriggerValue.StateID == stateValue.StateID
                    && currentSubscribers[i].TriggerValue.ObjectHash == stateValue.ObjectHash && stateValue.Data == currentSubscribers[i].TriggerValue.Data)
                {
                    //if(currentSubscribers[i].Values.Count > 0)
                    //{
                    //    ApplyStateValues(currentSubscribers[i].Values);
                    //}

                    string script = currentSubscribers[i].Script;


                    if(!currentSubscribers[i].Permanent)
                    {
                        StateSubscribers.RemoveAt(i);
                    }

                    LuaManager.ApplyScript(script);
                }
            }

            for (int i = QuestManager.Quests.Count - 1; i >= 0; i--)
            {
                QuestManager.Quests[i].CheckObjectives();
            }
        }

        public static void OnUnitKilled(Unit unit)
        {
            //update the feature ledger stating that this unit has died
            if (unit.FeatureID != 0)
            {
                StateIDValuePair killUnitState = new StateIDValuePair()
                {
                    Type = (int)LedgerUpdateType.Feature,
                    StateID = unit.FeatureID,
                    ObjectHash = unit.ObjectHash,
                    Data = (int)FeatureInteraction.Killed,
                };

                ApplyStateValue(killUnitState);
            }
        }

        public static void OnUnitRevived(Unit unit)
        {
            //update the feature ledger stating that this unit has been revived
            if (unit.FeatureID != 0)
            {
                StateIDValuePair killUnitState = new StateIDValuePair()
                {
                    Type = (int)LedgerUpdateType.Feature,
                    StateID = unit.FeatureID,
                    ObjectHash = unit.ObjectHash,
                    Data = (int)FeatureInteraction.Revived,
                };

                ApplyStateValue(killUnitState);
            }
        }

        public static void ApplyStateValue(StateIDValuePair val)
        {
            if(val.Instruction == (int)StateInstructions.Subscribe || val.Instruction == (int)StateInstructions.PermanentSubscriber)
            {
                StateSubscriber subscriber = new StateSubscriber();
                subscriber.TriggerValue = val;
                subscriber.SubscribedValues = new List<StateIDValuePair>(val.Values);

                subscriber.Permanent = val.Instruction == (int)StateInstructions.PermanentSubscriber;

                StateSubscribers.Add(subscriber);
            }
            else if(val.Instruction == (int)StateInstructions.Set)
            {
                switch (val.Type)
                {
                    case (int)LedgerUpdateType.Dialogue:
                        DialogueLedger.SetStateValue(val);
                        break;
                    case (int)LedgerUpdateType.Quest:
                        QuestLedger.ModifyStateValue(val);
                        break;
                    case (int)LedgerUpdateType.Feature:
                        FeatureLedger.SetFeatureStateValue(val);
                        break;
                    case (int)LedgerUpdateType.GeneralState:
                        GeneralLedger.SetStateValue(val);
                        break;
                }
            }
            else if(val.Instruction == (int)StateInstructions.Clear)
            {
                switch (val.Type)
                {
                    case (int)LedgerUpdateType.Dialogue:
                        DialogueLedger.RemoveStateValue(val);
                        break;
                    case (int)LedgerUpdateType.Quest:
                        QuestLedger.ModifyStateValue(val);
                        break;
                    case (int)LedgerUpdateType.Feature:
                        FeatureLedger.RemoveFeatureStateValue(val);
                        break;
                    case (int)LedgerUpdateType.GeneralState:
                        GeneralLedger.RemoveStateValue(val);
                        break;
                }
            }
        }

        public static void ApplyStateValues(List<StateIDValuePair> data)
        {
            foreach (StateIDValuePair val in data)
            {
                ApplyStateValue(val);
            }
        }

        public static void AddSubscriber(StateSubscriber subscriber)
        {
            StateSubscribers.Add(subscriber);
        }

        public static void EvaluateInstructions(List<Instructions> instructions)
        {
            for(int i = 0; i < instructions.Count; i++)
            {
                EvaluateInstruction(instructions[i]);
            }
        }

        public static void EvaluateInstruction(Instructions instruction)
        {
            LuaManager.ApplyScript(instruction.Script);
        }
    }

    public enum StateInstructions
    {
        Set,                 //Default, sets the state value to the passed in data

        Subscribe,           //Subscribes to changes in that state value.
                             //If the Values list has data then these will
                             //be evaluated once the subscribed state is hit

        Clear,               //Removes any occurrence of this state value from the
                             //targeted state]

        PermanentSubscriber, //Subscribes to a state value in exactly the same way as 
                             //the subscribe instruction but is not removed when triggered

    }
}
