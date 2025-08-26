/*
 * Created by SharpDevelop.
 * User: Reika
 * Date: 04/11/2019
 * Time: 11:28 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections;   //Working with Lists and Collections
using System.Collections.Generic;   //Working with Lists and Collections
using System.IO;    //For data read/write methods
using System.Linq;   //More advanced manipulation of lists/collections
using System.Reflection;
using System.Reflection.Emit;

using HarmonyLib;

using ReikaKalseki.DIAlterra;

using UnityEngine;  //Needed for most Unity Enginer manipulations: Vectors, GameObjects, Audio, etc.

namespace ReikaKalseki.Reefbalance {

	static class RBPatches {

		[HarmonyPatch(typeof(Eatable))]
		[HarmonyPatch("GetFoodValue")]
		public static class DecayRateAndTimePatch {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InsnList codes = new InsnList(instructions);
				try {/*
				int sub = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Sub);
				InsnList inject = new InsnList();
				inject.add(OpCodes.Ldsfld, InstructionHandlers.convertFieldOperand("ReikaKalseki.Reefbalance.ReefbalanceMod", "onRoomFindMachine"));
				codes.InsertRange(sub+1, inject);
				*/
					codes.Clear();
					codes.add(OpCodes.Ldarg_0);
					codes.add(OpCodes.Ldarg_0);
					codes.add(OpCodes.Ldfld, InstructionHandlers.convertFieldOperand("Eatable", "foodValue"));
					codes.invoke("ReikaKalseki.Reefbalance.ReefbalanceMod", "getFoodValue", false, typeof(Eatable), typeof(float));
					codes.add(OpCodes.Ret);
					FileLog.Log("Done patch " + MethodBase.GetCurrentMethod().DeclaringType);
					//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
				}
				catch (Exception e) {
					FileLog.Log("Caught exception when running patch " + MethodBase.GetCurrentMethod().DeclaringType + "!");
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}
		/*
		[HarmonyPatch(typeof(Creature))]
		[HarmonyPatch("Start")]
		public static class CreatureActivateHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InsnList codes = new InsnList(instructions);
				try {/*
					int sub = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Sub);
					InsnList inject = new InsnList();
					inject.add(OpCodes.Ldsfld, InstructionHandlers.convertFieldOperand("ReikaKalseki.Reefbalance.ReefbalanceMod", "onRoomFindMachine"));
					codes.InsertRange(sub+1, inject);
					*//*
					codes.Insert(0, new CodeInstruction(OpCodes.Ldarg_0));
					codes.Insert(1, InstructionHandlers.createMethodCall("ReikaKalseki.Reefbalance.ReefbalanceMod", "onCreatureActivate", false, typeof(Creature)));
					FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
					//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
				}
				catch (Exception e) {
					FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}*/

		[HarmonyPatch(typeof(SeamothStorageContainer))]
		[HarmonyPatch("Init")]
		public static class SeamothStorageBoost {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InsnList codes = new InsnList(instructions);
				try {/*
				int sub = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Sub);
				InsnList inject = new InsnList();
				inject.add(OpCodes.Ldsfld, InstructionHandlers.convertFieldOperand("ReikaKalseki.Reefbalance.ReefbalanceMod", "onRoomFindMachine"));
				codes.InsertRange(sub+1, inject);
				*/
					codes.patchInitialHook(new CodeInstruction(OpCodes.Ldarg_0), InstructionHandlers.createMethodCall("ReikaKalseki.Reefbalance.ReefbalanceMod", "initializeSeamothStorage", false, typeof(SeamothStorageContainer)));
					FileLog.Log("Done patch " + MethodBase.GetCurrentMethod().DeclaringType);
					//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
				}
				catch (Exception e) {
					FileLog.Log("Caught exception when running patch " + MethodBase.GetCurrentMethod().DeclaringType + "!");
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(Exosuit))]
		[HarmonyPatch("UpdateStorageSize")]
		public static class PrawnStorageBoost {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InsnList codes = new InsnList();
				try {/*
				int sub = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Sub);
				InsnList inject = new InsnList();
				inject.add(OpCodes.Ldsfld, InstructionHandlers.convertFieldOperand("ReikaKalseki.Reefbalance.ReefbalanceMod", "onRoomFindMachine"));
				codes.InsertRange(sub+1, inject);
				*/
					codes.add(OpCodes.Ldarg_0);
					codes.invoke("ReikaKalseki.Reefbalance.ReefbalanceMod", "calculatePrawnStorage", false, typeof(Exosuit));
					codes.add(OpCodes.Ret);
					FileLog.Log("Done patch " + MethodBase.GetCurrentMethod().DeclaringType);
					//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
				}
				catch (Exception e) {
					FileLog.Log("Caught exception when running patch " + MethodBase.GetCurrentMethod().DeclaringType + "!");
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(Drillable))]
		[HarmonyPatch("OnDrill")]
		public static class PrawnDrillSpeedHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InsnList codes = new InsnList(instructions);
				try {
					int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Ldc_R4, 5F);
					codes[idx] = InstructionHandlers.createMethodCall("ReikaKalseki.Reefbalance.ReefbalanceMod", "getDrillingSpeed", false, typeof(Drillable), typeof(Exosuit));
					codes.InsertRange(idx, new InsnList { new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldarg_2) });
					FileLog.Log("Done patch " + MethodBase.GetCurrentMethod().DeclaringType);
					//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
				}
				catch (Exception e) {
					FileLog.Log("Caught exception when running patch " + MethodBase.GetCurrentMethod().DeclaringType + "!");
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(ConstructableBase))]
		[HarmonyPatch("SetState")]
		public static class BuildingDestroyCollidingCheck {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InsnList codes = new InsnList(instructions);
				try {
					int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Call, "Builder", "CanDestroyObject", false, new Type[]{typeof(GameObject)});
					codes[idx] = InstructionHandlers.createMethodCall("ReikaKalseki.Reefbalance.ReefbalanceMod", "canBuildingDestroyObject", false, typeof(GameObject));
					FileLog.Log("Done patch " + MethodBase.GetCurrentMethod().DeclaringType);
					//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
				}
				catch (Exception e) {
					FileLog.Log("Caught exception when running patch " + MethodBase.GetCurrentMethod().DeclaringType + "!");
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

		[HarmonyPatch(typeof(DataboxSpawner))]
		[HarmonyPatch("Start")]
		public static class DataboxDuplicateRemovalHook {

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				InsnList codes = new InsnList(instructions);
				try {
					int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Call, "KnownTech", "Contains", false, new Type[]{typeof(TechType)});
					codes[idx] = InstructionHandlers.createMethodCall("ReikaKalseki.Reefbalance.ReefbalanceMod", "deleteDuplicateDatabox", false, typeof(TechType));
					FileLog.Log("Done patch " + MethodBase.GetCurrentMethod().DeclaringType);
					//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
				}
				catch (Exception e) {
					FileLog.Log("Caught exception when running patch " + MethodBase.GetCurrentMethod().DeclaringType + "!");
					FileLog.Log(e.Message);
					FileLog.Log(e.StackTrace);
					FileLog.Log(e.ToString());
				}
				return codes.AsEnumerable();
			}
		}

	}

}
