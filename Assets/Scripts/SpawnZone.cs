using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DefaultNamespace
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

            public MovementDirection movementDirection;

            public FloatRange speed;
        }

        public abstract Vector3 SpawnPoint { get; }


        [SerializeField] private SpawnConfiguration _config;


        public virtual void ConfigureSpawn(Shape shape)
        {
            Transform t = shape.transform;
            t.localPosition = SpawnPoint;
            t.localRotation = Random.rotation;
            t.localScale = Vector3.one * Random.Range(0.1f, 1f);
            shape.SetColor(Random.ColorHSV(
                hueMin: 0f, hueMax: 1f,
                saturationMin: 0.5f, saturationMax: 1f,
                valueMin: 0.25f, valueMax: 1f,
                alphaMin: 1f, alphaMax: 1f
            ));
            shape.AngularVelocity = Random.onUnitSphere * Random.Range(0f, 90f);

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
        }
    }
}