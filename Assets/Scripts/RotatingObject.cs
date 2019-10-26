using System;
using UnityEngine;

namespace DefaultNamespace
{
    public class RotatingObject : PersistableObject
    {
        [SerializeField] private Vector3 _angularVelocity;

        private void Update()
        {
            transform.Rotate(_angularVelocity * Time.deltaTime);
        }
    }
}