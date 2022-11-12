using DG.Tweening;
using UnityEngine;

public class UIRotates : MonoBehaviour
{
    public Transform targetTransform; //например, черная дыра
    public Quaternion newRotation;

    private void Update()
    {
        transform.rotation = Quaternion.Euler(Vector3.zero);
        transform.RotateAround(targetTransform.position, Vector3.forward, 20 * Time.deltaTime);
        transform.rotation = Quaternion.Euler(Vector3.zero);
        //transform.DOShakeRotation(0, 0.1f, 1);
        /*newRotation = Quaternion.AngleAxis(Random.Range(-90f, 0f),Vector3.forward);
        transform.rotation = Quaternion.Slerp(transform.rotation, newRotation, 0.05f);
        //transform.Rotate(0,0, Random.Range(-0.50f, 0.50f), Space.Self);*/
    }
}
