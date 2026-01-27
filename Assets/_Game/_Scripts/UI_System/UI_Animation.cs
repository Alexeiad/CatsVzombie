
using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

public class UI_Animation
{ 
    private float _colorA,_duration=0.3f;
    public void ToDesappear(Image image, TweenCallback tweenCallback)
    {

        image.DOFade(0, _duration).OnComplete(() =>
        {
            tweenCallback.Invoke();
            
        });
    }

    public void ToAppear(Image spriteRenderer)
    {
        spriteRenderer.DOFade(_colorA, _duration);
        
    }

}
