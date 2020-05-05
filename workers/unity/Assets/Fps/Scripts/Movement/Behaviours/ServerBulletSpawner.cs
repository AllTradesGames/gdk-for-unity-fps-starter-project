using System.Collections;
using Fps.Config;
using Fps.SchemaExtensions;
using Improbable.Gdk.Subscriptions;
using UnityEngine;

namespace Fps
{
    [WorkerType(WorkerUtils.UnityGameLogic)]
    public class ServerBulletSpawner : MonoBehaviour
    {
        [Require] private ServerProjectileComponentCommandReceiver commandReceiver;
        [SerializeField] private GameObject[] bullets;
        private Vector3 workerOrigin;

        private void OnEnable()
        {
            commandReceiver.OnSpawnProjectileRequestReceived += SpawnProjectile;
            workerOrigin = GetComponent<LinkedEntityComponent>().Worker.Origin;
        }

        private void SpawnProjectile(ServerProjectileComponent.SpawnProjectile.ReceivedRequest spawnProjectileRequest)
        {
            var bulletInfo = spawnProjectileRequest.Payload;
            var bulletPrefab = bullets[bulletInfo.ServerBulletIndex];
            var spawnLocation = bulletInfo.OriginLocation.ToVector3() + workerOrigin;
            var spawnDirection = bulletInfo.TargetDirection.ToVector3();

            var bullet = (Instantiate(bulletPrefab, spawnLocation, Quaternion.Euler(spawnDirection)) as GameObject).GetComponent<BulletServerBehavior>();
            bullet.sourceEntityId = bulletInfo.SourceEntityId;
            bullet.sourceWorkerOrigin = workerOrigin;
            bullet.spawnPosition = spawnLocation;
            bullet.targetDirection = spawnDirection;
        }
    }
}
