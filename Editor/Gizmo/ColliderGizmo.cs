#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace UnityEssentials
{
    using static RagdolHelper;

    public class ColliderGizmo : MonoBehaviour
    {
        public static Vector3 GetPosition(Transform transform) =>
            GetRotatorPosition(transform);

        public static void DrawControllers(RagdollGizmoDrawer drawer, Quaternion lastRotation, Transform transform, Vector3 position)
        {
            Quaternion rotatorRotation = GetRotatorRotation(transform);

            switch (Tools.current)
            {
                case Tool.Rotate:
                    ProcessRotation(rotatorRotation, lastRotation, transform, position);
                    break;
                case Tool.Move:
                    ProcessColliderMove(rotatorRotation, transform, position);
                    break;
                case Tool.Rect:
                case Tool.Scale:
                    ProcessColliderScale(drawer, rotatorRotation, transform, position);
                    break;
            }
        }

        static void ProcessRotation(Quaternion rotatorRotation, Quaternion lastRotation, Transform transform, Vector3 position)
        {
            Quaternion newRotation;
            bool changed;

            if (Tools.pivotRotation == PivotRotation.Global)
            {
                Quaternion fromStart = rotatorRotation * Quaternion.Inverse(lastRotation);
                newRotation = Handles.RotationHandle(fromStart, position);
                changed = fromStart != newRotation;
                newRotation = newRotation * lastRotation;
            }
            else
            {
                newRotation = Handles.RotationHandle(rotatorRotation, position);
                changed = rotatorRotation != newRotation;
            }

            if (changed)
            {
                transform = GetRotatorTransform(transform);
                RotateCollider(transform, newRotation);
            }
        }

        static void ProcessColliderMove(Quaternion rotatorRotation, Transform transform, Vector3 position)
        {
            if (Tools.pivotRotation == PivotRotation.Global)
                rotatorRotation = Quaternion.identity;

            Vector3 newPosition = Handles.PositionHandle(position, rotatorRotation);
            Vector3 translateBy = newPosition - position;

            if (translateBy != Vector3.zero)
                SetColliderPosition(transform, newPosition);
        }

        static void ProcessColliderScale(RagdollGizmoDrawer drawer, Quaternion rotatorRotation, Transform transform, Vector3 position)
        {
            float size = HandleUtility.GetHandleSize(position);
            var collider = GetCollider(transform);

            // process each collider type in its own way
            CapsuleCollider capsuleCollider = collider as CapsuleCollider;
            BoxCollider boxCollider = collider as BoxCollider;
            SphereCollider sphereCollider = collider as SphereCollider;

            float scale = (collider.transform.lossyScale.x + collider.transform.lossyScale.y + collider.transform.lossyScale.z) / 3f;
            if (capsuleCollider != null)
            {
                // for capsule collider draw circle and two dot controllers
                Vector3 direction = DirectionIntToVector(capsuleCollider.direction);
                Quaternion rotation = Quaternion.LookRotation(capsuleCollider.transform.TransformDirection(direction));

                // method "Handles.ScaleValueHandle" multiplies size on 0.15f
                // so to send exact size to "Handles.CircleCap",
                // I needed to multiply size on 1f/0.15f
                // Then to get a size a little bigger (to 130%) than
                // collider (for nice looking purpose), I multiply size by 1.3f
                const float magicNumber = 1f / 0.15f * 1.3f;

                // draw radius controll

                float radius = Handles.ScaleValueHandle(capsuleCollider.radius, position, rotation, capsuleCollider.radius * magicNumber * scale, Handles.CircleHandleCap, 0);
                bool radiusChanged = capsuleCollider.radius != radius;

                Vector3 scaleHeightShift = capsuleCollider.transform.TransformDirection(direction * capsuleCollider.height / 2);

                // draw height controlls
                Vector3 heightControl1Position = position + scaleHeightShift;
                Vector3 heightControl2Position = position - scaleHeightShift;

                float height1 = Handles.ScaleValueHandle(capsuleCollider.height, heightControl1Position, rotation, size * 0.5f, Handles.DotHandleCap, 0);
                float height2 = Handles.ScaleValueHandle(capsuleCollider.height, heightControl2Position, rotation, size * 0.5f, Handles.DotHandleCap, 0);
                float newHeight = 0f;

                bool moved = false;
                bool firstControlSelected = false;
                if (height1 != capsuleCollider.height)
                {
                    moved = true;
                    firstControlSelected = true;
                    newHeight = height1;
                }
                else if (height2 != capsuleCollider.height)
                {
                    moved = true;
                    newHeight = height2;
                }

                if (moved | radiusChanged)
                {
                    Undo.RecordObject(capsuleCollider, "Resize Capsule Collider");

                    bool upperSelected = false;
                    if (moved)
                    {
                        if (newHeight < 0.01f)
                            newHeight = 0.01f;

                        bool firstIsUpper = FirstIsUpper(capsuleCollider.transform, heightControl1Position, heightControl2Position);
                        upperSelected = firstIsUpper == firstControlSelected;

                        capsuleCollider.center += direction * (newHeight - capsuleCollider.height) / 2 * (firstControlSelected ? 1 : -1);
                        capsuleCollider.height = newHeight;
                    }
                    if (radiusChanged)
                        capsuleCollider.radius = radius;

                    // resize symmetric colliders too
                    Transform symmetricBone;
                    if (drawer.SymmetricBones != null && drawer.SymmetricBones.TryGetValue(transform.name, out symmetricBone))
                    {
                        var symmetricCapsule = GetCollider(symmetricBone) as CapsuleCollider;
                        if (symmetricCapsule == null)
                            return;

                        Undo.RecordObject(symmetricCapsule, "Resize symetric capsule collider");

                        if (moved)
                        {
                            Vector3 direction2 = DirectionIntToVector(symmetricCapsule.direction);

                            Vector3 scaleHeightShift2 = symmetricCapsule.transform.TransformDirection(direction2 * symmetricCapsule.height / 2);
                            Vector3 pos2 = GetRotatorPosition(symmetricCapsule.transform);

                            Vector3 heightControl1Pos2 = pos2 + scaleHeightShift2;
                            Vector3 heightControl2Pos2 = pos2 - scaleHeightShift2;

                            bool firstIsUpper2 = FirstIsUpper(symmetricCapsule.transform, heightControl1Pos2, heightControl2Pos2);

                            symmetricCapsule.center += direction2 * (newHeight - symmetricCapsule.height) / 2
                                * (upperSelected ? 1 : -1)
                                * (firstIsUpper2 ? 1 : -1);

                            symmetricCapsule.height = capsuleCollider.height;
                        }
                        if (radiusChanged)
                            symmetricCapsule.radius = capsuleCollider.radius;
                    }
                }
            }
            else if (boxCollider != null)
            {
                // resize Box collider
                var newSize = Handles.ScaleHandle(boxCollider.size, position, rotatorRotation, size);
                if (boxCollider.size != newSize)
                {
                    Undo.RecordObject(boxCollider, "Resize Box Collider");

                    boxCollider.size = newSize;
                }
            }
            else if (sphereCollider != null)
            {
                // resize Sphere collider
                float radius = sphereCollider.radius * scale;
                var newRadius = Handles.RadiusHandle(rotatorRotation, position, radius, true);
                if (radius != newRadius)
                {
                    Undo.RecordObject(sphereCollider, "Resize Sphere Collider");

                    sphereCollider.radius = newRadius / scale;
                }
            }
            else throw new InvalidOperationException("Unsupported Collider type: " + collider.GetType().FullName);
        }

        /// <summary>
        /// Int (PhysX specific) direction to Vector3 direction
        /// </summary>
        private static Vector3 DirectionIntToVector(int directionIndex) =>
            directionIndex switch
            {
                0 => Vector3.right,
                1 => Vector3.up,
                2 => Vector3.forward,
                _ => default,
            };

        private static bool FirstIsUpper(Transform transform, Vector3 heightControl1Position, Vector3 heightControl2Position)
        {
            if (transform.parent == null)
                return true;

            Vector3 currentPosition = transform.position;
            Vector3 parentPosition;
            do
            {
                transform = transform.parent;
                parentPosition = transform.position;
            }
            while (parentPosition == currentPosition & transform.parent != null);

            if (parentPosition == currentPosition)
                return true;

            Vector3 limbDirection = currentPosition - parentPosition;

            limbDirection.Normalize();

            float direction1 = Vector3.Dot(limbDirection, heightControl1Position - parentPosition);
            float direction2 = Vector3.Dot(limbDirection, heightControl2Position - parentPosition);


            bool firstIsUpper = direction1 < direction2;
            return firstIsUpper;
        }
    }
}
#endif