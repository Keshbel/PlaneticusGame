using UnityEngine;

public class Parallax : MonoBehaviour
{
    public GameObject cam;

    private float _startPosX;
    private float _startPosY;

    public float parallax;

    private void Start()
    {
        if (!cam)
            cam = Camera.main.gameObject;
        
        _startPosX = transform.position.x;
        _startPosY = transform.position.y;
    }

    private void FixedUpdate()
    {
        float distX = cam.transform.position.x * (1 - parallax);
        float distY = cam.transform.position.y * (1 - parallax);
        transform.position = new Vector3(_startPosX + distX, _startPosY + distY, transform.position.z);
    }
}
