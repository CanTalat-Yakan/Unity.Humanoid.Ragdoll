using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEssentials
{
    [ExecuteAlways]
    public class RagdollBuilder : MonoBehaviour
    {
        [ToggleButton(IconNames.EditCollider, "Edit")]
        [SerializeField] private bool _editCollider = false;
        [ToggleButton(IconNames.JointAngularLimits, "Edit")]
        [SerializeField] private bool _editJoints = false;
        [ToggleButton(IconNames.AvatarInspectorDotFill, "Edit")]
        [SerializeField] private bool _editCenterOfMass = false;

        [SerializeField] private Avatar _avatar;

        [LabelOverride("Settings")]
        [SerializeField] private RagdollData _data;

        private RagdollFactory _factory;
        private RagdollGizmoDrawer _drawer;

        [SerializeField] private bool _initializedRagdoll;
        private int _currentSelectedMode;

        public void Awake() =>
            StopEditing();

#if UNITY_EDITOR
        public void OnEnable() =>
            Selection.selectionChanged += OnSelectionChanged;

        public void OnDisable() =>
            Selection.selectionChanged -= OnSelectionChanged;

        private void OnSelectionChanged()
        {
            if (Selection.activeGameObject == gameObject) 
                InitializeRagdoll();
            else StopEditing();
        }
#endif

        public void StopEditing()
        {
            _editCollider = false;
            _editJoints = false;
            _editCenterOfMass = false;

            _currentSelectedMode = -1;

            if (_drawer == null)
                return;

            _drawer.ChangeMode(_currentSelectedMode);

            if (Selection.activeGameObject == null)
                _drawer.Render = false;
        }

        [OnValueChanged("_data")]
        public void OnDataValueChanged() =>
            _factory?.Update(_data);

        [OnValueChanged("_editCollider", "_editJoints", "_editCenterOfMass")]
        public void OnEditValueChanged()
        {
            if (!_initializedRagdoll)
            {
                _editCollider = false;
                _editJoints = false;
                _editCenterOfMass = false;
                _currentSelectedMode = -1;

                _drawer?.ChangeMode(_currentSelectedMode);
            }

            if (_editCollider && _currentSelectedMode != 0)
            {
                _currentSelectedMode = 0;
                _editJoints = false;
                _editCenterOfMass = false;
            }
            else if (_editJoints && _currentSelectedMode != 1)
            {
                _editCollider = false;
                _currentSelectedMode = 1;
                _editCenterOfMass = false;

            }
            else if (_editCenterOfMass && _currentSelectedMode != 2)
            {
                _editCollider = false;
                _editJoints = false;
                _currentSelectedMode = 2;
            }
            else _currentSelectedMode = -1;

            _drawer?.ChangeMode(_currentSelectedMode);
        }

        [If("_initializedRagdoll", true)]
        [Button]
        public void RemoveRagdoll()
        {
            GetAnimator(out var animator);

            _factory ??= new(animator, transform);
            _factory.Clear();

            StopEditing();

            _drawer.Render = false;
            _initializedRagdoll = false;

            DestroyImmediate(animator);
        }

        [IfNot("_initializedRagdoll", true)]
        [Button("Create Ragdoll")]
        public void InitializeRagdoll()
        {
            GetAnimator(out var animator);

            if (!animator.isHuman)
                Debug.LogError("Humanoid required");

            _factory ??= new(animator, transform);
            _factory.Apply(_data);

            _drawer ??= new();
            if (!_drawer.Initialize(animator, _factory.GetForwardDirection()))
                Debug.LogError("Gizmo Drawer initialization failed");

            StopEditing();

            _drawer.Render = true;
            _initializedRagdoll = true;

            DestroyImmediate(animator);
        }

        public RagdollGizmoDrawer GetGizmoDrawer() =>
            _drawer;

        private void GetAnimator(out Animator animator)
        {
            animator = gameObject.GetOrAddComponent<Animator>();
            animator.avatar = _avatar;
        }
    }
}