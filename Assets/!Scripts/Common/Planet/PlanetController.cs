using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class PlanetController : NetworkBehaviour
{
    [Header("Main")]
    [SyncVar]
    public string namePlanet;
    [SyncVar]
    public int indSpritePlanet;

    [Header("Resources")]
    [SyncVar]
    public int indexCurrentResource;
    public SyncList<ResourceForPlanet> planetResources = new SyncList<ResourceForPlanet>(); //ресурсы на планете
    public List<GameObject> resourcesIcon;
    
    [Header("Super Planet")]
    [SyncVar]
    public bool isSuperPlanet = false; //является ли супер планетой? (все 5 ресурсов на ней)
    public Coroutine SpawnInvaderCoroutine;
    
    [Header("HomeColonized")]
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

    public override void OnStartServer()
    {
        base.OnStartServer();
        
        StartCoroutine(ResourcesIconShowUpdated());
    }
    
    public override void OnStartClient()
    {
        base.OnStartClient();
        
        HomingPlanetShow();
        Invoke(nameof(HomingPlanetShow), 1f);
        ResourceIconShow();
        Invoke(nameof(ResourceIconShow), 1f);
    }

    public IEnumerator StartSpawnInvadersRoutine() //спавн захватчиков при статусе супер планеты
    {
        while (NetworkServer.active)
        {
            if (isSuperPlanet && hasAuthority)
            {
                //AllSingleton.instance.player.SpawnInvader(1, gameObject);
                yield return new WaitForSeconds(20f);
            }
        }
    }
    

    #region HomePlanetOptions
    
    public void SetHomePlanet() // установка домашней планеты
    {
        if (isServer) //очистка и простановка статуса
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
            ChangeResourceList(res1);
            ChangeResourceList(res2);
            ChangeResourceList(res3);
            ChangeResourceList(res4);
            ChangeResourceList(res5);
            RpcResourceIconShow();
        }
        else
        {
            CmdChangeResourceList(res1, true);
            CmdChangeResourceList(res2, true);
            CmdChangeResourceList(res3, true);
            CmdChangeResourceList(res4, true);
            CmdChangeResourceList(res5, true);
            RpcResourceIconShow();
        }
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

    #region Colonization
    
    [Server]
    public void Colonization()
    {
        isColonized = true;
        if (isServer)
        {
            RpcResourceIconShow();
        }
    }
    [Command]
    public void CmdColonization()
    {
        Colonization();
    }
    
    #endregion

    #region PlanetResource
    
    //добавление ресурса для планеты, с ограничением добавления
    [Server]
    public void ChangeResourceList(ResourceForPlanet resource, bool isAdding = true) 
    {
        if (isAdding) //добавляем ли планету?
        {
            if (planetResources.Count < 5)
            {
                planetResources.Add(resource);

                if (planetResources.Count == 5)
                {
                    isSuperPlanet = true;
                    //SpawnInvaderCoroutine = StartCoroutine(StartSpawnInvadersRoutine());
                }
                else
                {
                    isSuperPlanet = false;
                    /*if (SpawnInvaderCoroutine != null)
                        StopCoroutine(SpawnInvaderCoroutine);*/
                }
            }
            else
            {
                print("Вы пытаетесь добавить шестой ресурс...");
            }
        }
        else // отнимаем
        {
            if (planetResources.Count > 1)
                planetResources.Remove(resource);
        }
        
        resource.UpdateInfo();
        RpcResourceIconShow();
    }
    [Command]
    public void CmdChangeResourceList(ResourceForPlanet resource, bool isAdding)
    {
        ChangeResourceList(resource, isAdding);
    }
    
    
    //добавление ресурсов при создании планет
    public void AddResourceForPlanetGeneration() 
    {
        var res1 = new ResourceForPlanet {resourcePlanet = (Enums.ResourcePlanet) Random.Range(0, 4), resourceMining = 1};
        if (isServer)
        {
            ChangeResourceList(res1);
            RpcResourceIconShow();
        }
        else
        {
            CmdChangeResourceList(res1, true);
            ResourceIconShow();
        }
    }

    //подписка туглов с ресурсами
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
    
    // выбор ресурса (прожатый тугл)
    public void SelectResource(int indexResource) 
    {
        indexCurrentResource = indexResource;
        UpdateInfo();
    }
    
    
    //отображение иконок ресурсов на карте
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
    public void RpcResourceIconShow()
    {
        ResourceIconShow();
    }
    
    
    //обновление иконок на всех клиентах раз в пару секунд
    private IEnumerator ResourcesIconShowUpdated() 
    {
        while (NetworkServer.active)
        {
            RpcResourceIconShow();
            yield return new WaitForSeconds(2f);
        }
    }
    
    
    //очистка всех ресурсов планеты
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

    #region Logistic (delivery resource)

    public void LogisticResource()
    {
        
    }
    
    public void SubscribeLogisticButton()
    {
        var planetPanel = AllSingleton.instance.planetPanelUI;
        
        //очистка
        planetPanel.logisticButton.onClick.RemoveAllListeners();
        //заполнение
        planetPanel.logisticButton.onClick.AddListener(AllSingleton.instance.planetList.FillingList);
        //открытие панели
        planetPanel.logisticButton.onClick.AddListener(AllSingleton.instance.planetPanelUI.logisticListPanel.OpenPanel);
    }

    #endregion
    
    
    public void SetSpritePlanet() //назначить внешний вид планеты
    {
        var spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = AllSingleton.instance.mainPlanetController.listSpritePlanet[indSpritePlanet];
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
        SubscribeLogisticButton();
        UpdateInfo();
    }
    

    public void UpdateInfo()
    {
        SubscribeResourceToggle();
        SubscribeLogisticButton();
        
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
