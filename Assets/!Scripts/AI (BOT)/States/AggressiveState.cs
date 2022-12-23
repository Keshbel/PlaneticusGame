using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

//[CreateAssetMenu]
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
            var target = Utils.FindClosestEnemyPlayerPlanet(invader.transform, planets, currentPlayer);
            if (target != null) invader.MoveTowards(target);
        }

        _botInvaders.Clear();
        
        await Task.Delay(2000);
        IsFinished = true;
    }

    public override void Run()
    {
        if (!IsFinished) return;
        
        Destroy(this);
        return;
    }
}
