using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Mirror;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlanetController : NetworkBehaviour
{
    [SyncVar]
    public string namePlanet;

    //resources
    [SyncVar]
    public int indexCurrentResource;
    public SyncList<ResourceForPlanet> planetResources = new SyncList<ResourceForPlanet>(); //ресурсы на планете
    public List<GameObject> resourcesIcon;
    
    //super planet
    [SyncVar]
    public bool isSuperPlanet = false; //является ли супер планетой? (все 5 ресурсов на ней)
    
    //home
    [SyncVar]
    public bool isHomePlanet = false; //является ли стартовой планетой?
    [SyncVar]
    public bool isColonized;
    public GameObject homeIcon;

    private void Start()
    {
        HomingPlanetShow();
    }

    public void AddResource(ResourceForPlanet resource) //добавление ресурса для планеты, с ограничением добавления
    {
        if (planetResources.Count < 5)
        {
            planetResources.Add(resource);
            resource.UpdateInfo();
            ResourceIconShow();
        }
        else
        {
            print("Вы пытаетесь добавить шестой ресурс...");
        }
    }

    public void AddResourcesForPlanet()
    {
        var res1 = new ResourceForPlanet {resourcePlanet = (Enums.ResourcePlanet) Random.Range(0, 4), resourceMining = 1};
        var res2 = new ResourceForPlanet {resourcePlanet = (Enums.ResourcePlanet) Random.Range(0, 4), resourceMining = 1};
        AddResource(res1);
        AddResource(res2);
    }

    public void SetHomePlanet()
    {
        planetResources.Clear();
        
        var res1 = new ResourceForPlanet {resourcePlanet = Enums.ResourcePlanet.Wind, resourceMining = 1};
        var res2 = new ResourceForPlanet {resourcePlanet = Enums.ResourcePlanet.Water, resourceMining = 1};
        var res3 = new ResourceForPlanet {resourcePlanet = Enums.ResourcePlanet.Earth, resourceMining = 1};
        var res4 = new ResourceForPlanet {resourcePlanet = Enums.ResourcePlanet.Fire, resourceMining = 1};
        var res5 = new ResourceForPlanet {resourcePlanet = Enums.ResourcePlanet.Aether, resourceMining = 1};
        
        AddResource(res1);
        AddResource(res2);
        AddResource(res3);
        AddResource(res4);
        AddResource(res5);
    }

    public void OpenPlanet()
    {
        indexCurrentResource = 0;
        SubscribeResourceToggle();
        UpdateInfo();
    }
    
    public void SubscribeResourceToggle()
    {
        var planetPanel = AllSingleton.instance.planetPanelUI;
        
        for (var index = 0; index < 5; index++) //отписка
        {
            planetPanel.resToggles[index].onValueChanged.RemoveAllListeners();
            planetPanel.resToggles[index].interactable = false;
        }
        
        for (var index = 0; index < planetResources.Count; index++) //подписка
        {
            var index1 = index;
            planetPanel.resToggles[index].onValueChanged.AddListener((b)=>SelectResource(index1));
            planetPanel.resToggles[index].interactable = true;
        }
    }
    
    
    public void SelectResource(int indexResource)
    {
        indexCurrentResource = indexResource;
        UpdateInfo();
    }

    public void ResourceIconShow()
    {
        if (!isColonized) return;
        
        foreach (var icon in resourcesIcon) //вырубаем все иконки
        {
            icon.SetActive(false);
        }

        foreach (var resource in planetResources) //включаем нужные
        {
            switch (resource.resourcePlanet)
            {
                case Enums.ResourcePlanet.Wind:
                    resourcesIcon[0].SetActive(true);
                    break;
                case Enums.ResourcePlanet.Water:
                    resourcesIcon[1].SetActive(true);
                    break;
                case Enums.ResourcePlanet.Earth:
                    resourcesIcon[2].SetActive(true);
                    break;
                case Enums.ResourcePlanet.Fire:
                    resourcesIcon[3].SetActive(true);
                    break;
                case Enums.ResourcePlanet.Aether:
                    resourcesIcon[4].SetActive(true);
                    break;
            }
        }
    }
    
    public void HomingPlanetShow() //отображение иконки для родной планеты
    {
        if (!AllSingleton.instance.currentPlayer) return;
        if (AllSingleton.instance.currentPlayer.playerPlanets.Contains(this) && isHomePlanet)
        {
            if (homeIcon)
                homeIcon.SetActive(true);
        }
    }
    
    public void UpdateInfo()
    {
        SubscribeResourceToggle();
        
        var planetPanel = AllSingleton.instance.planetPanelUI;
        var curRes = planetResources[indexCurrentResource];
        
        planetPanel.planetNameText.text = namePlanet;
        
        planetPanel.resourceNameText.text = curRes.nameResource;
        planetPanel.resourceDeliveryText.text = curRes.resourceDelivery.ToString();
        planetPanel.resourceMiningText.text = curRes.resourceMining.ToString();
        planetPanel.resourceAllText.text = (curRes.resourceDelivery+curRes.resourceMining).ToString();
        planetPanel.resourceIconImage.sprite = curRes.spriteIcon;
        
        planetResources[indexCurrentResource].UpdateInfo();
    }
}
