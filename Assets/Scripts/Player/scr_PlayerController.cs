using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static scr_Models;

public class scr_PlayerController : MonoBehaviour {

    private CharacterController playerController;
    private DefaultInput inputActions;
    private Vector2 inputMovement;
    private Vector2 inputView;

    private Vector3 newCameraRotation;
    private Vector3 newPlayerRotation;

    private Vector2 speedMovement;
    private Vector2 speedMovementVelocity;

    private bool isSprinting;
    private float jumpingForce;
    private float currentPlayerSpeed;

    [Header("Preferences")]
    public Transform cameraHolder;
    public Transform pivotTransform;

    [Header("Settings")]
    public PlayerSettingsModel playerSettings;
    public LayerMask playerMask;

    [Header("Stance")]
    public PlayerStance playerStance;
    private Vector3 playerCameraVelocity;
    public Transform cameraPositionStand;
    public Transform cameraPositionCrouch;
    public Transform cameraPositionProne;
    public PlayerStanceCollider playerStanceStand;
    public PlayerStanceCollider playerStanceCrouch;
    public PlayerStanceCollider playerStanceProne;

    private float playerStanceVelocityFloat;
    private Vector3 playerStanceVelocityVector;

    private void Awake() {

        inputActions = new DefaultInput();
        playerController = GetComponent<CharacterController>();

        InitializeInputActions();
        InitializePlayerSettings();

        inputActions.Enable();
    }

    private void Update() {
        CalculateView();
        CalculateMovement();
        ApplyGravity();
        CalculateCameraPosition();
    }

    private void CalculateCameraPosition() {

        if (playerStance == PlayerStance.Crouch) {
            cameraHolder.transform.localPosition = Vector3.SmoothDamp(cameraHolder.transform.localPosition, cameraPositionCrouch.transform.localPosition, ref playerCameraVelocity, playerSettings.playerStanceSmoothing * Time.deltaTime);
            playerController.height = Mathf.SmoothDamp(playerController.height, playerStanceCrouch.playerStanceCollider.height, ref playerStanceVelocityFloat, playerSettings.playerStanceSmoothing * Time.deltaTime);
            playerController.center = Vector3.SmoothDamp(playerController.center, playerStanceCrouch.playerStanceCollider.center, ref playerStanceVelocityVector, playerSettings.playerStanceSmoothing * Time.deltaTime);
        } else if (playerStance == PlayerStance.Prone) {
            cameraHolder.transform.localPosition = Vector3.SmoothDamp(cameraHolder.transform.localPosition, cameraPositionProne.transform.localPosition, ref playerCameraVelocity, playerSettings.playerStanceSmoothing * Time.deltaTime);
            playerController.height = Mathf.SmoothDamp(playerController.height, playerStanceProne.playerStanceCollider.height, ref playerStanceVelocityFloat, playerSettings.playerStanceSmoothing * Time.deltaTime);
            playerController.center = Vector3.SmoothDamp(playerController.center, playerStanceProne.playerStanceCollider.center, ref playerStanceVelocityVector, playerSettings.playerStanceSmoothing * Time.deltaTime);
        } else {
            cameraHolder.transform.localPosition = Vector3.SmoothDamp(cameraHolder.transform.localPosition, cameraPositionStand.transform.localPosition, ref playerCameraVelocity, playerSettings.playerStanceSmoothing * Time.deltaTime);
            playerController.height = Mathf.SmoothDamp(playerController.height, playerStanceStand.playerStanceCollider.height, ref playerStanceVelocityFloat, playerSettings.playerStanceSmoothing * Time.deltaTime);
            playerController.center = Vector3.SmoothDamp(playerController.center, playerStanceStand.playerStanceCollider.center, ref playerStanceVelocityVector, playerSettings.playerStanceSmoothing * Time.deltaTime);
        }
    }

    private void CalculateMovement() {

        speedMovement = Vector2.SmoothDamp(speedMovement, 
            new Vector2(currentPlayerSpeed * inputMovement.y * Time.deltaTime, 
                currentPlayerSpeed * (isSprinting ? playerSettings.strafeSpeedMultiplierSprint : playerSettings.strafeSpeedMultiplier) * inputMovement.x * Time.deltaTime), 
            ref speedMovementVelocity, 
            playerSettings.playerMovementSmoothing, playerController.isGrounded ? playerSettings.playerMovementSmoothing : playerSettings.playerAirTimeSmoothing);

        Vector3 newPlayerMovement = new Vector3(speedMovement.y, jumpingForce * Time.deltaTime, speedMovement.x);
        newPlayerMovement = transform.TransformDirection(newPlayerMovement);

        playerController.Move(newPlayerMovement);
    }

    private void CalculateView() {

        newPlayerRotation.y += playerSettings.viewXSensitivity * (playerSettings.viewXInverted ? -inputView.x : inputView.x) * Time.deltaTime;
        transform.localRotation = Quaternion.Euler(newPlayerRotation);

        newCameraRotation.x += playerSettings.viewYSensitivity * (playerSettings.viewYInverted ? -inputView.y : inputView.y) * Time.deltaTime;
        newCameraRotation.x = Mathf.Clamp(newCameraRotation.x, playerSettings.viewClampYMin, playerSettings.viewClampYMax);

        cameraHolder.localRotation = Quaternion.Euler(newCameraRotation);
    }

