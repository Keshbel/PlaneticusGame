using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu]
public class AggressiveState : State
{
    private List<SpaceInvaderController> _botInvaders = new List<SpaceInvaderController>();

    public override async void Init()
    {
        await Task.Delay(2000);
        _botInvaders = currentPlayer.PlayerInvaders.FindAll(invader => invader.MoveTween == null);

        foreach (var invader in _botInvaders)
        {
            var planets = MainPlanetController.Instance.listPlanet;
            invader.MoveTowards(Utils.FindClosestEnemyPlayerPlanet(invader.transform, planets, currentPlayer).gameObject);
        }

        IsFinished = true;
    }

    public override void Run()
    {
        if (!IsFinished) return;
        
        Destroy(this);
        return;
    }
}
