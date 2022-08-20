using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlanetGeneration : MonoBehaviour
{
    private const float PlanetOffset = 0.8f;
    
    public GameObject planetPrefab;
    public Vector2 xBounds;
    public Vector2 yBounds;

    public List<GameObject> listPlanet;

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

            /*for (var index = 0; index < listPlanet.Count; index++) //сверка на одинаковость координат
            {
                var planetAnother = listPlanet[index];
                if (x - PlanetOffset <= planetAnother.transform.position.x &&
                    x + PlanetOffset >= planetAnother.transform.position.x)
                {
                    if (y - PlanetOffset <= planetAnother.transform.position.y &&
                        y + PlanetOffset >= planetAnother.transform.position.y)
                    {
                    }
                    else
                    {
                        index = -1;
                        RandomXY(ref x, ref y);
                    }
                }
                else
                {
                    index = -1;
                    RandomXY(ref x, ref y);
                }
            }*/

            planet.transform.position = new Vector3(x, y, 0);
            listPlanet.Add(planet);
            yield return null;
        }

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
