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
        
        Invoke(nameof(FillingListPlanet), 0.2f);
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
                var isNoDistance = listPlanet.Find(planController =>
                {
                    return Vector2.Distance(pos, planController.gameObject.transform.position) < 3 ? planController : false;
                });

                //если есть такая, то тусуем новую позицию, иначе присваиваем хорошую позицию
                if (isNoDistance) 
                    pos = GetRandomPosition();
                else
                {
                    planet.transform.position = pos;
                }
            }


            var randomScale = Random.Range(0.13f, 0.17f); // случайный размер
            
            planet.transform.localScale = new Vector3(randomScale, randomScale, randomScale); //присвоение размера
            
            listPlanet.Add(planetController);
        }
    }

    [Server]
    public void RemovePlanetFromListPlanet(PlanetController planet)
    {
        listPlanet.Remove(planet);
    }
    [Command]
    public void CmdRemovePlanetFromListPlanet(PlanetController planet)
    {
        RemovePlanetFromListPlanet(planet);
    }
    
    public void FillingListPlanet()
    {
        var planets = FindObjectsOfType<PlanetController>();
        
        listPlanet = planets.Reverse().ToList();
    }

    [Server]
    public void SetRandomPosition(GameObject planet)
    {
        //рандомные параметры
        float x = 0, y = 0;
        RandomXY(ref x, ref y);
        var position = new Vector2(x, y);
        
        //присвоение рандомной позиции
        planet.transform.position = position; //позиция
    }
    [Command]
    public void CmdSetRandomPosition(GameObject planet)
    {
        SetRandomPosition(planet);
    }
    
    [Server]
    public Vector2 GetRandomPosition()
    {
        //рандомные параметры
        float x = 0, y = 0;
        RandomXY(ref x, ref y);
        var position = new Vector2(x, y);

        return position;
    }
    
    public void RandomXY(ref float x, ref float y)
    {
        x = Random.Range(xBounds.x, xBounds.y);
        y = Random.Range(yBounds.x, yBounds.y);
    }
}
