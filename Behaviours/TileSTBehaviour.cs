using System.Collections.Generic;
using UnityEngine;

namespace SawTapes.Behaviours
{
    internal class TileSTBehaviour : MonoBehaviour
    {
        public List<DoorLock> doorLocks = new List<DoorLock>();
        public List<EntranceTeleport> entranceTeleports = new List<EntranceTeleport>();
        public SawTape sawTape;
    }
}
