using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

/// <<summary>>
/// This is a base class for checking requirement flags for certain events, such as Yarn Events.
/// The event in question will only run when requirementsMatchedis true for all instances of .this attached to the same GameObject.
/// This base class primarily checks for if Player Control is enabled. Child classes add additional flags.
/// <</summary>>

public abstract class EventRequirementsCheck : MonoBehaviour
{
   

    [ReadOnly]
    public bool requirementsMatched;

    [ReadOnly][SerializeField]
    protected string requirementsDebugLog;
    
    private void OnEnable()
    {
       // GameEvent.OnAnyEvent += CheckRequirements;
    }

    private void OnDisable()
    {
      //  GameEvent.OnAnyEvent -= CheckRequirements;
    }

    protected virtual void CheckRequirements()
    {
        requirementsDebugLog = "Requirements unmet: ";
        requirementsMatched = true;
        if (!GameManager.isControlEnabled)
        {
            requirementsMatched = false;
            requirementsDebugLog += "PlayerControl ";
        }
    }


    //EvaluateStringInequality takes an inequality (string) and an integer, and returns true if the expression evaluates as true for the given integer
    //The string must be in the form of "x", < x", "> x", "== x", "<= x", or ">= x" where 'x' is an integer (whitespace is ignored)
    //The string can also include multiple inequalities by seperating them with ','
    //Example: EvaluateStringInequality("> 3, < 5", 4) returns true
   
    protected bool EvaluateStringInequality(string inputString, int inputNumber)
    {
        if (inputString.Length == 0) return true;
        bool output = true;

        string[] strings = (new Regex("\\s+")).Replace(inputString, "").Split(',');
        foreach (string expression in strings)
        {

            int firstIntIndex = expression.IndexOfAny(new char[] { '1', '2', '3', '4', '5', '6', '7', '8', '9', '0' });

            string expressionEquality = expression.Substring(0, firstIntIndex);
            int expressionConstant = IntParseFast(expression.Substring(firstIntIndex));
            
            switch (expressionEquality)
            {
                case "<":
                    if (inputNumber >= expressionConstant) output = false;
                    break;
                case "<=":
                    if (inputNumber > expressionConstant) output = false;
                    break;
                case ">":
                    if (inputNumber <= expressionConstant) output = false;
                    break;
                case ">=":
                    if (inputNumber < expressionConstant) output = false;
                    break;
                case "==":
                    if (inputNumber != expressionConstant) output = false;
                    break;
                case "":
                    if (inputNumber != expressionConstant) output = false;
                    break;

            }
        }
        return output;
    }

    // Quick equivalence of string as integer
    int IntParseFast(string intString)
    {
        int y = 0;
        for (int i = 0; i < intString.Length; i++)
            y = y * 10 + (intString[i] - '0');
        return y;
    }




}
