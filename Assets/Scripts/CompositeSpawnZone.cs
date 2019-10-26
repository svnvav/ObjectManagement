using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DefaultNamespace
{
    public class CompositeSpawnZone : SpawnZone
    {
        [SerializeField] private SpawnZone[] _spawnZones;

        public override Vector3 SpawnPoint
        {
            get
            {
                int zoneIndex = Random.Range(0, _spawnZones.Length);

                return _spawnZones[zoneIndex].SpawnPoint;
            }
        }
    }
}