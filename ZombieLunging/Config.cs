using Exiled.API.Interfaces;

namespace ZombieLunging
{
	public class Config : IConfig
	{
		public bool IsEnabled { get; set; } = true;

		public float LungeTime { get; set; } = 5;
		public float SlowdownTime { get; set; } = 3;
		public float PenaltyTime { get; set; } = 25;
		public float SlowdownAmount { get; set; } = 3;
		public string VictimMessage { get; set; } = null;
		public string PenaltyMessage { get; set; } = null;
		public string LungeMessage { get; set; } = null;
		public float LungeCooldown { get; set; } = 10;
		public string LungeCooldownMessage { get; set; } = "You are currently on a cooldown, you can lunge again in <color=#ff0000>{time}</color> seconds.";
		public int ColaIntensity { get; set; } = 1;
	}
}
