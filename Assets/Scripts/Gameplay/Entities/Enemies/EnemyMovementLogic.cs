using UnityEngine;

namespace Gameplay.Entities.Enemies
{
    public class EnemyMovementLogic
    {
        private const float ColliderPadding = 0.5f;
        private const float ArrivalSqrDistance = 0.01f;
        private const float SurroundArrivalSqrDistance = 0.04f;

        private EnemyConfig _config;
        private float _wanderTimer;
        private bool _isWandering;
        private Vector3 _wanderDestination;

        public void Initialize(EnemyConfig config)
        {
            _config = config;
            _wanderTimer = config.GetWanderPauseDuration();
            _isWandering = false;
        }

        public void StopWander()
        {
            _isWandering = false;
        }

        public bool Chase(Transform body, Vector3 toTarget)
        {
            return Move(body, toTarget, _config.MoveSpeed);
        }

        public bool Surround(Transform body, Vector3 destination, Vector3 lookAtPosition)
        {
            Vector3 toDestination = destination - body.position;
            toDestination.y = 0f;

            if (toDestination.sqrMagnitude > SurroundArrivalSqrDistance)
                return Move(body, toDestination, _config.MoveSpeed);

            Vector3 lookDirection = lookAtPosition - body.position;
            lookDirection.y = 0f;

            if (lookDirection.sqrMagnitude > ArrivalSqrDistance)
                RotateTowards(body, lookDirection);

            return false;
        }

        public bool Wander(Transform body)
        {
            _wanderTimer -= Time.deltaTime;

            if (_isWandering)
                return ContinueWander(body);

            if (_wanderTimer <= 0f)
                BeginWander(body);

            return false;
        }

        private bool ContinueWander(Transform body)
        {
            Vector3 toDestination = _wanderDestination - body.position;
            toDestination.y = 0f;

            if (_wanderTimer <= 0f || toDestination.sqrMagnitude <= ArrivalSqrDistance)
            {
                _isWandering = false;
                _wanderTimer = _config.GetWanderPauseDuration();
                return false;
            }

            return Move(body, toDestination, _config.WanderSpeed);
        }

        private void BeginWander(Transform body)
        {
            float moveDuration = _config.GetWanderMoveDuration();
            float bound = Mathf.Max(0f, _config.RoadHalfWidth - ColliderPadding);
            float direction = Random.value < 0.5f ? -1f : 1f;
            float distance = _config.WanderSpeed * moveDuration;
            float targetX = Mathf.Clamp(body.position.x + direction * distance, -bound, bound);

            if (Mathf.Abs(targetX - body.position.x) < 0.1f)
                targetX = Mathf.Clamp(body.position.x - direction * distance, -bound, bound);

            _wanderDestination = new Vector3(targetX, body.position.y, body.position.z);
            _wanderTimer = moveDuration;
            _isWandering = true;
        }

        private bool Move(Transform body, Vector3 toTarget, float speed)
        {
            if (toTarget.sqrMagnitude <= ArrivalSqrDistance)
                return false;

            Vector3 direction = toTarget.normalized;
            body.position += direction * (speed * Time.deltaTime);
            RotateTowards(body, direction);
            return true;
        }

        private void RotateTowards(Transform body, Vector3 direction)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            body.rotation = Quaternion.RotateTowards(
                body.rotation,
                targetRotation,
                _config.RotationSpeed * Time.deltaTime);
        }
    }
}
