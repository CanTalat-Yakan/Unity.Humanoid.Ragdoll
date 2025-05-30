using System;
using UnityEngine;

namespace UnityEssentials
{
    using static RagdolHelper;

    public class RagdollColliderConfigurator
    {
        public void ConfigurePelvis(RagdollPartBox pelvis)
        {
            Vector3 pelvisSize = new(0.22f, 0.21f, 0.1f);
            Vector3 pelvisCenter = new(0f, 0.06f, -0.01f);

            pelvis.Collider.size = Abs(pelvis.Transform.InverseTransformVector(pelvisSize));
            pelvis.Collider.center = pelvis.Transform.InverseTransformVector(pelvisCenter);
        }

        public void ConfigureChest(RagdollPartBox chest, RagdollPartBox pelvis)
        {
            Vector3 chestSize = new(0.3f, 0.3f, 0.15f);
            Vector3 chestCenter = new(0f, 0f, -0.01f);
            Vector3 pelvisSize = new(0.22f, 0.21f, 0.2f);
            Vector3 pelvisCenter = new(0f, 0.06f, -0.01f);

            float y = (pelvisSize.y + chestSize.y) / 2f + pelvisCenter.y;
            y -= chest.Transform.position.y - pelvis.Transform.position.y;

            chest.Collider.size = Abs(chest.Transform.InverseTransformVector(chestSize));
            chest.Collider.center = chest.Transform.InverseTransformVector(chestCenter + Vector3.up * y);
        }

        public void ConfigureHead(RagdollPartSphere head)
        {
            Vector3 headCenter = new(0f, 0.06f, 0.02f);

            float headScale = 3f / (head.Transform.lossyScale.x + head.Transform.lossyScale.y + head.Transform.lossyScale.z);

            head.Collider.radius = 0.1f * headScale;
            head.Collider.center = head.Transform.InverseTransformVector(headCenter);
        }

        public void ConfigureLimb(RagdollPartCapsule limbUpper, RagdollPartCapsule limbLower, RagdollPartBox tip, bool createTips)
        {
            float totalLength = limbUpper.Transform.InverseTransformPoint(tip.Transform.position).magnitude;

            // Upper Limb
            var boneEndPosition = limbUpper.Transform.InverseTransformPoint(limbLower.Transform.position);

            limbUpper.Collider.direction = GetXYZDirectionIndex(limbLower.Transform.localPosition);
            limbUpper.Collider.radius = totalLength * 0.11f;
            limbUpper.Collider.height = boneEndPosition.magnitude;
            limbUpper.Collider.center = Vector3.Scale(boneEndPosition, Vector3.one * 0.5f);

            // Lower Limb
            boneEndPosition = limbLower.Transform.InverseTransformPoint(tip.Transform.position);

            limbLower.Collider.direction = GetXYZDirectionIndex(boneEndPosition);
            limbLower.Collider.radius = totalLength * 0.075f;
            limbLower.Collider.height = boneEndPosition.magnitude;
            limbLower.Collider.center = Vector3.Scale(boneEndPosition, Vector3.one * 0.5f);

            // Tip
            if (createTips)
            {
                boneEndPosition = GetLongestTransform(tip.Transform).position;
                boneEndPosition = tip.Transform.InverseTransformPoint(boneEndPosition);

                Vector3 tipDirection = GetXYZDirectionVector(boneEndPosition);
                Vector3 tipSides = (tipDirection - Vector3.one) * -1;
                Vector3 boxSize = tipDirection * boneEndPosition.magnitude * 0.75f + tipSides * totalLength * 0.125f;

                tip.Collider.size = boxSize;
                float halfTipLength = boneEndPosition.magnitude / 2f;
                tip.Collider.center = Vector3.Scale(boneEndPosition.normalized, Vector3.one * halfTipLength);
            }
        }
    }

    public class RagdollJointConfigurator
    {
        public AngularDriveData AngularDriveData;

        public void ConfigureChestJoint(RagdollPartBox chest, Rigidbody connectedBody, Transform rootNode)
        {
            ConfigureJointParams(chest, connectedBody, rootNode.right, rootNode.forward);
            ConfigureJointLimits(chest.Joint, -45f, 20f, 20f, 20f);
        }

        public void ConfigureHeadJoint(RagdollPartSphere head, Rigidbody connectedBody, Transform rootNode)
        {
            ConfigureJointParams(head, connectedBody, rootNode.right, rootNode.forward);
            ConfigureJointLimits(head.Joint, -45f, 20f, 20f, 20f);
        }

