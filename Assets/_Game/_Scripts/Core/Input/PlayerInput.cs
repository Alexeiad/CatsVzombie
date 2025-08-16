using UnityEngine;
public class PlayerInput
{
    private IPlayerInput inputHandler;

    public PlayerInput(Joystick joystick = null)
    {
        if (IsMobilePlatform() && joystick != null)
            inputHandler = new JoystickInput(joystick);
        else
            inputHandler = new KeyboardInput();
    }

    public Vector2 GetMovement()
    {
        return inputHandler.GetMovement();
    }

    private bool IsMobilePlatform()
    {
        return Application.isMobilePlatform;
    }
}
