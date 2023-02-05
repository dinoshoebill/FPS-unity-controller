using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static scr_Models;

public class scr_WeaponController : MonoBehaviour {

    private scr_PlayerController playerControllerScript;

    [Header("Weapon Settings")]
    public WeaponsSettingsModel weaponSettings;

    public bool isInitialized;
    Vector3 newWeaponRotation;
    Vector3 newWeaponRotationVelocity;

    Vector3 targetWeaponRotation;
    Vector3 targetWeaponRotationVelocity;

    public void Initialize(scr_PlayerController playerControllerScript) {
        this.playerControllerScript = playerControllerScript;
        isInitialized = true;
    }

    private void Start() {
        newWeaponRotation = transform.localRotation.eulerAngles;
    }

    private void Update() {
        if(!isInitialized) {
            return;
        }

        targetWeaponRotation.y += weaponSettings.swayAmount * (weaponSettings.swayXInverted ? -playerControllerScript.inputView.x : playerControllerScript.inputView.x) * Time.deltaTime;
        targetWeaponRotation.x += weaponSettings.swayAmount * (weaponSettings.swayYInverted ? -playerControllerScript.inputView.y : playerControllerScript.inputView.y) * Time.deltaTime;
        
        targetWeaponRotation.x = Mathf.Clamp(targetWeaponRotation.x, -weaponSettings.swayClampX, weaponSettings.swayClampX);
        targetWeaponRotation.y = Mathf.Clamp(targetWeaponRotation.y, -weaponSettings.swayClampY, weaponSettings.swayClampY);

        targetWeaponRotation = Vector3.SmoothDamp(targetWeaponRotation, Vector3.zero, ref targetWeaponRotationVelocity, weaponSettings.swayResetSmoothing);

        newWeaponRotation = Vector3.SmoothDamp(newWeaponRotation, targetWeaponRotation, ref newWeaponRotationVelocity, weaponSettings.swaySmoothing);

        transform.localRotation = Quaternion.Euler(newWeaponRotation);
    }

}
