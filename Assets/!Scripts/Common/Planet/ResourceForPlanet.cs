using System;
using Lean.Localization;
using UnityEngine;

[Serializable]
public class ResourceForPlanet
{
    //тип ресурса
    public Enums.ResourcePlanet resourcePlanet;
    //данные
    public string nameResource; //имя
    public int resourceDelivery;
    public int resourceMining;
    public int resourceAll;
    public bool isMined;
    //ссылки
    public Sprite spriteIcon;

    public void UpdateInfo()
    {
        switch (resourcePlanet)
        {
            case Enums.ResourcePlanet.Earth:
                spriteIcon = ResourceSingleton.instance.earthSprite;
                nameResource = LeanLocalization.GetFirstCurrentLanguage() == "English" ? "Earth" : "Земля";
                break;
            case Enums.ResourcePlanet.Fire:
                spriteIcon = ResourceSingleton.instance.fireSprite;
                nameResource = LeanLocalization.GetFirstCurrentLanguage() == "English" ? "Fire" : "Огонь";
                break;
            case Enums.ResourcePlanet.Aether:
                spriteIcon = ResourceSingleton.instance.aetherSprite;
                nameResource = LeanLocalization.GetFirstCurrentLanguage() == "English" ? "Aether" : "Эфир";
                break;
            case Enums.ResourcePlanet.Water:
                spriteIcon = ResourceSingleton.instance.waterSprite;
                nameResource = LeanLocalization.GetFirstCurrentLanguage() == "English" ? "Water" : "Вода";
                break;
            case Enums.ResourcePlanet.Wind:
                spriteIcon = ResourceSingleton.instance.windSprite;
                nameResource = LeanLocalization.GetFirstCurrentLanguage() == "English" ? "Wind" : "Ветер";
                break;
        }
    }
}
