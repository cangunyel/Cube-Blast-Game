using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : Cell
{
    public int health;
    public bool IsAffectedByBlast;
    public bool IsAffectedByExplosion;


    public virtual void TakeDamage()
    {
        this.health--;
        if (this.health == 0)
        {
            Destroy(this.gameObject);
            LevelManager.Instance.UpdateGrid(this.GetPositionI(), this.GetPositionJ(), "null");
        }
    }



}
