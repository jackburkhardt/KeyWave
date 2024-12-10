using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BroadcastAnimatorTrigger : MonoBehaviour
{
    public string trigger = "Show";
    public SendMessageOptions sendMessageOptions = SendMessageOptions.RequireReceiver;
    [Tooltip("Include the animator in this GameObject in the broadcast.")]
    public bool includeRoot = true;
    [Tooltip("If true, this script will require the function Reset() to be called before it can be triggered again.")]
    public bool requireReset = false;
    private bool _dirty = false;
    
    List<Animator> AnimatorsToBroadcast {
        get
        {
            var animators = GetComponentsInChildren<Animator>().ToList();
            if (!includeRoot) animators.Remove(GetComponent<Animator>());
            return animators;
        }
    }

    public void BroadcastWithException(Animator animator)
    {
        var animators = AnimatorsToBroadcast;
        animators.Remove(animator);
        Broadcast(animators, trigger);
    }
    
    public void BroadcastTrigger()
    {


        var animators = AnimatorsToBroadcast;
        
        if (animators.Count == 0 && sendMessageOptions == SendMessageOptions.RequireReceiver)
        {
            Debug.LogWarning("No animators found in children of " + name);
        }
        
        Broadcast(animators, trigger);
    }

    public void Broadcast(List<Animator> animators, string trigger)
    {
        if (_dirty && requireReset) return;
        
        foreach (var animator in animators)
        {
            animator.SetTrigger(trigger);
        }
        
        if (requireReset)
        {
            _dirty = true;
        }
    }
    
    public void Reset()
    {
        _dirty = false;
    }
}
