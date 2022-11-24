using System.Collections.Generic;
using System.Linq;
using Mirror;
using RandomNameGen;
using UnityEngine;
using Random = UnityEngine.Random;

public class MainPlanetController : NetworkBehaviour
{
    //Планеты
    public Transform parentTransform;
    public List<Sprite> listSpritePlanet;
    
    //public SyncList<PlanetController> syncListPlanet = new SyncList<PlanetController>();
    [SyncVar]
    [SerializeField]
    public List<PlanetController> listPlanet;
    
    //Границы видимости камеры
    public Vector2 xBounds;
    public Vector2 yBounds;

    public override void OnStartServer()
    {
        base.OnStartServer();
        Generation();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        Invoke(nameof(FillingList), 0.2f);
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        listPlanet.Clear();
    }

    [Server]
    private void Generation() //Генерация планет, если они не были сгенерированы
    {
        var countPlanet = Random.Range(50, 70);

        RandomName nameGen = new RandomName(); // create a new instance of the RandomName class
        List<string> allRandomNames = nameGen.RandomNames(countPlanet, 0); // generate 100 random names with up to two middle names

        while (listPlanet.Count < countPlanet)
        {
            //создание планеты
            var planet = Instantiate(AllSingleton.Instance.planetPrefab, parentTransform);
            NetworkServer.Spawn(planet);
            var planetController = planet.GetComponent<PlanetController>();

            //рандомные параметры для неё
            planetController.indSpritePlanet = Random.Range(0, listSpritePlanet.Count); //присвоение номера вида планеты
            planetController.namePlanet = allRandomNames[listPlanet.Count]; //имя
            planetController.AddResourceForPlanetGeneration(); //ресурсы
            var pos = GetRandomPosition();
            
            while (planet.transform.position == Vector3.zero) 
            {
                //есть ли планета, с дистанцией меньше 3 с предпологаемой новой позицией новой планеты
                var isNoDistance = listPlanet.Find(planController => Vector2.Distance(pos, planController.gameObject.transform.position) < 3 ? planController : false);
                
                if (isNoDistance) pos = GetRandomPosition(); //если есть такая, то тусуем новую позицию...
                else planet.transform.position = pos; //... иначе присваиваем хорошую позицию
            }
            
            var randomScale = Random.Range(0.13f, 0.17f); // случайный размер
            planet.transform.localScale = new Vector3(randomScale, randomScale, randomScale); //присвоение размера
            
            listPlanet.Add(planetController);
        }
    }

    [Server]
    public void RemovePlanetFromList(PlanetController planet)
    {
        listPlanet.Remove(planet);
    }
    [Command]
    public void CmdRemovePlanetFromList(PlanetController planet)
    {
        RemovePlanetFromList(planet);
    }
    
    [Client]
    public void FillingList()
    {
        var planets = FindObjectsOfType<PlanetController>();
        
        listPlanet = planets.Reverse().ToList();
    }

    [Server]
    private Vector2 GetRandomPosition()
    {
        //рандомные параметры
        float x = 0, y = 0;
        RandomXY(ref x, ref y);
        var position = new Vector2(x, y);

        return position;
    }

    private void RandomXY(ref float x, ref float y)
    {
        x = Random.Range(xBounds.x, xBounds.y);
        y = Random.Range(yBounds.x, yBounds.y);
    }
}
