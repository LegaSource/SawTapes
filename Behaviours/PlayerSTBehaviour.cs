using DunGen;
using GameNetcodeStuff;
using UnityEngine;

namespace SawTapes.Behaviours
{
    internal class PlayerSTBehaviour : MonoBehaviour
    {
        public PlayerControllerB playerProperties;
        public bool isInGame = false;
        public Tile tileGame;
        // SURVIVAL GAME
        public int campTime = 0;
    }
}
