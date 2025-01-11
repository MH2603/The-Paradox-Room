using UnityEngine;

namespace MH.Superliminal
{

    public enum InteractableState 
    {
        None = 0,   
        OnHand = 1,
    }



    public class Interactable : MonoBehaviour
    {
        #region -------------------- Fields -------------------

        [SerializeField] private float _checkDistance = 15f;
        [SerializeField] private float _minScale = 0.1f;
        [SerializeField] private float _maxScale = 100f;
        [SerializeField] private LayerMask _wallLayer;
        #endregion

        #region -------------------- Properties -------------------

        [Header("------------ Status ------------")]
        [SerializeField] private InteractableState State;
        private float _startDragDistance;
        private Vector3 _startDragSize;
        private float _currentDragDistance;
        [SerializeField] private Transform _limitObject;

        private Vector3 _boxSize => transform.localScale.x * _collider.size;
        public Vector3 BoxSize;

        private Vector3 _handDir => transform.parent.forward;
        private Vector3 _handPos => transform.parent.position;

        private BoxCollider _collider;
        private Rigidbody _rb;

        #endregion

        #region -------------------- Unity Methods -------------------

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();    
            _collider = GetComponent<BoxCollider>();  
            
            State = InteractableState.None;
        }

        private void Update()
        {
            BoxSize = _boxSize;

            if (State == InteractableState.OnHand) Dragging();
        }

        #endregion

        #region -------------------- Public Methods -------------------

        public void StartDrag(Transform newParent=null)
        {
            if (!CanDrag()) return;

            State = InteractableState.OnHand; 
            
            transform.SetParent(newParent);

            _currentDragDistance = Vector3.Distance(transform.position, transform.parent.position);
            _startDragDistance = _currentDragDistance;
            _startDragSize = transform.lossyScale;
            _rb.isKinematic = true; 
        }

        public void EndDrag()
        {
            State = InteractableState.None;

            transform.SetParent(null);
            _rb.isKinematic = false;
        }

        #endregion

        #region -------------------- Private Methods -------------------

        private bool CanDrag()
        {
            if ( State == InteractableState.None )
            {
                return true;
            }

            return false;
        }

        private void Dragging()
        {

            //MoveAndScale();
            //MoveAndScale_Update();
            //MoveAndScaleByScaleBoxCast();
            MoveAndScaleByScaleOvelap();
        }


        private void MoveAndScale()
        {
            RaycastHit hit;

            float oldDistance = _currentDragDistance;

            VisualDrawDebug.DisplayBox(transform.parent.position + _handDir * _checkDistance/2, new Vector3(transform.lossyScale.x / 2, transform.lossyScale.y / 2, _checkDistance / 2), transform.parent.rotation, Color.blue, 0.02f);

            if ( Physics.BoxCast( transform.parent.position, transform.lossyScale / 2, _handDir, out hit, transform.rotation, _checkDistance, _wallLayer) )
            {
                _limitObject = hit.transform;
                VisualDrawDebug.DisplayBox(transform.parent.position + _handDir * hit.distance, transform.lossyScale / 2, transform.rotation, Color.red, 0.02f);

                transform.position = transform.parent.position + _handDir * hit.distance;

                _currentDragDistance = Vector3.Distance(transform.position, transform.parent.position);
                //transform.localScale *= _currentDragDistance / oldDistance;
                transform.localScale = _startDragSize * _currentDragDistance / _startDragDistance;


            }
        }

