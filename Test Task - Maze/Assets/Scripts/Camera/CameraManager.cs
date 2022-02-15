using UnityEngine;

namespace RFTestTaskMaze
{
    [RequireComponent(typeof(Transform))]
    [RequireComponent(typeof(Camera))]
    public class CameraManager : MonoBehaviour, ICameraManager
    {
        private Transform _transform;
        private Camera _camera;

        private Vector3 _target;
        private float _zoom;

        private bool _isInitialized;

        private float _minZoomSize;
        private float _maxZoomSize;
        private Rect _field;

        public Camera Camera => _camera;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
            _transform = transform;

            _target = _transform.position;
            _zoom = _camera.orthographicSize;
            
            Services.Register(this);
        }
        
        private void OnDestroy()
        {
            Services.Unregister(this);
        }

        public void Initialize(float minSize, float maxSize, Rect field)
        {
            _minZoomSize = minSize;
            _maxZoomSize = maxSize;
            _field = field;

            _isInitialized = true;
        }

        public void SetTarget(Vector2 targetPos)
        {
            _target = new Vector3(targetPos.x, targetPos.y, _transform.position.z);
        }

        public void SetZoom(float zoom)
        {
            _zoom = zoom;
        }

        public void Recenter()
        {
            SetTarget(_field.center);
            SetZoom(_maxZoomSize);
        }

        private void Update()
        {
            if (_isInitialized)
            {
                UpdateCameraPosition();
                UpdateCameraZoom();
            }
        }
        
        private void UpdateCameraPosition()
        {
            Vector3 position = _transform.position;
            
            Vector3 posDelta = (_target - position) * Time.deltaTime;
            _transform.position = position + posDelta;
        }

        private void UpdateCameraZoom()
        {
            float orthographicSize = _camera.orthographicSize;
            float newZoom = Mathf.Clamp(_zoom, _minZoomSize, _maxZoomSize);
            
            float zoomDelta = newZoom - orthographicSize;
            _camera.orthographicSize = orthographicSize + zoomDelta * Time.deltaTime;
        }
    }
}