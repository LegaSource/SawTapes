using GameNetcodeStuff;
using System.Linq;
using UnityEngine;

namespace SawTapes.Behaviours
{
    public class ReverseBearTrap : PhysicsProp
    {
        public BoxCollider scanNodeCollider;
        public bool isReleased = false;

        public override void Start()
        {
            base.Start();

            scanNodeCollider = GetComponentsInChildren<BoxCollider>().FirstOrDefault(c => c.gameObject.name.Equals("ScanNode"));
            if (scanNodeCollider == null)
            {
                SawTapes.mls.LogWarning("The scan node collider could not be found for Reverse Bear Trap.");
            }
            SetCarriedState(true);
        }

        public void SetCarriedState(bool isCarried) => scanNodeCollider.enabled = !isCarried;

        public override void LateUpdate()
        {
            if (!isReleased)
            {
                if (parentObject != null)
                {
                    PlayerControllerB player = parentObject.GetComponentInParent<PlayerControllerB>();
                    if (player != null)
                    {
                        transform.rotation = parentObject.rotation;
                        if (GameNetworkManager.Instance.localPlayerController == player)
                        {
                            transform.position = parentObject.TransformPoint(Vector3.down * 0.17f);
                        }
                        else
                        {
                            transform.position = parentObject.TransformPoint(Vector3.up * 0.01f);
                        }
                    }
                }
                if (radarIcon != null)
                {
                    radarIcon.position = transform.position;
                }
            }
            else
            {
                base.LateUpdate();
            }
        }
    }
}
