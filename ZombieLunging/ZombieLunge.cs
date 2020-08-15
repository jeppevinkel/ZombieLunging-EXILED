using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CustomPlayerEffects;
using Exiled.API.Features;
using Exiled.Events;
using Exiled.Events.EventArgs;
using MEC;
using UnityEngine;

namespace ZombieLunging
{
	public class CustomZombie : MonoBehaviour
	{
		private ReferenceHub playerReferenceHub;
		public int victims = 0;

		public bool lunging = false;
		public float cooldown = 0;

		private Scp207 scp207;
		private SinkHole sinkHole;
		private CoroutineHandle forceSlowDownCoroutine;
		private CoroutineHandle forceSpeedUpCoroutine;

		private void Awake()
		{
			Exiled.Events.Handlers.Player.Hurting += OnPlayerHurt;
			Exiled.Events.Handlers.Player.Left += OnPlayerLeave;
			Exiled.Events.Handlers.Server.RestartingRound += OnRoundRestart;
			Exiled.Events.Handlers.Player.ChangingRole += OnSetClass;

			playerReferenceHub = GetComponent<ReferenceHub>();
			scp207 = playerReferenceHub.playerEffectsController.GetEffect<Scp207>();
			sinkHole = playerReferenceHub.playerEffectsController.GetEffect<SinkHole>();
			sinkHole.slowAmount = Plugin.instance.Config.SlowdownAmount;
		}

		public void Destroy()
		{
			Exiled.Events.Handlers.Player.Hurting -= OnPlayerHurt;
			Exiled.Events.Handlers.Player.Left -= OnPlayerLeave;
			Exiled.Events.Handlers.Server.RestartingRound -= OnRoundRestart;
			Exiled.Events.Handlers.Player.ChangingRole -= OnSetClass;

			KillCoroutines();
			GameObject.Destroy((UnityEngine.Object)this);
		}

		public void ActivateSlowdown()
		{
			if (forceSpeedUpCoroutine.IsRunning)
				Timing.KillCoroutines(forceSpeedUpCoroutine);
			forceSlowDownCoroutine = Timing.RunCoroutine(ForceSlowDown(Plugin.instance.Config.PenaltyTime, 0.1f), Segment.FixedUpdate);
		}

		public void ActivateSpeedUp()
		{
			if (forceSlowDownCoroutine.IsRunning)
				Timing.KillCoroutines(forceSlowDownCoroutine);

			victims = 0;
			lunging = true;

			forceSpeedUpCoroutine = Timing.RunCoroutine(ForceSpeedUp(Plugin.instance.Config.LungeTime, 0.1f), Segment.FixedUpdate);
		}

		public void OnPlayerHurt(HurtingEventArgs ev)
		{
			if (ev.Target.Id == playerReferenceHub.playerId && ev.Target.Role == RoleType.Scp0492 && ev.DamageType == DamageTypes.Scp207) ev.Amount = 0.0f;

			if (ev.Attacker.Id == playerReferenceHub.playerId && ev.Target.Id != playerReferenceHub.playerId && lunging)
			{
				victims++;
				ev.Target.ReferenceHub.GetComponent<PlayerSpeeds>().ActivateSlowdown();
				if (!string.IsNullOrEmpty(Plugin.instance.Config.VictimMessage)) ev.Target.Broadcast(5, Plugin.instance.Config.VictimMessage);
			}
		}

		public void OnPlayerLeave(LeftEventArgs ev)
		{
			if (!(ev.Player.Id == playerReferenceHub.playerId)) return;
			Destroy();
		}

		public void OnRoundRestart()
		{
			Destroy();
		}

		public void OnSetClass(ChangingRoleEventArgs ev)
		{
			if (!(ev.Player.Id == playerReferenceHub.playerId)) return;
			Destroy();
		}

		private IEnumerator<float> ForceSlowDown(float totalWaitTime, float interval)
		{
			float waitedTime = 0.0f;
			playerReferenceHub.playerEffectsController.DisableEffect<Scp207>();
			while ((double)waitedTime < (double)totalWaitTime)
			{
				if (!sinkHole.Enabled) playerReferenceHub.playerEffectsController.EnableEffect<SinkHole>();
				waitedTime += interval;
				yield return Timing.WaitForSeconds(interval);
			}
			playerReferenceHub.playerEffectsController.DisableEffect<SinkHole>();
			lunging = false;
			cooldown = Plugin.instance.Config.LungeCooldown;
			Timing.RunCoroutine(DecreaseCooldown(Plugin.instance.Config.LungeCooldown, 0.1f));
		}

		private IEnumerator<float> DecreaseCooldown(float totalWaitTime, float interval)
		{
			float waitedTime = 0.0f;
			while ((double)waitedTime < (double)totalWaitTime)
			{
				waitedTime += interval;
				cooldown = totalWaitTime - waitedTime;
				yield return Timing.WaitForSeconds(interval);
			}

			cooldown = 0;
		}

