using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CustomPlayerEffects;
using EXILED;
using EXILED.Extensions;
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
		private const float forceSlowDownInterval = 0.1f;

		private void Awake()
		{
			Events.PlayerHurtEvent += new Events.PlayerHurt(this.OnPlayerHurt);
			Events.PlayerLeaveEvent += new Events.OnPlayerLeave(this.OnPlayerLeave);
			Events.RoundRestartEvent += new Events.OnRoundRestart(this.OnRoundRestart);
			Events.SetClassEvent += new Events.SetClass(this.OnSetClass);

			this.playerReferenceHub = this.GetComponent<ReferenceHub>();
			this.scp207 = (Scp207)typeof(PlyMovementSync).GetField("_scp207", BindingFlags.Instance | BindingFlags.NonPublic).GetValue((object)this.playerReferenceHub.plyMovementSync);
			this.sinkHole = (SinkHole)typeof(PlyMovementSync).GetField("_sinkhole", BindingFlags.Instance | BindingFlags.NonPublic).GetValue((object)this.playerReferenceHub.plyMovementSync);
			this.sinkHole.slowAmount = Plugin.slowdownAmount;
		}

		public void Destroy()
		{
			Events.PlayerHurtEvent -= new Events.PlayerHurt(this.OnPlayerHurt);
			Events.PlayerLeaveEvent -= new Events.OnPlayerLeave(this.OnPlayerLeave);
			Events.RoundRestartEvent -= new Events.OnRoundRestart(this.OnRoundRestart);
			Events.SetClassEvent -= new Events.SetClass(this.OnSetClass);
			this.KillCoroutines();
			UnityEngine.Object.Destroy((UnityEngine.Object)this);
		}

		public void ActivateSlowdown()
		{
			if (this.forceSpeedUpCoroutine.IsRunning)
				Timing.KillCoroutines(this.forceSpeedUpCoroutine);
			this.forceSlowDownCoroutine = Timing.RunCoroutine(this.ForceSlowDown(Plugin.penaltyTime, 0.1f), Segment.FixedUpdate);
		}

		public void ActivateSpeedUp()
		{
			if (this.forceSlowDownCoroutine.IsRunning)
				Timing.KillCoroutines(this.forceSlowDownCoroutine);

			this.victims = 0;
			lunging = true;

			this.forceSpeedUpCoroutine = Timing.RunCoroutine(this.ForceSpeedUp(Plugin.lungeTime, 0.1f), Segment.FixedUpdate);
		}

		public void OnPlayerHurt(ref PlayerHurtEvent ev)
		{
			if ((UnityEngine.Object)ev.Player == (UnityEngine.Object)this.playerReferenceHub && ev.DamageType == DamageTypes.Scp207)
				ev.Amount = 0.0f;

			if ((UnityEngine.Object)ev.Attacker == (UnityEngine.Object)this.playerReferenceHub && (UnityEngine.Object)ev.Player != (UnityEngine.Object)this.playerReferenceHub && this.lunging)
			{
				this.victims++;

				ev.Player.GetComponent<PlayerSpeeds>().ActivateSlowdown();
				if (!string.IsNullOrEmpty(Plugin.victimMessage)) ev.Player.Broadcast(5, Plugin.victimMessage);
			}
		}

		public void OnPlayerLeave(PlayerLeaveEvent ev)
		{
			if (!((UnityEngine.Object)ev.Player == (UnityEngine.Object)this.playerReferenceHub))
				return;
			this.Destroy();
		}

		public void OnRoundRestart()
		{
			this.Destroy();
		}

		public void OnSetClass(SetClassEvent ev)
		{
			if (!((UnityEngine.Object)ev.Player == (UnityEngine.Object)this.playerReferenceHub))
				return;
			this.Destroy();
		}

		private IEnumerator<float> ForceSlowDown(float totalWaitTime, float interval)
		{
			float waitedTime = 0.0f;
			this.scp207.ServerDisable();
			while ((double)waitedTime < (double)totalWaitTime)
			{
				if (!this.sinkHole.Enabled)
					this.sinkHole.ServerEnable();
				waitedTime += interval;
				yield return Timing.WaitForSeconds(interval);
			}
			this.sinkHole.ServerDisable();
			lunging = false;
			cooldown = Plugin.lungeCooldown;
			Timing.RunCoroutine(DecreaseCooldown(Plugin.lungeCooldown, 0.1f));
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
			this.sinkHole.ServerDisable();
			while ((double)waitedTime < (double)totalWaitTime)
			{
				if (!this.scp207.Enabled)
					this.scp207.ServerEnable();
				waitedTime += interval;
				yield return Timing.WaitForSeconds(interval);
			}
			this.scp207.ServerDisable();

			if (victims == 0)
			{
				lunging = true;
				ActivateSlowdown();
				playerReferenceHub.Broadcast(4, Plugin.penaltyMessage);
			}
			else
			{
				lunging = false;
				cooldown = Plugin.lungeCooldown;
				Timing.RunCoroutine(DecreaseCooldown(Plugin.lungeCooldown, 0.1f));
			}
		}

		private void KillCoroutines()
		{
			if (this.forceSlowDownCoroutine.IsRunning)
				Timing.KillCoroutines(this.forceSlowDownCoroutine);
			if (!this.forceSpeedUpCoroutine.IsRunning)
				return;
			Timing.KillCoroutines(this.forceSpeedUpCoroutine);
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
			Events.PlayerHurtEvent += new Events.PlayerHurt(this.OnPlayerHurt);
			Events.PlayerLeaveEvent += new Events.OnPlayerLeave(this.OnPlayerLeave);
			Events.RoundRestartEvent += new Events.OnRoundRestart(this.OnRoundRestart);

			this.playerReferenceHub = this.GetComponent<ReferenceHub>();
			this.scp207 = (Scp207)typeof(PlyMovementSync).GetField("_scp207", BindingFlags.Instance | BindingFlags.NonPublic).GetValue((object)this.playerReferenceHub.plyMovementSync);
			this.sinkHole = (SinkHole)typeof(PlyMovementSync).GetField("_sinkhole", BindingFlags.Instance | BindingFlags.NonPublic).GetValue((object)this.playerReferenceHub.plyMovementSync);
			this.sinkHole.slowAmount = Plugin.slowdownAmount;
		}

		public void Destroy()
		{
			Events.PlayerHurtEvent -= new Events.PlayerHurt(this.OnPlayerHurt);
			Events.PlayerLeaveEvent -= new Events.OnPlayerLeave(this.OnPlayerLeave);
			Events.RoundRestartEvent -= new Events.OnRoundRestart(this.OnRoundRestart);
			this.KillCoroutines();
			UnityEngine.Object.Destroy((UnityEngine.Object)this);
		}

		public void ActivateSlowdown()
		{
			if (this.forceSpeedUpCoroutine.IsRunning)
				Timing.KillCoroutines(this.forceSpeedUpCoroutine);
			this.forceSlowDownCoroutine = Timing.RunCoroutine(this.ForceSlowDown(Plugin.slowdownTime, 0.1f), Segment.FixedUpdate);
		}

		public void ActivateSpeedUp()
		{
			if (this.forceSlowDownCoroutine.IsRunning)
				Timing.KillCoroutines(this.forceSlowDownCoroutine);

			this.forceSpeedUpCoroutine = Timing.RunCoroutine(this.ForceSpeedUp(Plugin.lungeTime, 0.1f), Segment.FixedUpdate);
		}

		public void OnPlayerHurt(ref PlayerHurtEvent ev)
		{
			if ((UnityEngine.Object)ev.Player == (UnityEngine.Object)this.playerReferenceHub && ev.DamageType == DamageTypes.Scp207)
				ev.Amount = 0.0f;
		}

		public void OnPlayerLeave(PlayerLeaveEvent ev)
		{
			if (!((UnityEngine.Object)ev.Player == (UnityEngine.Object)this.playerReferenceHub))
				return;
			this.Destroy();
		}

		public void OnRoundRestart()
		{
			this.Destroy();
		}

		private IEnumerator<float> ForceSlowDown(float totalWaitTime, float interval)
		{
			float waitedTime = 0.0f;
			this.scp207.ServerDisable();
			while ((double)waitedTime < (double)totalWaitTime)
			{
				if (!this.sinkHole.Enabled)
					this.sinkHole.ServerEnable();
				waitedTime += interval;
				yield return Timing.WaitForSeconds(interval);
			}
			this.sinkHole.ServerDisable();
		}

		private IEnumerator<float> ForceSpeedUp(float totalWaitTime, float interval)
		{
			float waitedTime = 0.0f;
			this.sinkHole.ServerDisable();
			while ((double)waitedTime < (double)totalWaitTime)
			{
				if (!this.scp207.Enabled)
					this.scp207.ServerEnable();
				waitedTime += interval;
				yield return Timing.WaitForSeconds(interval);
			}
			this.scp207.ServerDisable();
		}

		private void KillCoroutines()
		{
			if (this.forceSlowDownCoroutine.IsRunning)
				Timing.KillCoroutines(this.forceSlowDownCoroutine);
			if (!this.forceSpeedUpCoroutine.IsRunning)
				return;
			Timing.KillCoroutines(this.forceSpeedUpCoroutine);
		}
	}
}
