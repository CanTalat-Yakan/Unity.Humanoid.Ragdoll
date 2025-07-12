#if UNITY_EDITOR
using UnityEngine;

namespace UnityEssentials
{
    public sealed class RagdollFactory
    {
        private const string _colliderNodeSufix = "_ColliderRotator";

        private readonly RagdollPartFactory _partFactory = new();
        private readonly RagdollJointConfigurator _jointConfigurator = new();
        private readonly RagdollColliderConfigurator _colliderConfigurator = new();

        private readonly bool _readyToGenerate;
        private readonly Transform _rootNode;
        private readonly Vector3 _forward;

        // Ragdoll parts
        private readonly RagdollPartBox _pelvis;
        private readonly RagdollPartCapsule _leftHip, _leftKnee, _rightHip, _rightKnee;
        private readonly RagdollPartCapsule _leftArm, _leftElbow, _rightArm, _rightElbow;
        private readonly RagdollPartBox _chest;
        private readonly RagdollPartSphere _head;
        private readonly RagdollPartBox _leftFoot, _rightFoot, _leftHand, _rightHand;

        public RagdollFactory(Animator _animator, Transform player)
        {
            _readyToGenerate = false;

            if (_animator == null) 
                return;

            _rootNode = player.transform;
            _partFactory.CreateRagdollParts(_animator,
                out _pelvis, out _leftHip, out _leftKnee,
                out _rightHip, out _rightKnee, out _leftArm, out _leftElbow, out _rightArm,
                out _rightElbow, out _chest, out _head, out _leftFoot, out _rightFoot,
                out _leftHand, out _rightHand);

            if (!CheckFields())
            {
                Debug.LogError("Not all nodes were found!");
                return;
            }

            _forward = GetForwardDirection();

            _readyToGenerate = true;
        }

        public Vector3 GetForwardDirection()
        {
            Vector3 leftKnee = _leftKnee.Transform.position - _pelvis.Transform.position;
            Vector3 rightKnee = _rightKnee.Transform.position - _pelvis.Transform.position;

            return Vector3.Cross(leftKnee, rightKnee).normalized;
        }

        public void Update(RagdollData data) => 
            Apply(data);

        public void Apply(RagdollData data)
        {
            if (!_readyToGenerate)
            {
                Debug.LogError("Initialization failed. Reinstance object!");
                return;
            }

            RagdollWeightCalculator weight = new(data.TotalWeight, data.CreateTips);
            bool alreadyRagdolled = _pelvis.Transform.gameObject.GetComponent<Rigidbody>() != null;

            if (alreadyRagdolled) 
                return;

            // Setup all parts
            SetupParts(data, weight, alreadyRagdolled);


            // Configure Colliders
            _colliderConfigurator.ConfigurePelvis(_pelvis);
            _colliderConfigurator.ConfigureChest(_chest, _pelvis);
            _colliderConfigurator.ConfigureHead(_head);

            // Configure Joints
            _jointConfigurator.AngularDriveData = data.AngularDrive;
            _jointConfigurator.ConfigureChestJoint(_chest, _pelvis.Rigidbody, _rootNode);
            _jointConfigurator.ConfigureHeadJoint(_head, _chest.Rigidbody, _rootNode);

            // Configure Limbs
            ConfigureLimbs(true, data.CreateTips);
            ConfigureLimbs(false, data.CreateTips);
        }

        private void SetupParts(RagdollData properties, RagdollWeightCalculator weight, bool alreadyRagdolled)
        {
            _partFactory.AddComponentsTo(_pelvis, properties, weight.Pelvis, false);
            _partFactory.AddComponentsTo(_leftHip, properties, weight.Hip, true);
            _partFactory.AddComponentsTo(_leftKnee, properties, weight.Knee, true);
            _partFactory.AddComponentsTo(_rightHip, properties, weight.Hip, true);
            _partFactory.AddComponentsTo(_rightKnee, properties, weight.Knee, true);
            _partFactory.AddComponentsTo(_leftArm, properties, weight.Arm, true);
            _partFactory.AddComponentsTo(_leftElbow, properties, weight.Elbow, true);
            _partFactory.AddComponentsTo(_rightArm, properties, weight.Arm, true);
            _partFactory.AddComponentsTo(_rightElbow, properties, weight.Elbow, true);
            _partFactory.AddComponentsTo(_chest, properties, weight.Chest, true);
            _partFactory.AddComponentsTo(_head, properties, weight.Head, true);

            if (properties.CreateTips)
            {
                _partFactory.AddComponentsTo(_leftFoot, properties, weight.Foot, true);
                _partFactory.AddComponentsTo(_rightFoot, properties, weight.Foot, true);
                _partFactory.AddComponentsTo(_leftHand, properties, weight.Hand, true);
                _partFactory.AddComponentsTo(_rightHand, properties, weight.Hand, true);
            }
        }

        private void ConfigureLimbs(bool leftSide, bool createTips)
        {
            var hip = leftSide ? _leftHip : _rightHip;
            var knee = leftSide ? _leftKnee : _rightKnee;
            var foot = leftSide ? _leftFoot : _rightFoot;
            var arm = leftSide ? _leftArm : _rightArm;
            var elbow = leftSide ? _leftElbow : _rightElbow;
            var hand = leftSide ? _leftHand : _rightHand;

            _colliderConfigurator.ConfigureLimb(hip, knee, foot, createTips);
            _jointConfigurator.ConfigureLegJoints(hip, knee, foot, _pelvis.Rigidbody, _rootNode, createTips);

            _colliderConfigurator.ConfigureLimb(arm, elbow, hand, createTips);
            _jointConfigurator.ConfigureArmJoints(arm, elbow, hand, _chest.Rigidbody, _forward, leftSide, createTips);
        }

        bool CheckFields()
        {
            if (_rootNode == null |
                _pelvis == null |
                _leftHip == null |
                _leftKnee == null |
                _rightHip == null |
                _rightKnee == null |
                _leftArm == null |
                _leftElbow == null |
                _rightArm == null |
                _rightElbow == null |
                _chest == null |
                _head == null)
                return false;

            return true;
        }

        public void Clear()
        {
            foreach (var component in _pelvis.Transform.GetComponentsInChildren<Collider>())
                GameObject.DestroyImmediate(component);
            foreach (var component in _pelvis.Transform.GetComponentsInChildren<ConfigurableJoint>())
                GameObject.DestroyImmediate(component);
            foreach (var component in _pelvis.Transform.GetComponentsInChildren<Rigidbody>())
                GameObject.DestroyImmediate(component);

            DeleteColliderNodes(_pelvis.Transform);
        }

        private static void DeleteColliderNodes(Transform node)
        {
            for (int i = 0; i < node.childCount; ++i)
            {
                Transform child = node.GetChild(i);

                if (child.name.EndsWith(_colliderNodeSufix))
                    GameObject.DestroyImmediate(child.gameObject);
                else
                    DeleteColliderNodes(child);
            }
        }
    }
}
#endif