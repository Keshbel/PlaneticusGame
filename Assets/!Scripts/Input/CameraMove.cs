using UnityEngine;

public class CameraMove : MonoBehaviour
{
    public float speed;

    private Vector2 _startPos;
    private Camera _cam;

    private float _targetPosX;
    private float _targetPosY;

    // Start is called before the first frame update
    void Start()
    {
        _cam = GetComponent<Camera>();
        _targetPosX = transform.position.x;
        _targetPosY = transform.position.y;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) _startPos = _cam.ScreenToWorldPoint(Input.mousePosition);
        else if (Input.GetMouseButton(0))
        {
            float posX = _cam.ScreenToWorldPoint(Input.mousePosition).x - _startPos.x;
            float posY = _cam.ScreenToWorldPoint(Input.mousePosition).y - _startPos.y;
            
            _targetPosX = Mathf.Clamp(transform.position.x - posX, -11.5f, 11.5f);
            _targetPosY = Mathf.Clamp(transform.position.y - posY, -15.4f, 15.4f);
        }
        transform.position = new Vector3(Mathf.Lerp(transform.position.x, _targetPosX, speed * Time.deltaTime), 
                                         Mathf.Lerp(transform.position.y, _targetPosY, speed * Time.deltaTime), 
                                            transform.position.z);
    }
}