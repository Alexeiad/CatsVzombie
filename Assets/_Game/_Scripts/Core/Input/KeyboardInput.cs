
using UnityEngine;

public class KeyboardInput : IPlayerInput
{
    public Vector2 GetMovement()
    {
        float x = 0f, y = 0f;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) y += 1;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) y -= 1;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) x += 1;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) x -= 1;

        Vector2 dir = new Vector2(x, y);
        return dir.magnitude > 1 ? dir.normalized : dir;
    }
}
