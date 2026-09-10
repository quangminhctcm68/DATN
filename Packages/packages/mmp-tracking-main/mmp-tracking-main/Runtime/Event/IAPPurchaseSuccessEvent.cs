using System.Collections;
using System.Collections.Generic;
using NabaGame.Core.Runtime.EventManager;
using UnityEngine;

public class IAPPurchaseSuccessEvent : GameEvent
{
    public float amout;

    public IAPPurchaseSuccessEvent(float _amout)
    {
        amout = _amout;
    }
}