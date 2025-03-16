using UnityEngine;

namespace SawTapes.Managers
{
    public class ObjectSTManager
    {
        public static void ChangeObjectPosition(GrabbableObject grabbableObject, Vector3 position)
        {
            if (grabbableObject.isHeld) return;

            grabbableObject.EnableItemMeshes(false);
            grabbableObject.transform.localPosition = position;
            grabbableObject.transform.position = position;
            grabbableObject.startFallingPosition = position;
            grabbableObject.FallToGround();
            grabbableObject.hasHitGround = false;
            grabbableObject.EnableItemMeshes(true);
        }
    }
}
