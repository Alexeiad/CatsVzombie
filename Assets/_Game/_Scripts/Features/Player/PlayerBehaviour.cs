
using UnityEngine;
using Zenject;

public class PlayerBehaviour : MonoBehaviour
{
    public float speed;
    private PlayerInput inputFacade;

    [Inject] private Joystick joystick;
    



    void Start()
    {
        inputFacade = new PlayerInput(joystick);
    }

    void Update()
    {
        Vector2 dir = inputFacade.GetMovement();
        if (dir != Vector2.zero)
        {
            Vector3 move = new Vector3(dir.x, dir.y, 0) * speed * Time.deltaTime;
            transform.position += move;
        }
    }


}
