using UnityEngine;

namespace Project.Runtime.Scripts.Utility
{
    public class AudioSourceAnimationEvents : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        public void PlayAudioSource()
        {
            GetComponent<AudioSource>().Play();
        }
    }
}