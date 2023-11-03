using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Компонент передвижения игрока
/// </summary>
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    private PlayerInput playerInput;
    private InputAction movementAction;

    [SerializeField]
    private float speed = 5.5f;

    private void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        movementAction = playerInput.actions["Movement"];
    }

    private void Update() => Move(movementAction.ReadValue<Vector2>());

    private void Move(Vector2 dir) => transform.Translate(speed * Time.deltaTime * new Vector3(dir.x, 0, dir.y));
}
