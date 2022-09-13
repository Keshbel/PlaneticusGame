using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlanetController : MonoBehaviour
{
    public string namePlanet;

    public int indexCurrentResource;
    public List<ResourceForPlanet> planetResources; //ресурсы на планете
    public bool isSuperPlanet; //является ли супер планетой? (все 5 ресурсов на ней)

    public void AddResource(ResourceForPlanet resource) //добавление ресурса для планеты, с ограничение добавления
    {
        if (planetResources.Count < 5)
        {
            planetResources.Add(resource);
            resource.UpdateInfo();
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
