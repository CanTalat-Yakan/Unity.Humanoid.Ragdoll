using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UnityEssentials
{
    using static RagdolHelper;

    public class RagdollGizmoDrawer : EditorWindow
    {
        public bool Render { get; set; }
        public Dictionary<string, Transform> SymmetricBones { get; private set; }

        private int _currentPointIndex;
        private GameObject _selectedGameObject;
        private Vector3 _forward;

        Transform[] _transforms;

        private PivotMode _lastPivotMode;
        private PivotRotation _lastPivotRotation;

        private int _currentSelectedMode;
        private int _lastSelectedMode;

        private Quaternion _lastRotation;
        private bool _buttonPressed;

        public bool Initialize(Animator animator, Vector3 forward)
        {
            _forward = forward;

            Tools.hidden = false;

            SymmetricBones = new()
            {
                // Feet
                { animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg).name, animator.GetBoneTransform(HumanBodyBones.RightLowerLeg) },
                { animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg).name, animator.GetBoneTransform(HumanBodyBones.RightUpperLeg) },
                { animator.GetBoneTransform(HumanBodyBones.RightLowerLeg).name, animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg) },
                { animator.GetBoneTransform(HumanBodyBones.RightUpperLeg).name, animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg) },

                // Hands
                { animator.GetBoneTransform(HumanBodyBones.LeftLowerArm).name, animator.GetBoneTransform(HumanBodyBones.RightLowerArm) },
                { animator.GetBoneTransform(HumanBodyBones.LeftUpperArm).name, animator.GetBoneTransform(HumanBodyBones.RightUpperArm) },
                { animator.GetBoneTransform(HumanBodyBones.RightLowerArm).name, animator.GetBoneTransform(HumanBodyBones.LeftLowerArm) },
                { animator.GetBoneTransform(HumanBodyBones.RightUpperArm).name, animator.GetBoneTransform(HumanBodyBones.LeftUpperArm) }
            };

            _selectedGameObject = animator.gameObject;
            _currentPointIndex = -1;

            return true;
        }

        public void ChangeMode(int index)
        {
            if (!Render)
                return;

            _currentSelectedMode = index;

            bool selectionChanged =
                _lastSelectedMode != _currentSelectedMode |
                _lastPivotMode != Tools.pivotMode |
                _lastPivotRotation != Tools.pivotRotation;

            if (selectionChanged)
            {
                Tools.hidden = _currentSelectedMode != -1;

                _lastSelectedMode = _currentSelectedMode;
                _lastPivotMode = Tools.pivotMode;
                _lastPivotRotation = Tools.pivotRotation;

                FindColliders();
            }
        }

        public void OnSceneGUI(SceneView sceneView)
        {
            if (!Render)
                return;

            if (!sceneView.drawGizmos)
                return;

            if (_currentSelectedMode == -1)
                DrawPlayerDirection();
            else if (_transforms?.Length > 0)
                DrawControlGizmos();
        }

        public void OnEnable() =>
            SceneView.duringSceneGui += OnSceneGUI;

        public void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            Tools.hidden = false;
        }

        public void OnSelectionChange()
        {
            _selectedGameObject = Selection.activeGameObject;

            if (_selectedGameObject == null)
                Render = false;

            if (_selectedGameObject.GetComponent<RagdollBuilder>().GetGizmoDrawer() != this)
                DestroyImmediate(this);

            // if it is selected as asset, skip it
            if (EditorUtility.IsPersistent(_selectedGameObject))
                Render = false;

            Render = true;
        }

        private void DrawPlayerDirection()
        {
            if (_selectedGameObject == null)
                return;

            Vector3 position = _selectedGameObject.transform.position + Vector3.up;
            float size = HandleUtility.GetHandleSize(position);

            Color backupColor = Handles.color;
            Handles.color = Color.yellow;
            Handles.ArrowHandleCap(1, position, Quaternion.LookRotation(_forward, Vector3.up), size, EventType.Repaint);
            Handles.color = backupColor;
        }

        private void DrawControlGizmos()
        {
            for (int i = 0; i < _transforms.Length; i++)
            {
                var transform = _transforms[i];

                if (transform == null)
                    continue;

                var position = Vector3.zero;

                // Edit Colliders
                if (_currentSelectedMode == 0)
                    position = ColliderGizmo.GetPosition(transform);
                // Edit Joints
                else if (_currentSelectedMode == 1)
                    position = JointGizmo.GetPosition(transform);
                // Edit Center of Mass
                else if (_currentSelectedMode == 2)
                    position = RigidbodyGizmo.GetPosition(transform);

                float size = HandleUtility.GetHandleSize(position) / 6f;

                if (Handles.Button(position, Quaternion.identity, size, size, Handles.SphereHandleCap))
                {
                    _currentPointIndex = i;

                    Quaternion rotatorRotation2 = GetRotatorRotation(transform);

                    if (!_buttonPressed)
                    {
                        _lastRotation = rotatorRotation2;
                        _buttonPressed = true;
                    }
                }
                else _buttonPressed = false;

                if (_currentPointIndex != i)
                    continue;

                // Edit Colliders
                if (_currentSelectedMode == 0)
                    ColliderGizmo.DrawControllers(this, _lastRotation, transform, position);
                // Edit Joints
                else if (_currentSelectedMode == 1)
                    JointGizmo.DrawControllers(this, transform);
                // Edit Center of Mass
                else if (_currentSelectedMode == 2)
                    RigidbodyGizmo.DrawControllers(transform);
            }
        }

        private void FindColliders()
        {
            if (_selectedGameObject == null)
            {
                _transforms = new Transform[0];
                return;
            }

            // Edit Colliders
            if (_currentSelectedMode == 0)
            {
                var colliders = _selectedGameObject.GetComponentsInChildren<Collider>();

                _transforms = new Transform[colliders.Length];
                for (int i = 0; i < colliders.Length; ++i)
                {
                    Transform transform = colliders[i].transform;
                    if (transform.name.EndsWith(ColliderRotatorNodeSufix, false, System.Globalization.CultureInfo.InvariantCulture))
                        transform = transform.parent;

                    _transforms[i] = transform;
                }
            }
            // Edit Joints
            else if (_currentSelectedMode == 1)
            {
                var joints = _selectedGameObject.GetComponentsInChildren<Joint>();

                _transforms = new Transform[joints.Length];
                for (int i = 0; i < joints.Length; ++i)
                {
                    Transform transform = joints[i].transform;
                    _transforms[i] = transform;
                }
            }
            // Edit Center of Mass
            else if (_currentSelectedMode == 2)
            {
                var rigids = _selectedGameObject.GetComponentsInChildren<Rigidbody>();

                _transforms = new Transform[rigids.Length];
                for (int i = 0; i < rigids.Length; ++i)
                {
                    Transform transform = rigids[i].transform;
                    _transforms[i] = transform;
                }
            }

            if (_transforms?.Length == 0)
            {
                Debug.LogError("No Colliders found in Ragdoll");
                Render = false;
            }
        }
    }
}