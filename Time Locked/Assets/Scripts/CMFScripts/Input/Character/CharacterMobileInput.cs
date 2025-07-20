using System.Collections;
using System.Collections.Generic;
using CMF;
using UnityEngine;
using UnityEngine.UI;

public class CharacterMobileInput : CharacterInput
{
    public Button jumpButton;
    public Joystick joystick;
    private bool jumpState;
    public bool locked;
    
    void Start()
    {
        jumpButton.onClick.AddListener(PressJump);
    }
    
    public override float GetHorizontalMovementInput()
    {
#if UNITY_EDITOR
        float joystickValue = joystick.Horizontal;
        float keyboardValue = Input.GetAxis("Horizontal");

        if (Mathf.Abs(joystickValue) > Mathf.Abs(keyboardValue))
            return locked ? 0f : joystickValue;
        else
            return locked ? 0f : keyboardValue;

#else
        return locked ? 0f : joystick.Horizontal;
#endif
    }

    public override float GetVerticalMovementInput()
    {
#if UNITY_EDITOR
        float joystickValue = joystick.Vertical;
        float keyboardValue = Input.GetAxis("Vertical");

        if (Mathf.Abs(joystickValue) > Mathf.Abs(keyboardValue))
            return locked ? 0f : joystickValue;
        else
            return locked ? 0f : keyboardValue;

#else
        return locked ? 0f : joystick.Vertical;
#endif
    }

    public override bool IsJumpKeyPressed()
    {
        return !locked && jumpState;
    }

    private void PressJump()
    {
        StartCoroutine(ActivateJump());
    }
    
    private IEnumerator ActivateJump()
    {
        jumpState = true;
        yield return null;
        jumpState = false;
    }
}
