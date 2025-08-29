using UnityEngine;

public class EnemyInput : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3f;

    // События для внешних систем
    public System.Action<Vector2> OnMovementChanged;
    public System.Action<bool> OnShootingChanged;
    public Vector2 MovementInput => _movementInput;
    public bool IsShooting => _isShooting;




    private Vector2 _movementInput;
    private bool _isShooting;

    
    public void SetMovementInput(Vector2 input)
    {
        _movementInput = Vector2.ClampMagnitude(input, 1f);
        OnMovementChanged?.Invoke(_movementInput);
    }

    public void SetShooting(bool shooting)
    {
        _isShooting = shooting;
        OnShootingChanged?.Invoke(_isShooting);
    }

    private void Update()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        if (_movementInput != Vector2.zero)
        {
            Vector3 movement = new Vector3(_movementInput.x, _movementInput.y, 0) * moveSpeed * Time.deltaTime;
            transform.position += movement;
        }
    }
}