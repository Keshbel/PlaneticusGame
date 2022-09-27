using UnityEngine;
using UnityEngine.EventSystems;

public class SelectablePlanet : MonoBehaviour, IPointerDownHandler
{
    public PlanetController planetController;
    
    public GameObject selectingObject;
    public bool isSelecting;

    public Vector3 defaultScale;

    private void OnEnable()
    {
        if (!selectingObject)
            selectingObject = gameObject.transform.GetChild(0).gameObject;

        if (!planetController)
            planetController = GetComponent<PlanetController>();
        
        Invoke(nameof(SetDefaultScale),0.5f);
    }

    private void SelectingProcess() //процесс выделяемости/развыделяемости для других планет
    {
        if (SelectablePlanetManager.SelectablePlanets.Count != 0)
        {
            SelectablePlanetManager.SelectablePlanets[0].SelectingChange();
            SelectablePlanetManager.SelectablePlanets.Clear();
        }

        SelectingChange();
        
        SelectablePlanetManager.SelectablePlanets.Add(this);
    }
    
    private void SelectingChange() //изменение выделяемости планеты
    {
        selectingObject.SetActive(!selectingObject.activeSelf);
        isSelecting = !isSelecting;
        
        gameObject.transform.localScale = isSelecting ? defaultScale * 1.1f : defaultScale;
    }

    public void SetDefaultScale()
    {
        defaultScale = gameObject.transform.localScale;
    }
    
    public void OnPointerDown(PointerEventData eventData) //нажатие на планету
    {
        if (!FindObjectOfType<CurrentPlayer>().playerPlanets.Contains(planetController)) return;
        
        SelectingProcess();
        planetController.OpenPlanet();
        AllSingleton.instance.planetPanelController.OpenPanel();
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (!other.collider.CompareTag("Planet") || defaultScale.x > 0) return;
        
        FindObjectOfType<PlanetGeneration>().syncListPlanet.Remove(other.gameObject);
        Destroy(other.gameObject);
        print("Sorting Planet");
    }
}