		private IEnumerator<float> ForceSpeedUp(float totalWaitTime, float interval)
		{
			float waitedTime = 0.0f;
			playerReferenceHub.playerEffectsController.DisableEffect<SinkHole>();
			while ((double)waitedTime < (double)totalWaitTime)
			{
				if (!scp207.Enabled) playerReferenceHub.playerEffectsController.ChangeByString("scp207", (byte)Plugin.instance.Config.ColaIntensity);
				waitedTime += interval;
				yield return Timing.WaitForSeconds(interval);
			}
			playerReferenceHub.playerEffectsController.DisableEffect<Scp207>();

			if (victims == 0)
			{
				lunging = true;
				ActivateSlowdown();
				Player.Get(playerReferenceHub).Broadcast(4, Plugin.instance.Config.PenaltyMessage);
			}
			else
			{
				lunging = false;
				cooldown = Plugin.instance.Config.LungeCooldown;
				Timing.RunCoroutine(DecreaseCooldown(Plugin.instance.Config.LungeCooldown, 0.1f));
			}
		}

		private void KillCoroutines()
		{
			if (forceSlowDownCoroutine.IsRunning) Timing.KillCoroutines(forceSlowDownCoroutine);
			if (!forceSpeedUpCoroutine.IsRunning) return;
			Timing.KillCoroutines(forceSpeedUpCoroutine);
		}
	}

	public class PlayerSpeeds : MonoBehaviour
	{
		private ReferenceHub playerReferenceHub;

		private Scp207 scp207;
		private SinkHole sinkHole;
		private CoroutineHandle forceSlowDownCoroutine;
		private CoroutineHandle forceSpeedUpCoroutine;

		private void Awake()
		{
			Exiled.Events.Handlers.Player.Hurting += OnPlayerHurt;
			Exiled.Events.Handlers.Player.Left += OnPlayerLeave;
			Exiled.Events.Handlers.Server.RestartingRound += OnRoundRestart;

			playerReferenceHub = this.GetComponent<ReferenceHub>();
			scp207 = playerReferenceHub.playerEffectsController.GetEffect<Scp207>();
			sinkHole = playerReferenceHub.playerEffectsController.GetEffect<SinkHole>();
			sinkHole.slowAmount = Plugin.instance.Config.SlowdownAmount;
		}

		public void Destroy()
		{
			Exiled.Events.Handlers.Player.Hurting -= OnPlayerHurt;
			Exiled.Events.Handlers.Player.Left -= OnPlayerLeave;
			Exiled.Events.Handlers.Server.RestartingRound -= OnRoundRestart;

			KillCoroutines();
			GameObject.Destroy((UnityEngine.Object)this);
		}

		public void ActivateSlowdown()
		{
			if (forceSpeedUpCoroutine.IsRunning) Timing.KillCoroutines(forceSpeedUpCoroutine);
			forceSlowDownCoroutine = Timing.RunCoroutine(ForceSlowDown(Plugin.instance.Config.SlowdownTime, 0.1f), Segment.FixedUpdate);
		}

		public void ActivateSpeedUp()
		{
			if (forceSlowDownCoroutine.IsRunning) Timing.KillCoroutines(forceSlowDownCoroutine);

			forceSpeedUpCoroutine = Timing.RunCoroutine(ForceSpeedUp(Plugin.instance.Config.LungeTime, 0.1f), Segment.FixedUpdate);
		}

		public void OnPlayerHurt(HurtingEventArgs ev)
		{
			if (ev.Target.Id == playerReferenceHub.playerId && ev.Target.Role == RoleType.Scp0492 && ev.DamageType == DamageTypes.Scp207) ev.Amount = 0.0f;
		}

		public void OnPlayerLeave(LeftEventArgs ev)
		{
			if (!(ev.Player.Id == playerReferenceHub.playerId)) return;
			Destroy();
		}

		public void OnRoundRestart()
		{
			Destroy();
		}

		private IEnumerator<float> ForceSlowDown(float totalWaitTime, float interval)
		{
			float waitedTime = 0.0f;
			playerReferenceHub.playerEffectsController.DisableEffect<Scp207>();
			while ((double)waitedTime < (double)totalWaitTime)
			{
				if (!sinkHole.Enabled) playerReferenceHub.playerEffectsController.EnableEffect<SinkHole>();
				waitedTime += interval;
				yield return Timing.WaitForSeconds(interval);
			}
			playerReferenceHub.playerEffectsController.DisableEffect<SinkHole>();
		}

		private IEnumerator<float> ForceSpeedUp(float totalWaitTime, float interval)
		{
			float waitedTime = 0.0f;
			playerReferenceHub.playerEffectsController.DisableEffect<SinkHole>();
			while ((double)waitedTime < (double)totalWaitTime)
			{
				if (!scp207.Enabled) playerReferenceHub.playerEffectsController.ChangeByString("scp207", (byte)Plugin.instance.Config.ColaIntensity);
				waitedTime += interval;
				yield return Timing.WaitForSeconds(interval);
			}
			playerReferenceHub.playerEffectsController.DisableEffect<Scp207>();
		}

		private void KillCoroutines()
		{
			if (forceSlowDownCoroutine.IsRunning) Timing.KillCoroutines(forceSlowDownCoroutine);
			if (!forceSpeedUpCoroutine.IsRunning) return;
			Timing.KillCoroutines(forceSpeedUpCoroutine);
		}
	}
}
