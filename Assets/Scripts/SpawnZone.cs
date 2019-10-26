using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DefaultNamespace
{
    public class SpawnZone : MonoBehaviour
    {
        [SerializeField] private bool _surfaceOnly;

        public Vector3 SpawnPoint =>
            transform.TransformPoint(_surfaceOnly ? Random.onUnitSphere : Random.insideUnitSphere);

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireSphere(Vector3.zero, 1);
        }
    }
}