using System;
using Exiled.Events.EventArgs;
using Object = UnityEngine.Object;

namespace ZombieLunging
{
	public class EventHandlers
	{
		public Plugin plugin;
		public EventHandlers(Plugin plugin) => this.plugin = plugin;

		public void OnSetClass(ChangingRoleEventArgs ev)
		{
			if (ev.Player.Nickname == "Dedicated Server") return;

			PlayerSpeeds component1 = ev.Player.ReferenceHub.gameObject.GetComponent<PlayerSpeeds>();
			if ((Object)component1 != (Object)null) component1.Destroy();
			ev.Player.ReferenceHub.gameObject.AddComponent<PlayerSpeeds>();

			CustomZombie component = ev.Player.ReferenceHub.gameObject.GetComponent<CustomZombie>();
			if (ev.NewRole != RoleType.Scp0492) return;
			if ((Object)component != (Object)null) component.Destroy();
			ev.Player.ReferenceHub.gameObject.AddComponent<CustomZombie>();

			MEC.Timing.CallDelayed(3f, () => ev.Player.Position = Exiled.API.Features.Map.GetRandomSpawnPoint(RoleType.Scp173));
		}

		public void OnConsoleCommand(SendingConsoleCommandEventArgs ev)
		{
			if (ev.Player.Role != RoleType.Scp0492) return;
			if (ev.Name.ToLower() == "lunge")
			{
				if (ev.Player.ReferenceHub.GetComponent<CustomZombie>().cooldown > 0)
				{
					if (!string.IsNullOrEmpty(Plugin.instance.Config.LungeMessage)) ev.Player.Broadcast(2, Plugin.instance.Config.LungeCooldownMessage.Replace("{time}",  Math.Round(ev.Player.ReferenceHub.GetComponent<CustomZombie>().cooldown).ToString()));
				}
				else if (!ev.Player.ReferenceHub.GetComponent<CustomZombie>().lunging)
				{
					if (!string.IsNullOrEmpty(Plugin.instance.Config.LungeMessage)) ev.Player.Broadcast(5, Plugin.instance.Config.LungeMessage);
					ev.Player.ReferenceHub.GetComponent<CustomZombie>().ActivateSpeedUp();
					ev.ReturnMessage = !string.IsNullOrEmpty(Plugin.instance.Config.LungeMessage) ? Plugin.instance.Config.LungeMessage : "You have activated your lunge!";
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