using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSwitcher : MonoBehaviour
{
    public Player player1;
    public Player player2;

    public PlayerSwitchSet input { get; private set; }

    private void Awake()
    {
        input = new PlayerSwitchSet();
    }

    private void Start()
    {
        player1.isControlled = true;
        player1.SwitchInputState();
        player2.isControlled = false;
        player2.SwitchInputState();
    }

    private void Update()
    {
        if (input.PlayerSwitch.Switch.WasPressedThisFrame())
        {
            player1.isControlled = !player1.isControlled;
            player1.SwitchInputState();
            player2.isControlled = !player2.isControlled;
            player2.SwitchInputState();
        }
    }

    private void OnEnable()
    {
        input.Enable();
    }

    private void OnDisable()
    {
        input.Disable();
    }
}
