using GameNetcodeStuff;
using SawTapes.Behaviours.Items;
using SawTapes.Behaviours.Tapes;
using UnityEngine;

namespace SawTapes.Behaviours;

public class PlayerSTBehaviour : MonoBehaviour
{
    // GLOBAL 
    public PlayerControllerB playerProperties;
    public SawTape sawTape;
    public ReverseBearTrap reverseBearTrap;

    public bool isTargetable = true;
    public bool isInGame = false;
    public bool hasBeenGassed = false;
}
