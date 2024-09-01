using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlCapsule : Interactable
{
    public static Action OnInteracted;

    public override void OnFocus()
    {

    }

    public override void OnInteract()
    {   
        if (!CharacterSwitch.isCaph)
            OnInteracted?.Invoke();
    }

    public override void OnLoseFocus()
    {

    }
}
