using ModSettings;

namespace MooseSatchelMod
{
	internal class MooseSatchelModSettings : JsonModSettings
	{
		[Name("Scent Multiplier")]
		[Description("Multiplier of scent of items in bag. Lower means less scent.\nRecommended: 0.1")]
		[Slider(0f, 1f)]
		public float scent = 0.1f;

		[Name("Indoor base decay multiplier")]
		[Description("Mutiplier of decay of perishabe food, that is in bag while indoor. Lower means slower decay.\nRecommended: 0.5")]
		[Slider(0f, 1f)]
		public float indoor = 0.5f;

		[Name("Outdoor base decay multiplier")]
		[Description("Mutiplier of decay of perishabe food, that is in bag while outdoor. Lower means slower decay.\nRecommended: 0 (no decay)")]
		[Slider(0f, 1f)]
		public float outdoor = 0f;

		[Name("Store processed food")]
		[Description("Items like veggie pies or cured meat/fish. Those do not give away scent, but keeping them in bag, slows decay. Recommended: True")]
		public bool storeNonSmellyCooked = true;

	}
	internal static class Settings
	{
		public static MooseSatchelModSettings options = new();
		public static void OnLoad()
		{
			options = new MooseSatchelModSettings();
			options.AddToModSettings("Moose Satchel Settings");
		}
	}
}