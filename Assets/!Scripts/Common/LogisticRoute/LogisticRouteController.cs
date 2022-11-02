using Mirror;

public class LogisticRouteController : NetworkBehaviour
{
    /*public void OpenRoute(LogisticRoute route)
    {
        if (route?.SaveResources?.Count == 0) return;
        
        var routeUI = AllSingleton.instance.logisticRouteUI;

        var fromName = route.fromTransform.GetComponent<PlanetController>().namePlanet;
        var toName = route.toTransform.GetComponent<PlanetController>().namePlanet;
        routeUI.planetNameText.text = fromName + "-" + toName;

        foreach (var button in routeUI.resButtons)
        {
            button.interactable = false;
            button.onClick.RemoveAllListeners();
        }
        
        routeUI.waterText.text = "0";
        routeUI.earthText.text = "0";
        routeUI.fireText.text = "0";
        routeUI.airText.text = "0";
        routeUI.aetherText.text = "0";
        
        foreach (var resource in route.SaveResources)
        {
            switch (resource.resourcePlanet)
            {
                case Enums.ResourcePlanet.Water:
                    routeUI.waterText.text = resource.resourceDelivery.ToString();
                    
                    if (isServer) routeUI.waterButton.onClick.AddListener(()=> StartCoroutine(route.ReturnResource(resource)));
                    else routeUI.waterButton.onClick.AddListener(()=> route.CmdReturnResource(resource));
                    
                    routeUI.waterButton.onClick.AddListener(()=> routeUI.waterText.text = resource.resourceDelivery.ToString());
                    routeUI.waterButton.onClick.AddListener(() => { routeUI.waterButton.interactable = resource.resourceDelivery > 0 && resource.isLogistic; });
                    routeUI.waterButton.interactable = resource.isLogistic;
                    break;
                
                case Enums.ResourcePlanet.Earth:
                    routeUI.earthText.text = resource.resourceDelivery.ToString();
                    
                    if (isServer) routeUI.earthButton.onClick.AddListener(()=> StartCoroutine(route.ReturnResource(resource)));
                    else routeUI.earthButton.onClick.AddListener(()=> route.CmdReturnResource(resource));
                    
                    routeUI.earthButton.onClick.AddListener(()=> routeUI.earthText.text = resource.resourceDelivery.ToString());
                    routeUI.earthButton.onClick.AddListener(() => { routeUI.earthButton.interactable = resource.resourceDelivery > 0 && resource.isLogistic; });
                    routeUI.earthButton.interactable = resource.isLogistic;
                    break;
                
                case Enums.ResourcePlanet.Fire:
                    routeUI.fireText.text = resource.resourceDelivery.ToString();
                    
                    if (isServer) routeUI.fireButton.onClick.AddListener(()=> StartCoroutine(route.ReturnResource(resource)));
                    else routeUI.fireButton.onClick.AddListener(()=> route.CmdReturnResource(resource)); 
                    
                    routeUI.fireButton.onClick.AddListener(()=> routeUI.fireText.text = resource.resourceDelivery.ToString());
                    routeUI.fireButton.onClick.AddListener(() => { routeUI.fireButton.interactable = resource.resourceDelivery > 0 && resource.isLogistic; });
                    routeUI.fireButton.interactable = resource.isLogistic;
                    break;
                
                case Enums.ResourcePlanet.Air:
                    routeUI.airText.text = resource.resourceDelivery.ToString();
                    
                    if (isServer) routeUI.airButton.onClick.AddListener(()=> StartCoroutine(route.ReturnResource(resource)));
                    else routeUI.airButton.onClick.AddListener(()=> route.CmdReturnResource(resource));
                    
                    routeUI.airButton.onClick.AddListener(()=> routeUI.airText.text = resource.resourceDelivery.ToString());
                    routeUI.airButton.onClick.AddListener(() => { routeUI.airButton.interactable = resource.resourceDelivery > 0 && resource.isLogistic; });
                    routeUI.airButton.interactable = resource.isLogistic;
                    break;
                
                case Enums.ResourcePlanet.Aether:
                    routeUI.aetherText.text = resource.resourceDelivery.ToString();
                    
                    if (isServer) routeUI.aetherButton.onClick.AddListener(() => StartCoroutine(route.ReturnResource(resource)));
                    else routeUI.aetherButton.onClick.AddListener(()=> route.CmdReturnResource(resource));
                    
                    routeUI.aetherButton.onClick.AddListener(()=> routeUI.aetherText.text = resource.resourceDelivery.ToString());
                    routeUI.aetherButton.onClick.AddListener(() => { routeUI.aetherButton.interactable = resource.resourceDelivery > 0 && resource.isLogistic; });
                    routeUI.aetherButton.interactable = resource.isLogistic;
                    break;
            }
        }

        routeUI.deleteRouteButton.interactable = route.SaveResources?.Find(r => r.isLogistic) != null; //если нет прожимаемых кнопок, то пока что нельзя удалить маршрут
        
        if (isServer) routeUI.deleteRouteButton.onClick.AddListener(()=> route.ReturnAllResource(route));
        else routeUI.deleteRouteButton.onClick.AddListener(()=> route.CmdReturnAllResource(route));
        
        routeUI.deleteRouteButton.onClick.AddListener(AllSingleton.instance.logisticRouteUI.panel.ClosePanel);
    }*/
}
