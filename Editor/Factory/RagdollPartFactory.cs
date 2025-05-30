using UnityEngine;

namespace UnityEssentials
{
    public class RagdollPartFactory
    {
        public void CreateRagdollParts(Animator animator,
            out RagdollPartBox pelvis, out RagdollPartCapsule leftHip, out RagdollPartCapsule leftKnee,
            out RagdollPartCapsule rightHip, out RagdollPartCapsule rightKnee, out RagdollPartCapsule leftArm,
            out RagdollPartCapsule leftElbow, out RagdollPartCapsule rightArm, out RagdollPartCapsule rightElbow,
            out RagdollPartBox chest, out RagdollPartSphere head, out RagdollPartBox leftFoot,
            out RagdollPartBox rightFoot, out RagdollPartBox leftHand, out RagdollPartBox rightHand)
        {
            pelvis = new(animator.GetBoneTransform(HumanBodyBones.Hips));
            leftHip = new(animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg));
            leftKnee = new(animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg));
            rightHip = new(animator.GetBoneTransform(HumanBodyBones.RightUpperLeg));
            rightKnee = new(animator.GetBoneTransform(HumanBodyBones.RightLowerLeg));
            leftArm = new(animator.GetBoneTransform(HumanBodyBones.LeftUpperArm));
            leftElbow = new(animator.GetBoneTransform(HumanBodyBones.LeftLowerArm));
            rightArm = new(animator.GetBoneTransform(HumanBodyBones.RightUpperArm));
            rightElbow = new(animator.GetBoneTransform(HumanBodyBones.RightLowerArm));
            chest = new(animator.GetBoneTransform(HumanBodyBones.Chest));
            head = new(animator.GetBoneTransform(HumanBodyBones.Head));
            leftFoot = new(animator.GetBoneTransform(HumanBodyBones.LeftFoot));
            rightFoot = new(animator.GetBoneTransform(HumanBodyBones.RightFoot));
            leftHand = new(animator.GetBoneTransform(HumanBodyBones.LeftHand));
            rightHand = new(animator.GetBoneTransform(HumanBodyBones.RightHand));

            if (chest.Transform == null)
                chest = new(animator.GetBoneTransform(HumanBodyBones.Spine));
        }

        public void AddComponentsTo(RagdollPartBox part, RagdollData properties, float mass, bool addJoint)
        {
            AddComponentsToBase(part, properties, mass, addJoint);

            part.Collider = part.Transform.gameObject.GetOrAddComponent<BoxCollider>();
            part.Collider.isTrigger = properties.AsTrigger;
        }

        public void AddComponentsTo(RagdollPartCapsule part, RagdollData properties, float mass, bool addJoint)
        {
            AddComponentsToBase(part, properties, mass, addJoint);

            part.Collider = part.Transform.gameObject.GetOrAddComponent<CapsuleCollider>();
            part.Collider.isTrigger = properties.AsTrigger;
        }

        public void AddComponentsTo(RagdollPartSphere part, RagdollData properties, float mass, bool addJoint)
        {
            AddComponentsToBase(part, properties, mass, addJoint);

            part.Collider = part.Transform.gameObject.GetOrAddComponent<SphereCollider>();
            part.Collider.isTrigger = properties.AsTrigger;
        }

        private void AddComponentsToBase(RagdollPartBase part, RagdollData properties, float mass, bool addJoint)
        {
            var gameObject = part.Transform.gameObject;

            part.Rigidbody = gameObject.GetOrAddComponent<Rigidbody>();
            part.Rigidbody.mass = mass;
            part.Rigidbody.linearDamping = properties.RigidbodyDrag;
            part.Rigidbody.angularDamping = properties.RigidbodyAngularDrag;
            part.Rigidbody.collisionDetectionMode = properties.CollisionDetectionMode;
            part.Rigidbody.isKinematic = properties.IsKinematic;
            part.Rigidbody.useGravity = properties.UseGravity;

            if (addJoint)
                part.Joint = gameObject.GetOrAddComponent<ConfigurableJoint>();
        }
    }
}