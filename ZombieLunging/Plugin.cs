using System;
using System.Collections.Generic;
using EXILED;

namespace ZombieLunging
{
	public class Plugin : EXILED.Plugin
	{
		public EventHandlers EventHandlers;

		public static bool enabled;
		public static float lungeTime;
		public static float slowdownTime;
		public static float penaltyTime;
		public static float slowdownAmount;
		public static string victimMessage;
		public static string penaltyMessage;
		public static string lungeMessage;
		public static float lungeCooldown;
		public static string lungeCooldownMessage;

		public override void OnEnable()
		{
			LoadConfig();
			if (!enabled) return;
			Log.Debug("Initializing event handlers..");
			EventHandlers = new EventHandlers(this);
			Events.ConsoleCommandEvent += EventHandlers.OnConsoleCommand;
			Events.WaitingForPlayersEvent += EventHandlers.OnWaitingForPlayers;
			Events.SetClassEvent += EventHandlers.OnSetClass;
			Log.Info($"ZombieLunging is ready to rumble!");
		}

		public override void OnDisable()
		{
			Events.ConsoleCommandEvent -= EventHandlers.OnConsoleCommand;
			Events.WaitingForPlayersEvent -= EventHandlers.OnWaitingForPlayers;
			Events.SetClassEvent -= EventHandlers.OnSetClass;

			EventHandlers = null;
		}

		public void LoadConfig()
		{
			enabled = Config.GetBool("zl_enable", true);
			lungeTime = Config.GetFloat("zl_lunge_time", 5);
			slowdownTime = Config.GetFloat("zl_slow_time", 3);
			slowdownAmount = Config.GetFloat("zl_slow_amount", 25);
			penaltyTime = Config.GetFloat("zl_penalty_time", 3);
			victimMessage = Config.GetString("zl_victim_message", null);
			penaltyMessage = Config.GetString("zl_penalty_message", null);
			lungeMessage = Config.GetString("zl_lunge_message", null);
			lungeCooldown = Config.GetFloat("zl_lunge_cooldown", 10);
			lungeCooldownMessage = Config.GetString("zl_lunge_cooldown_message", "You are currently on a cooldown, you can lunge again in <color=#ff0000>{time}</color> seconds.");
		}

		public override void OnReload()
		{
			//This is only fired when you use the EXILED reload command, the reload command will call OnDisable, OnReload, reload the plugin, then OnEnable in that order. There is no GAC bypass, so if you are updating a plugin, it must have a unique assembly name, and you need to remove the old version from the plugins folder
		}

		public override string getName { get; } = "ZombieLunging";
	}
}