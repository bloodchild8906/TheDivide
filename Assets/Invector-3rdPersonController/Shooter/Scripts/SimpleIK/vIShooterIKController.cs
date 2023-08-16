using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Invector
{
    using IK;
    using vShooter;
    namespace PlayerController
    {
        public interface vILockCamera
        {
            bool LockCamera { get; set; }
        }
    }
    /// <summary>
    /// Event used to the IK Update. 
    /// </summary>
    public delegate void IKUpdateEvent();

    public partial interface vIShooterIKController
    {
        GameObject gameObject { get; }
        /// <summary>
        /// IK Solver used to the Left Arm
        /// </summary>
        vIKSolver LeftIK { get; }
        /// <summary>
        /// IK Solver used to the Right Arm
        /// </summary>
        vIKSolver RightIK { get; }
        /// <summary>
        /// List with all adjusts
        /// </summary>
        vWeaponIKAdjustList WeaponIKAdjustList { get; set; }
        /// <summary>
        /// Current IK with adjusts for current weapon
        /// </summary>
        vWeaponIKAdjust CurrentWeaponIK { get; }
        /// <summary>
        /// Current adjust based on <seealso cref="CurrentIKAdjustState"/>
        /// </summary>
        IKAdjust CurrentIKAdjust { get; }

        /// <summary>
        /// Update <seealso cref="CurrentWeaponIK"/>  for Weapon Category
        /// </summary>     
        void UpdateWeaponIK();

        /// <summary>
        /// Set Lock aiming state
        /// </summary>
        bool LockAiming { get; set; }

        /// <summary>
        /// Set Lock Hipfire aiming state
        /// </summary>
        bool LockHipFireAiming { get; set; }

        /// <summary>
        /// Current active weapon
        /// </summary>
        vShooterWeapon CurrentActiveWeapon { get; }

        /// <summary>
        /// Used to Edit Global IK Offsets in Editor 
        /// </summary>
        bool EditingIKGlobalOffset {get;set;}
        /// <summary>
        /// Check if Controller is Aiming
        /// </summary>
        bool IsAiming { get; }

        /// <summary>
        /// Check if Controller is Crouching
        /// </summary>
        bool IsCrouching { get; set; }

        /// <summary>
        /// Check if <see cref="CurrentActiveWeapon"/> a Left weapon
        /// </summary>
        bool IsLeftWeapon { get; }

        /// <summary>
        /// Aim Position
        /// </summary>
        Vector3 AimPosition { get;}
        /// <summary>
        /// Current IK adjust state
        /// </summary>
        string CurrentIKAdjustState { get;}

        /// <summary>
        /// Current IK adjust state including weapon Category separeted by @ "Category@State"
        /// </summary>
        string CurrentIKAdjustStateWithTag { get; }

        /// <summary>
        /// Check if Is using a custom ik adjust 
        /// </summary>
        bool IsUsingCustomIKAdjust{ get; }

        /// <summary>
        /// Check if can use IK
        /// </summary>
        bool IsIgnoreIK { get; }

        /// <summary>
        /// Check if Support hand ik is enabled
        /// </summary>
        bool IsSupportHandIKEnabled { get; }
        /// <summary>
        /// Current custom IK adjust State
        /// </summary>
        string CustomIKAdjustState { get; }
        /// <summary>
        /// Set a custom IK state
        /// </summary>
        /// <param name="value">IK state name</param>
        void SetCustomIKAdjustState(string value);
        /// <summary>
        /// Reset custom IK to use Default ik state
        /// </summary>
        void ResetCustomIKAdjustState();
        /// <summary>
        /// Event called when IK is Started
        /// </summary>
        event IKUpdateEvent onStartUpdateIK;
        /// <summary>
        /// Event called when IK Update is Finished
        /// </summary>
        event IKUpdateEvent onFinishUpdateIK;
    }
   
}