using Exiled.API.Features;

namespace ZombieLunging
{
	public class Plugin : Plugin<Config>
	{
		public EventHandlers EventHandlers;
		public static Plugin instance;

		public override void OnEnabled()
		{
			base.OnEnabled();

			instance = this;
			if (!Config.IsEnabled) return;
			Log.Debug("Initializing event handlers..");
			EventHandlers = new EventHandlers(this);

			Exiled.Events.Handlers.Server.SendingConsoleCommand += EventHandlers.OnConsoleCommand;
			Exiled.Events.Handlers.Player.ChangingRole += EventHandlers.OnSetClass;

			Log.Info($"ZombieLunging is ready to rumble!");
		}

		public override void OnDisabled()
		{
			base.OnDisabled();

			Exiled.Events.Handlers.Server.SendingConsoleCommand -= EventHandlers.OnConsoleCommand;
			Exiled.Events.Handlers.Player.ChangingRole -= EventHandlers.OnSetClass;

			EventHandlers = null;
		}

		public override string Name => "ZombieLunging";
	}
}