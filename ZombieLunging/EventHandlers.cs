using System;
using System.Collections.Generic;
using CustomPlayerEffects;
using EXILED;
using EXILED.Extensions;
using EXILED.Patches;
using Grenades;
using MEC;
using Object = UnityEngine.Object;
using PlayerHurtEvent = EXILED.PlayerHurtEvent;
using SetClassEvent = EXILED.SetClassEvent;

namespace ZombieLunging
{
	public class EventHandlers
	{
		public Plugin plugin;
		public EventHandlers(Plugin plugin) => this.plugin = plugin;

		public void OnWaitingForPlayers()
		{
			plugin.LoadConfig();
		}

		public void OnSetClass(SetClassEvent ev)
		{
			if (ev.Player.GetNickname() == "Dedicated Server")
				return;

			PlayerSpeeds component1 = ev.Player.gameObject.GetComponent<PlayerSpeeds>();
			if ((Object)component1 != (Object)null)
				component1.Destroy();
			ev.Player.gameObject.AddComponent<PlayerSpeeds>();

			CustomZombie component = ev.Player.gameObject.GetComponent<CustomZombie>();
			if (ev.Role != RoleType.Scp0492)
				return;
			if ((Object)component != (Object)null)
				component.Destroy();
			ev.Player.gameObject.AddComponent<CustomZombie>();
		}

		public void OnConsoleCommand(ConsoleCommandEvent ev)
		{
			if (ev.Player.GetRole() != RoleType.Scp0492) return;
			if (ev.Command == "lunge")
			{
				if (ev.Player.GetComponent<CustomZombie>().cooldown > 0)
				{
					if (!string.IsNullOrEmpty(Plugin.lungeMessage)) ev.Player.Broadcast(2, Plugin.lungeCooldownMessage.Replace("{time}",  Math.Round(ev.Player.GetComponent<CustomZombie>().cooldown).ToString()));
				}
				else if (!ev.Player.GetComponent<CustomZombie>().lunging)
				{
					if (!string.IsNullOrEmpty(Plugin.lungeMessage)) ev.Player.Broadcast(5, Plugin.lungeMessage);
					ev.Player.GetComponent<CustomZombie>().ActivateSpeedUp();
					ev.ReturnMessage = !string.IsNullOrEmpty(Plugin.lungeMessage) ? Plugin.lungeMessage : "You have activated your lunge!";
					ev.Color = "Green";
				}
				else
				{
					ev.ReturnMessage = "You are already lunging!";
					ev.Color = "Red";
				}
			}
		}
	}
}