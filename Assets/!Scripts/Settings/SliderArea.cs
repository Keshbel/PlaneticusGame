using System.Collections;
using UnityEngine;
using UnityEngine.Events;
 
public class SliderArea: MonoBehaviour
{
    public RectTransform Handle;
    public RectTransform Area;
 
    float Width;
    float Height;
 
    public UnityEvent onValueChanged;

    void Start()
    {
        if (onValueChanged == null) onValueChanged = new UnityEvent();
 
        Width = Area.rect.width;
        Height = Area.rect.height;
        
        StartParameters();
    }

    public Vector2 Value()
    {
        float x = 0;
        float y = 0;
        Vector2 returnValue;
 
        x = Handle.localPosition.x / Width;
        y = Handle.localPosition.y / Height;
 
        returnValue = new Vector2(x, y);
        return returnValue;
    }
 
    public IEnumerator Drag()
    {
        Vector2 currentValue;
        Vector2 prevValue;
 
        while (Input.GetMouseButton(0))
        {
            prevValue = Value();
 
            Handle.position = Input.mousePosition;

            if (Handle.localPosition.x < 0)
            {
                Handle.localPosition = new Vector3(0, Handle.localPosition.y, Handle.localPosition.z);
            }else if(Handle.localPosition.x > Width)
            {
                Handle.localPosition = new Vector3(Width, Handle.localPosition.y, Handle.localPosition.z);
            }
 
            if (Handle.localPosition.y < 0)
            {
                Handle.localPosition = new Vector3(Handle.localPosition.x, 0, Handle.localPosition.z);
            }
            else if (Handle.localPosition.y > Height)
            {
                Handle.localPosition = new Vector3(Handle.localPosition.x, Height, Handle.localPosition.z);
            }
 
            currentValue = Value();

            bool valueChange = currentValue.x != prevValue.x || currentValue.y != prevValue.y;
            if (valueChange)
            {
                onValueChanged.Invoke();
                
                PlayerPrefsExtra.SetVector3("HandlePosition", Handle.localPosition);
                PlayerPrefs.Save();
            }

            yield return null;
        }
 
        yield return null;
    }

    private void StartParameters()
    {
        Handle.localPosition = PlayerPrefsExtra.GetVector3("HandlePosition", Handle.localPosition);
        onValueChanged.Invoke();
    }

    public void MouseDown()
    {
        StartCoroutine(Drag());
    }
}