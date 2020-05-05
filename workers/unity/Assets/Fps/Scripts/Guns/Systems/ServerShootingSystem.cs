using Improbable.Gdk.Core;
using Unity.Entities;

namespace Fps.Guns
{
    [UpdateInGroup(typeof(SpatialOSUpdateGroup))]
    public class ServerShootingSystem : ComponentSystem
    {
        private WorkerSystem workerSystem;
        private CommandSystem commandSystem;
        private ComponentUpdateSystem componentUpdateSystem;

        protected override void OnCreate()
        {
            base.OnCreate();

            workerSystem = World.GetExistingSystem<WorkerSystem>();
            commandSystem = World.GetExistingSystem<CommandSystem>();
            componentUpdateSystem = World.GetExistingSystem<ComponentUpdateSystem>();
        }

        protected override void OnUpdate()
        {
            var hitscanEvents = componentUpdateSystem.GetEventsReceived<ShootingComponent.Shots.Event>();
            var projectileFireEvents = componentUpdateSystem.GetEventsReceived<ShootingComponent.ProShots.Event>();
            var projectileHitEvents = componentUpdateSystem.GetEventsReceived<ServerProjectileComponent.Hit.Event>();

            handleHitEvents(hitscanEvents);
            handleProjectileHitEvents(projectileHitEvents);
            handleProjectileEvents(projectileFireEvents);


        }

        private void handleHitEvents(MessagesSpan<ComponentEventReceived<ShootingComponent.Shots.Event>> events)
        {

            if (events.Count > 0)
            {
                var gunDataForEntity = GetComponentDataFromEntity<GunComponent.Component>();
                for (var i = 0; i < events.Count; ++i)
                {
                    ref readonly var shotEvent = ref events[i];
                    var shotInfo = shotEvent.Event.Payload;
                    if (!shotInfo.HitSomething || !shotInfo.TargetEntityId.IsValid())
                    {
                        continue;
                    }

                    var shooterSpatialID = shotInfo.SourceEntityId;
                    if (!workerSystem.TryGetEntity(shooterSpatialID, out var shooterEntity))
                    {
                        continue;
                    }

                    if (!gunDataForEntity.Exists(shooterEntity))
                    {
                        continue;
                    }

                    var gunComponent = gunDataForEntity[shooterEntity];
                    var damage = GunDictionary.Get(gunComponent.GunId).ShotDamage;

                    var modifyHealthRequest = new HealthComponent.ModifyHealth.Request(
                        shotInfo.TargetEntityId,
                        new HealthModifier
                        {
                            Amount = -damage,
                            Origin = shotInfo.HitOrigin,
                            AppliedLocation = shotInfo.HitLocation,
                            Owner = shotInfo.SourceEntityId,
                        }
                    );

                    commandSystem.SendCommand(modifyHealthRequest);
                }
            }
        }

        // Having to duplicate this because of different input type is BS. I hate C#
        private void handleProjectileHitEvents(MessagesSpan<ComponentEventReceived<ServerProjectileComponent.Hit.Event>> events)
        {

            if (events.Count > 0)
            {
                var gunDataForEntity = GetComponentDataFromEntity<GunComponent.Component>();
                for (var i = 0; i < events.Count; ++i)
                {
                    ref readonly var shotEvent = ref events[i];
                    var shotInfo = shotEvent.Event.Payload;
                    if (!shotInfo.HitSomething || !shotInfo.TargetEntityId.IsValid())
                    {
                        continue;
                    }

                    var shooterSpatialID = shotInfo.SourceEntityId;
                    if (!workerSystem.TryGetEntity(shooterSpatialID, out var shooterEntity))
                    {
                        continue;
                    }

                    if (!gunDataForEntity.Exists(shooterEntity))
                    {
                        continue;
                    }

                    var gunComponent = gunDataForEntity[shooterEntity];
                    var damage = GunDictionary.Get(gunComponent.GunId).ShotDamage;

                    var modifyHealthRequest = new HealthComponent.ModifyHealth.Request(
                        shotInfo.TargetEntityId,
                        new HealthModifier
                        {
                            Amount = -damage,
                            Origin = shotInfo.HitOrigin,
                            AppliedLocation = shotInfo.HitLocation,
                            Owner = shotInfo.SourceEntityId,
                        }
                    );

                    commandSystem.SendCommand(modifyHealthRequest);
                }
            }
        }

        private void handleProjectileEvents(MessagesSpan<ComponentEventReceived<ShootingComponent.ProShots.Event>> events)
        {

            if (events.Count > 0)
            {
                var gunDataForEntity = GetComponentDataFromEntity<GunComponent.Component>();
                for (var i = 0; i < events.Count; ++i)
                {
                    ref readonly var shotEvent = ref events[i];
                    var shotInfo = shotEvent.Event.Payload;
                    var shooterSpatialID = shotInfo.SourceEntityId;
                    if (!workerSystem.TryGetEntity(shooterSpatialID, out var shooterEntity))
                    {
                        continue;
                    }

                    if (!gunDataForEntity.Exists(shooterEntity))
                    {
                        continue;
                    }
                    
                    var gunComponent = gunDataForEntity[shooterEntity];
                    shotInfo.ServerBulletIndex = GunDictionary.Get(gunComponent.GunId).ServerBulletIndex;

                    var spawnProjectileRequest = new ServerProjectileComponent.SpawnProjectile.Request(
                        shooterSpatialID,
                        shotInfo
                    );

                    commandSystem.SendCommand(spawnProjectileRequest);
                }
            }
        }
    }
}
