using UnityEngine;

public class UltraSensitiveDirectionController : MonoBehaviour
{
    public Animator animator;

    private Vector3 previousPosition;
    private Vector2 currentDirection;

    private const string UP_BOOL = "Up";
    private const string DOWN_BOOL = "Down";
    private const string LEFT_BOOL = "Left";
    private const string RIGHT_BOOL = "Right";
    private const string UP_LEFT_BOOL = "UpLeft";
    private const string UP_RIGHT_BOOL = "UpRight";
    private const string DOWN_LEFT_BOOL = "DownLeft";
    private const string DOWN_RIGHT_BOOL = "DownRight";
    private const string MOVING_BOOL = "IsMoving";

    void Update()
    {
        Vector3 movement = transform.position - previousPosition;
        currentDirection = new Vector2(movement.x, movement.y);

        UpdateAnimator();
        previousPosition = transform.position;
    }

    private void UpdateAnimator()
    {
        bool isMoving = currentDirection.magnitude > 0;
        animator.SetBool(MOVING_BOOL, isMoving);

        if (!isMoving)
        {
            ResetAllDirectionBools();
            return;
        }

        ResetAllDirectionBools();

        // Мгновенное определение направления без порогов
        Vector2 normalizedDir = currentDirection.normalized;
        float angle = Mathf.Atan2(normalizedDir.y, normalizedDir.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360;

        if (angle >= 337.5f || angle < 22.5f) animator.SetBool(RIGHT_BOOL, true);
        else if (angle >= 22.5f && angle < 67.5f) animator.SetBool(UP_RIGHT_BOOL, true);
        else if (angle >= 67.5f && angle < 112.5f) animator.SetBool(UP_BOOL, true);
        else if (angle >= 112.5f && angle < 157.5f) animator.SetBool(UP_LEFT_BOOL, true);
        else if (angle >= 157.5f && angle < 202.5f) animator.SetBool(LEFT_BOOL, true);
        else if (angle >= 202.5f && angle < 247.5f) animator.SetBool(DOWN_LEFT_BOOL, true);
        else if (angle >= 247.5f && angle < 292.5f) animator.SetBool(DOWN_BOOL, true);
        else if (angle >= 292.5f && angle < 337.5f) animator.SetBool(DOWN_RIGHT_BOOL, true);
    }

    private void ResetAllDirectionBools()
    {
        animator.SetBool(UP_BOOL, false);
        animator.SetBool(DOWN_BOOL, false);
        animator.SetBool(LEFT_BOOL, false);
        animator.SetBool(RIGHT_BOOL, false);
        animator.SetBool(UP_LEFT_BOOL, false);
        animator.SetBool(UP_RIGHT_BOOL, false);
        animator.SetBool(DOWN_LEFT_BOOL, false);
        animator.SetBool(DOWN_RIGHT_BOOL, false);
    }
}