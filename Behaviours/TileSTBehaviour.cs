using System.Collections.Generic;
using SawTapes.Behaviours.Tapes;
using UnityEngine;

namespace SawTapes.Behaviours
{
    public class TileSTBehaviour : MonoBehaviour
    {
        public List<DoorLock> doorLocks = new List<DoorLock>();
        public List<EntranceTeleport> entranceTeleports = new List<EntranceTeleport>();
        public SawTape sawTape;
    }
}
