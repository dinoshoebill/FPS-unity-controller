using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class scr_Models {

    #region - Player -

    public enum PlayerStance {
        Stand,
        Crouch,
        Prone
    }

    [Serializable]
    public class PlayerSettingsModel {

        [Header("View Settings")]
        public float viewXSensitivity;
        public float viewYSensitivity;
        public bool viewXInverted;
        public bool viewYInverted;
        public float viewClampYMin;
        public float viewClampYMax;

        [Header("Movement Settings")]
        public float playerSpeedSprint;
        public float playerSpeedStand;
        public float playerSpeedProne;
        public float playerSpeedCrouch;

        [Header("Movement Smoothing")]
        public float playerMovementSmoothing;
        public float playerStanceSmoothing;
        public float playerAirTimeSmoothing;

        [Header("Jump Settings")]
        public float jumpPower;

        [Header("Gravity Settings")]
        public float gravity;

        [Header("Multipliers")]
        public float strafeSpeedMultiplier;
        public float strafeSpeedMultiplierSprint;
        public float gravityMultiplier;
        public float doubleJumpMultiplier;
    }

    [Serializable]
    public class PlayerStanceCollider {
        public CapsuleCollider playerStanceCollider;
    }

    #endregion

}
