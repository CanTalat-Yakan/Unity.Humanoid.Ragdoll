using System;
using UnityEngine;

namespace UnityEssentials
{
    [Serializable]
    public class RagdollData
    {
        public int TotalWeight = 60;
        [LabelOverride("Create Hands & Feet")]
        public bool CreateTips = true;

        [Foldout("Advanced")]
        public bool UseGravity = true;

        [Space]
        public bool AsTrigger = false;
        public bool IsKinematic = false;

        [Space]
        public float RigidbodyDrag = 1;
        public float RigidbodyAngularDrag = 25;

        [Space]
        [Tooltip("Configure drives for more natural movement")]
        public AngularDriveData AngularDrive = new();

        [Space]
        public CollisionDetectionMode CollisionDetectionMode;
    }

    [Serializable]
    public class AngularDriveData
    {
        public float PositionSpring = 1000; // Increase for more stability
        public float PositionDamper = 100;
        public float MaximumForce = 1000; 
        public bool UseAcceleration = true;
    }
}