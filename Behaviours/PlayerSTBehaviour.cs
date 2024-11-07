using DunGen;
using GameNetcodeStuff;
using UnityEngine;

namespace SawTapes.Behaviours
{
    public class PlayerSTBehaviour : MonoBehaviour
    {
        public PlayerControllerB playerProperties;
        public bool isInGame = false;
        public Tile tileGame;
        // SURVIVAL GAME
        public int campTime = 0;
        // HUNTING GAME
        public ReverseBearTrap assignedReverseBearTrap;
        public EnemyAI assignedEnemy;
    }
}
