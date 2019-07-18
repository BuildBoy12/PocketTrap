using Smod2;
using Smod2.API;
using Smod2.Attributes;
using Smod2.Config;
using Smod2.EventHandlers;
using Smod2.Events;
using UnityEngine;
using MEC;
using System.Collections.Generic;
using ServerMod2.API;

namespace PocketTrap
{
    [PluginDetails(
    author = "sanyae2439",
    name = "PocketTrap",
    description = "add trap function to SCP-106 portal",
    id = "sanyae2439.pockettrap",
    configPrefix = "ptrap",
    version = "1.0",
    SmodMajor = 3,
    SmodMinor = 5,
    SmodRevision = 0
    )]
    public class PocketTrap : Plugin
    {
        static internal PocketTrap instance;

        [ConfigOption]
        internal bool Animation = false;
        [ConfigOption]
        internal bool ScpTeleport = true;
        [ConfigOption]
        internal float Range = 2.5f;
        [ConfigOption]
        internal float Cooltime = 10.0f;

        public PocketTrap()
        {
            PocketTrap.instance = this;
        }

        public override void OnDisable()
        {
            this.Info("Pocket Trap Disabled");
        }

        public override void OnEnable()
        {
            this.Info("Pocket Trap Enabled!");
        }

        public override void Register()
        {
            this.AddEventHandlers(new EventHandler());
        }

        public IEnumerator<float> _106PortalAnimation(Player player)
        {
            GameObject gameObject = player.GetGameObject() as GameObject;
            Scp106PlayerScript ply106 = gameObject.GetComponent<Scp106PlayerScript>();
            PlyMovementSync pms = gameObject.GetComponent<PlyMovementSync>();

            if(ply106.goingViaThePortal) yield break;

            ply106.goingViaThePortal = true;
            if(PocketTrap.instance.Animation)
            {
                pms.SetAllowInput(false);

                for(float i = 0f; i < 50; i++)
                {
                    var pos = gameObject.transform.position;
                    pos.y -= i * 0.01f;
                    pms.SetPosition(pos);
                    yield return 0f;
                }
                if(AlphaWarheadController.host.doorsClosed)
                {
                    if(player.TeamRole.Team != Smod2.API.Team.SCP) player.Damage(500, DamageType.POCKET);
                }
                else
                {
                    if(player.TeamRole.Team != Smod2.API.Team.SCP) player.Damage(40, DamageType.SCP_106);
                    pms.SetPosition(Vector3.down * 1997f);
                }
                pms.SetAllowInput(true);
            }
            else
            {
                if(AlphaWarheadController.host.doorsClosed)
                {
                    if(player.TeamRole.Team != Smod2.API.Team.SCP) player.Damage(500, DamageType.POCKET);
                }
                else
                {
                    if(player.TeamRole.Team != Smod2.API.Team.SCP) player.Damage(40, DamageType.SCP_106);
                    pms.SetPosition(Vector3.down * 1997f);
                }
            }
            yield return Timing.WaitForSeconds(Cooltime);
            ply106.goingViaThePortal = false;
            yield break;
        }
    }

    public class EventHandler : IEventHandlerWaitingForPlayers, IEventHandlerFixedUpdate, IEventHandlerPocketDimensionDie, IEventHandlerPocketDimensionExit
    {
        GameObject portal = null;

        public void OnWaitingForPlayers(WaitingForPlayersEvent ev)
        {
            portal = null;
        }

        public void OnPocketDimensionDie(PlayerPocketDimensionDieEvent ev)
        {
            PocketTrap.instance.Debug($"[OnPocketDimensionDie] {ev.Player.Name}<{ev.Player.TeamRole.Role}> / {ev.Die}");
            if(ev.Player.TeamRole.Team == Smod2.API.Team.SCP)
            {
                ev.Die = false;
                ev.Player.Teleport(new Vector(portal.transform.position.x, portal.transform.position.y, portal.transform.position.z) + Vector.Up * 1.5f);
            }
        }

        public void OnPocketDimensionExit(PlayerPocketDimensionExitEvent ev)
        {
            PocketTrap.instance.Debug($"[OnPocketDimensionExit] {ev.Player.Name}<{ev.Player.TeamRole.Role}> / {ev.ExitPosition}");
            if(ev.Player.TeamRole.Team == Smod2.API.Team.SCP)
            {
                ev.ExitPosition = new Vector(portal.transform.position.x, portal.transform.position.y, portal.transform.position.z) + Vector.Up * 1.5f;
            }
        }

        public void OnFixedUpdate(FixedUpdateEvent ev)
        {
            if(portal != null)
            {
                foreach(Player player in PocketTrap.instance.Server.GetPlayers())
                {
                    if( (Vector3.Distance(player.GetPosition().ToVector3(), portal.transform.position) < PocketTrap.instance.Range && !(player.GetGameObject() as GameObject).GetComponent<Scp106PlayerScript>().goingViaThePortal)
                        && (PocketTrap.instance.ScpTeleport || player.TeamRole.Team != Smod2.API.Team.SCP)
                        )
                    {
                        PocketTrap.instance.Debug($"[OnFixedUpdate] Target found:{player.Name}<{player.TeamRole.Role}>");
                        Timing.RunCoroutine(PocketTrap.instance._106PortalAnimation(player), Segment.FixedUpdate);
                    }
                }
            }
            else
            {
                portal = GameObject.Find("SCP106_PORTAL");
            }
        }
    }
}