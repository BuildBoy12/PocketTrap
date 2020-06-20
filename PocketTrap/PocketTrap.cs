using UnityEngine;
using EXILED;
using System.Collections.Generic;
using MEC;
using EXILED.Extensions;
using System.Linq;
using Pocket_Trap;

namespace Pocket_Trap
{
    public class PocketTrap : Plugin
    {
        public List<CoroutineHandle> Handle = new List<CoroutineHandle>();
        CoroutineHandle waitcoroutine;

        internal bool Enabled;
        internal bool Damage;
        internal float DamageAmount;
        internal float Cooldown;
        internal bool Animation;
        internal List<int> IgnoredTeams;
        internal List<int> IgnoredRoles;      
        internal float PortalRange;
        internal bool Ignore035;

        GameObject portal = null;
        public bool Active;

        public override void OnEnable()
        {
            ReloadConfigs();
            if (!Enabled)
                return;

            Events.RoundStartEvent += OnRoundStart;
            Events.Scp106CreatedPortalEvent += OnCreatePortal;
        }

        public override void OnDisable()
        {
            Timing.KillCoroutines(Handle);
            Events.RoundStartEvent -= OnRoundStart;
            Events.Scp106CreatedPortalEvent -= OnCreatePortal;
        }

        public override void OnReload()
        {
            
        }

        public override string getName => "PocketTrap";

        internal void ReloadConfigs()
        {
            Enabled = Config.GetBool("pt_enable", true);
            Damage = Config.GetBool("pt_damage", true);
            DamageAmount = Config.GetFloat("pt_damage_amount", 40);
            IgnoredTeams = Config.GetIntList("pt_ignored_teams");
            IgnoredRoles = Config.GetIntList("pt_ignored_roles");
            Ignore035 = Config.GetBool("pt_ignored_035");
            PortalRange = Config.GetFloat("pt_range", 2.5f);
            Cooldown = Config.GetFloat("pt_cooldown", 5);
            Animation = Config.GetBool("pt_animation", true);
        }

        public void OnRoundStart()
        {
            Handle.Add(Timing.RunCoroutine(CheckPositions()));
        }

        public void OnCreatePortal(Scp106CreatedPortalEvent ev)
        {
            if (waitcoroutine == null)
            {
                waitcoroutine = Timing.RunCoroutine(WaitForPortalActivated(), Segment.FixedUpdate);
            }
            else
            {
                Timing.KillCoroutines(waitcoroutine);
                waitcoroutine = Timing.RunCoroutine(WaitForPortalActivated(), Segment.FixedUpdate);
            }
        }

        public IEnumerator<float> CheckPositions()
        {
            for (; ; )
            {
                if (portal != null)
                {
                    foreach (ReferenceHub hub in Player.GetHubs().ToList().FindAll(x => x.GetTeam() != Team.RIP && x.GetRole() != RoleType.Scp079))
                    {
                        if (!IgnoredTeams.Contains((int)hub.GetTeam()) && !IgnoredRoles.Contains((int)hub.GetRole()))
                        {
                            if (Vector3.Distance(hub.GetPosition(), portal.transform.position) < PortalRange
                                && !hub.gameObject.GetComponent<Scp106PlayerScript>().goingViaThePortal
                                && Active
                                && (!Ignore035 || !hub.gameObject.GetComponent<ServerRoles>().GetUncoloredRoleString().Contains("SCP-035")))
                            {
                                Handle.Add(Timing.RunCoroutine(Teleportation(hub), Segment.FixedUpdate));
                            }
                        }
                    }
                }
                else
                {
                    portal = GameObject.Find("SCP106_PORTAL");
                }
                yield return Timing.WaitForOneFrame;
            }
        }

        public IEnumerator<float> Teleportation(ReferenceHub hub)
        {
            GameObject gameObject = hub.gameObject;
            Scp106PlayerScript ply106 = gameObject.GetComponent<Scp106PlayerScript>();
            PlayerEffectsController effects = hub.GetComponentInParent<PlayerEffectsController>();

            if (ply106.goingViaThePortal) yield break;

            ply106.goingViaThePortal = true;
            if(Animation)
            {
                for (float i = 0f; i < 50; i++)
                {
                    var pos = hub.GetPosition();
                    pos.y -= i * 0.01f;
                    hub.SetPosition(pos);
                    yield return 0f;
                }
                if(Map.IsNukeDetonated)
                {
                    if(!hub.characterClassManager.IsAnyScp() && Damage)
                    {
                        hub.Kill(DamageTypes.Pocket);
                    }
                }               
                else
                {
                    if(!hub.characterClassManager.IsAnyScp() && Damage)
                    {
                        hub.AddHealth(-DamageAmount);                    
                        hub.SetPosition(Vector3.down * 1997f);
                        if (hub.GetHealth() <= 0)
                        {
                            yield return Timing.WaitForSeconds(.5f);
                            hub.Kill(DamageTypes.Pocket);
                            yield return Timing.WaitForSeconds(Cooldown);
                            ply106.goingViaThePortal = false;
                            yield break;
                        }
                        effects.EnableByString("Corroding");
                    }
                }
            }
            else
            {
                if (Map.IsNukeDetonated)
                {
                    if (!hub.characterClassManager.IsAnyScp() && Damage)
                    {
                        hub.SetPosition(Vector3.down * 1997f);
                        yield return Timing.WaitForSeconds(.1f);
                        hub.Kill(DamageTypes.Pocket);
                    }
                }
                else
                {
                    if (!hub.characterClassManager.IsAnyScp() && Damage)
                    {
                        hub.AddHealth(-DamageAmount);                
                        hub.SetPosition(Vector3.down * 1997f);
                        if (hub.GetHealth() <= 0)
                        {
                            yield return Timing.WaitForSeconds(.1f);
                            hub.Kill(DamageTypes.Pocket);
                            yield return Timing.WaitForSeconds(Cooldown);
                            ply106.goingViaThePortal = false;
                            yield break;
                        }                             
                        effects.EnableByString("Corroding");
                    }
                }
            }
            yield return Timing.WaitForSeconds(Cooldown);
            ply106.goingViaThePortal = false;
            yield break;
        }

        public IEnumerator<float> WaitForPortalActivated()
        {
            Active = false;
            yield return Timing.WaitForSeconds(4f);
            Active = true;
            yield break;
        }
    }
}