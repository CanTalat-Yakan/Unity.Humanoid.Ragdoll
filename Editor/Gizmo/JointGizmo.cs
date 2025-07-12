#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace UnityEssentials
{
    public class JointGizmo : MonoBehaviour
    {
        public static Vector3 GetPosition(Transform transform) =>
            transform.position;

        public static void DrawControllers(RagdollGizmoDrawer drawer, Transform transform)
        {
            ConfigurableJoint joint = transform.GetComponent<ConfigurableJoint>();
            if (joint == null)
                return;

            Undo.RecordObject(joint, "Set Joint");

            ConfigurableJoint symmetricJoint = null;
            Transform symmetricBone;
            if (drawer.SymmetricBones != null && drawer.SymmetricBones.TryGetValue(transform.name, out symmetricBone))
            {
                symmetricJoint = symmetricBone.GetComponent<ConfigurableJoint>();
                if (symmetricJoint == null)
                    return;

                Undo.RecordObject(symmetricJoint, "Setup Symmetric Joint");
            }

            Color backupColor = Handles.color;
            Vector3 position = joint.transform.position + joint.anchor;
            float size = HandleUtility.GetHandleSize(position);

            // ConfigurableJoint uses different axis references
            Vector3 axisDir = joint.axis.normalized;                                    // Primary axis (yellow)
            Vector3 secondaryAxisDir = joint.secondaryAxis.normalized;                  // Secondary axis (green)
            Vector3 swingAxisDir = Vector3.Cross(axisDir, secondaryAxisDir).normalized; // Calculated swing axis (red)

            DrawTwist(joint, symmetricJoint, position, swingAxisDir, axisDir, size);
            DrawSwing1(joint, symmetricJoint, position, swingAxisDir, axisDir, secondaryAxisDir, size);
            DrawSwing2(joint, symmetricJoint, position, swingAxisDir, secondaryAxisDir, size);

            var currRot = Quaternion.LookRotation(secondaryAxisDir, axisDir);
            Quaternion newRotation = Handles.RotationHandle(currRot, position);

            // Update joint axes based on new rotation
            joint.axis = newRotation * Vector3.up;                  // Primary axis (yellow)
            joint.secondaryAxis = newRotation * Vector3.forward;    // Secondary axis (green)

            Handles.color = backupColor;
        }

        private static void DrawTwist(ConfigurableJoint joint, ConfigurableJoint symJoint, Vector3 position, Vector3 swingAxisDir, Vector3 axisDir, float size)
        {
            Handles.color = new Color(0.7f, 0.7f, 0.0f, 1f);
            Handles.ArrowHandleCap(0, position, Quaternion.LookRotation(axisDir), size * 1.1f, EventType.Repaint);

            Handles.color = new Color(0.7f, 0.7f, 0.0f, 1f);
            Vector3 twistNormal = axisDir;

            // Get current twist limits
            float highLimit = joint.highAngularXLimit.limit;
            float lowLimit = joint.lowAngularXLimit.limit;

            float newHighLimit = highLimit;
            float newLowLimit = lowLimit;

            newHighLimit = -ProcessLimit(position, twistNormal, swingAxisDir, size, -newHighLimit);
            newLowLimit = -ProcessLimit(position, twistNormal, swingAxisDir, size, -newLowLimit);

            if (joint.highAngularXLimit.limit != newHighLimit)
            {
                SoftJointLimit highLimitStruct = joint.highAngularXLimit;
                highLimitStruct.limit = newHighLimit;
                joint.highAngularXLimit = highLimitStruct;

                if (symJoint != null)
                {
                    SoftJointLimit symHighLimit = symJoint.highAngularXLimit;
                    symHighLimit.limit = newHighLimit;
                    symJoint.highAngularXLimit = symHighLimit;
                }
            }

            if (joint.lowAngularXLimit.limit != newLowLimit)
            {
                SoftJointLimit lowLimitStruct = joint.lowAngularXLimit;
                lowLimitStruct.limit = newLowLimit;
                joint.lowAngularXLimit = lowLimitStruct;

                if (symJoint != null)
                {
                    SoftJointLimit symLowLimit = symJoint.lowAngularXLimit;
                    symLowLimit.limit = newLowLimit;
                    symJoint.lowAngularXLimit = symLowLimit;
                }
            }
        }

        private static void DrawSwing1(ConfigurableJoint joint, ConfigurableJoint symmetricJoint, Vector3 position, Vector3 swingAxisDirection, Vector3 axisDirection, Vector3 secondaryAxisDirection, float size)
        {
            Handles.color = new Color(0.0f, 0.7f, 0.0f, 1f);
            Handles.ArrowHandleCap(0, position, Quaternion.LookRotation(secondaryAxisDirection), size * 1.1f, EventType.Repaint);

            Handles.color = new Color(0.0f, 0.7f, 0.0f, 1f);
            Vector3 swing1Normal = Vector3.Cross(axisDirection, swingAxisDirection);
            float swing1Limit = joint.angularYLimit.limit;

            float newLimit = swing1Limit;
            newLimit = ProcessLimit(position, swing1Normal, swingAxisDirection, size, newLimit);
            newLimit = -ProcessLimit(position, swing1Normal, swingAxisDirection, size, -newLimit);

            if (newLimit < 10f)
                newLimit = 0f;

            if (joint.angularYLimit.limit != newLimit)
            {
                SoftJointLimit swing1LimitStruct = joint.angularYLimit;
                swing1LimitStruct.limit = newLimit;
                joint.angularYLimit = swing1LimitStruct;

                if (symmetricJoint != null)
                {
                    SoftJointLimit symSwing1Limit = symmetricJoint.angularYLimit;
                    symSwing1Limit.limit = newLimit;
                    symmetricJoint.angularYLimit = symSwing1Limit;
                }
            }
        }

        private static void DrawSwing2(ConfigurableJoint joint, ConfigurableJoint symmetricJoint, Vector3 position, Vector3 swingAxisDirection, Vector3 secondaryAxisDirection, float size)
        {
            Handles.color = new Color(1f, 0f, 0f, 1f);
            Handles.ArrowHandleCap(0, position, Quaternion.LookRotation(swingAxisDirection), size * 2f, EventType.Repaint);

            Handles.color = new Color(0.0f, 0.0f, 0.7f, 1f);
            Vector3 swing2Normal = swingAxisDirection;
            float swing2Limit = joint.angularZLimit.limit;

            float newLimit = swing2Limit;
            newLimit = ProcessLimit(position, swing2Normal, secondaryAxisDirection, size, newLimit);
            newLimit = -ProcessLimit(position, swing2Normal, secondaryAxisDirection, size, -newLimit);

            if (newLimit < 10f)
                newLimit = 0f;

            if (joint.angularZLimit.limit != newLimit)
            {
                SoftJointLimit swing2LimitStruct = joint.angularZLimit;
                swing2LimitStruct.limit = newLimit;
                joint.angularZLimit = swing2LimitStruct;

                if (symmetricJoint != null)
                {
                    SoftJointLimit symSwing2Limit = symmetricJoint.angularZLimit;
                    symSwing2Limit.limit = newLimit;
                    symmetricJoint.angularZLimit = symSwing2Limit;
                }
            }
        }

        private static Vector3 GetDirection(CharacterJoint joint, Vector3 swingAxisDir, Vector3 axisDir)
        {
            Vector3 direction = Vector3.Cross(swingAxisDir, axisDir);
            Vector3 direction2 = GetDirection(joint);

            //Handles.color = new Color(1f, 0f, 0f, 1f);
            //Handles.DrawLine(joint.transform.position, joint.transform.position + direction * 100);
            //Handles.color = new Color(0f, 1f, 0f, 1f);
            //Handles.DrawLine(joint.transform.position, joint.transform.position + direction2 * 100);
            float r = Vector3.Dot(direction, direction2);

            return direction * Mathf.Sign(r);
        }

        private static Vector3 GetDirection(CharacterJoint joint)
        {
            var transform = joint.transform;
            if (transform.childCount == 0)
                // in now children. Return direction related to parent
                return (joint.transform.position - joint.connectedBody.transform.position).normalized;

            Vector3 direction = Vector3.zero;

            for (int ch = 0; ch < transform.childCount; ++ch)
            {
                // take to account colliders that attached to children
                var colliders = transform.GetChild(ch).GetComponents<Collider>();
                for (int i = 0; i < colliders.Length; ++i)
                {
                    Collider collider = colliders[i];
                    CapsuleCollider cCollider = collider as CapsuleCollider;
                    BoxCollider bCollider = collider as BoxCollider;
                    SphereCollider sCollider = collider as SphereCollider;
                    if (cCollider != null)
                        direction += collider.transform.TransformDirection(cCollider.center);
                    if (bCollider != null)
                        direction += collider.transform.TransformDirection(bCollider.center);
                    if (sCollider != null)
                        direction += collider.transform.TransformDirection(sCollider.center);
                }
            }

            // if colliders was found, return average direction to colliders.
            if (direction != Vector3.zero)
                return direction.normalized;

            // otherwise, take direction to first child
            for (int i = 0; i < transform.childCount; ++i)
                direction += transform.GetChild(i).localPosition;
            return transform.TransformDirection(direction).normalized;
        }

        /// <summary>
        /// Draws arc with controls
        /// </summary>
        /// <returns>New limit.</returns>
        /// <param name="position">Position of center of arc</param>
        /// <param name="planeNormal">Plane normal in which arc are to be drawn</param>
        /// <param name="startDir">Start direction of arc</param>
        /// <param name="size">Radius of arc</param>
        /// <param name="limit">Current limit</param>
        private static float ProcessLimit(Vector3 position, Vector3 planeNormal, Vector3 startDir, float size, float limit)
        {
            Vector3 cross = Vector3.Cross(planeNormal, startDir);
            startDir = Vector3.Cross(cross, planeNormal);

            Vector3 controllerDir = (Quaternion.AngleAxis(limit, planeNormal) * startDir);
            Vector3 controllerPos = position + (controllerDir * size * 1.2f);

            Color backupColor = Handles.color;
            Color newColor = backupColor * 2;
            newColor.a = 1f;
            Handles.color = newColor;
            Handles.DrawLine(position, controllerPos);

            newColor.a = 0.2f;
            Handles.color = newColor;

            Handles.DrawSolidArc(
                position,
                planeNormal,
                startDir,
                limit, size);

            newColor.a = 1f;
            Handles.color = newColor;

#if UNITY_2022_1_OR_NEWER
            bool positionChanged = Handles.FreeMoveHandle(controllerPos, size * 0.1f, Vector3.zero, Handles.SphereHandleCap) != controllerPos;
#else
			bool positionChanged = Handles.FreeMoveHandle(controllerPos, Quaternion.identity, size * 0.1f, Vector3.zero, Handles.SphereHandleCap) != controllerPos;
#endif

            if (positionChanged)
            {
                var ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                float rayDistance;

                Plane plane = new(planeNormal, position);
                if (plane.Raycast(ray, out rayDistance))
                    controllerPos = ray.GetPoint(rayDistance);
                controllerPos = position + (controllerPos - position).normalized * size * 1.2f;

                // Get the angle in degrees between 0 and 180
                limit = Vector3.Angle(startDir, controllerPos - position);
                // Determine if the degree value should be negative.  Here, a positive value
                // from the dot product means that our vector is on the right of the reference vector   
                // whereas a negative value means we're on the left.
                float sign = Mathf.Sign(Vector3.Dot(cross, controllerPos - position));
                limit *= sign;

                limit = Mathf.Round(limit / 5f) * 5f;   // i need this to snap rotation
            }

            Handles.color = backupColor;
            return limit;
        }
    }
}
#endif