using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Users;

[RequireComponent(typeof(PlayerInput))]
public class Manager_Input : Singleton<Manager_Input>
{

    private static InputDevice m_lastUsedDevice;

    [SerializeField] private PlayerInput _playerInput;
    private InputUser m_user;

    protected override void Init()
    {
        base.Init();

        m_user = _playerInput.user;

        InputUser.onChange += OnUserChange;
        InputSystem.onEvent += OnInputEvent;   // ← captura input real
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        InputUser.onChange -= OnUserChange;
        InputSystem.onEvent -= OnInputEvent;
    }

    // private void OnPlayer1Move(InputValue value) => Manager_Events.Player.OnPlayer1Move.Notify(value.Get<Vector2>());

    private void OnUserChange(InputUser user, InputUserChange change, InputDevice device)
    {
        if (user != m_user)
            return;
    }

    private void OnInputEvent(InputEventPtr eventPtr, InputDevice device)
    {
        if (!eventPtr.handled && m_user.pairedDevices.Contains(device))
        {
            m_lastUsedDevice = device;
        }
    }

    public static bool IsGamepad => m_lastUsedDevice is Gamepad;
    public static bool IsUsingKeyboardMouse => m_lastUsedDevice is Keyboard || m_lastUsedDevice is Mouse;

}