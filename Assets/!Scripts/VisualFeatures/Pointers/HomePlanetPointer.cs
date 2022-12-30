using UnityEngine;

public class HomePlanetPointer : MonoBehaviour
{
    private void Start() 
    {
        PointerManager.Instance.AddToList(this);
    }

    public void Destroy() 
    {
        PointerManager.Instance.RemoveFromList(this);
    }
}
