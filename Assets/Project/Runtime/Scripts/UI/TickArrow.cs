using UnityEngine;
using Random = UnityEngine.Random;

namespace Project.Runtime.Scripts.UI
{
    public class TickArrow : MonoBehaviour
    {
        private int _count = 0;
        private float _tick = 0;
        private float nextAngle;

        private int randomStartingAngle;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame


        void Update()
        {
            // every second, rotate the rect transform by -6 degrees

            _tick += Time.deltaTime;
        

            if (_tick >= 1)
            {
                //  Debug.Log("ticking! " + (transform.localRotation.z - 6));
                _tick = 0;
                _count++;
                nextAngle = -6 * _count;
                LeanTween.cancel(gameObject);
                LeanTween.rotateLocal(gameObject, new Vector3(0, 0, nextAngle), 0.2f).setEaseInOutSine();
            }
        
            if (_count >= 60)
            {
                _count = 0;
            }

        
        }

        private void OnEnable()
        {
            _count = (int)Random.Range(0, 59);
            transform.localRotation = Quaternion.Euler(0, 0, _count * -6);
        
        }
    }
}