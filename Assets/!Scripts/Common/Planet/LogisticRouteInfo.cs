using Mirror;
using UnityEngine.EventSystems;

public class LogisticRouteInfo : NetworkBehaviour, IPointerDownHandler
{
    public LogisticRoute route;
    
    public void OnPointerDown(PointerEventData eventData) //нажатие на планету
    {
        AllSingleton.instance.logisticRouteUI.panel.OpenPanel();
        AllSingleton.instance.logisticRouteController.OpenRoute(route);
    }
}
