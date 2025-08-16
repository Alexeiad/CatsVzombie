
using UnityEngine;

public class JoystickInput : IPlayerInput
{
    private Joystick _joystick;

    public JoystickInput(Joystick joystick)
    {
        _joystick = joystick;
    }

    public Vector2 GetMovement()
    {
        Vector2 dir = new Vector2(_joystick.Horizontal, _joystick.Vertical);
        return dir.magnitude > 1 ? dir.normalized : dir;
    }
}
