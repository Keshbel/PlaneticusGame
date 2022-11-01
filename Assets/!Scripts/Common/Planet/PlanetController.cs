using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Lean.Localization;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

[Serializable]
public class PlanetController : NetworkBehaviour
{
    [Header("Main")]
    public TMP_Text textName;
    public TMP_Text textTimeDistance;
    [SyncVar(hook = nameof(UpdateTextName))] public string namePlanet;
    [SyncVar(hook = nameof(UpdateTextColor))] public Color colorPlanet;
    [SyncVar] public int indSpritePlanet;


    [Header("Resources")] 
    [SyncVar] public int indexCurrentResource;
    public SyncList<ResourceForPlanet> PlanetResources = new SyncList<ResourceForPlanet>(); //ресурсы на планете
    public List<GameObject> resourcesIcon;


    [Header("LogisticRoutes")] 
    public SyncList<LogisticRoute> LogisticRoutes = new SyncList<LogisticRoute>();


    [Header("Home / Super Planet")] 
    [SyncVar] public bool isHomePlanet = false; //является ли стартовой планетой?
    public GameObject homeIcon;
    
    [SyncVar/*(hook = nameof(EffectChangeActive))*/] public bool isSuperPlanet = false; //является ли супер планетой? (все 5 ресурсов на ней)
    [SyncVar] public GameObject effectSuperPlanet;
    public GameObject sliderCanvas;
    public Slider slider;
    public float counterToSpawn = 0;
    public float timeToSpawn = 20f;


    [Header("Colonized")]
    [SyncVar] public bool isColonized;
    public SyncList<SpaceInvaderController> SpaceOrbitInvader = new SyncList<SpaceInvaderController>(); //союзные захватчики на орбите
    

