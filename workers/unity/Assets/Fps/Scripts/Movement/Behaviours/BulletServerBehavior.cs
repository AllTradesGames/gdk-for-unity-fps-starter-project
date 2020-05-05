using System.Collections;
using Fps.Config;
using Fps.SchemaExtensions;
using Improbable.Gdk.Core;
using Improbable.Gdk.Subscriptions;
using UnityEngine;

namespace Fps
{
    [WorkerType(WorkerUtils.UnityGameLogic)]
    public class BulletServerBehavior : MonoBehaviour
    {
        [Require] private ServerProjectileComponentWriter serverProjectileWriter;
        [SerializeField] private float range;
        [SerializeField] private float speed;

        public EntityId sourceEntityId;
        public Vector3 sourceWorkerOrigin;
        public Vector3 spawnPosition;
        public Vector3 targetDirection;

        private Coroutine respawnCoroutine;
        private Collider collider;
        private float distanceTravelled = 0f;
        private bool killNextStep = false;

        private void OnEnable()
        {
            collider = gameObject.GetComponentInChildren<Collider>();
        }

        private void FixedUpdate()
        {
            if (!killNextStep)
            {
                var toTravel = targetDirection * speed * Time.deltaTime;
                transform.position += toTravel;
                distanceTravelled += toTravel.magnitude;
                killNextStep = distanceTravelled >= range;
            }
            else
            {
                HandleEndOfBullet(null);
                Destroy(this.gameObject);
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            // OnCollisionEnter is fired regardless of whether the MonoBehaviour is enabled/disabled.
            if (serverProjectileWriter == null)
            {
                return;
            }

            HandleEndOfBullet(collision.other.gameObject);
        }

        private void HandleEndOfBullet(GameObject hitObject)
        {
            var playerSpatialOsComponent = hitObject?.GetComponent<LinkedEntityComponent>();
            var hitSomething = hitObject != null;
            var entityId = new EntityId(0);

            if (playerSpatialOsComponent != null)
            {
                entityId = playerSpatialOsComponent.EntityId;
            }

            var shotInfo = new ShotInfo()
            {
                TargetEntityId = entityId,
                HitSomething = hitSomething,
                HitLocation = (transform.position - sourceWorkerOrigin).ToVector3Int(),
                HitOrigin = (spawnPosition - sourceWorkerOrigin).ToVector3Int(),
                SourceEntityId = sourceEntityId,
            };

            serverProjectileWriter.SendHitEvent(shotInfo);
        }
    }
}
