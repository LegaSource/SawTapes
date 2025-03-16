using GameNetcodeStuff;
using SawTapes.Behaviours.Items;
using SawTapes.Behaviours.Tapes;
using UnityEngine;

namespace SawTapes.Behaviours
{
    public class PlayerSTBehaviour : MonoBehaviour
    {
        public enum ControlTip
        {
            NONE,
            SAW_ITEM
        }

        // GLOBAL
        public PlayerControllerB playerProperties;
        public SawTape sawTape;

        public bool isTargetable = true;
        public bool isInGame = false;
        public bool hasBeenGassed = false;

        public int currentControlTipState = (int)ControlTip.NONE;

        // HUNTING GAME
        public ReverseBearTrap reverseBearTrap;
    }
}
