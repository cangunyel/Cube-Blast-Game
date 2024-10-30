using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ScaleAnimator : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        transform.DOPunchScale(10*Vector3.one,0.5F,1,0).SetLoops(-1);
    }

   
}
