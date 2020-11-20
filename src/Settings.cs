using ModSettings;

namespace MooseSatchelMod
{
	internal class MooseSatchelModSettings : JsonModSettings
	{
		[Name("Arrowhead craft time")]
		[Description("Minutes to craft arrowhead. Default is 60, recommended 20-40")]
		[Slider(1, 180)]
		public int arrowHeadCraftTime = 30;

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