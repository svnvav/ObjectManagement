using System;
using UnityEngine;

namespace DefaultNamespace
{
    public class GameLevel : PersistableObject
    {
        public static GameLevel Current;
        
        [SerializeField] private SpawnZone _spawnZone;

        public Vector3 SpawnPoint => _spawnZone.SpawnPoint;

        private void OnEnable()
        {
            Current = this;
        }

        public override void Save(GameDataWriter writer)
        {
            
        }

        public override void Load(GameDataReader reader)
        {
            
        }
    }
}