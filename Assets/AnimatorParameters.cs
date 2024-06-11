using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]

public class AnimatorParameters : MonoBehaviour
{
    private Animator Animator => GetComponent<Animator>();
    // Start is called before the first frame update
    public void SetBoolTrue(string boolParameter)
    {
        Animator.SetBool(boolParameter, true);
    }
    
    public void SetBoolFalse(string boolParameter)
    {
        Animator.SetBool(boolParameter, false);
    }
    
    
}
