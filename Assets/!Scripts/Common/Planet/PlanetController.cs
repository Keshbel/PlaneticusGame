using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class PlanetController : NetworkBehaviour
{
    [Header("Main")]
    public TMP_Text textName;
    [SyncVar] public string namePlanet;
    [SyncVar] public int indSpritePlanet;

    
    [Header("Resources")]
    [SyncVar] public int indexCurrentResource;
    public SyncList<ResourceForPlanet> planetResources = new SyncList<ResourceForPlanet>(); //ресурсы на планете
    public List<GameObject> resourcesIcon;


    [Header("LogisticRoutes")] 
    public SyncList<LogisticRoute> LogisticRoutes = new SyncList<LogisticRoute>();
    
    
    [Header("Super Planet")]
    [SyncVar] public bool isSuperPlanet = false; //является ли супер планетой? (все 5 ресурсов на ней)
       

    [Header("HomeColonized")]
    [SyncVar] public bool isHomePlanet = false; //является ли стартовой планетой?
    [SyncVar] public bool isColonized;
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
        if (isHomePlanet) //начальная задержка после спавна стартовых захватичков
            yield return new WaitForSeconds(26f);
        
        while (isSuperPlanet)
        {
            if (hasAuthority)
            {
                StartCoroutine(AllSingleton.instance.player.SpawnInvader(1, gameObject));
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
            }
            
            if (planetResources.Count == 5)
            {
                isSuperPlanet = true;
                StartCoroutine(StartSpawnInvadersRoutine());
            }
        }
        else // отнимаем
        {
            planetResources.Remove(resource);
            isSuperPlanet = false;
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
        var res1 = new ResourceForPlanet {resourcePlanet = (Enums.ResourcePlanet) Random.Range(0, 5), resourceMining = 1};
        print(res1.resourcePlanet);
        
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
            planetPanel.resToggles[index].onValueChanged.AddListener((b) => SelectResource(index1));
            planetPanel.resToggles[index].interactable = true;
        }
    }
    
    // выбор ресурса (прожатый тугл)
    public void SelectResource(int indexResource) 
    {
        indexCurrentResource = indexResource;
        UpdateInfo(indexResource);
    }
    
    
    //отображение иконок ресурсов на карте
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

    [Server]
    public IEnumerator LogisticResource(ResourceForPlanet resource, PlanetController toPlanet)
    {
        if (resource.resourceAll == 0) yield break;
        
        //инициализация
        var newRes = resource;
        var planetResource = toPlanet.planetResources.Find(p => p.resourcePlanet == resource.resourcePlanet);

        //transform/distance
        var fromTransform = transform;
        var toTransform = toPlanet.transform;
        var distance = Vector2.Distance(toTransform.position, fromTransform.position);
        
        var route = LogisticRoutes.Find(r => r.FromTransform == fromTransform && r.ToTransform == toTransform);
        
        newRes.isLogistic = true;
        
        if (route != null) //если визуальный маршрут уже есть
        {
            route.SaveResources?.Add(newRes);
        }
        else
        {
            //spawn
            var logisticRoute = Instantiate(ResourceSingleton.instance.logisticRoutePrefab, fromTransform).GetComponent<LogisticRoute>();
            NetworkServer.Spawn(logisticRoute.gameObject);
            
            //data
            logisticRoute.FromTransform = fromTransform;
            logisticRoute.ToTransform = toTransform;
            
            //start visual
            StartCoroutine(logisticRoute.StartRouteRoutine());
            
            //initialization
            LogisticRoutes?.Add(logisticRoute);
            route = logisticRoute;
            route.SaveResources?.Add(newRes);
            
        }

        //ожидание доставки ресурса (прибытие первой стрелочки)
        yield return new WaitForSeconds(distance*route.speed);

        
        //избавляемся от ресурса на планете-отдавателе
        ChangeResourceList(resource, false);
        
        //добавляем планете приемнику
        if (planetResource == null) //если на планете-получателе нет ресурса этого типа, то добавляем
        {
            toPlanet.ChangeResourceList(newRes, true);
        }
        else //если есть ресурс этого типа
        {
            planetResource.resourceDelivery++; //плюсуем ресурс в доставку у планеты-получателя
        }
    }
    [Command]
    public void CmdLogisticResource(ResourceForPlanet resource, PlanetController toPlanet)
    {
        StartCoroutine(LogisticResource(resource, toPlanet));
    }
    
    
    public void SubscribeLogisticButton()
    {
        var planetPanel = AllSingleton.instance.planetPanelUI;
        
        if (!planetResources[indexCurrentResource].isLogistic) //если ресурс ещё не учавствует в логистике
        {
            //очистка
            planetPanel.logisticButton.onClick.RemoveAllListeners();

            //заполнение
            planetPanel.logisticButton.onClick.AddListener(AllSingleton.instance.planetList.FillingList);

            //открытие панели
            planetPanel.logisticButton.onClick.AddListener(AllSingleton.instance.planetPanelUI.logisticListPanel
                .OpenPanel);
            
            //прожимаемая кнопка
            planetPanel.logisticButton.interactable = true;
        }
        else
        {
            planetPanel.logisticButton.interactable = false;
        }
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
        UpdateInfo(0);
    }


    public void UpdateInfo(int index)
    {
        SubscribeResourceToggle();
        SubscribeLogisticButton();

        var planetPanel = AllSingleton.instance.planetPanelUI;

        if (planetResources.Count-1 >= index)
        {
            var curRes = planetResources[index];

            planetPanel.planetNameText.text = namePlanet;

            planetPanel.resourceNameText.text = curRes.nameResource;
            planetPanel.resourceDeliveryText.text = curRes.resourceDelivery.ToString();
            planetPanel.resourceMiningText.text = curRes.resourceMining.ToString();
            planetPanel.resourceAllText.text = curRes.resourceAll.ToString();
            planetPanel.resourceIconImage.sprite = curRes.spriteIcon;

            planetResources[index].UpdateInfo();
        }
        else
        {
            AllSingleton.instance.planetPanelController.ClosePanel();
        }
    }
}
