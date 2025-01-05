using SawTapes.Behaviours.Items;
using SawTapes.Behaviours.Tapes;
using UnityEngine;

namespace SawTapes.Managers
{
    public class ObjectSTManager
    {
        public static void SpawnBlackParticle(GrabbableObject grabbableObject)
        {
            if (grabbableObject is SawTape || grabbableObject is Saw)
            {
                GameObject curseParticleEffect = Object.Instantiate(SawTapes.tapeParticle, grabbableObject.transform.position, Quaternion.identity);
                curseParticleEffect.transform.SetParent(grabbableObject.transform);
                if (grabbableObject is SawTape sawTape)
                    sawTape.particleEffect = curseParticleEffect;
                else if (grabbableObject is Saw saw)
                    saw.particleEffect = curseParticleEffect;
            }
        }

        public static void EnableBlackParticle(GrabbableObject grabbableObject, bool enable)
        {
            if (grabbableObject != null)
            {
                if (grabbableObject is SawTape sawTape && sawTape.particleEffect != null)
                    sawTape.particleEffect.SetActive(enable);
                else if (grabbableObject is Saw saw && saw.particleEffect != null)
                    saw.particleEffect.SetActive(enable);
            }
        }

        public static void ChangeObjectPosition(GrabbableObject grabbableObject, Vector3 position)
        {
            grabbableObject.EnableItemMeshes(false);
            grabbableObject.transform.localPosition = position;
            grabbableObject.transform.position = position;
            grabbableObject.startFallingPosition = position;
            grabbableObject.FallToGround();
            grabbableObject.EnableItemMeshes(true);
            EnableBlackParticle(grabbableObject, true);
        }
    }
}
