using System.Collections;
using System.Collections.Generic;
using Mirror;
using RandomNameGen;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlanetGeneration : NetworkBehaviour
{
    //Планеты
    public Transform parentTransform;
    public GameObject planetPrefab;
    public List<Sprite> listSpritePlanet;
    public SyncList<GameObject> syncListPlanet = new SyncList<GameObject>();
    public List<GameObject> listPlanet;
    
    //Границы видимости камеры
    public Vector2 xBounds;
    public Vector2 yBounds;

    public TMP_Text tmpDebug;

    private void Start()
    {
        if (syncListPlanet.Count==0)
            StartCoroutine(Generation());
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        syncListPlanet.Callback += SyncListPlanet; //вместо hook, для SyncList используем подписку на Callback

        listPlanet = new List<GameObject>(syncListPlanet.Count); //так как Callback действует только на изменение массива,  
        
        for (int i = 0; i < syncListPlanet.Count; i++) //а у нас на момент подключения уже могут быть какие-то данные в массиве, нам нужно эти данные внести в локальный массив
        {
            //SyncListPlanet(SyncList<GameObject>.Operation.OP_ADD, i, new GameObject(), syncListPlanet[i]);
            listPlanet.Add(syncListPlanet[i]);
            tmpDebug.text = listPlanet.Count + " планета в локальном списке";
        }

        foreach (var planet in listPlanet)
        {
            Instantiate(planet);
        }
        //Debug.Log(AllSingleton.instance.currentPlayer.playerName + "добавление планет");
    }

    [Server]
    private void ChangeListPlanet(GameObject newGO) //метод добавление в лист
    {
        syncListPlanet.Add(newGO);
    }
    
    [Command]
    private void CmdChangeListPlanet(GameObject newGO) //метод добавление в лист
    {
        ChangeListPlanet(newGO);
    }
    
    private void SyncListPlanet(SyncList<GameObject>.Operation op, int index, GameObject oldItem, GameObject newItem) //обработчик ля синхронизации планет
    {
        switch (op)
        {
            case SyncList<GameObject>.Operation.OP_ADD:
            {
                listPlanet.Add(newItem);
                break;
            }
            case SyncList<GameObject>.Operation.OP_CLEAR:
            {

                break;
            }
            case SyncList<GameObject>.Operation.OP_INSERT:
            {

                break;
            }
            case SyncList<GameObject>.Operation.OP_REMOVEAT:
            {

                break;
            }
            case SyncList<GameObject>.Operation.OP_SET:
            {

                break;
            }
        }
    }
    
    [Server]
    public IEnumerator Generation() //Генерация планет, если они не были сгенерированы
    {
        yield return new WaitForSeconds(0.5f);
        AllSingleton.instance.currentPlayer = FindObjectOfType<CurrentPlayer>().GetComponent<CurrentPlayer>();
        var countPlanet = Random.Range(50, 70);
        //Random rand = new Random.Range(0, DateTime.Now.Second); // we need a random variable to select names randomly

        RandomName nameGen = new RandomName(); // create a new instance of the RandomName class
        List<string> allRandomNames = nameGen.RandomNames(countPlanet, 0); // generate 100 random names with up to two middle names

        while (syncListPlanet.Count < countPlanet)
        {
            //создание планеты
            var planet = Instantiate(planetPrefab, parentTransform);

            var planetController = planet.GetComponent<PlanetController>();

            planetController.namePlanet = allRandomNames[syncListPlanet.Count];
            planetController.AddResourcesForPlanet();

            //рандомные параметры для неё
            float x = 0, y = 0;
            RandomXY(ref x, ref y);

            planet.transform.position = new Vector3(x, y, 0);
            var randomScale = Random.Range(0.1f, 0.2f);
            planet.transform.localScale = new Vector3(randomScale, randomScale, randomScale);
            planet.GetComponent<SpriteRenderer>().sprite = listSpritePlanet[Random.Range(0, listSpritePlanet.Count)];

            //добавление планеты в список
            if (isServer)
                ChangeListPlanet(planet);
            else
            {
                CmdChangeListPlanet(planet);
            }
            
            yield return null;
        }

        yield return null;
    }

    public void HomePlanetAddingToPlayer()
    {
        //домашняя планета
        var homePlanet = syncListPlanet[Random.Range(0, syncListPlanet.Count)].GetComponent<PlanetController>();
        homePlanet.SetHomePlanet();
        AllSingleton.instance.currentPlayer.playerPlanets.Add(homePlanet);
        homePlanet.isHomePlanet = true;
        homePlanet.HomingPlanetShow();
        AllSingleton.instance.cameraMove.DoMove(homePlanet.transform.position.x, homePlanet.transform.position.y, 1f);
        homePlanet.isColonized = true;
        homePlanet.ResourceIconShow();
    }

    private void RandomXY(ref float x, ref float y)
    {
        x = Random.Range(xBounds.x, xBounds.y);
        y = Random.Range(yBounds.x, yBounds.y);
    }
}
