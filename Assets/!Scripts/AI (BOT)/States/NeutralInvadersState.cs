using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

//[CreateAssetMenu]
public class NeutralInvadersState : State
{
    private List<PlanetController> Planets => MainPlanetController.Instance.listPlanet;
    public List<SpaceInvaderController> botInvaders = new List<SpaceInvaderController>();

    public override async void Init()
    {
        await Task.Delay(2000);
        
        var unusedInvader = currentPlayer.PlayerInvaders.Find(invader => invader.MoveTween == null);
        if (unusedInvader != null) botInvaders.Add(unusedInvader); //= currentPlayer.PlayerInvaders.FindAll(invader => invader.MoveTween == null);

        foreach (var invader in botInvaders)
        {
            var target = Utils.FindClosestNonPlayerPlanet(invader.transform, Planets);
            if (target != null) invader.MoveTowards(target);
        }

        botInvaders.Clear();
        
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
