using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Catlike.ObjectManagement
{
    public class GameLevel : PersistableObject
    {
        public static GameLevel Current;

        [SerializeField] private int _populationLimit;
        
        [SerializeField] private SpawnZone _spawnZone;

        [FormerlySerializedAs("_persistentObjects")] 
        [SerializeField] private GameLevelObject[] _levelObjects;

        public bool HasMissingLevelObjects {
            get {
                if (_levelObjects != null) {
                    for (int i = 0; i < _levelObjects.Length; i++) {
                        if (_levelObjects[i] == null) {
                            return true;
                        }
                    }
                }
                return false;
            }
        }
        
        public int PopulationLimit => _populationLimit;

        private void OnEnable()
        {
            Current = this;
            if (_levelObjects == null)
            {
                _levelObjects = new GameLevelObject[0];
            }
        }

        public void GameUpdate()
        {
            foreach (var levelObject in _levelObjects)
            {
                levelObject.GameUpdate();
            }
        }
        
        public void SpawnShape() {
            _spawnZone.SpawnShape();
        }

        public override void Save(GameDataWriter writer)
        {
            writer.Write(_levelObjects.Length);
            foreach (var _persistentObject in _levelObjects)
            {
                _persistentObject.Save(writer);
            }
        }

        public override void Load(GameDataReader reader)
        {
            var savedCount = reader.ReadInt();
            for (int i = 0; i < _levelObjects.Length; i++)
            {
                _levelObjects[i].Load(reader);
            }
        }
    }
}