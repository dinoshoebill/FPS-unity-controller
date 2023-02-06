using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static scr_Models;

public class scr_PlayerController : MonoBehaviour {

    private CharacterController playerController;
    private DefaultInput inputActions;
    [HideInInspector]
    public Vector2 inputMovement;
    [HideInInspector]
    public Vector2 inputView;

    private Vector3 newCameraRotation;
    private Vector3 newPlayerRotation;

    private Vector2 speedMovement;
    private Vector2 speedMovementVelocity;

    [HideInInspector]
    public bool isSprinting;
    public bool wantsSprinting;
    private float jumpingForce;
    private float currentPlayerSpeed;

    private float playerStanceVelocityFloat;
    private Vector3 playerStanceVelocityVector;

    public float weaponAnimationSpeed;

    [Header("Preferences")]
    public Transform cameraHolder;
    public Transform pivotTransform;

    [Header("Settings")]
    public PlayerSettingsModel playerSettings;

    [Header("Player Mask")]
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

    [Header("Weapon")]
    public scr_WeaponController currentWeapon;

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
            cameraHolder.transform.localPosition = Vector3.SmoothDamp(cameraHolder.transform.localPosition, cameraPositionCrouch.transform.localPosition, ref playerCameraVelocity, playerSettings.stanceSmoothing * Time.deltaTime);
            playerController.height = Mathf.SmoothDamp(playerController.height, playerStanceCrouch.stanceCollider.height, ref playerStanceVelocityFloat, playerSettings.stanceSmoothing * Time.deltaTime);
            playerController.center = Vector3.SmoothDamp(playerController.center, playerStanceCrouch.stanceCollider.center, ref playerStanceVelocityVector, playerSettings.stanceSmoothing * Time.deltaTime);
        } else if (playerStance == PlayerStance.Prone) {
            cameraHolder.transform.localPosition = Vector3.SmoothDamp(cameraHolder.transform.localPosition, cameraPositionProne.transform.localPosition, ref playerCameraVelocity, playerSettings.stanceSmoothing * Time.deltaTime);
            playerController.height = Mathf.SmoothDamp(playerController.height, playerStanceProne.stanceCollider.height, ref playerStanceVelocityFloat, playerSettings.stanceSmoothing * Time.deltaTime);
            playerController.center = Vector3.SmoothDamp(playerController.center, playerStanceProne.stanceCollider.center, ref playerStanceVelocityVector, playerSettings.stanceSmoothing * Time.deltaTime);
        } else {
            cameraHolder.transform.localPosition = Vector3.SmoothDamp(cameraHolder.transform.localPosition, cameraPositionStand.transform.localPosition, ref playerCameraVelocity, playerSettings.stanceSmoothing * Time.deltaTime);
            playerController.height = Mathf.SmoothDamp(playerController.height, playerStanceStand.stanceCollider.height, ref playerStanceVelocityFloat, playerSettings.stanceSmoothing * Time.deltaTime);
            playerController.center = Vector3.SmoothDamp(playerController.center, playerStanceStand.stanceCollider.center, ref playerStanceVelocityVector, playerSettings.stanceSmoothing * Time.deltaTime);
        }
    }

    private void CalculateMovement() {

        speedMovement = Vector2.SmoothDamp(speedMovement, 
            new Vector2(currentPlayerSpeed * inputMovement.y * Time.deltaTime, 
                currentPlayerSpeed * (isSprinting ? playerSettings.speedStrafeSprintMultiplier : playerSettings.speedStrafeMultiplier) * inputMovement.x * Time.deltaTime), 
            ref speedMovementVelocity, 
            playerSettings.movementSmoothing, playerController.isGrounded ? playerSettings.movementSmoothing : playerSettings.airTimeSmoothing);

        Vector3 newPlayerMovement = new Vector3(speedMovement.y, jumpingForce * Time.deltaTime, speedMovement.x);
        newPlayerMovement = transform.TransformDirection(newPlayerMovement);

        weaponAnimationSpeed = playerController.velocity.magnitude / (currentPlayerSpeed);

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
            if (CanChangeStance(playerStanceStand.stanceCollider.height)) {
                return;
            }
        }
        else if (nextPlayerStance == PlayerStance.Crouch) {
            if (nextPlayerStance.CompareTo(playerStance) == 0) {
                if (CanChangeStance(playerStanceStand.stanceCollider.height)) {
                    return;
                }
                else {
                    nextPlayerStance = PlayerStance.Stand;
                }
            } else if (CanChangeStance(playerStanceCrouch.stanceCollider.height) || !playerController.isGrounded) {
                return;
            }
        }
        
        playerStance = nextPlayerStance;
        SetPlayerSpeed(playerStance);
    }

    private void SetPlayerSpeed(PlayerStance newPlayerStance) {
        if (newPlayerStance == PlayerStance.Stand)
            if(isSprinting)
                currentPlayerSpeed = playerSettings.speedSprint;
            else
                currentPlayerSpeed = playerSettings.speedStand;
        else if (newPlayerStance == PlayerStance.Crouch)
            currentPlayerSpeed = playerSettings.speedCrouch;
        else if (newPlayerStance == PlayerStance.Prone)
            currentPlayerSpeed = playerSettings.speedProne;
    }

    private bool CanChangeStance(float stanceCheckHeight) {
        Vector3 start = new Vector3(pivotTransform.position.x, pivotTransform.position.y + playerController.radius + 0.01f, pivotTransform.position.z);
        Vector3 end = new Vector3(pivotTransform.position.x, pivotTransform.position.y - playerController.radius - 0.01f + stanceCheckHeight, pivotTransform.position.z);
        return Physics.CheckCapsule(start, end, playerController.radius, playerMask);
    }

    private void InitializeInputActions() {
        inputActions.Player.Movement.performed += e => {
            inputMovement = e.ReadValue<Vector2>();

            if(wantsSprinting) {
                StartSprinting();
            }
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
        
        inputActions.Player.Sprinting.started += e => WantsSprinting();

        inputActions.Player.Sprinting.performed += e => StopSprinting();
    }
    private void InitializePlayerSettings() {

        newPlayerRotation = transform.localRotation.eulerAngles;
        newCameraRotation = cameraHolder.localRotation.eulerAngles;

        playerSettings.viewXSensitivity = 12;
        playerSettings.viewYSensitivity = 12;
        playerSettings.viewXInverted = false;
        playerSettings.viewYInverted = true;

        playerSettings.speedSprint = 10;
        playerSettings.speedStand = 6;
        playerSettings.speedCrouch = 4;
        playerSettings.speedProne = 2;

        playerSettings.viewClampYMin = -80;
        playerSettings.viewClampYMax = 80;

        playerSettings.jumpPower = 15;

        playerSettings.gravity = -10;
        playerSettings.gravityMultiplier = 5;

        playerSettings.stanceSmoothing = 12f;
        playerSettings.movementSmoothing = 0.3f;
        playerSettings.airTimeSmoothing = 0.05f;

        playerSettings.speedStrafeMultiplier = 0.7f;
        playerSettings.speedStrafeSprintMultiplier = 0.5f;

        playerSettings.doubleJumpMultiplier = 1.3f;

        isSprinting = false;
        wantsSprinting = false;
        SetPlayerStance(PlayerStance.Stand);

        if(currentWeapon) {
            currentWeapon.Initialize(this);
        }
    }

    private void StopSprinting() {
        isSprinting = false;
        wantsSprinting = false;
        SetPlayerSpeed(playerStance);
    }

    private void WantsSprinting() {
        wantsSprinting = true;
        StartSprinting();
    }

    private void StartSprinting() {
        if(isSprinting) {
            return;
        } else if ((!CanChangeStance(playerStanceStand.stanceCollider.height) || playerStance == PlayerStance.Stand) && inputMovement.y > 0) {
            wantsSprinting = false;
            isSprinting = true;
            SetPlayerStance(PlayerStance.Stand);
        }
    }
}
