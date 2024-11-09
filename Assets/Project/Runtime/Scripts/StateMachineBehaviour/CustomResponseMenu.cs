using UnityEngine;

namespace Project.Runtime.Scripts.StateMachineBehaviour
{
    public class CustomResponseMenu : UnityEngine.StateMachineBehaviour
    {
        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (stateInfo.IsName("Roll In"))
            {
                animator.SetBool("Active", true);
            }
        
            // if (stateInfo.IsName("Focus"))
            // {
            //     WatchHandCursor.Unfreeze();
            // }
            //
            // if (stateInfo.IsName("Unfocus"))
            // {
            //     WatchHandCursor.Freeze();
            // }
        
            if (stateInfo.IsName("Hide"))
            {
                animator.SetBool("Active", false);
            }
        }

        // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
        //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        //{
        //    
        //}

        // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
        //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        //{
        //    
        //}

        // OnStateMove is called right after Animator.OnAnimatorMove()
        //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        //{
        //    // Implement code that processes and affects root motion
        //}

        // OnStateIK is called right after Animator.OnAnimatorIK()
        //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        //{
        //    // Implement code that sets up animation IK (inverse kinematics)
        //}
    }
}