        public void ConfigureLegJoints(RagdollPartCapsule hip, RagdollPartCapsule knee, RagdollPartBox foot,
            Rigidbody pelvisBody, Transform rootNode, bool createTips)
        {
            ConfigureJointParams(hip, pelvisBody, rootNode.right, rootNode.forward);
            ConfigureJointParams(knee, hip.Rigidbody, rootNode.right, rootNode.forward);

            ConfigureJointLimits(hip.Joint, -10f, 120f, 90f, 20f);
            ConfigureJointLimits(knee.Joint, -120f, 0f, 10f, 20f);

            if (createTips)
            {
                ConfigureJointParams(foot, knee.Rigidbody, rootNode.right, rootNode.forward);
                ConfigureJointLimits(foot.Joint, -70f, 70f, 45f, 20f);
            }
        }

        public void ConfigureArmJoints(RagdollPartCapsule arm, RagdollPartCapsule elbow, RagdollPartBox hand,
            Rigidbody chestBody, Vector3 playerDirection, bool leftSide, bool createTips)
        {
            var directionUpper = elbow.Transform.position - arm.Transform.position;
            var directionLower = hand.Transform.position - elbow.Transform.position;
            var directionHand = GetLongestTransform(hand.Transform).position - hand.Transform.position;

            if (leftSide)
            {
                ConfigureJointLimits(arm.Joint, -100f, 30f, 100f, 45f);
                ConfigureJointLimits(elbow.Joint, -120f, 0f, 10f, 90f);

                if (createTips) 
                    ConfigureJointLimits(hand.Joint, -90f, 90f, 90f, 45f);

                directionUpper = -directionUpper;
                directionLower = -directionLower;
                directionHand = -directionHand;
            }
            else
            {
                ConfigureJointLimits(arm.Joint, -30f, 100f, 100f, 45f);
                ConfigureJointLimits(elbow.Joint, 0f, 120f, 10f, 90f);

                if (createTips) 
                    ConfigureJointLimits(hand.Joint, -90f, 90f, 90f, 45f);
            }

            var crossDirectionUpper = Vector3.Cross(playerDirection, directionUpper);
            var crossDirectionLower = Vector3.Cross(playerDirection, directionLower);
            var crossDirectionHand = Vector3.Cross(playerDirection, directionHand);

            ConfigureJointParams(arm, chestBody, playerDirection, crossDirectionUpper);
            ConfigureJointParams(elbow, arm.Rigidbody, playerDirection, crossDirectionLower);

            if (createTips) 
                ConfigureJointParams(hand, elbow.Rigidbody, playerDirection, crossDirectionHand);
        }

        private void ConfigureJointParams(RagdollPartBase part, Rigidbody connectedBody, Vector3 primaryAxis, Vector3 secondaryAxis)
        {
            var joint = part.Joint;
            joint.enablePreprocessing = true;
            joint.autoConfigureConnectedAnchor = true;
            joint.connectedBody = connectedBody;
            joint.anchor = Vector3.zero;
            joint.connectedAnchor = Vector3.zero;

            joint.axis = primaryAxis;
            joint.secondaryAxis = secondaryAxis;

            joint.axis = part.Transform.InverseTransformDirection(primaryAxis);
            joint.secondaryAxis = part.Transform.InverseTransformDirection(secondaryAxis);

            // Lock all motions except angular
            joint.xMotion = ConfigurableJointMotion.Locked;
            joint.yMotion = ConfigurableJointMotion.Locked;
            joint.zMotion = ConfigurableJointMotion.Locked;

            joint.angularXMotion = ConfigurableJointMotion.Limited;
            joint.angularYMotion = ConfigurableJointMotion.Limited;
            joint.angularZMotion = ConfigurableJointMotion.Limited;

            JointDrive angularDrive = new()
            {
                positionSpring = AngularDriveData.PositionSpring,
                positionDamper = AngularDriveData.PositionDamper,
                maximumForce = AngularDriveData.MaximumForce,
                useAcceleration = AngularDriveData.UseAcceleration
            };

            joint.angularXDrive = angularDrive;
            joint.angularYZDrive = angularDrive;
            joint.rotationDriveMode = RotationDriveMode.Slerp;
        }

        private void ConfigureJointLimits(ConfigurableJoint joint, float lowTwist, float highTwist, float swingY, float swingZ)
        {
            if (lowTwist > highTwist)
                throw new ArgumentException("Wrong Limitation: LowTwist > HighTwist");

            SoftJointLimit lowX = joint.lowAngularXLimit;
            lowX.limit = lowTwist;
            joint.lowAngularXLimit = lowX;

            SoftJointLimit highX = joint.highAngularXLimit;
            highX.limit = highTwist;
            joint.highAngularXLimit = highX;

            SoftJointLimit yLimit = joint.angularYLimit;
            yLimit.limit = swingY;
            joint.angularYLimit = yLimit;

            SoftJointLimit zLimit = joint.angularZLimit;
            zLimit.limit = swingZ;
            joint.angularZLimit = zLimit;
        }
    }
}