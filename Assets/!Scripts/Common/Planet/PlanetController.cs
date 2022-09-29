using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class PlanetController : NetworkBehaviour
{
    [SyncVar]
    public string namePlanet;
    [SyncVar]
    public int indSpritePlanet;

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
        SetSpritePlanet();
        Invoke(nameof(SetSpritePlanet), 0.4f);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        HomingPlanetShow();
        if (isServer)
            RcpResourceIconShow();
        else
        {
            ResourceIconShow();
        }

        Invoke(nameof(ResourceIconShow), 1f);
        Invoke(nameof(HomingPlanetShow), 1f);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            /*if (isClient)
            {
                CmdAddResource(new ResourceForPlanet
                    {resourcePlanet = (Enums.ResourcePlanet) Random.Range(0, 4), resourceMining = 1});
                planetResources.Clear();
                CmdResourceIconShow();
            }*/

            if (isServer)
            {
                RcpResourceIconShow();
            }
        }
    }

    [Server]
    public void AddResource(ResourceForPlanet resource) //добавление ресурса для планеты, с ограничением добавления
    {
        if (planetResources.Count < 5)
        {
            planetResources.Add(resource);
            
            resource.UpdateInfo();
            RcpResourceIconShow();
        }
        else
        {
            print("Вы пытаетесь добавить шестой ресурс...");
        }
    }
    
    [Command]
    public void CmdAddResource(ResourceForPlanet resource)
    {
        AddResource(resource);
    }
    
    public void AddResourcesForPlanet()
    {
        var res1 = new ResourceForPlanet {resourcePlanet = (Enums.ResourcePlanet) Random.Range(0, 4), resourceMining = 1};
        var res2 = new ResourceForPlanet {resourcePlanet = (Enums.ResourcePlanet) Random.Range(0, 4), resourceMining = 1};
        if (isServer)
        {
            AddResource(res1);
            AddResource(res2);
            RcpResourceIconShow();
        }
        else
        {
            CmdAddResource(res1);
            CmdAddResource(res2);
            ResourceIconShow();
        }
    }
    
    public void SetHomePlanet()
    {
        if (isServer) //очистка и буль он
        {
            ClearPlanetResource();
            SetHomePlanetBoolTrue();
        }
        else
        {
            CmdClearPlanetResource();
            CmdSetHomePlanetBoolTrue();
        }

        var res1 = new ResourceForPlanet {resourcePlanet = Enums.ResourcePlanet.Wind, resourceMining = 1};
        var res2 = new ResourceForPlanet {resourcePlanet = Enums.ResourcePlanet.Water, resourceMining = 1};
        var res3 = new ResourceForPlanet {resourcePlanet = Enums.ResourcePlanet.Earth, resourceMining = 1};
        var res4 = new ResourceForPlanet {resourcePlanet = Enums.ResourcePlanet.Fire, resourceMining = 1};
        var res5 = new ResourceForPlanet {resourcePlanet = Enums.ResourcePlanet.Aether, resourceMining = 1};

        if (isServer) //добавление ресурсов
        {
            AddResource(res1);
            AddResource(res2);
            AddResource(res3);
            AddResource(res4);
            AddResource(res5);
            RcpResourceIconShow();
        }
        else
        {
            CmdAddResource(res1);
            CmdAddResource(res2);
            CmdAddResource(res3);
            CmdAddResource(res4);
            CmdAddResource(res5);
            ResourceIconShow();
        }
    }

    #region HomePlanetBoolTrue
    [Server]
    public void SetHomePlanetBoolTrue()
    {
        isHomePlanet = true;
    }
    [Command]
    public void CmdSetHomePlanetBoolTrue()
    {
        SetHomePlanetBoolTrue();
    }
    #endregion

    #region ColonizedBoolTrue
    [Server]
    public void SetColonizedBoolTrue()
    {
        isColonized = true;
    }
    [Command]
    public void CmdSetColonizedBoolTrue()
    {
        SetColonizedBoolTrue();
    }
    #endregion

    #region ClearPlanetResource
    [Server]
    public void ClearPlanetResource()
    {
        planetResources.Clear();
    }
    [Command]
    public void CmdClearPlanetResource()
    {
        ClearPlanetResource();
    }
    #endregion
    
    public void SetSpritePlanet() //назначить внешний вид планеты
    {
        var spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = AllSingleton.instance.planetGeneration.listSpritePlanet[indSpritePlanet];
    }

    public void OpenPlanet()
    {
        if (planetResources.Count > 0)
            SelectResource(0);
        if (planetResources.Count > 1)
            SelectResource(1);
        if (planetResources.Count > 2)
            SelectResource(2);
        if (planetResources.Count > 3)
            SelectResource(3);
        if (planetResources.Count > 4)
            SelectResource(4);
        
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
        if (!isColonized || planetResources.Count == 0) return;
        
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

    [ClientRpc]
    public void RcpResourceIconShow()
    {
        ResourceIconShow();
    }

    public void HomingPlanetShow() //отображение иконки для родной планеты
    {
        if (isHomePlanet && homeIcon)
        {
            homeIcon.SetActive(true);
        }
    }
    
    [Command]
    public void CmdHomingPlanetShow() //отображение иконки для родной планеты
    {
        HomingPlanetShow();
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
