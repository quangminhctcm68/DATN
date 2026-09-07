using System;
using System.Collections;
using System.Collections.Generic;
using Coffee.UIExtensions;
using DG.Tweening;
using UnityEngine;

public class ShinyEffect : ShinyEffectForUGUI
{
    public float time;
    public float timeRepeat;

    private float timer;
    public bool isActive = true;
    private void StartShiny()
    {
        location = 0;
        DOVirtual.Float(0, 1, time, x => { location = x; });
    }

    private void Update()
    {
        if (!gameObject.activeSelf || !isActive) return;
        timer += Time.deltaTime;
        if (timer > timeRepeat)
        {
            timer = 0;
            StartShiny();
        }
    }
}