using UnityEngine;

namespace DefaultNamespace
{
    public abstract class SpawnZone : PersistableObject
    {
        public abstract Vector3 SpawnPoint { get; }
        
    }
}