        private void MoveAndScale_Update()
        {
            RaycastHit hit;

            // Adjust the size of the BoxCast slightly to avoid edge issues
            Vector3 adjustedScale = transform.lossyScale / 2 + new Vector3(0.01f, 0.01f, 0.01f);

            // Debug the initial position and box size
            VisualDrawDebug.DisplayBox(transform.parent.position + _handDir * _checkDistance / 2, new Vector3(transform.lossyScale.x / 2, transform.lossyScale.y / 2, _checkDistance / 2), transform.parent.rotation, Color.blue, 0.02f);

            // Perform the BoxCast
            if (Physics.BoxCast(transform.parent.position, adjustedScale, _handDir, out hit, transform.rotation, _checkDistance, _wallLayer))
            {
                _limitObject = hit.transform;

                // Debug the hit position and box size
                VisualDrawDebug.DisplayBox(hit.point, Vector3.one / 4, transform.rotation, Color.red, 0.02f);
                VisualDrawDebug.DisplayBox(transform.parent.position + _handDir * hit.distance, adjustedScale, transform.rotation, Color.yellow, 0.02f);

                // Update position based on hit distance
                transform.position = transform.parent.position + _handDir * hit.distance;

                // Calculate the current drag distance
                _currentDragDistance = Vector3.Distance(transform.position, transform.parent.position);

                // Scale the object based on drag distance
                transform.localScale = _startDragSize * _currentDragDistance / _startDragDistance;
            }
            else
            {
                // Debug the case when no hit is detected
                Debug.Log("No hit detected with BoxCast");
            }
        }

        private void MoveAndScaleByScaleBoxCast()
        {
            RaycastHit hit;

            float unitDistance = 0.05f;
            int loopAmount = (int)(_checkDistance / unitDistance);

            for (int i = 0; i < loopAmount; i++)
            {

                Vector3 checkPos = _handPos + _handDir * i * unitDistance;
                float checkScale = i * unitDistance / _startDragDistance;

                VisualDrawDebug.DisplayBox(checkPos, _boxSize / 2 * checkScale, transform.rotation, Color.yellow, 0.02f);

                if (Physics.BoxCast(checkPos, _boxSize / 2 * checkScale, _handDir, out hit, transform.rotation, 0f, _wallLayer))
                {
                    _limitObject = hit.transform;

                    // Debug the hit position and box size
                    VisualDrawDebug.DisplayBox(hit.point, Vector3.one / 4, transform.rotation, Color.red, 0.02f);
                    VisualDrawDebug.DisplayBox(checkPos, _boxSize / 2 * checkScale, transform.rotation, Color.yellow, 0.02f);

                    // Update position based on hit distance
                    transform.position = checkPos;

                    // Calculate the current drag distance
                    _currentDragDistance = Vector3.Distance(transform.position, transform.parent.position);

                    // Scale the object based on drag distance
                    transform.localScale = _startDragSize * _currentDragDistance / _startDragDistance;

                    break;
                }

            }

        }

        private void MoveAndScaleByScaleOvelap()
        {
            Collider[] colliders;

            float unitDistance = 0.02f;
            int loopAmount = (int)(_checkDistance / unitDistance);

            for (int i = 0; i < loopAmount; i++)
            {

                Vector3 checkPos = _handPos + _handDir * i * unitDistance;
                float checkScale = i * unitDistance / _startDragDistance;
                Vector3 checkHalfSize = _startDragSize / 2 * checkScale;

                colliders = Physics.OverlapBox(checkPos, checkHalfSize, transform.rotation, _wallLayer );

                //VisualDrawDebug.DisplayBox(checkPos, checkHalfSize, transform.rotation, Color.yellow, 0.02f);

                if (colliders.Length != 0 )
                {
                    //_limitObject = hit.transform;

                    // Debug the hit position and box size
                    //VisualDrawDebug.DisplayBox(hit.point, Vector3.one / 4, transform.rotation, Color.red, 0.02f);
                    VisualDrawDebug.DisplayBox(checkPos, checkHalfSize, transform.rotation, Color.yellow, 0.02f);

                    // Update position based on hit distance
                    transform.position = checkPos;

                    // Calculate the current drag distance
                    _currentDragDistance = Vector3.Distance(transform.position, transform.parent.position);

                    // Scale the object based on drag distance
                    transform.localScale = _startDragSize * _currentDragDistance / _startDragDistance;

                    break;
                }

            }
        }

        #endregion

    }

}
