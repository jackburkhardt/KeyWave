using UnityEngine;

namespace Project.Runtime.Scripts.UI.Map
{
    public class MapArrow : MonoBehaviour
    {
        Animator animator;

        // Start is called before the first frame update
        void Start()
        {
            animator = GetComponent<Animator>();
        }

        // Update is called once per frame
        void Update()
        {
        
        }


        private void OnMouseExit()
        {
        
            animator.StopPlayback();
        }

        private void OnMouseOver()
        {
            //  animator.Play("Hold");
            animator.StartPlayback();

        }
    }
}