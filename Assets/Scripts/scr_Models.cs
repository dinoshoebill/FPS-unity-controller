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
        public float speedSprint;
        public float speedStand;
        public float speedProne;
        public float speedCrouch;

        [Header("Movement Smoothing")]
        public float movementSmoothing;
        public float stanceSmoothing;
        public float airTimeSmoothing;

        [Header("Jump Settings")]
        public float jumpPower;

        [Header("Gravity Settings")]
        public float gravity;

        [Header("Multipliers")]
        public float speedStrafeMultiplier;
        public float speedStrafeMultiplierSprint;
        public float gravityMultiplier;
        public float doubleJumpMultiplier;
    }

    [Serializable]
    public class PlayerStanceCollider {
        public CapsuleCollider stanceCollider;
    }

    #endregion

    #region - Weapons -
    
    public class WeaponsModelSettings {

        [Header("Sway")]
        public float swayAmount;
        public float swaySmoothing;
    }
    
    #endregion

}
