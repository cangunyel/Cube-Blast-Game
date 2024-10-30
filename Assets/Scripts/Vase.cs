using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vase : Obstacle
{

    public Sprite damagedVase;
    public Vase()
    {
        //Cell attributes
        IsInteractable = false;
        DoesFall = true;

        //Obstacle attributes
        health = 2;
        IsAffectedByBlast = true;
        IsAffectedByExplosion = true;

    }
    public override void TakeDamage()
    {
        this.health--;
        ActivateParticles();
        if (this.health == 0)
        {
            Destroy(this.gameObject);
            LevelManager.Instance.UpdateGrid(this.GetPositionI(), this.GetPositionJ(), "null");
            LevelManager.Instance.numberOfVase--;
        }
        else if (this.health == 1)
        {
            SpriteRenderer spriteRenderer = this.GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = damagedVase;
        }

    }

}
