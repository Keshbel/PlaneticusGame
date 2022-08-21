using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlanetGeneration : MonoBehaviour
{
    //planets
    private const float PlanetOffset = 0.8f;
    public GameObject planetPrefab;
    public List<Sprite> listSpritePlanet;
    public List<GameObject> listPlanet;
    //Bounds of Camera Movement
    public Vector2 xBounds;
    public Vector2 yBounds;

    

    private void Start()
    {
        StartCoroutine(Generation());
    }

    public IEnumerator Generation() //Генерация планет, если они не были сгенерированы
    {
        var countPlanet = Random.Range(50, 70);

        while (listPlanet.Count < countPlanet)
        {
            var planet = Instantiate(planetPrefab);

            float x = 0, y = 0;

            RandomXY(ref x, ref y);

            planet.transform.position = new Vector3(x, y, 0);
            var randomScale = Random.Range(0.1f, 0.2f);
            planet.transform.localScale = new Vector3(randomScale,randomScale,randomScale);
            planet.GetComponent<SpriteRenderer>().sprite = listSpritePlanet[Random.Range(0, listSpritePlanet.Count)];
            listPlanet.Add(planet);
            yield return null;
        }

        //расселение в другие координаты пересекающихся планет
        foreach (var planeT in listPlanet)
        {
            Bounds _bounds = planeT.GetComponent<CircleCollider2D>().bounds;
            foreach (var planet in listPlanet)
            {
                if (_bounds.Intersects(planet.GetComponent<CircleCollider2D>().bounds))
                {
                    float x = 0, y = 0;
                    RandomXY(ref x, ref y);
                    planet.transform.position = new Vector3(x, y, 0);
                    print("Intersect");
                }
            }
        }
        
        yield return new WaitForSeconds(0.1f);
        
        foreach (var planeT in listPlanet)
        {
            Bounds _bounds = planeT.GetComponent<CircleCollider2D>().bounds;
            foreach (var planet in listPlanet)
            {
                if (_bounds.Intersects(planet.GetComponent<CircleCollider2D>().bounds))
                {
                    float x = 0, y = 0;
                    RandomXY(ref x, ref y);
                    planet.transform.position = new Vector3(x, y, 0);
                    print("Intersect");
                }
            }
        }
        
        yield return new WaitForSeconds(0.1f);
        
        foreach (var planeT in listPlanet)
        {
            Bounds _bounds = planeT.GetComponent<CircleCollider2D>().bounds;
            foreach (var planet in listPlanet)
            {
                if (_bounds.Intersects(planet.GetComponent<CircleCollider2D>().bounds))
                {
                    float x = 0, y = 0;
                    RandomXY(ref x, ref y);
                    planet.transform.position = new Vector3(x, y, 0);
                    print("Intersect");
                }
            }
        }

        yield return null;
    }

    private void RandomXY(ref float x, ref float y)
    {
        x = Random.Range(xBounds.x, xBounds.y);
        y = Random.Range(yBounds.x, yBounds.y);
    }
}
