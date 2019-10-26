using UnityEngine;

namespace DefaultNamespace
{
    public abstract class SpawnZone : MonoBehaviour
    {
        public abstract Vector3 SpawnPoint { get; }
    }
}