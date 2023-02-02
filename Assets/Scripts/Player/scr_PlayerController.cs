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
    
    private float jumpingForce;

    private float doubleJumpMultiplier = 1.3f;
    private float jumpMultiplier = 1f;
    private float ViewInputSensitivity = 20f;
    private float gravityValue = -10f;
    private float gravityValueMultiplier = 5f;
    private float jumpingForceValue = 15f;
    private float playerSpeedStandValue = 8f;
    private float playerSpeedCrouchValue = 4f;
    private float playerSpeedProneValue = 2f;
    private float playerSpeedSprintValue = 10f;
    private float ViewClampYMin = -80f;
    private float ViewClampYMax = 80f;
    private float playerStanceSmoothing = 15f;
    private bool DefaultInverted = false;

    [Header("Preferences")]
    public Transform cameraHolder;

    [Header("Settings")]
    public PlayerSettingsModel playerSettings;

    [Header("Stance")]
    public PlayerStance playerStance;
    private Vector3 playerCameraVelocity;
    private float playerStanceVelocityFloat;
    private Vector3 playerStanceVelocityVector;
    public Transform cameraPositionStand;
    public Transform cameraPositionCrouch;
    public Transform cameraPositionProne;
    public PlayerStanceCollider playerStanceColliderStand;
    public PlayerStanceCollider playerStanceColliderCrouch;
    public PlayerStanceCollider playerStanceColliderProne;

    private void Awake() {

        inputActions = new DefaultInput();
        playerController = GetComponent<CharacterController>();

        inputActions.Player.Movement.performed += e => inputMovement = e.ReadValue<Vector2>();
        inputActions.Player.View.performed += e => inputView = e.ReadValue<Vector2>();
        inputActions.Player.Jump.performed += e => {
            Debug.Log(e.interaction);
            if (playerStance == PlayerStance.Stand && playerController.isGrounded) {
                if (e.interaction is UnityEngine.InputSystem.Interactions.TapInteraction) {
                    Jump(jumpMultiplier);
                } else {
                    Jump(doubleJumpMultiplier);
                }
            }
            else {
                SetPlayerStance(PlayerStance.Stand);
            }
        };
        inputActions.Player.Crouch.performed += e => SetPlayerStance(PlayerStance.Crouch);
        inputActions.Player.Prone.performed += e => SetPlayerStance(PlayerStance.Prone);

        newPlayerRotation = transform.localRotation.eulerAngles;
        newCameraRotation = cameraHolder.localRotation.eulerAngles;

        playerSettings.viewXSensitivity = ViewInputSensitivity;
        playerSettings.viewYSensitivity = ViewInputSensitivity;

        playerSettings.viewXInverted = DefaultInverted;
        playerSettings.viewYInverted = !DefaultInverted;

        playerSettings.playerSpeedSprint = playerSpeedSprintValue;
        playerSettings.playerSpeedStand = playerSpeedStandValue;
        playerSettings.playerSpeedCrouch = playerSpeedCrouchValue;
        playerSettings.playerSpeedProne = playerSpeedProneValue;

        playerSettings.viewClampYMin = ViewClampYMin;
        playerSettings.viewClampYMax = ViewClampYMax;

        playerSettings.gravity = gravityValue;
        playerSettings.gravityMultiplier = gravityValueMultiplier;

        playerSettings.jumpPower = jumpingForceValue;

        playerStance = PlayerStance.Stand;

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
            cameraHolder.transform.localPosition = Vector3.SmoothDamp(cameraHolder.transform.localPosition, cameraPositionCrouch.transform.localPosition, ref playerCameraVelocity, playerStanceSmoothing * Time.deltaTime);
            playerController.height = Mathf.SmoothDamp(playerController.height, playerStanceColliderCrouch.playerStanceCollider.height, ref playerStanceVelocityFloat, playerStanceSmoothing * Time.deltaTime);
            playerController.center = Vector3.SmoothDamp(playerController.center, playerStanceColliderCrouch.playerStanceCollider.center, ref playerStanceVelocityVector, playerStanceSmoothing * Time.deltaTime);
        } else if (playerStance == PlayerStance.Prone) {
            cameraHolder.transform.localPosition = Vector3.SmoothDamp(cameraHolder.transform.localPosition, cameraPositionProne.transform.localPosition, ref playerCameraVelocity, playerStanceSmoothing * Time.deltaTime);
            playerController.height = Mathf.SmoothDamp(playerController.height, playerStanceColliderProne.playerStanceCollider.height, ref playerStanceVelocityFloat, playerStanceSmoothing * Time.deltaTime);
            playerController.center = Vector3.SmoothDamp(playerController.center, playerStanceColliderProne.playerStanceCollider.center, ref playerStanceVelocityVector, playerStanceSmoothing * Time.deltaTime);
        } else {
            cameraHolder.transform.localPosition = Vector3.SmoothDamp(cameraHolder.transform.localPosition, cameraPositionStand.transform.localPosition, ref playerCameraVelocity, playerStanceSmoothing * Time.deltaTime);
            playerController.height = Mathf.SmoothDamp(playerController.height, playerStanceColliderStand.playerStanceCollider.height, ref playerStanceVelocityFloat, playerStanceSmoothing * Time.deltaTime);
            playerController.center = Vector3.SmoothDamp(playerController.center, playerStanceColliderStand.playerStanceCollider.center, ref playerStanceVelocityVector, playerStanceSmoothing * Time.deltaTime);
        }
    }

    private void CalculateMovement() {

        float playerSpeed = playerSettings.playerSpeedStand;

        if (playerStance == PlayerStance.Sprint) {
            playerSpeed = playerSettings.playerSpeedSprint;
        } else if (playerStance == PlayerStance.Crouch) {
            playerSpeed = playerSettings.playerSpeedCrouch;
        } else if (playerStance == PlayerStance.Prone) {
            playerSpeed = playerSettings.playerSpeedProne;
        }

        var verticalSpeed = playerSpeed * inputMovement.y * Time.deltaTime;
        var horizontalSpeed = playerSpeed * inputMovement.x * Time.deltaTime;

        Vector3 newMovementDirection = new Vector3(horizontalSpeed, jumpingForce * Time.deltaTime, verticalSpeed);
        newMovementDirection = transform.TransformDirection(newMovementDirection);

        playerController.Move(newMovementDirection);
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

        jumpingForce += playerSettings.jumpPower * jumpStrength;
    }

    private void SetPlayerStance(PlayerStance playerStance) {
        if (playerStance == PlayerStance.Crouch) {
            if (this.playerStance == PlayerStance.Crouch) {
                this.playerStance = PlayerStance.Prone;
            } else if (playerController.isGrounded) {
                this.playerStance = playerStance;
            }
        } else {
            this.playerStance = playerStance;
        }
    }
}
