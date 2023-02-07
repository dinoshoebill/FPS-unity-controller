using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static scr_Models;

public class scr_WeaponController : MonoBehaviour {

    private scr_PlayerController playerControllerScript;

    [Header("References")]
    public Animator weaponAnimator;
    public Transform weaponSwayObject;

    [Header("Weapon Settings")]
    public WeaponsSettingsModel weaponSettings;

    public bool isInitialized;

    Vector3 newWeaponRotation;
    Vector3 newWeaponRotationVelocity;

    Vector3 targetWeaponRotation;
    Vector3 targetWeaponRotationVelocity;

    Vector3 newWeaponMovementRotation;
    Vector3 newWeaponMovementRotationVelocity;

    Vector3 targetWeaponMovementRotation;
    Vector3 targetWeaponMovementRotationVelocity;

    public Vector3 swayPosition;
    public float swayTime;

    public void Initialize(scr_PlayerController playerControllerScript) {
        this.playerControllerScript = playerControllerScript;
        isInitialized = true;
    }

    private void Start() {
        newWeaponRotation = transform.localRotation.eulerAngles;

        InitializeWeaponSettings();
    }

    private void Update() {

        if(!isInitialized) {
            return;
        }

        CalculateWeaponRotation();
        CalculateWeaponSway();
    }

    private void CalculateWeaponRotation() {
        weaponAnimator.speed = playerControllerScript.weaponAnimationSpeed * weaponSettings.animationSpeedMultiplier;

        targetWeaponRotation.y += weaponSettings.swayAmount * (weaponSettings.swayXInverted ? -playerControllerScript.inputView.x : playerControllerScript.inputView.x) * Time.deltaTime;
        targetWeaponRotation.x += weaponSettings.swayAmount * (weaponSettings.swayYInverted ? -playerControllerScript.inputView.y : playerControllerScript.inputView.y) * Time.deltaTime;

        targetWeaponMovementRotation.z += weaponSettings.movementSwayAmount * (weaponSettings.swayXInverted ? -playerControllerScript.inputMovement.x : playerControllerScript.inputMovement.x) * Time.deltaTime;
        targetWeaponMovementRotation.x += weaponSettings.movementSwayAmount * (weaponSettings.swayYInverted ? -playerControllerScript.inputMovement.y : playerControllerScript.inputMovement.y) * Time.deltaTime;

        targetWeaponRotation = Vector3.SmoothDamp(targetWeaponRotation, Vector3.zero, ref targetWeaponRotationVelocity, weaponSettings.swayResetSmoothing);
        newWeaponRotation = Vector3.SmoothDamp(newWeaponRotation, targetWeaponRotation, ref newWeaponRotationVelocity, weaponSettings.swaySmoothing);

        targetWeaponMovementRotation = Vector3.SmoothDamp(targetWeaponMovementRotation, Vector3.zero, ref targetWeaponMovementRotationVelocity, weaponSettings.swayResetSmoothing);
        newWeaponMovementRotation = Vector3.SmoothDamp(newWeaponMovementRotation, targetWeaponMovementRotation, ref newWeaponMovementRotationVelocity, weaponSettings.swaySmoothing);

        Vector3 combinedWeaponRotation = newWeaponRotation + newWeaponMovementRotation;
        combinedWeaponRotation.x = Mathf.Clamp(combinedWeaponRotation.x, -weaponSettings.swayClampX, weaponSettings.swayClampX);
        combinedWeaponRotation.y = Mathf.Clamp(combinedWeaponRotation.y, -weaponSettings.swayClampY, weaponSettings.swayClampY);
        combinedWeaponRotation.z = Mathf.Clamp(combinedWeaponRotation.z, -weaponSettings.swayClampZ, weaponSettings.swayClampZ);
        transform.localRotation = Quaternion.Euler(newWeaponRotation + newWeaponMovementRotation);
    }

    public void SetWeaponAnimation() {
        weaponAnimator.SetBool("isSprinting", playerControllerScript.isSprinting);
    }

    private void InitializeWeaponSettings() {

        weaponSettings.swayAmount = 1;
        weaponSettings.movementSwayAmount = 5;

        weaponSettings.swayClampX = 3;
        weaponSettings.swayClampY = 3;
        weaponSettings.swayClampZ = 3;

        weaponSettings.swaySmoothing = 0.1f;
        weaponSettings.swayResetSmoothing = 0.1f;

        weaponSettings.swayXInverted = false;
        weaponSettings.swayXInverted = true;

        weaponSettings.animationSpeedMultiplier = 0.5f;

        weaponSettings.weaponSprintResetSmoothing = 0.3f;

        weaponSettings.swayScale = 400;
        weaponSettings.swayAmountA = 1;
        weaponSettings.swayAmountB = 2;
        weaponSettings.swayLerpSpeed = 1;

        swayTime = 0;
    }

    private void CalculateWeaponSway() {
        Vector3 targetPosition = LissajousCurve(swayTime, weaponSettings.swayAmountA, weaponSettings.swayAmountB) / weaponSettings.swayScale;
        swayPosition = Vector3.Lerp(swayPosition, targetPosition, Time.smoothDeltaTime * weaponSettings.swayLerpSpeed);

        swayTime += Time.deltaTime;
        
        if(swayTime > 6.5f) {
            swayTime = 0;
        }


        weaponSwayObject.localPosition = swayPosition;
    }

    private Vector3 LissajousCurve(float Time, float A, float B) {
        return new Vector3(Mathf.Sin(Time), A * Mathf.Sin(B * Time * Mathf.PI));
    }
}
