using DG.Tweening;
using Mirror;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Camera _mainCam;

    [Header("Options")]
    public bool isEnable = true;
    public float sensivity = 4;
    
    //движение камеры
    private Vector2 _startPos;
    private float _ortographicOffset;
    private float _targetPosX;
    private float _targetPosY;
    private Vector3 _newPos;
    
    //ограничение границ камеры
    public float xMinBorder;
    public float xMaxBorder;
    public float yMinBorder;
    public float yMaxBorder;

    [Header("Zoom")]
    public float zoomMax;
    public float zoomMin;
    private float _zoom;
    private Touch _touchA;
    private Touch _touchB;
    private Vector2 _touchADirection;
    private Vector2 _touchBDirection;
    private float _dstBtwTouchesPositions;
    private float _dstBtwTouchesDirections;

    // Start is called before the first frame update
    void Start()
    {
        _mainCam = GetComponent<Camera>();
        _targetPosX = transform.position.x;
        _targetPosY = transform.position.y;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isEnable) return;

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
#else //масштабирование(зум) для пк платформ 
        _zoom = _mainCam.orthographicSize - Input.GetAxis("Mouse ScrollWheel") * sensivity;
        _mainCam.orthographicSize = Mathf.Clamp(_zoom, zoomMin, zoomMax);;
        
        Move(1);

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
        }
#endif

        transform.position = new Vector3(Mathf.Lerp(transform.position.x, _targetPosX, sensivity * Time.deltaTime),
            Mathf.Lerp(transform.position.y, _targetPosY, sensivity * Time.deltaTime),
            transform.position.z);
    }

    private void Move(int isStandalone)
    {
        //передвижение камеры c помощью мыши или тача
        if (Input.GetMouseButtonDown(isStandalone)) _startPos = _mainCam.ScreenToWorldPoint(Input.mousePosition);
        else if (Input.GetMouseButton(isStandalone))
        {
            sensivity = 4f;
            float posX = _mainCam.ScreenToWorldPoint(Input.mousePosition).x - _startPos.x;
            float posY = _mainCam.ScreenToWorldPoint(Input.mousePosition).y - _startPos.y;

            _targetPosX = Mathf.Clamp(transform.position.x - posX, xMinBorder, xMaxBorder);
            _targetPosY = Mathf.Clamp(transform.position.y - posY, yMinBorder, yMaxBorder);
        }
    }
    
    public void DoMove(float x, float y, float duration)
    {
        isEnable = false;
        _targetPosX = x;
        _targetPosY = y;
        transform.DOLocalMoveX(x, duration);
        transform.DOLocalMoveY(y, duration).OnComplete(()=> isEnable = true);
    }
}