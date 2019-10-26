using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DefaultNamespace
{
    public class CompositeSpawnZone : SpawnZone
    {
        [SerializeField] private bool _sequential;
        [SerializeField] private SpawnZone[] _spawnZones;

        [NonSerialized] private int _nextSequentialIndex;
        
        public override Vector3 SpawnPoint
        {
            get
            {
                int zoneIndex;

                if (_sequential)
                {
                    zoneIndex = _nextSequentialIndex++;
                    _nextSequentialIndex = _nextSequentialIndex % _spawnZones.Length;
                }
                else
                {
                    zoneIndex = Random.Range(0, _spawnZones.Length);
                }

                return _spawnZones[zoneIndex].SpawnPoint;
            }
        }
    }
}