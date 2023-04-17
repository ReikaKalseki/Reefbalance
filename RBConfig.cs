using System;

using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Xml;
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.Reefbalance
{
	public class RBConfig
	{		
		public enum ConfigEntries {
			[ConfigEntry("Reduce creepvine sample size to 1x2", true)]COMPACT_KELP,
			[ConfigEntry("Make nonfunctional decorative blocks 1x1", true)]COMPACT_DECO,
			[ConfigEntry("Make decorative seeds 1x1", true)]COMPACT_SEEDS,
			[ConfigEntry("Downsize some of the larger tools to 1x2 or 2x2", false)]SMALL_TOOLS,
			[ConfigEntry("Vegetable-Based food spoilage start delay (days)", typeof(float), 0.75F, 0, 100, 0)]FOOD_DELAY_VEG,
			[ConfigEntry("Meat-Based food spoilage start delay (days)", typeof(float), 0.25F, 0, 100, 0)]FOOD_DELAY_MEAT,
			[ConfigEntry("Generic food spoilage start delay (days)", typeof(float), 0.5F, 0, 100, 0)]FOOD_DELAY,
			[ConfigEntry("Reduce glass cost to 1 quartz", false)]CHEAP_GLASS,
			[ConfigEntry("Replace scanner HUD chip magnetite", true)]CHEAP_HUDCHIP,
			[ConfigEntry("Replace glass in seabase parts with reinforced glass which costs half as much quartz", true)]REINF_GLASS,
			[ConfigEntry("Reduce cost of some seabase components", true)]CHEAP_SEABASE,
			[ConfigEntry("Increase size of cyclops lockers", true)]LARGE_CYCLOCKER,
			[ConfigEntry("Thermoblade gives doubled coral tube yield", true)]DOUBLE_THERMAL_CORAL,
			[ConfigEntry("Thermoblade gives doubled mushroom disk yield", true)]DOUBLE_THERMAL_MUSHDISK,
			[ConfigEntry("Lantern Tree Growth Rate Multiplier", typeof(float), 1F, 0.1F, 10F, 1)]LANTERN_SPEED,
		}
	}
}
