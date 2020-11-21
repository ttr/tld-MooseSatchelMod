using ModSettings;

namespace MooseSatchelMod
{
	internal class MooseSatchelModSettings : JsonModSettings
	{
		[Name("Scent Multiplier")]
		[Description("Recommended: 0.1")]
		[Slider(0f, 1f)]
		public float scent = 0.1f;

		[Name("Indoor base decay multiplier")]
		[Description("Recommended: 0.5")]
		[Slider(0f, 1f)]
		public float indoor = 0.5f;

		[Name("Outdoor base decay multiplier")]
		[Description("Recommended: 0")]
		[Slider(0f, 1f)]
		public float outdoor = 0f;

	}
	internal static class Settings
	{
		public static MooseSatchelModSettings options;
		public static void OnLoad()
		{
			options = new MooseSatchelModSettings();
			options.AddToModSettings("Moose Satchel Settings");
		}
	}
}