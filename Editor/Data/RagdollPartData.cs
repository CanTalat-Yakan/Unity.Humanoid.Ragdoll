using UnityEngine;

namespace UnityEssentials
{
    public abstract class RagdollPartBase
    {
        public readonly Transform Transform;
        public Rigidbody Rigidbody;
        public ConfigurableJoint Joint;

        protected RagdollPartBase(Transform transform) =>
            Transform = transform;
    }

    public sealed class RagdollPartBox : RagdollPartBase
    {
        public BoxCollider Collider;

        public RagdollPartBox(Transform transform) : base(transform) { }
    }

    public sealed class RagdollPartCapsule : RagdollPartBase
    {
        public CapsuleCollider Collider;

        public RagdollPartCapsule(Transform transform) : base(transform) { }
    }

    public sealed class RagdollPartSphere : RagdollPartBase
    {
        public SphereCollider Collider;

        public RagdollPartSphere(Transform transform) : base(transform) { }
    }
}
