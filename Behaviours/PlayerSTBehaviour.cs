using DunGen;
using GameNetcodeStuff;
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
        public bool isTargetable = true;
        public bool isInGame = false;
        public int currentControlTipState = (int)ControlTip.NONE;

        // TILE GAMES
        public Tile tileGame;

        // GASSED GAMES
        public bool hasBeenGassed = false;

        // SURVIVAL GAME
        public int campTime = 0;

        // HUNTING GAME
        public HuntingTape huntingTape;

        // ESCAPE GAME
        public EscapeTape escapeTape;
    }
}
