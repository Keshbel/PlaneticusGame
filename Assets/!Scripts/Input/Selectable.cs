using UnityEngine;
using UnityEngine.EventSystems;

public class Selectable : MonoBehaviour, IPointerDownHandler
{
    public GameObject selectingObject;
    public bool isSelecting;

    private void OnEnable()
    {
        if (!selectingObject)
            selectingObject = gameObject.transform.GetChild(0).gameObject;
    }

    private void Selecting()
    {
        selectingObject.SetActive(!selectingObject.activeSelf);
        isSelecting = !isSelecting;
        
        if (isSelecting)
            gameObject.transform.localScale = new Vector3(0.17f,0.17f,0.17f);
        else
        {
            gameObject.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);
        }
    }

    /*public void OnPointerEnter(PointerEventData eventData)
    {
        Selecting();
    }
    
    
    
    public void OnPointerClick(PointerEventData eventData)
    {
        print("CLICK!!!");
        //if (eventData.pointerClick == gameObject)
            Selecting();
    }*/

    public void OnPointerDown(PointerEventData eventData)
    {
        Selecting();
    }
}
