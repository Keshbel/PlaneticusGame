using System.Collections.Generic;
using UnityEngine;

public class PointerManager : MonoBehaviour
{
    [SerializeField] private bool isOn;
    public bool IsOn
    {
        get => isOn;
        set
        {
            isOn = value;
            if (value != true) HidePointers();
        }
    }
    
    [SerializeField] private PointerIcon pointerPrefab;
    public readonly Dictionary<HomePlanetPointer, PointerIcon> Dictionary = new Dictionary<HomePlanetPointer, PointerIcon>();
    [SerializeField] Transform playerTransform;
    [SerializeField] Camera cam;

    #region Singleton
    
    public static PointerManager Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);
    }

    #endregion

    private void Start()
    {
        if (!cam) cam = Camera.main;
        if (!playerTransform && cam is not null) playerTransform = cam.transform;
        
        IsOn = PlayerPrefsExtra.GetBool("IsPointers", true);
    }

    void LateUpdate()
    {
        if (!IsOn) return;
        
        // Left, Right, Down, Up
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(cam);

        foreach (var kvp in Dictionary)
        {
            HomePlanetPointer enemyPointer = kvp.Key;
            PointerIcon pointerIcon = kvp.Value;

            var pPos = playerTransform.position;
            Vector3 toEnemy = enemyPointer.transform.position - pPos;
            Ray ray = new Ray(pPos, toEnemy);
            //Debug.DrawRay(pPos, toEnemy);

            float rayMinDistance = Mathf.Infinity;
            int index = 0;

            for (int p = 0; p < 4; p++)
            {
                if (!planes[p].Raycast(ray, out float distance)) continue;
                if (!(distance < rayMinDistance)) continue;
                
                rayMinDistance = distance;
                index = p;
            }

            rayMinDistance = Mathf.Clamp(rayMinDistance, 0, toEnemy.magnitude);
            Vector3 worldPosition = ray.GetPoint(rayMinDistance);
            Vector3 position = cam.WorldToScreenPoint(worldPosition);
            Quaternion rotation = GetIconRotation(index);

            if (toEnemy.magnitude > rayMinDistance) pointerIcon.Show();
            else pointerIcon.Hide();

            pointerIcon.SetIconPosition(position, rotation);
        }
    }

    private void HidePointers()
    {
        foreach (var kvp in Dictionary)
        {
            kvp.Value.Hide();
        }
    }
    
    public void AddToList(HomePlanetPointer enemyPointer) 
    {
        PointerIcon newPointer = Instantiate(pointerPrefab, transform);
        Dictionary.Add(enemyPointer, newPointer);
    }

    public void RemoveFromList(HomePlanetPointer enemyPointer)
    {
        Destroy(Dictionary[enemyPointer].gameObject);
        Dictionary.Remove(enemyPointer);
    }

    Quaternion GetIconRotation(int planeIndex)
    {
        return planeIndex switch
        {
            0 => Quaternion.Euler(0f, 0f, 90f),
            1 => Quaternion.Euler(0f, 0f, -90f),
            2 => Quaternion.Euler(0f, 0f, 180),
            3 => Quaternion.Euler(0f, 0f, 0f),
            _ => Quaternion.identity
        };
    }
}
