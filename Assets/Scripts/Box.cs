using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Box : Obstacle
{

    public Box()
    {
        IsInteractable = false;
        DoesFall = false;

        //Obstacle attributes
        health = 1;
        IsAffectedByBlast = true;
        IsAffectedByExplosion = true;

    }

    public override void TakeDamage()
    {
        this.health--;

        if (this.health == 0)
        {
            ActivateParticles();
            Destroy(this.gameObject);
            LevelManager.Instance.UpdateGrid(this.GetPositionI(), this.GetPositionJ(), "null");
            LevelManager.Instance.numberOfBox--;
        }

    }
}
