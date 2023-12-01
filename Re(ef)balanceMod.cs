using UnityEngine;  //Needed for most Unity Enginer manipulations: Vectors, GameObjects, Audio, etc.
using System.IO;    //For data read/write methods
using System;    //For data read/write methods
using System.Collections.Generic;   //Working with Lists and Collections
using System.Reflection;
using System.Linq;   //More advanced manipulation of lists/collections
using HarmonyLib;
using QModManager.API.ModLoading;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Crafting;
using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.Reefbalance
{
  [QModCore]
  public class ReefbalanceMod
  {
    public const string MOD_KEY = "ReikaKalseki.Reefbalance";
    
    public static readonly Assembly modDLL = Assembly.GetExecutingAssembly();
    
    public static readonly Config<RBConfig.ConfigEntries> config = new Config<RBConfig.ConfigEntries>(modDLL);
    
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
    private static readonly Dictionary<TechType, int> scanCountOverrides = new Dictionary<TechType, int>();
    
    public static event Action<Dictionary<TechType, int>> scanCountOverridesCalculation;

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
        
        ModVersionCheck.getFromGitVsInstall("Re(ef)Balance", modDLL, "Reefbalance").register();
        SNUtil.checkModHash(modDLL);
	    	
        DIHooks.onFruitPlantTickEvent += fpt => {
        	FruitPlant fp = fpt.getPlant();
        	if (CraftData.GetTechType(fp.gameObject) == TechType.HangingFruitTree)
        		fp.fruitSpawnInterval = fpt.getBaseGrowthTime()/config.getFloat(RBConfig.ConfigEntries.LANTERN_SPEED);
        };
        
        DIHooks.onSkyApplierSpawnEvent += (sk) => {
        	if (ObjectUtil.isDragonRepellent(sk.gameObject)) {
	    		sk.gameObject.EnsureComponent<ContainmentFacilityDragonRepellent>();
	    		return;
	    	}
        	if (config.getBoolean(RBConfig.ConfigEntries.LARGE_CYCLOCKER)) {
	        	SubRoot sr = sk.gameObject.GetComponentInParent<SubRoot>();
	        	if (sr && sr.isCyclops) {
	        		foreach (CyclopsLocker cl in sk.GetComponentsInChildren<CyclopsLocker>()) {
		        		StorageContainer sc = cl.GetComponent<StorageContainer>();
		        		sc.Resize(6, 8);
	        		}
	        	}
        	}
        };
        
        DIHooks.knifeHarvestEvent += h => {
        	if (h.drops.Count > 0 && config.getBoolean(RBConfig.ConfigEntries.DOUBLE_THERMAL_CORAL) && Inventory.main.GetHeld().GetTechType() == TechType.HeatBlade) {
        		if (h.objectType == TechType.BigCoralTubes)
        			h.drops[h.defaultDrop] = h.drops[h.defaultDrop]*2;
        		else if (h.objectType == TechType.TreeMushroom)
        			h.drops[h.defaultDrop] = h.drops[h.defaultDrop]*2;
        	}
        };
        
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
        	RecipeUtil.modifyIngredients(TechType.Glass, i => {if (i.techType == TechType.Quartz) i.amount = 1; return false;});
        }
        if (config.getBoolean(RBConfig.ConfigEntries.CHEAP_HUDCHIP)) {
        	RecipeUtil.modifyIngredients(TechType.MapRoomHUDChip, i => i.techType == TechType.Magnetite);
        	RecipeUtil.addIngredient(TechType.MapRoomHUDChip, TechType.Diamond, 1);
        }
        cacheFoodTypes();
        
        RecipeUtil.getRecipe(TechType.LEDLight).craftAmount = 3;
    }
    
    [QModPostPatch]
    public static void PostLoad() {
    	TechTypeMappingConfig<int>.loadInline("fragment_scan_requirements", TechTypeMappingConfig<int>.IntParser.instance, TechTypeMappingConfig<int>.dictionaryAssign(scanCountOverrides));
    	
    	if (scanCountOverridesCalculation != null)
    		scanCountOverridesCalculation.Invoke(scanCountOverrides);
        
    	foreach (KeyValuePair<TechType, int> kvp in scanCountOverrides) {
        	PDAHandler.EditFragmentsToScan(kvp.Key, kvp.Value);
        	SNUtil.log("Setting fragment scan requirement: "+kvp.Key+" = "+kvp.Value);
    	}
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
    
    public static void initializeSeamothStorage(SeamothStorageContainer sc) {
  		sc.width = 6;
  		sc.height = 5;
    }
  
  	public static void calculatePrawnStorage(Exosuit s) {
		int height = 4 + s.modules.GetCount(TechType.VehicleStorageModule);
		s.storageContainer.Resize(8, 2*height);
  	}
  
	public static float getDrillingSpeed(Drillable dr, Exosuit s) {
    	if (!s)
    		return 3;
  		float charge;
  		float capacity;
  		s.energyInterface.GetValues(out charge, out capacity);
  		double f = Math.Sqrt(charge/capacity);
  		float sp = (float)(5*MathUtil.linterpolate(capacity, 400, 2000, 1, 3, true));
  		//SNUtil.writeToChat(charge+"/"+capacity+" ("+f+") > "+sp);
  		return sp;
	}
    
    public static bool canBuildingDestroyObject(GameObject go) {
    	return !config.getBoolean(RBConfig.ConfigEntries.NO_BUILDER_CLEAR) && Builder.CanDestroyObject(go);
    }
    
    public static bool deleteDuplicateDatabox(TechType tt) {
    	return !config.getBoolean(RBConfig.ConfigEntries.ALWAYS_SPAWN_DB) && KnownTech.Contains(tt);
    }
	
	class ContainmentFacilityDragonRepellent : MonoBehaviour {
		
		void Update() {
			float r = 120;
			if (Player.main.transform.position.y <= 1350 && Vector3.Distance(transform.position, Player.main.transform.position) <= 100) {
				RaycastHit[] hit = Physics.SphereCastAll(gameObject.transform.position, r, new Vector3(1, 1, 1), r);
				foreach (RaycastHit rh in hit) {
					if (rh.transform != null && rh.transform.gameObject) {
						SeaDragon c = rh.transform.gameObject.GetComponent<SeaDragon>();
						if (c) {
							Vector3 vec = transform.position+((c.transform.position-transform.position).normalized*120);
							c.GetComponent<SwimBehaviour>().SwimTo(vec, 20);
						}
					}
				}
			}
		}
		
	}
  }
}
