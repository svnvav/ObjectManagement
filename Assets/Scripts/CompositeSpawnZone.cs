using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Catlike.ObjectManagement
{
    public class CompositeSpawnZone : SpawnZone
    {
        [SerializeField] private bool _sequential;
        [SerializeField] private SpawnZone[] _spawnZones;
        [SerializeField] private bool _overrideConfig;

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

        public override Shape SpawnShape()
        {
            if (_overrideConfig)
            {
                return base.SpawnShape();
            }
            else
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

                return _spawnZones[zoneIndex].SpawnShape();
            }
        }

        public override void Save(GameDataWriter writer)
        {
            writer.Write(_nextSequentialIndex);
        }

        public override void Load(GameDataReader reader)
        {
            _nextSequentialIndex = reader.ReadInt();
        }
    }
}