    private void Start()
    {
        SetSpritePlanet();
        Invoke(nameof(SetSpritePlanet), 0.4f);
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        if (isServer)
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
        sliderCanvas.SetActive(true);
        
        if (isHomePlanet) //начальная задержка после спавна стартовых захватичков
        {
            for (int i = 0; i < 2; i++)
            {
                slider.value = 0;
                slider.DOValue(1, 2f).SetEase(Ease.Linear);
                
                yield return new WaitForSeconds(2f);
            }
        }

        while (isSuperPlanet)
        {
            if (hasAuthority)
            {
                StartCoroutine(AllSingleton.instance.player.SpawnInvader(1, gameObject));
                
                counterToSpawn = 0;
                slider.value = counterToSpawn;
                slider.DOValue(1, timeToSpawn).SetEase(Ease.Linear);
                
                yield return new WaitForSeconds(timeToSpawn);
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

        var res1 = new ResourceForPlanet { resourcePlanet = Enums.ResourcePlanet.Water, resourceMining = 1 };
        var res2 = new ResourceForPlanet { resourcePlanet = Enums.ResourcePlanet.Earth, resourceMining = 1 };
        var res3 = new ResourceForPlanet { resourcePlanet = Enums.ResourcePlanet.Fire, resourceMining = 1 };
        var res4 = new ResourceForPlanet { resourcePlanet = Enums.ResourcePlanet.Air, resourceMining = 1 };
        var res5 = new ResourceForPlanet { resourcePlanet = Enums.ResourcePlanet.Aether, resourceMining = 1 };

        if (isServer) //добавление ресурсов
        {
            ChangeResourceList(res1, true);
            ChangeResourceList(res2, true);
            ChangeResourceList(res3, true);
            ChangeResourceList(res4, true);
            ChangeResourceList(res5, true);
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

    [Server]
    public void ChangeOrbitInvaderList(SpaceInvaderController invader, bool isAdding)
    {
        if (isAdding)
        {
            SpaceOrbitInvader.Add(invader);
        }
        else
        {
            SpaceOrbitInvader.Remove(invader);
        }
    }
    [Command]
    public void CmdChangeOrbitInvaderList(SpaceInvaderController invader, bool isAdding)
    {
        ChangeOrbitInvaderList(invader, isAdding);
    }
    #endregion

    #region PlanetResource

    //добавление ресурса для планеты, с ограничением добавления
    [Server]
    public void ChangeResourceList(ResourceForPlanet resource, bool isAdding)
    {
        if (isAdding) //добавляем ли планету?
        {
            if (PlanetResources.Count < 5)
            {
                PlanetResources.Add(resource);
            }

            if (PlanetResources.Count == 5)
            {
                isSuperPlanet = true;
                //StartCoroutine(StartSpawnInvadersRoutine());
            }
        }
        else // отнимаем
        {
            if (resource.ResourceAll > 1)
            {
                if (resource.resourceMining > 0)
                    resource.resourceMining--;
                else
                {
                    resource.resourceDelivery--;
                }
            }
            else
            {
                PlanetResources.Remove(resource);
                sliderCanvas.SetActive(false);
                isSuperPlanet = false;
            }
        }
        
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
            ChangeResourceList(res1, true);
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
        
        for (var index = 0; index < PlanetResources.Count; index++) //подписка
        {
            var index1 = index;

            switch (PlanetResources[index].resourcePlanet)
            {
                case Enums.ResourcePlanet.Water:
                    planetPanel.waterToggle.onValueChanged.AddListener(b => SelectResource(index1));
                    planetPanel.waterToggle.interactable = true;
                    break;
                case Enums.ResourcePlanet.Earth:
                    planetPanel.earthToggle.onValueChanged.AddListener((b) => SelectResource(index1));
                    planetPanel.earthToggle.interactable = true;
                    break;
                case Enums.ResourcePlanet.Fire:
                    planetPanel.fireToggle.onValueChanged.AddListener((b) => SelectResource(index1));
                    planetPanel.fireToggle.interactable = true;
                    break;
                case Enums.ResourcePlanet.Air:
                    planetPanel.airToggle.onValueChanged.AddListener((b) => SelectResource(index1));
                    planetPanel.airToggle.interactable = true;
                    break;
                case Enums.ResourcePlanet.Aether:
                    planetPanel.aetherToggle.onValueChanged.AddListener((b) => SelectResource(index1));
                    planetPanel.aetherToggle.interactable = true;
                    break;
            }
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

        foreach (var resource in PlanetResources) //включаем нужные
        {
            switch (resource.resourcePlanet)
            {
                case Enums.ResourcePlanet.Air:
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
    
    [SerializeField]
    public void EffectChangeActive(bool oldBool, bool newBool)
    {
        effectSuperPlanet.SetActive(newBool);
    }

    //обновление иконок на всех клиентах раз в пару секунд
    private IEnumerator ResourcesIconShowUpdated() 
    {
        while (NetworkServer.active)
        {
            RpcResourceIconShow();
            //RpcEffectChangeActive();
            yield return new WaitForSeconds(2f);
        }
    }
    
    
    //очистка всех ресурсов планеты
    [Server]
    public void ClearPlanetResource()
    {
        PlanetResources.Clear();
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
        if (resource.resourceMining == 0) yield break;
        
        //инициализация
        var routeResource = new ResourceForPlanet{ resourcePlanet = resource.resourcePlanet, resourceDelivery = 1, isLogistic = false}; //ресурс в маршруте
        var finalRes = new ResourceForPlanet{ resourcePlanet = resource.resourcePlanet, resourceDelivery = 1, isLogistic = false}; //финальный ресурс доставленный на планету
        var planetResource = toPlanet.PlanetResources.Find(p => p.resourcePlanet == resource.resourcePlanet); //если ресурс этого типа на планете уже есть

        //transform/distance
        var fromTransform = transform;
        var toTransform = toPlanet.transform;
        var distance = Vector2.Distance(toTransform.position, fromTransform.position);
        
        var route = LogisticRoutes.Find(r => r.fromTransform == fromTransform && r.toTransform == toTransform);
        
        //если на планете-получателе есть ресурс этого типа, то выключаем логистику у него (для узнавания, долетел или нет)
        if (planetResource != null) 
        {
            planetResource.isLogistic = false;
        }

        if (route != null) //если визуальный маршрут уже есть
        {
            var routeRes = route.SaveResources?.Find(res => res.resourcePlanet == routeResource.resourcePlanet);
            if (routeRes != null)
            {
                routeRes.resourceDelivery++;
                routeRes.isLogistic = false;
            }
            else
                route.SaveResources?.Add(routeResource);
        }
        else // если нет, то создаём
        {
            //spawn
            var logisticRoute = Instantiate(ResourceSingleton.instance.logisticRoutePrefab, fromTransform).GetComponent<LogisticRoute>();
            NetworkServer.Spawn(logisticRoute.gameObject);
            
            //data
            logisticRoute.fromTransform = fromTransform;
            logisticRoute.toTransform = toTransform;
            
            //start visual
            StartCoroutine(logisticRoute.StartRouteRoutine());
            
            //initialization
            LogisticRoutes?.Add(logisticRoute);
            route = logisticRoute;
            route.SaveResources?.Add(routeResource);
        }

        var res = PlanetResources.Find(r => r.resourcePlanet == resource.resourcePlanet);
        //избавляемся от ресурса на планете-отдавателе
        ChangeResourceList(res, false);

        
        //ожидание доставки ресурса (прибытие первой стрелочки)
        yield return new WaitForSeconds(distance/route.speed);

        planetResource = toPlanet.PlanetResources.Find(p => p.resourcePlanet == routeResource.resourcePlanet); //если ресурс этого типа на планете уже есть

        //добавляем планете приемнику
        if (planetResource == null) //если на планете-получателе нет ресурса этого типа, то добавляем
        {
            finalRes.isLogistic = true;
            toPlanet.ChangeResourceList(finalRes, true);
        }
        else //если есть ресурс этого типа
        {
            planetResource.resourceDelivery++; //плюсуем ресурс в доставку у планеты-получателя
            planetResource.isLogistic = true;
        }
        routeResource.isLogistic = true;
    }
    [Command]
    public void CmdLogisticResource(ResourceForPlanet resource, PlanetController toPlanet)
    {
        StartCoroutine(LogisticResource(resource, toPlanet));
    }
    
    
    public void SubscribeLogisticButton()
    {
        var planetPanel = AllSingleton.instance.planetPanelUI;
        
        if (PlanetResources[indexCurrentResource].resourceMining > 0) //если ресурс ещё не учавствует в логистике
        {
            //очистка
            planetPanel.logisticButton.onClick.RemoveAllListeners();

            //включение логистического режима
            planetPanel.logisticButton.onClick.AddListener(() => AllSingleton.instance.player.isLogisticMode = true);
            
            //просчёт дистанции
            planetPanel.logisticButton.onClick.AddListener(() =>
            {
                foreach (var planet in AllSingleton.instance.mainPlanetController.listPlanet)
                {
                    if (AllSingleton.instance.player.playerPlanets.Contains(planet.gameObject) && planet.gameObject != gameObject)
                    {
                        float speed = AllSingleton.instance.speed;;
                        if (planet.LogisticRoutes.Count != 0)
                        {
                            speed = planet.LogisticRoutes[0].speed;
                        }

                        planet.CalculateDistance(transform.position, speed);
                    }
                }
            });
            
            //закрытие панели
            planetPanel.logisticButton.onClick.AddListener(AllSingleton.instance.planetPanelController.ClosePanel);

            //прожимаемая кнопка
            planetPanel.logisticButton.interactable = true;
        }
        else
        {
            planetPanel.logisticButton.interactable = false;
        }
    }

    #endregion


    public void CalculateDistance(Vector2 fromPosition, float speed)
    {
        var secondsLanguage = "";
        if (LeanLocalization.GetFirstCurrentLanguage() == "Russian")
            secondsLanguage = " с";
        if (LeanLocalization.GetFirstCurrentLanguage() == "English")
            secondsLanguage = " s";

        textTimeDistance.text = (Vector2.Distance(fromPosition, transform.position)/speed).ToString("0.0") + secondsLanguage;
    }
    
    public void ClearDistanceText()
    {
        textTimeDistance.text = "";
    }
    
    public void SetSpritePlanet() //назначить внешний вид планеты
    {
        var spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = AllSingleton.instance.mainPlanetController.listSpritePlanet[indSpritePlanet];
    }

    public void OpenPlanet()
    {
        if (PlanetResources.Count > 0)
            SelectResource(0);
        if (PlanetResources.Count > 1)
            SelectResource(1);
        if (PlanetResources.Count > 2)
            SelectResource(2);
        if (PlanetResources.Count > 3)
            SelectResource(3);
        if (PlanetResources.Count > 4)
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

        if (PlanetResources.Count-1 >= index)
        {
            var curRes = PlanetResources[index];

            planetPanel.planetNameText.text = namePlanet;

            planetPanel.resourceNameText.text = curRes.nameResource;
            planetPanel.resourceDeliveryText.text = curRes.resourceDelivery.ToString();
            planetPanel.resourceMiningText.text = curRes.resourceMining.ToString();
            planetPanel.resourceAllText.text = curRes.ResourceAll.ToString();
            planetPanel.resourceIconImage.sprite = curRes.spriteIcon;

            PlanetResources[index].UpdateInfo();
        }
        else
        {
            AllSingleton.instance.planetPanelController.ClosePanel();
        }
    }

    public void UpdateTextName(string oldS, string newS)
    {
        textName.text = newS;
    }
    
    public void UpdateTextColor(Color oldS, Color newS)
    {
        textName.color = newS;
        textTimeDistance.color = newS;
    }
}
