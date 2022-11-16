using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    private CurrentPlayer Player => AllSingleton.Instance.player;
    private PlanetPanelUI PlanetPanel => AllSingleton.Instance.planetPanelUI;
    
    [Header("Main")]
    public TMP_Text textName;
    public TMP_Text textTimeDistance;
    [SyncVar(hook = nameof(UpdateTextName))] public string namePlanet;
    [SyncVar(hook = nameof(UpdateColor))] public Color colorPlanet;
    [SyncVar(hook = nameof(UpdateSpritePlanet))] public int indSpritePlanet;

    [Header("Resources")] 
    public int indexCurrentResource;
    public readonly SyncList<ResourceForPlanet> PlanetResources = new SyncList<ResourceForPlanet>(); //ресурсы на планете
    public List<GameObject> resourcesIcon;

    [Header("LogisticRoutes")] 
    public readonly SyncList<LogisticRoute> LogisticRoutes = new SyncList<LogisticRoute>();

    [Header("Home / Super Planet")] 
    [SyncVar(hook = nameof(HomingPlanetShow))] public bool isHomePlanet = false; //является ли стартовой планетой?
    public GameObject homeIcon;
    
    [SyncVar(hook = nameof(UpdateSuperEffect))] public bool isSuperPlanet; //является ли супер планетой? (все 5 ресурсов на ней)
    public GameObject effectSuperPlanet;
    public SpriteRenderer selectRenderer;
    public GameObject sliderCanvas;
    public Slider slider;
    public float counterToSpawn = 0;
    public float timeToSpawn = 20f;

    [Header("Colonized")]
    [SyncVar(hook = nameof(UpdateResourceIcon))] public bool isColonized;
    public readonly SyncList<SpaceInvaderController> SpaceOrbitInvader = new SyncList<SpaceInvaderController>(); //союзные захватчики на орбите

    [Client]
    private void Start()
    {
        sliderCanvas.GetComponent<Canvas>().worldCamera = Camera.main;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        PlanetResources.Callback += SyncResourceForPlanetVars;
    }

    [Client]
    public IEnumerator StartSpawnInvadersRoutine() //спавн захватчиков при статусе супер планеты
    {
        if (!isSuperPlanet) yield break;
        
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
            if (isOwned) AllSingleton.Instance.player.CmdSpawnInvader(1, gameObject);

            counterToSpawn = 0;
            slider.value = counterToSpawn;
            slider.DOValue(1, timeToSpawn).SetEase(Ease.Linear);

            yield return new WaitForSeconds(timeToSpawn);
        }
    }

    #region HomePlanetOptions

    [Server]
    public void SetHomePlanet() // установка домашней планеты
    {
        ClearPlanetResource();
        SetHomePlanetBoolTrue();

        for (int i = 0; i < 5; i++)
        {
            var res = new ResourceForPlanet {resourcePlanet = (Enums.ResourcePlanet) i, countResource = 1};
            ChangeResourceList(res, true);
        }
    }

    [Client]
    public void HomingPlanetShow(bool oldBool, bool newBool) //отображение иконки для родной планеты
    {
        if (homeIcon) homeIcon.SetActive(newBool);
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
    }
    [Command]
    public void CmdColonization()
    {
        Colonization();
    }

    [Server]
    public void ChangeOrbitInvaderList(SpaceInvaderController invader, bool isAdding)
    {
        if (!NetworkServer.active) return;
        
        if (isAdding) SpaceOrbitInvader.Add(invader);
        else SpaceOrbitInvader.Remove(invader);
    }

    [Command (requiresAuthority = false)]
    public void CmdChangeOrbitInvaderList(SpaceInvaderController invader, bool isAdding)
    {
        if (NetworkServer.active && NetworkClient.active) ChangeOrbitInvaderList(invader, isAdding);
    }

    #endregion

    #region PlanetResource

    //добавление ресурса для планеты, с ограничением добавления
    [Server]
    public void ChangeResourceList(ResourceForPlanet resource, bool isAdding)
    {
        if (isAdding) //добавляем ли планету?
        {
            if (PlanetResources.Count < 5) PlanetResources.Add(resource);
            if (PlanetResources.Count == 5) isSuperPlanet = true;
        }
        else // отнимаем
        {
            if (resource.countResource > 1) resource.countResource--;
            else
            {
                PlanetResources.Remove(resource);
                sliderCanvas.SetActive(false);
                isSuperPlanet = false;
            }
        }
    }

    [Command (requiresAuthority = false)]
    public void CmdChangeResourceList(ResourceForPlanet resource, bool isAdding)
    {
        ChangeResourceList(resource, isAdding);
    }
    
    void SyncResourceForPlanetVars(SyncList<ResourceForPlanet>.Operation op, int index, ResourceForPlanet oldItem, ResourceForPlanet newItem)
    {
        switch (op)
        {
            case SyncList<ResourceForPlanet>.Operation.OP_ADD:
            {
                break;
            }
            case SyncList<ResourceForPlanet>.Operation.OP_CLEAR:
            {
                break;
            }
            case SyncList<ResourceForPlanet>.Operation.OP_INSERT:
            {
                break;
            }
            case SyncList<ResourceForPlanet>.Operation.OP_REMOVEAT:
            {
                break;
            }
            case SyncList<ResourceForPlanet>.Operation.OP_SET:
            {
                break;
            }
        }
        ResourceIconShow();
    }
    
    [Server]
    public void AddResourceForPlanetGeneration() //добавление ресурсов при создании планет
    {
        var resource = new ResourceForPlanet {resourcePlanet = (Enums.ResourcePlanet) Random.Range(0, 5), countResource = 1};
        ChangeResourceList(resource, true);
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
        if (resource.countResource == 0) yield break;
        
        var planetResourceInit = toPlanet.PlanetResources.Find(resForPlanet => resForPlanet.resourcePlanet == resource.resourcePlanet); //если ресурс этого типа на старте на планете уже есть
        
        if (planetResourceInit != null) yield break;
        
        //инициализация
        var finalRes = new ResourceForPlanet{ resourcePlanet = resource.resourcePlanet, countResource = 1}; //финальный ресурс доставленный на планету

        //transform/distance
        var fromTransform = transform;
        var toTransform = toPlanet.transform;
        var distance = Vector2.Distance(toTransform.position, fromTransform.position);

        //запуск стрелочек
        StartCoroutine(StartArrowRoutine(fromTransform, toTransform, distance/AllSingleton.Instance.speed));

        //избавляемся от ресурса на планете-отдавателе
        var res = PlanetResources.Find(r => r.resourcePlanet == resource.resourcePlanet);
        ChangeResourceList(res, false);

        //ожидание доставки ресурса (прибытие первой стрелочки)
        yield return new WaitForSeconds(distance/AllSingleton.Instance.speed);
        
        var planetResource = toPlanet.PlanetResources.Find(resForPlanet => resForPlanet.resourcePlanet == resource.resourcePlanet); //если ресурс этого типа на планете уже есть
        //добавляем планете-приемнику
        if (planetResource == null) //если на планете-получателе нет ресурса этого типа, то добавляем
        {
            toPlanet.ChangeResourceList(finalRes, true);
        }
    }
    [Command]
    public void CmdLogisticResource(ResourceForPlanet resource, PlanetController toPlanet)
    {
        StartCoroutine(LogisticResource(resource, toPlanet));
    }

    [Server]
    public IEnumerator StartArrowRoutine(Transform fromTransform, Transform toTransform, float timeCount)
    {
        while (timeCount > 0)
        {
            //инициализация
            var logisticArrowGO = ResourceSingleton.Instance.arrowPoolManager.GetFromPool(fromTransform.position, Quaternion.identity); //достаём объект из пула
            NetworkServer.Spawn(logisticArrowGO, connectionToClient); //клиентский "спавн"
            
            var logisticArrow = logisticArrowGO.GetComponent<LogisticArrow>();
            logisticArrow.TargetStartMove(fromTransform, toTransform);

            //задержка
            yield return new WaitForSeconds(2f);
            timeCount -= 2;
        }
    }

    #endregion

    #region UI

    [Client]
    public void SubscribeLogisticButton()
    {
        if (PlanetResources[indexCurrentResource].countResource > 0) //если ресурс ещё не учавствует в логистике
        {
            PlanetPanel.logisticButton.onClick.RemoveAllListeners(); //очистка
            
            PlanetPanel.logisticButton.onClick.AddListener(() => Player.selectUnits.isLogisticMode = true);

            PlanetPanel.logisticButton.onClick.AddListener(() => //просчёт дистанции
            {
                foreach (var planet in AllSingleton.Instance.mainPlanetController.listPlanet.Where(planet => AllSingleton.Instance.player.playerPlanets.Contains(planet.gameObject) && planet.gameObject != gameObject))
                {
                    planet.CalculateDistance(transform.position, AllSingleton.Instance.speed);
                }
            });
            
            PlanetPanel.logisticButton.onClick.AddListener(AllSingleton.Instance.planetPanelController.ClosePanel); //закрытие панели
            
            PlanetPanel.logisticButton.interactable = true; //прожимаемая кнопка
        }
        else PlanetPanel.logisticButton.interactable = false;
    }
    
    [Client]
    public void SubscribeResourceToggle() //подписка туглов с ресурсами
    {
        for (var index = 0; index < 5; index++) //отписка
        {
            PlanetPanel.resToggles[index].onValueChanged.RemoveAllListeners();
            PlanetPanel.resToggles[index].interactable = false;
        }
        
        for (var index = 0; index < PlanetResources.Count; index++) //подписка
        {
            var index1 = index;

            switch (PlanetResources[index].resourcePlanet)
            {
                case Enums.ResourcePlanet.Water:
                    PlanetPanel.waterToggle.onValueChanged.AddListener(b => SelectResource(index1));
                    PlanetPanel.waterToggle.interactable = true;
                    break;
                case Enums.ResourcePlanet.Earth:
                    PlanetPanel.earthToggle.onValueChanged.AddListener((b) => SelectResource(index1));
                    PlanetPanel.earthToggle.interactable = true;
                    break;
                case Enums.ResourcePlanet.Fire:
                    PlanetPanel.fireToggle.onValueChanged.AddListener((b) => SelectResource(index1));
                    PlanetPanel.fireToggle.interactable = true;
                    break;
                case Enums.ResourcePlanet.Air:
                    PlanetPanel.airToggle.onValueChanged.AddListener((b) => SelectResource(index1));
                    PlanetPanel.airToggle.interactable = true;
                    break;
                case Enums.ResourcePlanet.Aether:
                    PlanetPanel.aetherToggle.onValueChanged.AddListener((b) => SelectResource(index1));
                    PlanetPanel.aetherToggle.interactable = true;
                    break;
            }
        }
    }
    
    [Client]
    public void SelectResource(int indexResource) // выбор ресурса (прожатый тугл)
    {
        indexCurrentResource = indexResource;
        UpdateInfo(indexResource);
    }

    [Client]
    public void ResourceIconShow() //отображение иконок ресурсов на карте
    {
        if (!isColonized) return;

        foreach (var icon in resourcesIcon) //вырубаем все иконки
        {
            icon.gameObject.SetActive(false);
        }

        foreach (var resource in PlanetResources) //включаем нужные
        {
            switch (resource.resourcePlanet)
            {
                case Enums.ResourcePlanet.Air:
                    resourcesIcon[0].gameObject.SetActive(true);
                    break;
                case Enums.ResourcePlanet.Water:
                    resourcesIcon[1].gameObject.SetActive(true);
                    break;
                case Enums.ResourcePlanet.Earth:
                    resourcesIcon[2].gameObject.SetActive(true);
                    break;
                case Enums.ResourcePlanet.Fire:
                    resourcesIcon[3].gameObject.SetActive(true);
                    break;
                case Enums.ResourcePlanet.Aether:
                    resourcesIcon[4].gameObject.SetActive(true);
                    break;
            }
        }
    }
    
    [Client]
    public void CalculateDistance(Vector2 fromPosition, float speed)
    {
        var secondsLanguage = "";
        if (LeanLocalization.GetFirstCurrentLanguage() == "Russian")
            secondsLanguage = " сек";
        if (LeanLocalization.GetFirstCurrentLanguage() == "English")
            secondsLanguage = " sec";

        textTimeDistance.text = (Vector2.Distance(fromPosition, transform.position)/speed).ToString("0.0") + secondsLanguage;
    }
    
    [Client]
    public void ClearDistanceText()
    {
        textTimeDistance.text = "";
    }

    [Client]
    public void OpenPlanet()
    {
        indexCurrentResource = 0;
        for (var index = 0; index < PlanetResources.Count; index++) //для нормального отображения картинок
        {
            SelectResource(index);
        }

        SubscribeResourceToggle();
        SubscribeLogisticButton();
        SelectResource(0);
    }

    [Client]
    public void UpdateInfo(int index)
    {
        SubscribeResourceToggle();
        SubscribeLogisticButton();

        if (PlanetResources.Count-1 >= index)
        {
            var curRes = PlanetResources[index];

            PlanetPanel.planetNameText.text = namePlanet;

            PlanetPanel.resourceNameText.text = curRes.nameResource;
            PlanetPanel.resourceCountText.text = curRes.countResource.ToString();
            PlanetPanel.resourceIconImage.sprite = curRes.SpriteIcon;

            PlanetResources[index].UpdateInfo();
        }
        else AllSingleton.Instance.planetPanelController.ClosePanel();
    }
    
    #endregion

    #region Hooks
    
    [Client]
    public void UpdateResourceIcon(bool oldBool, bool newBool)
    {
        ResourceIconShow();
    }

    [Client]
    public void UpdateSuperEffect(bool oldBool, bool newBool)
    {
        transform.GetChild(0).gameObject.SetActive(newBool);
        sliderCanvas.SetActive(newBool);
        if (newBool) StartCoroutine(StartSpawnInvadersRoutine());
    }
    
    [Client]
    public void UpdateSpritePlanet(int oldInt, int newInt) //назначить внешний вид планеты
    {
        var spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = AllSingleton.Instance.mainPlanetController.listSpritePlanet[newInt];
    }
    
    [Client]
    public void UpdateTextName(string oldS, string newS)
    {
        textName.text = newS;
    }
    
    [Client]
    public void UpdateColor(Color oldColor, Color newColor)
    {
        textName.color = newColor;
        textTimeDistance.color = newColor;
        selectRenderer.color = newColor;
    }
    
    #endregion
}
