using System;
using UnityEngine;

namespace DefaultNamespace
{
    public class GameLevel : MonoBehaviour
    {
        [SerializeField] private SpawnZone _spawnZone;

        private void Start()
        {
            GameController.Instance.SpawnZoneOfALevel = _spawnZone;
        }
    }
}