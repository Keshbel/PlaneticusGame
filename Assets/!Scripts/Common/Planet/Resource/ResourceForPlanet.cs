using System;
using Lean.Localization;
using UnityEngine;

[Serializable]
public class ResourceForPlanet
{
    public int id;
    public string nameResource; //имя
    public Enums.ResourcePlanet resourcePlanet; //тип ресурса
    
    public int countResource = 1; //количество
    
    [NonSerialized]
    public Sprite SpriteIcon;

    public void UpdateInfo()
    {
        switch (resourcePlanet)
        {
            case Enums.ResourcePlanet.Earth:
                SpriteIcon = ResourceSingleton.Instance.earthSprite;
                nameResource = LeanLocalization.GetFirstCurrentLanguage() == "English" ? "Earth" : "Земля";
                break;
            case Enums.ResourcePlanet.Fire:
                SpriteIcon = ResourceSingleton.Instance.fireSprite;
                nameResource = LeanLocalization.GetFirstCurrentLanguage() == "English" ? "Fire" : "Огонь";
                break;
            case Enums.ResourcePlanet.Aether:
                SpriteIcon = ResourceSingleton.Instance.aetherSprite;
                nameResource = LeanLocalization.GetFirstCurrentLanguage() == "English" ? "Aether" : "Эфир";
                break;
            case Enums.ResourcePlanet.Water:
                SpriteIcon = ResourceSingleton.Instance.waterSprite;
                nameResource = LeanLocalization.GetFirstCurrentLanguage() == "English" ? "Water" : "Вода";
                break;
            case Enums.ResourcePlanet.Air:
                SpriteIcon = ResourceSingleton.Instance.windSprite;
                nameResource = LeanLocalization.GetFirstCurrentLanguage() == "English" ? "Air" : "Воздух";
                break;
        }
    }
}
