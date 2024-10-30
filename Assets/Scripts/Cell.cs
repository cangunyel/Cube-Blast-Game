using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class Cell : MonoBehaviour
{
    public bool IsInteractable;
    public bool DoesFall;
    public Tuple<int, int>position;

    public ParticleSystem particleSystemPrefab;

    public virtual void ActivateParticles(){
        Instantiate(particleSystemPrefab, transform.position, transform.rotation);
    }

    public virtual void Initialize(Tuple<int, int> position)
    {
        this.position = position;
    }

    public int GetPositionI(){
        return this.position.Item1;
    }

    public int GetPositionJ(){
        return this.position.Item2;
    }
}
