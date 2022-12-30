using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    private PlanetPanelUI PlanetPanel => AllSingleton.Instance.planetPanelUI;

    [Header("Owner")] 
    [SyncVar(hook = nameof(UpdateOwner))] public CurrentPlayer playerOwner;

    [Header("TMP_Text")] 
    [SerializeField] private TMP_Text textName;
    [SerializeField] private TMP_Text textTimeDistance;

    [Header("Parameters")] 
    [SyncVar(hook = nameof(UpdateTextName))] public string namePlanet;
    [SyncVar(hook = nameof(UpdateColor))] public Color colorPlanet;
    [SyncVar(hook = nameof(UpdateSpritePlanet))] public int indSpritePlanet;

    [Header("Resources")] 
    public int indexCurrentResource;
    public readonly SyncList<ResourceForPlanet> PlanetResources = new SyncList<ResourceForPlanet>(); //ресурсы планеты
    public readonly SyncList<ResourceForPlanet> PlanetSaveResources = new SyncList<ResourceForPlanet>(); //родные ресы
    [SerializeField] private List<GameObject> resourcesIcon;

    [Header("Home planet")] 
    [SyncVar(hook = nameof(HomingPlanetShow))] public bool isHomePlanet; //является ли стартовой планетой?
    [SyncVar] public int countToDestroy = 5;
    [SerializeField] private GameObject homeIcon;
    public HomePlanetPointer homePlanetPointer;
    
    [Header("Super planet")] 
    [SyncVar(hook = nameof(UpdateSuperEffect))] public bool isSuperPlanet; //все ресурсы на ней
    [SerializeField] private GameObject effectSuperPlanet;
    [SerializeField] private GameObject sliderCanvas;
    [SerializeField] private Slider slider;
    [SerializeField] private float counterToSpawn;
    [SerializeField] private float timeToSpawn = 20f;
    private Coroutine _spawnInvaderCoroutine;

    [Header("Colonized")] 
    [SyncVar(hook = nameof(UpdateResourceIcon))] public bool isColonized;
    [SerializeField] private SpriteRenderer selectRenderer;

    //союзные захватчики на орбите
    public readonly SyncList<SpaceInvaderController> SpaceOrbitInvader = new SyncList<SpaceInvaderController>();

    public override void OnStartClient()
    {
        base.OnStartClient();

        PlanetResources.Callback += SyncResourceForPlanetVars;
        sliderCanvas.GetComponent<Canvas>().worldCamera = Camera.main;
    }

    [Client]
    public IEnumerator StartSpawnInvadersRoutine() //спавн захватчиков при статусе супер планеты
    {
        if (!isSuperPlanet) yield break;

        if (isHomePlanet) //начальная задержка после спавна стартовых захватичков
        {
            for (int i = 0; i < 2; i++)
            {
                if (isServer && playerOwner.isBot || isOwned) playerOwner.CmdSpawnInvader(1, gameObject);
                //else if (isOwned) playerOwner.CmdSpawnInvader(1, gameObject);
                slider.value = 0;
                slider.DOValue(1, 2f).SetEase(Ease.Linear);

                yield return new WaitForSeconds(2f);
            }
        }

        while (isSuperPlanet)
        {
            if (isServer && playerOwner.isBot || isOwned) playerOwner.CmdSpawnInvader(1, gameObject);
            //else if (isOwned) playerOwner.CmdSpawnInvader(1, gameObject);

            counterToSpawn = 0;
            slider.value = counterToSpawn;
            slider.DOValue(1, timeToSpawn).SetEase(Ease.Linear);

            yield return new WaitForSeconds(timeToSpawn);
        }

        print("Спавн захватчиков прекратился!");
    }

    #region HomePlanetOptions

    [Command(requiresAuthority = false)]
    public void CmdChangeCountToDestroy(int value, bool isAdding)
    {
        if (isAdding) countToDestroy += value;
        else countToDestroy -= value;
    }

    [Server]
    public void SetHomePlanet() // установка домашней планеты
    {
        ClearPlanetResource();
        ChangeIsHomePlanet(true);

        for (int i = 0; i < 5; i++)
        {
            var idRes = MainPlanetController.Instance.idResource;
            var res = new ResourceForPlanet {resourcePlanet = (Enums.ResourcePlanet) i, countResource = 1, id = idRes};
            ChangeResourceList(res, true);
            MainPlanetController.Instance.idResource++;
        }
    }

    [Client]
    public async void HomingPlanetShow(bool oldBool, bool newBool) //отображение иконки для родной планеты
    {
        if (homeIcon) homeIcon.SetActive(newBool);

        if (newBool)
        {
            homePlanetPointer = gameObject.AddComponent<HomePlanetPointer>();
            await Task.Delay(100);
            PointerManager.Instance.Dictionary[homePlanetPointer].arrowImage.color = colorPlanet;
            PointerManager.Instance.Dictionary[homePlanetPointer].isHomeIcon = isOwned;
        }
        else if (homePlanetPointer) homePlanetPointer.Destroy();
    }

    #endregion

    #region Colonization

    [Server]
    private void ChangeIsHomePlanet(bool isTrue)
    {
        isHomePlanet = isTrue;
    }

    [Command(requiresAuthority = false)]
    private void CmdChangeIsHomePlanet(bool isTrue)
    {
        ChangeIsHomePlanet(isTrue);
    }

    [Command(requiresAuthority = false)]
    public void CmdRestartSpawnInvader()
    {
        if (_spawnInvaderCoroutine == null) return;

        StopCoroutine(_spawnInvaderCoroutine);
        _spawnInvaderCoroutine = StartCoroutine(StartSpawnInvadersRoutine());
    }

    [Server]
    public void Colonization()
    {
        isColonized = true;
    }

    [Command(requiresAuthority = false)]
    public void CmdColonization()
    {
        Colonization();
    }

    [Server]
    public void ChangeOrbitInvaderList(SpaceInvaderController invader, bool isAdding)
    {
        if (!NetworkServer.active && !playerOwner.isBot) return;

        if (isAdding) SpaceOrbitInvader.Add(invader);
        else SpaceOrbitInvader.Remove(invader);
    }

    [Command(requiresAuthority = false)]
    public void CmdChangeOrbitInvaderList(SpaceInvaderController invader, bool isAdding)
    {
        ChangeOrbitInvaderList(invader, isAdding);
    }

    #endregion

    #region PlanetResource

    [Server] //добавление ресурса для планеты, с ограничением добавления
    public void ChangeResourceList(ResourceForPlanet resource, bool isAdding)
    {
        if (isAdding) //добавляем ли планету?
        {
            if (PlanetResources.Count < 5) PlanetResources.Add(resource);
            if (PlanetResources.Count == 5) isSuperPlanet = true;
        }
        else // отнимаем
        {
            var res = PlanetResources.Find(r => r.resourcePlanet == resource.resourcePlanet);
            PlanetResources.Remove(res ?? resource);
            isSuperPlanet = false;
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdChangeResourceList(ResourceForPlanet resource, bool isAdding)
    {
        ChangeResourceList(resource, isAdding);
    }

    void SyncResourceForPlanetVars(SyncList<ResourceForPlanet>.Operation op, int index, ResourceForPlanet oldItem,
        ResourceForPlanet newItem)
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
    public void AddResourceForPlanetGeneration(int idIndex) //добавление ресурсов при создании планет
    {
        var resource = new ResourceForPlanet
            {resourcePlanet = (Enums.ResourcePlanet) Random.Range(0, 5), countResource = 1, id = idIndex};
        PlanetSaveResources.Add(resource);
        ChangeResourceList(resource, true);
    }


    //очистка всех ресурсов планеты
    [Server]
    public void ClearPlanetResource()
    {
        PlanetResources.Clear();
    }

    [Command(requiresAuthority = false)]
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

        var planetResourceInit =
            toPlanet.PlanetResources.Find(resForPlanet =>
                resForPlanet.resourcePlanet ==
                resource.resourcePlanet); //если ресурс этого типа на старте на планете уже есть

        if (planetResourceInit != null) yield break;

        //инициализация
        //var finalRes = new ResourceForPlanet{ resourcePlanet = resource.resourcePlanet, countResource = 1}; //финальный ресурс доставленный на планету
        var networkOwner = netIdentity.connectionToClient;

        //transform/distance
        var fromTransform = transform;
        var toTransform = toPlanet.transform;
        var distance = Vector2.Distance(toTransform.position, fromTransform.position);

        //запуск стрелочек
        StartCoroutine(StartArrowRoutine(fromTransform, toTransform, distance / AllSingleton.Instance.speed));

        //избавляемся от ресурса на планете-отдавателе
        var res = PlanetResources.Find(r => r.resourcePlanet == resource.resourcePlanet);
        ChangeResourceList(res, false);

        //ожидание доставки ресурса (прибытие первой стрелочки)
        yield return new WaitForSeconds(distance / AllSingleton.Instance.speed);

        //проверка перезахвата другим игроком
        if (networkOwner != netIdentity.connectionToClient) yield break;

        var planetResource =
            toPlanet.PlanetResources.Find(resForPlanet =>
                resForPlanet.resourcePlanet == resource.resourcePlanet); //если ресурс этого типа на планете уже есть
        //добавляем планете-приемнику
        if (planetResource == null) //если на планете-получателе нет ресурса этого типа, то добавляем
        {
            toPlanet.ChangeResourceList(res, true);
        }
        else ChangeResourceList(resource, true); //иначе возвращаем на родную планету
    }

    [Command(requiresAuthority = false)]
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
            var logisticArrowGO =
                ResourceSingleton.Instance.arrowPoolManager.GetFromPool(fromTransform.position,
                    Quaternion.identity); //достаём объект из пула
            if (!playerOwner.isBot) NetworkServer.Spawn(logisticArrowGO, connectionToClient); //клиентский "спавн"
            else NetworkServer.Spawn(logisticArrowGO);

            var logisticArrow = logisticArrowGO.GetComponent<LogisticArrow>();
            logisticArrow.playerOwner = playerOwner;
            /*logisticArrow.fromTransform = fromTransform;
            logisticArrow.toTransform = toTransform;*/
            if (!playerOwner.isBot) logisticArrow.RpcTargetStartMove(fromTransform, toTransform);
            else logisticArrow.TargetStartMove(fromTransform, toTransform);

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

            PlanetPanel.logisticButton.onClick.AddListener(() => playerOwner.selectUnits.isLogisticMode = true);

            PlanetPanel.logisticButton.onClick.AddListener(() => //просчёт дистанции
            {
                foreach (var planet in MainPlanetController.Instance.listPlanet.Where(planet =>
                    playerOwner.PlayerPlanets.Contains(planet) && planet != this))
                {
                    planet.CalculateDistance(transform.position, AllSingleton.Instance.speed);
                }
            });

            PlanetPanel.logisticButton.onClick.AddListener(AllSingleton.Instance.planetPanelController
                .ClosePanel); //закрытие панели

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

        textTimeDistance.text = (Vector2.Distance(fromPosition, transform.position) / speed).ToString("0.0") +
                                secondsLanguage;
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


        SelectResource(IntResourceSorting());
    }

    public int IntResourceSorting() //ищет ресурс по распорядку в enum
    {
        for (int i = 0; i < 5; i++)
        {
            var res = PlanetResources.Find(resource => resource.resourcePlanet == (Enums.ResourcePlanet) i);
            if (res != null) return PlanetResources.IndexOf(res);
        }

        return 0;
    }

    [Client]
    public void UpdateInfo(int index)
    {
        SubscribeResourceToggle();
        SubscribeLogisticButton();

        if (PlanetResources.Count - 1 >= index)
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
        if (newBool) _spawnInvaderCoroutine = StartCoroutine(StartSpawnInvadersRoutine());
    }

    [Client]
    public void UpdateSpritePlanet(int oldInt, int newInt) //назначить внешний вид планеты
    {
        var spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = MainPlanetController.Instance.listSpritePlanet[newInt];
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

    [Client]
    public void UpdateOwner(CurrentPlayer oldPlayer, CurrentPlayer newPlayer)
    {
        var saveRes = PlanetSaveResources[0];
        PlanetController planetController = null;

        if (newPlayer != null)
        {
            //восстанавливаем изначальный ресурс и убираем статус домашней планеты
            if (!PlanetResources.Contains(saveRes)) CmdChangeResourceList(saveRes, true);
            if (isHomePlanet) CmdChangeIsHomePlanet(false);

            /*Debug.Log("Id сохраненного ресурса " + PlanetSaveResources[0].id);
            oldPlayer.PlayerPlanets.ToList().ForEach(planet => planet.PlanetResources.ToList().ForEach(res => Debug.Log("Id = " + res.id)));*/
        }

        if (oldPlayer != null)
        {
            //ищем и убираем "гуляющий" нужный нам ресурс у старого игрока
            planetController = oldPlayer.PlayerPlanets.Find(planet =>
                planet.PlanetResources.Any(res => res == saveRes));

            if (planetController != null)
            {
                var resource = planetController.PlanetResources.Find(res => res.id == saveRes.id);
                if (resource != null) planetController.CmdChangeResourceList(resource, false);
            }
        }
    }

    #endregion
}
