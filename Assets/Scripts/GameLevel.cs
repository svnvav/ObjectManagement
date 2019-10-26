using System;
using UnityEngine;

namespace DefaultNamespace
{
    public class GameLevel : PersistableObject
    {
        public static GameLevel Current;
        
        [SerializeField] private SpawnZone _spawnZone;

        [SerializeField] private PersistableObject[] _persistentObjects;

        public Vector3 SpawnPoint => _spawnZone.SpawnPoint;

        private void OnEnable()
        {
            Current = this;
            if (_persistentObjects == null)
            {
                _persistentObjects = new PersistableObject[0];
            }
        }

        public override void Save(GameDataWriter writer)
        {
            writer.Write(_persistentObjects.Length);
            foreach (var _persistentObject in _persistentObjects)
            {
                _persistentObject.Save(writer);
            }
        }

        public override void Load(GameDataReader reader)
        {
            var savedCount = reader.ReadInt();
            for (int i = 0; i < _persistentObjects.Length; i++)
            {
                _persistentObjects[i].Load(reader);
            }
        }
    }
}