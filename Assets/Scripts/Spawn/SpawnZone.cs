﻿using System;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;
using Random = UnityEngine.Random;

namespace Catlike.ObjectManagement
{
    public abstract class SpawnZone : GameLevelObject
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

            public MovementDirection oscillationDirection;
            public FloatRange oscillationAmplitude;
            public FloatRange oscillationFrequency;

            [System.Serializable]
            public struct SatelliteConfiguration
            {
                public IntRange amount;

                [FloatRangeSlider(0.1f, 1f)] public FloatRange relativeScale;

                public FloatRange orbitRadius;

                public FloatRange orbitFrequency;

                public bool uniformLifecycles;
            }

            public SatelliteConfiguration satellite;

            [System.Serializable]
            public struct LifecycleConfiguration
            {
                [FloatRangeSlider(0f, 2f)] public FloatRange growingDuration;

                [FloatRangeSlider(0f, 100f)] public FloatRange adultDuration;

                [FloatRangeSlider(0f, 2f)] public FloatRange dyingDuration;

                public Vector3 RandomDurations =>
                    new Vector3(
                        growingDuration.RandomValueInRange,
                        adultDuration.RandomValueInRange,
                        dyingDuration.RandomValueInRange
                    );
            }

            public LifecycleConfiguration lifecycle;
        }

        [SerializeField, Range(0f, 50f)] float _spawnSpeed;

        [SerializeField] private SpawnConfiguration _config;

        private float _spawnProgress;

        public abstract Vector3 SpawnPoint { get; }

        public override void GameUpdate()
        {
            _spawnProgress += Time.deltaTime * _spawnSpeed;
            while (_spawnProgress >= 1f)
            {
                _spawnProgress -= 1f;
                SpawnShape();
            }
        }

        public virtual void SpawnShape()
        {
            int factoryIndex = Random.Range(0, _config.factories.Length);
            Shape shape = _config.factories[factoryIndex].GetRandom();
            shape.gameObject.layer = gameObject.layer;

            Transform t = shape.transform;
            t.localPosition = SpawnPoint;
            t.localRotation = Random.rotation;
            t.localScale = Vector3.one * _config.scale.RandomValueInRange;
            SetupColor(shape);

            var angularSpeed = _config.angularSpeed.RandomValueInRange;
            if (angularSpeed != 0f)
            {
                var rotation = shape.AddBehaviour<RotationShapeBehaviour>();
                rotation.AngularVelocity = Random.onUnitSphere * angularSpeed;
            }

            float speed = _config.speed.RandomValueInRange;
            if (speed != 0f)
            {
                var movement = shape.AddBehaviour<MovementShapeBehaviour>();

                movement.Velocity = speed * GetDirectionVector(_config.movementDirection, t);
            }

            SetupOscillation(shape);

            var lifecycleDurations =
                _config.lifecycle.RandomDurations;

            var satelliteCount = _config.satellite.amount.RandomValueInRange;
            for (int i = 0; i < satelliteCount; i++)
            {
                CreateSatelliteFor(shape,
                    _config.satellite.uniformLifecycles ? lifecycleDurations : _config.lifecycle.RandomDurations
                );
            }

            SetupLifecycle(shape, lifecycleDurations);
        }

        private void CreateSatelliteFor(Shape focalShape, Vector3 durations)
        {
            int factoryIndex = Random.Range(0, _config.factories.Length);
            Shape shape = _config.factories[factoryIndex].GetRandom();
            shape.gameObject.layer = gameObject.layer;
            Transform t = shape.transform;
            t.localRotation = Random.rotation;
            t.localScale =
                focalShape.transform.localScale * _config.satellite.relativeScale.RandomValueInRange;
            SetupColor(shape);
            shape.AddBehaviour<SatelliteShapeBehaviour>().Initialize(
                shape,
                focalShape,
                _config.satellite.orbitRadius.RandomValueInRange,
                _config.satellite.orbitFrequency.RandomValueInRange
            );

            SetupLifecycle(shape, durations);
        }

        private void SetupLifecycle(Shape shape, Vector3 durations)
        {
            if (durations.x > 0f)
            {
                if (durations.y > 0f || durations.z > 0f)
                {
                    shape.AddBehaviour<LifecycleShapeBehaviour>().Initialize(
                        shape, durations.x, durations.y, durations.z
                    );
                }
                else
                {
                    shape.AddBehaviour<GrowingShapeBehaviour>().Initialize(
                        shape, durations.x
                    );
                }
            }
            else if (durations.y > 0f)
            {
                shape.AddBehaviour<LifecycleShapeBehaviour>().Initialize(
                    shape, durations.x, durations.y, durations.z
                );
            }
            else if (durations.z > 0f)
            {
                shape.AddBehaviour<DyingShapeBehaviour>().Initialize(
                    shape, durations.z
                );
            }
        }

        private void SetupColor(Shape shape)
        {
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
        }

        private void SetupOscillation(Shape shape)
        {
            float amplitude = _config.oscillationAmplitude.RandomValueInRange;
            float frequency = _config.oscillationFrequency.RandomValueInRange;
            if (amplitude == 0f || frequency == 0f)
            {
                return;
            }

            var oscillation = shape.AddBehaviour<OscillationShapeBehaviour>();
            oscillation.Offset = GetDirectionVector(
                                     _config.oscillationDirection, shape.transform
                                 ) * amplitude;
            oscillation.Frequency = frequency;
        }

        private Vector3 GetDirectionVector(
            SpawnConfiguration.MovementDirection direction, Transform t
        )
        {
            switch (direction)
            {
                case SpawnConfiguration.MovementDirection.Upward:
                    return transform.up;
                case SpawnConfiguration.MovementDirection.Outward:
                    return (t.localPosition - transform.position).normalized;
                case SpawnConfiguration.MovementDirection.Random:
                    return Random.onUnitSphere;
                default:
                    return transform.forward;
            }
        }
        
        public override void Save (GameDataWriter writer) {
            writer.Write(_spawnProgress);
        }

        public override void Load (GameDataReader reader) {
            _spawnProgress = reader.ReadFloat();
        }
    }
}