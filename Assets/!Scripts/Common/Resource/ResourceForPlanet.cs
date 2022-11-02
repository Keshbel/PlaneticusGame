using System;
using Lean.Localization;
using UnityEngine;

[Serializable]
public class ResourceForPlanet
{
    public string nameResource; //имя
    public Enums.ResourcePlanet resourcePlanet; //тип ресурса
    
    //данные
    //resource count
    public int countResource;

    //ссылки
    [NonSerialized]
    public Sprite SpriteIcon;

    public void UpdateInfo()
    {
        switch (resourcePlanet)
        {
            case Enums.ResourcePlanet.Earth:
                SpriteIcon = ResourceSingleton.instance.earthSprite;
                nameResource = LeanLocalization.GetFirstCurrentLanguage() == "English" ? "Earth" : "Земля";
                break;
            case Enums.ResourcePlanet.Fire:
                SpriteIcon = ResourceSingleton.instance.fireSprite;
                nameResource = LeanLocalization.GetFirstCurrentLanguage() == "English" ? "Fire" : "Огонь";
                break;
            case Enums.ResourcePlanet.Aether:
                SpriteIcon = ResourceSingleton.instance.aetherSprite;
                nameResource = LeanLocalization.GetFirstCurrentLanguage() == "English" ? "Aether" : "Эфир";
                break;
            case Enums.ResourcePlanet.Water:
                SpriteIcon = ResourceSingleton.instance.waterSprite;
                nameResource = LeanLocalization.GetFirstCurrentLanguage() == "English" ? "Water" : "Вода";
                break;
            case Enums.ResourcePlanet.Air:
                SpriteIcon = ResourceSingleton.instance.windSprite;
                nameResource = LeanLocalization.GetFirstCurrentLanguage() == "English" ? "Air" : "Воздух";
                break;
        }
    }
}
