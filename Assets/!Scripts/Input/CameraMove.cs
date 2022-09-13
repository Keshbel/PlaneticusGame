using UnityEngine;

public class CameraMove : MonoBehaviour
{
    private Camera _mainCam;

    public bool isEnable = true;
    public float sensivity = 4;
    //движение камеры
    private Vector2 _startPos;
    private float _ortographicOffset;
    private float _targetPosX;
    private float _targetPosY;
    
    //зум
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
        
        //передвижение камеры
        if (Input.GetMouseButtonDown(0)) _startPos = _mainCam.ScreenToWorldPoint(Input.mousePosition);
        else if (Input.GetMouseButton(0))
        {
            float posX = _mainCam.ScreenToWorldPoint(Input.mousePosition).x - _startPos.x;
            float posY = _mainCam.ScreenToWorldPoint(Input.mousePosition).y - _startPos.y;

            //_ortographicOffset = _mainCam.orthographicSize / 5;
            _targetPosX = Mathf.Clamp(transform.position.x - posX, -11.5f, 11.5f);
            _targetPosY = Mathf.Clamp(transform.position.y - posY, -15.4f, 15.4f);
        }

        transform.position = new Vector3(Mathf.Lerp(transform.position.x, _targetPosX, sensivity * Time.deltaTime),
            Mathf.Lerp(transform.position.y, _targetPosY, sensivity * Time.deltaTime),
            transform.position.z);

        #if !UNITY_STANDALONE //масштабирование(зум) для мобилок
        //масштабирование(зум)
        if (Input.touchCount == 2)
        {
            _touchA = Input.GetTouch(0);
            _touchB = Input.GetTouch(1);

            _touchADirection = _touchA.position - _touchA.deltaPosition;
            _touchBDirection = _touchB.position - _touchB.deltaPosition;

            _dstBtwTouchesPositions = Vector2.Distance(_touchA.position, _touchB.position);
            _dstBtwTouchesDirections = Vector2.Distance(_touchADirection, _touchBDirection);

            _zoom = _dstBtwTouchesPositions - _dstBtwTouchesDirections;

            var currentZoom = _mainCam.orthographicSize - _zoom * sensivity;
            _mainCam.orthographicSize = Mathf.Clamp(currentZoom, zoomMin, zoomMax);
        }
        #else //масштабирование(зум) для пк платформ 
        _zoom = _mainCam.orthographicSize - Input.GetAxis("Mouse ScrollWheel") * sensivity;
        _mainCam.orthographicSize = Mathf.Clamp(_zoom, zoomMin, zoomMax);;
        #endif


    }
}