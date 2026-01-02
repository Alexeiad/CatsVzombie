
using UnityEngine;
using UnityEngine.InputSystem;

public class KeyboardInput : IPlayerInput
{
    private InputAction _movementAction;

    public KeyboardInput()
    {
        // Создаем Input Action для движения
        _movementAction = new InputAction("Movement", InputActionType.Value);

        // Настраиваем композитное связывание (WASD + стрелки)
        _movementAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Up", "<Keyboard>/upArrow")
            .With("Down", "<Keyboard>/s")
            .With("Down", "<Keyboard>/downArrow")
            .With("Left", "<Keyboard>/a")
            .With("Left", "<Keyboard>/leftArrow")
            .With("Right", "<Keyboard>/d")
            .With("Right", "<Keyboard>/rightArrow");
        _movementAction.Enable();
    }
    public Vector2 GetMovement()
    {
        Vector2 dir = _movementAction.ReadValue<Vector2>();
        return dir.magnitude > 1 ? dir.normalized : dir;
    }
}
