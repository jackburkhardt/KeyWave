using UnityEngine;

namespace Interaction
{
    public class InputManager : MonoBehaviour
    {

        [SerializeField] private Camera _camera;
        [SerializeField] private LayerMask _interactLayerMask;
        public static bool _controlsEnabled = true;
        private GameObject lastHitGO;

        private void Awake()
        {
            GameEvent.OnInteractionStart += obj => _controlsEnabled = false;
            GameEvent.OnInteractionEnd += obj => _controlsEnabled = true;
        }

        private void Update()
        {
            if (!_controlsEnabled) return;

        }
    }
}
