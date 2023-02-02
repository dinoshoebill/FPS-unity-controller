using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class scr_Models{

    #region - Player -

    public enum PlayerStance {
        Sprint,
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

        [Header("Jump Settings")]
        public float jumpPower;

        [Header("Jump Settings")]
        public float gravity;
        public float gravityMultiplier;
    }

    [Serializable]
    public class PlayerStanceCollider {
        public CapsuleCollider playerStanceCollider;
    }

    #endregion

}
