﻿using System;
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
            
            public MovementDirection oscillationDirection;
            public FloatRange oscillationAmplitude;
            public FloatRange oscillationFrequency;
        }

        public abstract Vector3 SpawnPoint { get; }


        [SerializeField] private SpawnConfiguration _config;


        public virtual void SpawnShape()
        {
            int factoryIndex = Random.Range(0, _config.factories.Length);
            Shape shape = _config.factories[factoryIndex].GetRandom();

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
            
            CreateSatelliteFor(shape);
            
            //return shape;
        }
        
        private void CreateSatelliteFor (Shape focalShape) {
            int factoryIndex = Random.Range(0, _config.factories.Length);
            Shape shape = _config.factories[factoryIndex].GetRandom();
            Transform t = shape.transform;
            t.localRotation = Random.rotation;
            t.localScale = focalShape.transform.localScale * 0.5f;
            t.localPosition = focalShape.transform.localPosition + Vector3.up;
            shape.AddBehaviour<MovementShapeBehaviour>().Velocity = Vector3.up;
            SetupColor(shape);
        }
        
        private void SetupColor (Shape shape) {
            if (_config.uniformColor) {
                shape.SetColor(_config.color.RandomInRange);
            }
            else {
                for (int i = 0; i < shape.ColorCount; i++) {
                    shape.SetColor(_config.color.RandomInRange, i);
                }
            }
        }
        
        private void SetupOscillation (Shape shape) {
            float amplitude = _config.oscillationAmplitude.RandomValueInRange;
            float frequency = _config.oscillationFrequency.RandomValueInRange;
            if (amplitude == 0f || frequency == 0f) {
                return;
            }
            var oscillation = shape.AddBehaviour<OscillationShapeBehaviour>();
            oscillation.Offset = GetDirectionVector(
                                     _config.oscillationDirection, shape.transform
                                 ) * amplitude;
            oscillation.Frequency = frequency;
        }
        
        private Vector3 GetDirectionVector (
            SpawnConfiguration.MovementDirection direction, Transform t
        ) {
            switch (direction) {
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
    }
}