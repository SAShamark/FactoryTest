using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Gameplay.Entities.Enemies
{
    [Serializable]
    public class EnemyConfig
    {
        [Header("Chase")]
        [SerializeField, Min(0f)] private float _activationDistance = 18f;
        [SerializeField, Min(0f)] private float _moveSpeed = 5.5f;
        [SerializeField, Min(0f)] private float _rotationSpeed = 360f;

        [Header("Idle Wander")]
        [SerializeField, Min(0f)] private float _roadHalfWidth = 3.5f;
        [SerializeField, Min(0f)] private float _wanderSpeed = 1.25f;
        [SerializeField, Min(0f)] private float _wanderMinPause = 1.2f;
        [SerializeField, Min(0f)] private float _wanderMaxPause = 2.5f;
        [SerializeField, Min(0.1f)] private float _wanderMinMoveDuration = 0.8f;
        [SerializeField, Min(0.1f)] private float _wanderMaxMoveDuration = 1.8f;

        [Header("Contact Damage")]
        [SerializeField, Min(0f)] private float _contactDamage = 15f;

        public float ActivationDistance => _activationDistance;
        public float MoveSpeed => _moveSpeed;
        public float RotationSpeed => _rotationSpeed;
        public float RoadHalfWidth => _roadHalfWidth;
        public float WanderSpeed => _wanderSpeed;
        public float ContactDamage => _contactDamage;

        public float GetWanderPauseDuration()
        {
            return Random.Range(
                Mathf.Min(_wanderMinPause, _wanderMaxPause),
                Mathf.Max(_wanderMinPause, _wanderMaxPause));
        }

        public float GetWanderMoveDuration()
        {
            return Random.Range(
                Mathf.Min(_wanderMinMoveDuration, _wanderMaxMoveDuration),
                Mathf.Max(_wanderMinMoveDuration, _wanderMaxMoveDuration));
        }
    }
}
