using Mirror;
using UnityEngine;
using UnityEngine.EventSystems;

public class SelectablePlanet : NetworkBehaviour, IPointerDownHandler
{
    public PlanetController planetController;
    
    public GameObject selectingObject;
    public bool isSelecting;

    private void OnEnable()
    {
        if (!selectingObject)
            selectingObject = gameObject.transform.GetChild(0).gameObject;

        if (!planetController)
            planetController = GetComponent<PlanetController>();
    }

    private void SelectingProcess() //процесс выделяемости/развыделяемости для других планет
    {
        if (AllSingleton.instance.selectablePlanets.Count != 0)
        {
            AllSingleton.instance.selectablePlanets[0].SelectingChange();
            AllSingleton.instance.selectablePlanets.Clear();
        }

        SelectingChange();
        
        AllSingleton.instance.selectablePlanets.Add(this);
    }
    
    private void SelectingChange() //изменение выделяемости планеты
    {
        selectingObject.SetActive(!selectingObject.activeSelf);
        isSelecting = !isSelecting;
    }

    public void OnPointerDown(PointerEventData eventData) //нажатие на планету
    {
        if (!AllSingleton.instance.player.playerPlanets.Contains(planetController.gameObject) 
            || AllSingleton.instance.player.invaderController != null 
            || planetController.PlanetResources.Count == 0
            || planetController.PlanetResources.FindAll(p => p.resourceMining > 0).Count == 0) return;

        SelectingProcess();
        planetController.OpenPlanet();
        AllSingleton.instance.planetPanelController.OpenPanel();
    }

    private void OnTriggerEnter2D(Collider2D other) 
    {
        if (!other.CompareTag("Planet")) return;
        
        if (isServer) //если планеты в зоне коллайдера друг друга, то они рандомно перемещаются
            AllSingleton.instance.mainPlanetController.SetRandomPosition(other.gameObject);
        else
        {
            AllSingleton.instance.mainPlanetController.CmdSetRandomPosition(other.gameObject);
        }
    }
}
