using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Catlike.ObjectManagement
{
    public abstract class SpawnZone : PersistableObject
    {
        [System.Serializable]
        public struct SpawnConfiguration
        {
            public enum MovementDirection
            {
                Forward,
                Upward,
                Outward,
                Random
            }

            public ShapeFactory[] factories;
            
            public MovementDirection movementDirection;

            public FloatRange speed;
            public FloatRange angularSpeed;
            public FloatRange scale;
            public ColorRangeHSV color;

            public bool uniformColor;
        }

        public abstract Vector3 SpawnPoint { get; }


        [SerializeField] private SpawnConfiguration _config;


        public virtual Shape SpawnShape()
        {
            int factoryIndex = Random.Range(0, _config.factories.Length);
            Shape shape = _config.factories[factoryIndex].GetRandom();

            Transform t = shape.transform;
            t.localPosition = SpawnPoint;
            t.localRotation = Random.rotation;
            t.localScale = Vector3.one * _config.scale.RandomValueInRange;
            if (_config.uniformColor)
            {
                shape.SetColor(_config.color.RandomInRange);
            }
            else
            {
                for (int i = 0; i < shape.ColorCount; i++)
                {
                    shape.SetColor(_config.color.RandomInRange, i);
                }
            }
            
            shape.AngularVelocity = Random.onUnitSphere * _config.angularSpeed.RandomValueInRange;

            Vector3 direction;
            switch (_config.movementDirection)
            {
                case SpawnConfiguration.MovementDirection.Forward:
                    direction = transform.forward;
                    break;
                case SpawnConfiguration.MovementDirection.Upward:
                    direction = transform.up;
                    break;
                case SpawnConfiguration.MovementDirection.Outward:
                    direction = (t.localPosition - transform.position).normalized;
                    break;
                case SpawnConfiguration.MovementDirection.Random:
                    direction = Random.onUnitSphere;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            shape.Velocity = direction * _config.speed.RandomValueInRange;

            return shape;
        }
    }
}