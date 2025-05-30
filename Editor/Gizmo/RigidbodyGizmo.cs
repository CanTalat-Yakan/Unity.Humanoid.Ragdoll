using UnityEditor;
using UnityEngine;

namespace UnityEssentials
{
    public class RigidbodyGizmo : MonoBehaviour
    {
        public static Vector3 GetPosition(Transform transform)
        {
            var rigidbody = transform.GetComponent<Rigidbody>();
            if (rigidbody == null)
                return Vector3.zero;

            return transform.position + transform.TransformDirection(rigidbody.centerOfMass);
        }

        public static void DrawControllers(Transform transform)
        {
            Rigidbody rigidbody = transform.GetComponent<Rigidbody>();
            if (rigidbody == null)
                return;

            Quaternion rotatorRotation = Tools.pivotRotation == PivotRotation.Global ? Quaternion.identity : transform.rotation;
            Vector3 position = transform.position + transform.TransformDirection(rigidbody.centerOfMass);
            Vector3 newPosition = Handles.PositionHandle(position, rotatorRotation);

            if (newPosition == position)
                return;

            Undo.RecordObject(rigidbody, "Set Rigidbody");

            var centerOfMass = transform.InverseTransformDirection(newPosition - transform.position);
            rigidbody.centerOfMass = centerOfMass;
        }
    }
}