    private void ApplyGravity() {

        if (playerController.isGrounded) {
            jumpingForce = -1f;
        } else {
            jumpingForce += playerSettings.gravity * playerSettings.gravityMultiplier * Time.deltaTime;
        }
    }

    private void Jump(float jumpStrength) {

        if (!playerController.isGrounded) {
            return;
        }

        jumpingForce += jumpStrength;
    }

    private void SetPlayerStance(PlayerStance nextPlayerStance) {

        if (nextPlayerStance == PlayerStance.Stand) {
            if (CanChangeStance(playerStanceStand.playerStanceCollider.height)) {
                return;
            }
        } else if (nextPlayerStance == PlayerStance.Crouch) {
            if (nextPlayerStance.CompareTo(playerStance) == 0) {
                if (CanChangeStance(playerStanceStand.playerStanceCollider.height)) {
                    return;
                } else {
                    nextPlayerStance = PlayerStance.Stand;
                }
            }
        } else if (CanChangeStance(playerStanceCrouch.playerStanceCollider.height) || !playerController.isGrounded) {
            return;
        }
        
        playerStance = nextPlayerStance;
        SetPlayerSpeed(playerStance);
    }

    private void SetPlayerSpeed(PlayerStance newPlayerStance) {
        if (newPlayerStance == PlayerStance.Stand)
            if(isSprinting)
                currentPlayerSpeed = playerSettings.playerSpeedSprint;
            else
                currentPlayerSpeed = playerSettings.playerSpeedStand;
        else if (newPlayerStance == PlayerStance.Crouch)
            currentPlayerSpeed = playerSettings.playerSpeedCrouch;
        else if (newPlayerStance == PlayerStance.Prone)
            currentPlayerSpeed = playerSettings.playerSpeedProne;
    }

    private bool CanChangeStance(float stanceCheckHeight) {
        Vector3 start = new Vector3(pivotTransform.position.x, pivotTransform.position.y + playerController.radius + 0.01f, pivotTransform.position.z);
        Vector3 end = new Vector3(pivotTransform.position.x, pivotTransform.position.y - playerController.radius - 0.01f + stanceCheckHeight, pivotTransform.position.z);
        return Physics.CheckCapsule(start, end, playerController.radius, playerMask);
    }

    private void InitializeInputActions() {
        inputActions.Player.Movement.performed += e => {
            inputMovement = e.ReadValue<Vector2>();
        };

        inputActions.Player.View.performed += e => inputView = e.ReadValue<Vector2>();

        inputActions.Player.Jump.performed += e => {
            if (playerStance == PlayerStance.Stand)
                if (e.interaction is UnityEngine.InputSystem.Interactions.TapInteraction)
                    Jump(playerSettings.jumpPower);
                else
                    Jump(playerSettings.jumpPower * playerSettings.doubleJumpMultiplier);
            else
                SetPlayerStance(PlayerStance.Stand);
        };

        inputActions.Player.Crouch.performed += e => SetPlayerStance(PlayerStance.Crouch);
        inputActions.Player.Prone.performed += e => SetPlayerStance(PlayerStance.Prone);
        
        inputActions.Player.Sprinting.started += e => StartSprinting();

        inputActions.Player.Sprinting.performed += e => StopSprinting();
    }
    private void InitializePlayerSettings() {

        newPlayerRotation = transform.localRotation.eulerAngles;
        newCameraRotation = cameraHolder.localRotation.eulerAngles;

        playerSettings.viewXSensitivity = 12;
        playerSettings.viewYSensitivity = 12;
        playerSettings.viewXInverted = false;
        playerSettings.viewYInverted = true;

        playerSettings.playerSpeedSprint = 10;
        playerSettings.playerSpeedStand = 6;
        playerSettings.playerSpeedCrouch = 4;
        playerSettings.playerSpeedProne = 2;

        playerSettings.viewClampYMin = -80;
        playerSettings.viewClampYMax = 80;

        playerSettings.jumpPower = 15;

        playerSettings.gravity = -10;
        playerSettings.gravityMultiplier = 5;

        playerSettings.playerStanceSmoothing = 12f;
        playerSettings.playerMovementSmoothing = 0.3f;
        playerSettings.playerAirTimeSmoothing = 0.05f;

        playerSettings.strafeSpeedMultiplier = 0.7f;
        playerSettings.strafeSpeedMultiplierSprint = 0.5f;

        playerSettings.doubleJumpMultiplier = 1.3f;

        isSprinting = false;
        SetPlayerStance(PlayerStance.Stand);
    }

    private void StopSprinting() {
        isSprinting = false;
        SetPlayerSpeed(playerStance);
    }

    private void StartSprinting() {
        if (!CanChangeStance(playerStanceStand.playerStanceCollider.height) && inputMovement.y > 0) {
            isSprinting = true;
            SetPlayerStance(PlayerStance.Stand);
        }
    }
}
