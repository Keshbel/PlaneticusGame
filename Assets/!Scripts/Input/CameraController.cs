using Cinemachine;
using DG.Tweening;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public bool isEnable = true;
    
    [Header("Components")]
    [SerializeField] private CinemachineInputProvider inputProvider;
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    
    [Header("Follow & Bounds")]
    [SerializeField] private Collider2D boundCollider2D;
    [SerializeField] private Transform cameraTransform;
    
    [Header("Options")]
    [SerializeField] private float panSpeed = 10f;
    
    [Space]
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float zoomInMax = 5f;
    public float zoomOutMax = 10f;
    
    [Header("Zoom")]
    private Touch _touchA;
    private Touch _touchB;
    private Vector2 _touchADirection;
    private Vector2 _touchBDirection;
    private float _dstBtwTouchesPositions;
    private float _dstBtwTouchesDirections;

    private void Awake()
    {
        if (inputProvider == null) inputProvider = GetComponent<CinemachineInputProvider>();
        if (virtualCamera == null) virtualCamera = GetComponent<CinemachineVirtualCamera>();
        if (cameraTransform == null && virtualCamera != null) cameraTransform = virtualCamera.VirtualCameraGameObject.transform;
        
        //_targetPosX = transform.position.x;
        //_targetPosY = transform.position.y;
        
        //cinemachineConfiner.
    }
    
    void Update()
    {
        if (!isEnable || inputProvider == null) return;

        float x = 0; 
        float y = 0;
        float z = 0; 

        if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0) //передвижение с клавиатуры/стиков
        {
            float xKey = Input.GetAxis("Horizontal"); 
            float yKey = Input.GetAxis("Vertical");

            if (xKey != 0) x = xKey;
            if (yKey != 0) y = yKey;
            
            StickScreen(x, y);
        }
        else
        {
            x = inputProvider.GetAxisValue(0);
            y = inputProvider.GetAxisValue(1);
            
            if (x != 0 || y != 0) PanScreen(x,y);
        }

        z = inputProvider.GetAxisValue(2);
        if (z != 0) ZoomScreen(z);

#if !UNITY_STANDALONE //масштабирование(зум) для мобилок
        if (Input.touchCount == 1) Move(0);
        
        if (Input.touchCount == 2)
        {
            _touchA = Input.GetTouch(0);
            _touchB = Input.GetTouch(1);

            _touchADirection = _touchA.position - _touchA.deltaPosition;
            _touchBDirection = _touchB.position - _touchB.deltaPosition;

            _dstBtwTouchesPositions = Vector2.Distance(_touchA.position, _touchB.position);
            _dstBtwTouchesDirections = Vector2.Distance(_touchADirection, _touchBDirection);

            _zoom = _dstBtwTouchesPositions - _dstBtwTouchesDirections;

            var currentZoom = _mainCam.orthographicSize - _zoom * sensivity/4;
            _mainCam.orthographicSize = Mathf.Clamp(currentZoom, zoomMin, zoomMax);
        }
/*#else //масштабирование(зум) для пк платформ 
        /*_zoom = cameraTransform.orthographicSize - Input.GetAxis("Mouse ScrollWheel") * sensivity;
        cameraTransform.orthographicSize = Mathf.Clamp(_zoom, zoomMin, zoomMax);;
        
        Move(1);#1#

        if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0) //передвижение с клавиатуры/стиков
        {
            sensivity = 12f;
            if (Input.GetAxis("Horizontal") > 0) 
            {
                _targetPosX = Mathf.Clamp(transform.position.x + 1f, xMinBorder, xMaxBorder);
            }
            else if (Input.GetAxis("Horizontal") < 0)
            {
                _targetPosX = Mathf.Clamp(transform.position.x - 1f, xMinBorder, xMaxBorder);
            }
            if (Input.GetAxis("Vertical") > 0)
            {
                _targetPosY = Mathf.Clamp(transform.position.y + 1f, yMinBorder, yMaxBorder);
            }
            else if (Input.GetAxis("Vertical") < 0)
            {
                _targetPosY = Mathf.Clamp(transform.position.y - 1f, yMinBorder, yMaxBorder);
            }
        }*/
#endif

        /*transform.position = new Vector3(Mathf.Lerp(transform.position.x, _targetPosX, sensivity * Time.deltaTime),
            Mathf.Lerp(transform.position.y, _targetPosY, sensivity * Time.deltaTime),
            transform.position.z);*/
    }

    private void ZoomScreen(float increment)
    {
        float fov = virtualCamera.m_Lens.OrthographicSize;
        float target = Mathf.Clamp(fov - increment, zoomInMax, zoomOutMax);
        virtualCamera.m_Lens.OrthographicSize = Mathf.Lerp(fov, target, zoomSpeed * Time.deltaTime);
    }
    
    private Vector2 PanDirection(float x, float y)
    {
        Vector2 direction = Vector2.zero;

        if (y >= Screen.height * .95f) direction.y += 1;
        else if (y <= Screen.height * .05f) direction.y -= 1;
        
        if (x >= Screen.width * .95f) direction.x += 1;
        else if (x <= Screen.width * .05f) direction.x -= 1;

        return direction;
    }

    private void PanScreen(float x, float y)
    {
        Vector2 direction = PanDirection(x, y);
        
        var position = cameraTransform.position;
        if (!boundCollider2D.OverlapPoint(position + (Vector3)direction )) return;
        position = Vector3.Lerp(position, position + (Vector3)direction, panSpeed * Time.deltaTime);
        cameraTransform.position = position;
    }

    private void StickScreen(float x, float y)
    {
        Vector2 direction = Vector2.zero;
        direction.x += x;
        direction.y += y;
        
        var position = cameraTransform.position;
        if (!boundCollider2D.OverlapPoint(position + (Vector3)direction )) return;
        position = Vector3.Lerp(position, position + (Vector3)direction, panSpeed * Time.deltaTime);
        cameraTransform.position = position;
    }

    public void DoCustomMove(float x, float y, float duration)
    {
        isEnable = false;
        cameraTransform.DOLocalMoveX(x, duration);
        cameraTransform.DOLocalMoveY(y, duration).OnComplete(()=> isEnable = true);
    }
}