using UnityEngine;  //Needed for most Unity Enginer manipulations: Vectors, GameObjects, Audio, etc.
using System.IO;    //For data read/write methods
using System;    //For data read/write methods
using System.Collections.Generic;   //Working with Lists and Collections
using System.Reflection;
using System.Linq;   //More advanced manipulation of lists/collections
using HarmonyLib;
using QModManager.API.ModLoading;
using SMLHelper.V2.Crafting;
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.Reefbalance
{
  [QModCore]
  public class ReefbalanceMod
  {
    public const string MOD_KEY = "ReikaKalseki.Reefbalance";
    
    public static readonly Assembly modDLL = Assembly.GetExecutingAssembly();
    
    public static readonly Config<RBConfig.ConfigEntries> config = new Config<RBConfig.ConfigEntries>();
    
    private static readonly TechType[] decoratives = new TechType[]{
    	TechType.ToyCar,
    	TechType.LabEquipment1,
    	TechType.LabEquipment2,
    	TechType.LabEquipment3,
    	TechType.LabContainer,
    };
    
    private static readonly TechType[] smallSeeds = new TechType[]{
    	TechType.BluePalmSeed,
    	TechType.EyesPlantSeed,
    	TechType.KooshChunk,
    	TechType.GabeSFeatherSeed,
    	TechType.MembrainTreeSeed,
    	TechType.PurpleStalkSeed,
    	TechType.RedBushSeed,
    	TechType.RedRollPlantSeed,
    	TechType.RedConePlantSeed,
    };
    
    private static readonly HashSet<TechType> meatFoods = new HashSet<TechType>();
    private static readonly HashSet<TechType> vegFoods = new HashSet<TechType>();

    [QModPrePatch]
    public static void PreLoad()
    {
        config.load();
    }

    [QModPatch]
    public static void Load()
    {        
        Harmony harmony = new Harmony(MOD_KEY);
        Harmony.DEBUG = true;
        FileLog.logPath = Path.Combine(Path.GetDirectoryName(modDLL.Location), "harmony-log.txt");
        FileLog.Log("Ran mod register, started harmony (harmony log)");
        SNUtil.log("Ran mod register, started harmony");
        try {
        	harmony.PatchAll(modDLL);
        }
        catch (Exception ex) {
			FileLog.Log("Caught exception when running patcher!");
			FileLog.Log(ex.Message);
			FileLog.Log(ex.StackTrace);
			FileLog.Log(ex.ToString());
        }
        
        if (config.getBoolean(RBConfig.ConfigEntries.REINF_GLASS)) {
        	BasicCraftingItem baseGlass = new BasicCraftingItem("BaseGlass", "Reinforced Glass", "Laminated glass with titanium reinforcement, suitable for underwater pressure vessels.", "WorldEntities/Natural/Glass");
	        baseGlass.craftingSubCategory = ""+TechCategory.BasicMaterials;
	        baseGlass.craftingTime = 1.5F;
	        baseGlass.numberCrafted = 2;
	        baseGlass.unlockRequirement = TechType.Unobtanium;
	        baseGlass.addIngredient(TechType.Glass, 1).addIngredient(TechType.Titanium, 1);
			baseGlass.sprite = TextureManager.getSprite(modDLL, "Textures/Items/baseglass");
	        baseGlass.Patch();
        
	        HashSet<TechType> set = new HashSet<TechType>{TechType.Spotlight, TechType.Techlight, TechType.Aquarium};
	        for (TechType tt = TechType.BaseRoom; tt <= TechType.BaseNuclearReactor; tt++) {
	        	set.Add(tt);
	        }
	        foreach (TechType tt in set) {
	        	if (RecipeUtil.recipeExists(tt)) {
		        	Ingredient i = RecipeUtil.removeIngredient(tt, TechType.Glass);
		        	if (i != null) {
		        		RecipeUtil.addIngredient(tt, baseGlass.TechType, i.amount);
		        	}
	        	}
	        }
	        
	        TechnologyUnlockSystem.instance.addDirectUnlock(TechType.Glass, baseGlass.TechType);
	    	
	        RecipeUtil.removeIngredient(TechType.EnameledGlass, TechType.Glass);
	        RecipeUtil.addIngredient(TechType.EnameledGlass, baseGlass.TechType, 1);
        }
        
        if (config.getBoolean(RBConfig.ConfigEntries.CHEAP_SEABASE)) {
	        RecipeUtil.modifyIngredients(TechType.BaseRoom, i => {if (i.techType == TechType.Titanium) i.amount = 4; return false;});
	        RecipeUtil.modifyIngredients(TechType.BaseBulkhead, i => {if (i.techType == TechType.Titanium) i.amount = 2; return false;});
	        RecipeUtil.modifyIngredients(TechType.PlanterBox, i => {if (i.techType == TechType.Titanium) i.amount = 3; return false;});
	        RecipeUtil.modifyIngredients(TechType.BaseWaterPark, i => {if (i.techType == TechType.Titanium) i.amount = 1; return false;});
        }
	        
        RecipeUtil.addIngredient(TechType.BasePlanter, TechType.CreepvinePiece, 1);
        
        adjustItemSizes();
        if (config.getBoolean(RBConfig.ConfigEntries.CHEAP_GLASS)) {
        	RecipeUtil.modifyIngredients(TechType.Glass, i => {if (i.techType == TechType.Quartz)i.amount = 1;return false;});
        }
        if (config.getBoolean(RBConfig.ConfigEntries.CHEAP_HUDCHIP)) {
        	RecipeUtil.modifyIngredients(TechType.MapRoomHUDChip, i => i.techType == TechType.Magnetite);
        	RecipeUtil.addIngredient(TechType.MapRoomHUDChip, TechType.Diamond, 1);
        }
        cacheFoodTypes();
        
        RecipeUtil.getRecipe(TechType.LEDLight).craftAmount = 3;
    }
    
    private static void cacheFoodTypes() {
    	meatFoods.Add(TechType.CookedReginald);
    	meatFoods.Add(TechType.CookedBladderfish);
    	meatFoods.Add(TechType.CookedEyeye);
    	meatFoods.Add(TechType.CookedGarryFish);
    	meatFoods.Add(TechType.CookedHoleFish);
    	meatFoods.Add(TechType.CookedHoopfish);
    	meatFoods.Add(TechType.CookedHoverfish);
    	meatFoods.Add(TechType.CookedLavaBoomerang);
    	meatFoods.Add(TechType.CookedLavaEyeye);
    	meatFoods.Add(TechType.CookedOculus);
    	meatFoods.Add(TechType.CookedPeeper);
    	meatFoods.Add(TechType.CookedSpadefish);
    	meatFoods.Add(TechType.CookedSpinefish);
    	
    	meatFoods.Add(TechType.CuredReginald);
    	meatFoods.Add(TechType.CuredBladderfish);
    	meatFoods.Add(TechType.CuredEyeye);
    	meatFoods.Add(TechType.CuredGarryFish);
    	meatFoods.Add(TechType.CuredHoleFish);
    	meatFoods.Add(TechType.CuredHoopfish);
    	meatFoods.Add(TechType.CuredHoverfish);
    	meatFoods.Add(TechType.CuredLavaBoomerang);
    	meatFoods.Add(TechType.CuredLavaEyeye);
    	meatFoods.Add(TechType.CuredOculus);
    	meatFoods.Add(TechType.CuredPeeper);
    	meatFoods.Add(TechType.CuredSpadefish);
    	meatFoods.Add(TechType.CuredSpinefish);
    	
    	meatFoods.Add(TechType.HoleFish);
		meatFoods.Add(TechType.Jumper);
		meatFoods.Add(TechType.Peeper);
		meatFoods.Add(TechType.Oculus);
		meatFoods.Add(TechType.GarryFish);
		//meatFoods.Add(TechType.Slime); //what is this?
		meatFoods.Add(TechType.Boomerang);
		meatFoods.Add(TechType.Eyeye);
		meatFoods.Add(TechType.Bladderfish);
		meatFoods.Add(TechType.Hoverfish);
		meatFoods.Add(TechType.Reginald);
		meatFoods.Add(TechType.Spadefish);
		meatFoods.Add(TechType.Floater);
    	
    	vegFoods.Add(TechType.CreepvinePiece);
    	vegFoods.Add(TechType.Melon);
    	vegFoods.Add(TechType.MelonSeed);
    	vegFoods.Add(TechType.SmallMelon);
    	vegFoods.Add(TechType.BulboTreePiece);
    	vegFoods.Add(TechType.HangingFruit);
    	vegFoods.Add(TechType.PurpleVegetable);
    	vegFoods.Add(TechType.KooshChunk);
    }
    
    private static void adjustItemSizes() {
    	if (config.getBoolean(RBConfig.ConfigEntries.COMPACT_KELP))
        	CraftData.itemSizes[TechType.CreepvinePiece] = new Vector2int(1, 2); //1 wide 2 high
        
        if (config.getBoolean(RBConfig.ConfigEntries.SMALL_TOOLS)) {
	        CraftData.itemSizes[TechType.PropulsionCannon] = new Vector2int(2, 1); //2 wide 1 high     
	        CraftData.itemSizes[TechType.RepulsionCannon] = new Vector2int(2, 1);      
	        CraftData.itemSizes[TechType.Seaglide] = new Vector2int(2, 2);
        }
        
        if (config.getBoolean(RBConfig.ConfigEntries.COMPACT_DECO)) {
	        foreach (TechType deco in decoratives) {
	        	CraftData.itemSizes[deco] = new Vector2int(1, 1);
	        }
        }
        
        if (config.getBoolean(RBConfig.ConfigEntries.COMPACT_SEEDS)) {
			for (int i = (int)TechType.TreeMushroomPiece; i < (int)(object)TechType.HangingFruit-1; i++) {
    			if (Enum.IsDefined(typeof(TechType), i)) {
    				TechType item = (TechType)i;
    				//if (item == TechType.MelonSeed || item == TechType.HangingFruit)
    				//	continue;
		    		CraftData.itemSizes[item] = new Vector2int(1, 1);
    			}
		    }
			for (int i = (int)TechType.MelonSeed+1; i < (int)(object)TechType.SnakeMushroomSpore; i++) {
		    	if (Enum.IsDefined(typeof(TechType), i))
		    		CraftData.itemSizes[(TechType)i] = new Vector2int(1, 1);
		    }
    		//CraftData.itemSizes[TechType.JellyPlantSeed] = new Vector2int(1, 1);
        }
    	
    	//CraftData.itemSizes[TechType.Shocker] = new Vector2int(1, 3);//2,4
    }
    
    public static float getFoodValue(Eatable e, float baseVal) {
		float ret = baseVal;
		if (e.decomposes) {
			RBConfig.ConfigEntries ce = getFoodType(e);
			float elapsed = Mathf.Max(0, DayNightCycle.main.timePassedAsFloat - e.timeDecayStart - 1200*config.getFloat(ce)); //1 day = 1200 float units
			if (elapsed > 0)
				ret = Mathf.Max(baseVal - elapsed * e.kDecayRate, -25f);
		}
		return ret;
    }
    
    private static RBConfig.ConfigEntries getFoodType(Eatable e) {
    	TechType id = CraftData.GetTechType(e.gameObject);
    	if (meatFoods.Contains(id))
    		return RBConfig.ConfigEntries.FOOD_DELAY_MEAT;
    	if (vegFoods.Contains(id))
    		return RBConfig.ConfigEntries.FOOD_DELAY_VEG;
    	return RBConfig.ConfigEntries.FOOD_DELAY;
    }
    /*
    public static void onCreatureActivate(Creature c) {
    	//GameObject container = c.__instance.gameObject;
    	GameObject container = c.gameObject;
    	TechType id = CraftData.GetTechType(container);/*
    	if (id == TechType.Shocker) {
	    	Pickupable p = container.GetComponent<Pickupable>();
	    	if (p == null) {
	    		p = container.AddComponent<Pickupable>();
	    	}
	    	p.isPickupable = true;
    	}*/	/*
    	if (id == TechType.Stalker || id == TechType.Crash || id == TechType.Warper || id == TechType.Crabsnake || id == TechType.Gasopod || id == TechType.Biter || id == TechType.Shocker || id == TechType.BoneShark || id == TechType.Reefback || id == TechType.ReefbackBaby || id == TechType.Sandshark || id == TechType.RabbitRay) {
	    	Pickupable p = container.GetComponent<Pickupable>();
	    	if (p == null) {
	    		return;
	    	}/*
	    	ali = container.GetComponent<Pickupable>();
	    	if (acu != null) {
	    		return;
	    	}
	    	UnityEngine.Object.Destroy(p);
	    	SBUtil.log("De-pickupabled "+c+": "+p+" / "+container+" = "+Enum.GetName(typeof(TechType), id));
	    	 
    	}*//*
    }*/
    
    public static void initializeSeamothStorage(SeamothStorageContainer sc) {
  		sc.width = 6;
  		sc.height = 5;
    }
  
  	public static void calculatePrawnStorage(Exosuit s) {
		int height = 4 + s.modules.GetCount(TechType.VehicleStorageModule);
		s.storageContainer.Resize(8, 2*height);
  	}
  
	public static float getDrillingSpeed(Drillable dr, Exosuit s) {
  		float charge;
  		float capacity;
  		s.energyInterface.GetValues(out charge, out capacity);
  		double f = Math.Sqrt(charge/capacity);
  		float sp = (float)(5*MathUtil.linterpolate(capacity, 400, 2000, 1, 3, true));
  		//SNUtil.writeToChat(charge+"/"+capacity+" ("+f+") > "+sp);
  		return sp;
	}
  }
}
