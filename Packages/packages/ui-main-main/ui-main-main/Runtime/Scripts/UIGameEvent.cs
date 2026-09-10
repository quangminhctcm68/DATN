using NabaGame.Core.Runtime.EventManager;
using UnityEngine;

namespace NabaGame.UI
{
    public class UIBackgroundShowEvent : GameEvent
    {
        public UIElement uiElement;
        public bool instantAction;
    }

    public class UIBackgroundHideEvent : GameEvent
    {
        public UIElement uiElement;
        public bool instantAction;
    }
}