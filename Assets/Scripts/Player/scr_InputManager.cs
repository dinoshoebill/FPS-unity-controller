using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scr_InputManager : MonoBehaviour {

    private PlayerInput playerInput;
    private PlayerInput.PlayerActions playerMovement; 

    void Awake() {
        playerInput = new PlayerInput();
        playerMovement = playerInput.Player;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnEnable() {
        playerMovement.Enable();
    }

    private void OnDisable() {
        playerMovement.Disable();
    }


}
