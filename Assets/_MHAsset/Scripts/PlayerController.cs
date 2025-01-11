using UnityEngine;

namespace MH.Superliminal
{

    public class PlayerController : MonoBehaviour
    {
        #region -------------------- Fields -------------------

        [SerializeField] private float _canDragDistance = 10f;
        [SerializeField] private Transform _hand;

        #endregion

        #region -------------------- Properties -------------------

        [SerializeField] private Interactable _currentInteractable;

        #endregion



        #region -------------------- Unity Methods -------------------

        private void Update()
        {
            if ( Input.GetMouseButtonDown(0) &&
                 Physics.Raycast(_hand.position, _hand.forward, out RaycastHit hit, _canDragDistance) &&
                 hit.transform && 
                 hit.transform.GetComponent<Interactable>() )
            {
                _currentInteractable = hit.transform.GetComponent<Interactable>();
                _currentInteractable.StartDrag(_hand);
            }

            if (Input.GetMouseButtonUp(0) &&
                 _currentInteractable != null )
            {
                _currentInteractable.EndDrag();
                _currentInteractable = null;  
            }
        }

        #endregion

    }

}
