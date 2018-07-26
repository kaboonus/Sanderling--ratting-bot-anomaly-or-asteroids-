/*

This bot rats anomaly and/or asteroids; warp to asteroids at 30km+; anomaly at 50km;
-orbit at  distance (W + click) or around selected rat.A lternatively,  by overview menu at max 30 km ...I let the code there.
- run from reds
-activate /stop armor repairer automaticaly 
-activate afterburner
take the rats in order
Set your own tab for combat pve
examples:

AnomalyToIgnore = "Belt|asteroid|drone|forlorn|rally|sanctum|blood hub|serpentis hub|hidden|haven|port|den",// what anomaly to ignore" in takeanomaly void; avoid anomaly haven with celestialtoavoid
DB: "Belt|asteroid|drone|forlorn|rally|sanctum|blood hub|serpentis hub|hidden|haven|port|den",// what anomaly to ignore
run from reds on site( if chatlocal fails is good)
To Do:

 wrecks - inventory open and check offload
*/

using BotSharp.ToScript.Extension;
using Parse = Sanderling.Parse;
using MemoryStruct = Sanderling.Interface.MemoryStruct;

//	begin of configuration section ->

var RetreatOnNeutralOrHostileInLocal = true;   // warp to RetreatBookmark when a neutral or hostile is visible in local.
var RattingAnomaly = true;	//	when this is set to true, you take anomaly
var RattingAsteroids = true;	//	when this is set to true, you take asteroids


/////settings anomaly
string AnomalyToTakeColumnHeader = "name";  // the column header from table ex : name
string AnomalyToTake = "forsaken hub"; // his name , ex:  "forsaken hub"  " combat"
string IgnoreAnomalyName = "Belt|asteroid|drone|forlorn|rally|sanctum|blood hub|serpentis hub|hidden|haven|port|den";// what anomaly to ignore
string IgnoreColumnheader = "Name";//the head  of anomaly to ignore
// you have to run from this rats:
string runFromRats = "Dreadnought|Autothysian|Autothysian lancer|punisher|bestower|harbringer";// you run from him

//celestial to orbit
string celestialOrbit = "broken|pirate gate|wreck";

string CelestialToAvoid = "Chemical Factory"; // this one make difference between haven rock and gas
////////

int DroneNumber = 5;// set number of drones in space
	
int TargetCountMax = 2; //target numbers
//set  hardeners, repairer, set true if you want to run them all time, if not, there is set StartArmorRepairerHitPoints
var ActivateHardener = true;
var ActivateArmorRepairer = false;

//	warpout emergency armor

var EmergencyWarpOutHitpointPercent = 40; // just in case, when you warp on emergensy
var StartArmorRepairerHitPoints = 95; // armor value in % , when it starts armor repairer



//	Bookmark of location where ore should be unloaded.
string UnloadBookmark = "home"; //suposed your bookmark is named home

//	Name of the container to unload to as shown in inventory.
string UnloadDestContainerName = "Item Hangar"; //suposed it is Item Hangar


//	Bookmark of place to retreat to to prevent ship loss.
string RetreatBookmark = UnloadBookmark;

//register the visited locations
Queue<string> visitedLocations = new Queue<string>();

//diverses
	var lockTargetKeyCode = VirtualKeyCode.LCONTROL;// lock target

	var targetLockedKeyCode = VirtualKeyCode.SHIFT;//locked target

	var orbitKeyCode = (VirtualKeyCode)'W';

var attackDrones = VirtualKeyCode.VK_F;


const string StatusStringFromDroneEntryTextRegexPattern = @"\((.*)\)";				
static public string StatusStringFromDroneEntryText(this string droneEntryText) =>droneEntryText?.RegexMatchIfSuccess(StatusStringFromDroneEntryTextRegexPattern)?.Groups[1]?.Value?.RemoveXmlTag()?.Trim();

/////

bool returnDronesToBayOnRetreat = true; // when set to true, bot will attempt to dock back the drones before retreating


//	<- end of configuration section


Func<object> BotStopActivity = () => null;

Func<object> NextActivity = MainStep;

for(;;)
{
	MemoryUpdate();

	Host.Log(

		" ; shield.hp: " + ShieldHpPercent + "%" +
		" ; armor.hp: " + ArmorHpPercent + "%" +
		" ; retreat: " +( chatLocal?.ParticipantView?.Entry?.Count(IsNeutralOrEnemy)-1)+ " # "  + RetreatReason + 
		" ; overview.rats: " + ListRatOverviewEntry?.Length +
		" ; drones in space: " + DronesInSpaceCount +
		" ; targeted rats :  " + Measurement?.Target?.Length+
		" ; anchors : "  + ListCelestialObjects?.Length  +
		" ; nextAct: " + NextActivity?.Method?.Name);

	CloseModalUIElement();

	if(0 < RetreatReason?.Length && !(Measurement?.IsDocked ?? false))
	{
				Host.Log("retreat ????");
		if ((returnDronesToBayOnRetreat)  && (0 != DronesInSpaceCount))
		{
			DroneEnsureInBay();
		}

		if (!returnDronesToBayOnRetreat || (returnDronesToBayOnRetreat && 0 == DronesInSpaceCount))
		{
		Host.Log("Yes, I have a reason");

			ClickMenuEntryOnPatternMenuRoot(Measurement?.InfoPanelCurrentSystem?.ListSurroundingsButton, RetreatBookmark, "dock");
		}
		continue;
	}
	if(Measurement.WindowOther != null) CloseWindowOther();
	NextActivity = NextActivity?.Invoke() as Func<object>;

	if(BotStopActivity == NextActivity)
		break;
	
	if(null == NextActivity)
		NextActivity = MainStep;
	

	Host.Delay(1111);
}

int?	ShieldHpPercent => ShipUi?.HitpointsAndEnergy?.Shield / 10;
int?	ArmorHpPercent => ShipUi?.HitpointsAndEnergy?.Armor / 10;

bool	DefenseExit =>
	(Measurement?.IsDocked ?? false) ||
	!(0 < ListRatOverviewEntry?.Length);

bool	DefenseEnter =>
	!DefenseExit	;

string RetreatReasonTemporary = null;
string RetreatReasonPermanent = null;
string RetreatReason => RetreatReasonPermanent ?? RetreatReasonTemporary;
int? LastCheckOreHoldFillPercent = null;

int OffloadCount = 0;

Func<object>	MainStep()
{
var probeScannerWindow = Measurement?.WindowProbeScanner?.FirstOrDefault();
	var scanResultCombatSite = probeScannerWindow?.ScanResultView?.Entry?.FirstOrDefault(AnomalySuitableGeneral);
Host.Log("enter mainstep");
	if(Measurement?.IsDocked ?? false)
	{		
		InInventoryUnloadItems();

		if (0 < RetreatReasonPermanent?.Length)
		{ Host.Log("bot stop"); return BotStopActivity;}

		if (0 < RetreatReason?.Length) { Host.Log("return main"); return MainStep;}

		Undock();
	}

	if(ReadyForManeuver)
	{

		DroneEnsureInBay();
		Host.Log("ready");
		if( 0 == DronesInSpaceCount && 0 ==ListRatOverviewEntry?.Length)
		{
		Host.Log("drones in space 0 going to asteroids or anomaly");
			if(ReadyForManeuver)
			{
				if ((RattingAnomaly ) && (null != scanResultCombatSite))
				{Host.Log("I have anomaly??");
					return TakeAnomaly;
				}
				if (RattingAsteroids )
				{
			
			Host.Log("no rats arround, no anomaly, taking an asteroid from listSurroundingsButton");

				InitiateWarpToMiningSite();
			return MainStep;
				}
			}
		}
	
	}

	ModuleMeasureAllTooltip();

	if(ActivateHardener)
		ActivateHardenerExecute();

	return InBeltMineStep;
}


void CloseModalUIElement()
{
	var	ButtonClose =
		ModalUIElement?.ButtonText?.FirstOrDefault(button => (button?.Text).RegexMatchSuccessIgnoreCase("close|no|ok"));		
	Sanderling.MouseClickLeft(ButtonClose);
}
public void CloseWindowOther()
{
    Host.Log("close WindowOther");
    var windowOther = Sanderling?.MemoryMeasurementParsed?.Value?.WindowOther?.FirstOrDefault();

    //	if close button not visible then move mouse to the our window
    if (!windowOther.HeaderButtonsVisible ?? false)
        Sanderling.MouseMove(windowOther.LabelText.FirstOrDefault());

    Sanderling.InvalidateMeasurement(); //	make sure we have new measurement
    if (windowOther.HeaderButton != null)
    {
        //	we have 3 buttons and looking with HintText "Close"
        var closeButton = windowOther.HeaderButton?.FirstOrDefault(x => x.HintText == "Close");
        if (closeButton != null)
            Sanderling.MouseClickLeft(closeButton);
    }
}
void DroneLaunch()
{
	Host.Log("launch drones.");
	Sanderling.MouseClickRight(DronesInBayListEntry);
	Sanderling.MouseClickLeft(Menu?.FirstOrDefault()?.EntryFirstMatchingRegexPattern("launch", RegexOptions.IgnoreCase));
}

void DroneEnsureInBay()
{
	if(0 == DronesInSpaceCount)
		return;
	DroneReturnToBay();
	
	Host.Delay(4444);
}

void DroneReturnToBay()
{
	Host.Log("return drones to bay.");
	Sanderling.MouseClickRight(DronesInSpaceListEntry);
	Sanderling.MouseClickLeft(Menu?.FirstOrDefault()?.EntryFirstMatchingRegexPattern("return.*bay", RegexOptions.IgnoreCase));
	// Sanderling.KeyboardPressCombined(new[]{ targetLockedKeyCode, VirtualKeyCode.VK_R });
}

var ShipManeuverStatus = Measurement.ShipUi?.Indication?.ManeuverType;



Func<object>	DefenseStep()
{

	var NPCtargheted = Measurement?.Target?.Length;
	var shouldAttackTarget = ListRatOverviewEntry?.Any(entry => entry?.MeActiveTarget ?? false) ?? false;
		var targetSelected = Measurement?.Target?.FirstOrDefault(target => target?.IsSelected ?? false);
		
		var Broken = ListCelestialObjects?.FirstOrDefault();

	var droneListView = Measurement?.WindowDroneView?.FirstOrDefault()?.ListView;
	
	var droneGroupWithNameMatchingPattern = new Func<string, DroneViewEntryGroup>(namePattern =>
		droneListView?.Entry?.OfType<DroneViewEntryGroup>()?.FirstOrDefault(group => group?.LabelTextLargest()?.Text?.RegexMatchSuccessIgnoreCase(namePattern) ?? false));
	
	var overviewEntryLockTarget =
		ListRatOverviewEntry?.FirstOrDefault(entry => !((entry?.MeTargeted ?? false) || (entry?.MeTargeting ?? false)));

	var droneGroupInLocalSpace = droneGroupWithNameMatchingPattern("local space");
	
	var setDroneInLocalSpace =	droneListView?.Entry?.OfType<DroneViewEntryItem>()
					?.Where(drone => droneGroupInLocalSpace?.RegionCenter()?.B < drone?.RegionCenter()?.B)
					?.ToArray();
	var droneInLocalSpaceSetStatus =
		setDroneInLocalSpace?.Select(drone => drone?.LabelText?.Select(label => label?.Text?.StatusStringFromDroneEntryText()))?.ConcatNullable()?.WhereNotDefault()?.Distinct()?.ToArray();

	var droneInLocalSpaceIdle =
		droneInLocalSpaceSetStatus?.Any(droneStatus => droneStatus.RegexMatchSuccessIgnoreCase("idle")) ?? false;
	
	var droneGroupInBay = droneGroupWithNameMatchingPattern("bay");

	
//	if(ReadyForManeuver)
//	{
	Host.Log("enter defense from defense step.");
//	if (ShipManeuverStatus != ShipManeuverTypeEnum.Orbit)
//	return InBeltMineStep;
		if(ActivateArmorRepairer == true || ArmorHpPercent < StartArmorRepairerHitPoints)
			{		Host.Log("ArmorHpPercent < 95");	
				ActivateArmorRepairerExecute();
			}
		
		if(ArmorHpPercent>StartArmorRepairerHitPoints)
			{ StopArmorRepairer();}			

		if(DefenseExit)
			{ Host.Log("exit defense.");  return null; }
				
				
			
	

		if(null == targetSelected)
			LockTarget ();
		
		if (0 < DronesInBayCount && DronesInSpaceCount < DroneNumber)
				DroneLaunch();
			
		if (!(0 < DronesInSpaceCount))
				DroneLaunch();

		if (null != targetSelected )
		{	if (shouldAttackTarget) 
			 ActivateWeaponExecute();
			else
			UnlockTarget();
		}
		if (Measurement?.Target?.Length < TargetCountMax) 
			LockTarget();

		if (droneInLocalSpaceIdle  &&  (Measurement?.Target?.Length > 0))
			{	Host.Log("drones idle");
			Sanderling.KeyboardPress(attackDrones);
			Host.Log("engage target");
			}
		if (EWarToAttack.Count() > 0)//thx pikacuq
		{


			var EWarSelected = EWarToAttack.FirstOrDefault(target => target?.IsSelected ?? false);
			var EWarLocked = EWarToAttack?.FirstOrDefault(target => target?.MeTargeted ?? false);

			if (EWarLocked == null)
			{	
			Sanderling.KeyDown(lockTargetKeyCode);
			Sanderling.MouseClickLeft(EWarToAttack?.FirstOrDefault(entry => !((entry?.MeTargeted ?? false))));
			Sanderling.KeyUp(lockTargetKeyCode);

			}
			else if (EWarSelected == null)
			{ Sanderling.MouseClickLeft(EWarToAttack?.FirstOrDefault()); }
				
			else
			{	Host.Log("drones change to ewar target");
			Sanderling.KeyboardPress(attackDrones);
			Host.Log("engage Ewar target ");
			}
		}
		if (0 ==ListRatOverviewEntry?.Length) 	{	StopAfterburner();}
		
	return DefenseStep;
}


Func<object> InBeltMineStep()
{	
	if ((listOverviewEntryFriends.Length != 0) && ListCelestialObjects?.Length > 0)
	{if (ShipManeuverStatus != ShipManeuverTypeEnum.Orbit)
		{	Sanderling.KeyboardPressCombined(new[]{ VirtualKeyCode.LMENU, VirtualKeyCode.VK_P });			
		return TakeAnomaly;
		}
	}
	if ((ReadyForManeuver) && (ShipManeuverStatus != ShipManeuverTypeEnum.Orbit) && (0 < ListRatOverviewEntry?.Length))
	{
	Orbitkeyboard();

	if (DefenseEnter)
		{
		Host.Log("enter defense.");
		return DefenseStep;
		}
		
	}

	EnsureWindowInventoryOpenActiveShip();
	if ((0 ==ListRatOverviewEntry?.Length) || (0 != listOverviewDreadCheck?.Length ))
	{
		Host.Log("return to main step");

		StopAfterburner();
	return MainStep;
	}

	return InBeltMineStep;	
}



Sanderling.Parse.IMemoryMeasurement	Measurement	=>
	Sanderling?.MemoryMeasurementParsed?.Value;

IWindow ModalUIElement =>
	Measurement?.EnumerateReferencedUIElementTransitive()?.OfType<IWindow>()?.Where(window => window?.isModal ?? false)
	?.OrderByDescending(window => window?.InTreeIndex ?? int.MinValue)
	?.FirstOrDefault();	

IEnumerable<Parse.IMenu> Menu => Measurement?.Menu;

Parse.IShipUi ShipUi => Measurement?.ShipUi;


Sanderling.Interface.MemoryStruct.IMenuEntry MenuEntryLockTarget =>
	Menu?.FirstOrDefault()?.Entry?.FirstOrDefault(entry => entry.Text.RegexMatchSuccessIgnoreCase("^lock"));
	
Sanderling.Interface.MemoryStruct.IMenuEntry MenuEntryUnLockTarget =>
	Menu?.FirstOrDefault()?.Entry?.FirstOrDefault(entry => entry.Text.RegexMatchSuccessIgnoreCase("^unlock"));

Sanderling.Parse.IWindowOverview	WindowOverview	=>
	Measurement?.WindowOverview?.FirstOrDefault();

	

Sanderling.Parse.IWindowInventory	WindowInventory	=>
	Measurement?.WindowInventory?.FirstOrDefault();

IWindowDroneView	WindowDrones	=>
	Measurement?.WindowDroneView?.FirstOrDefault();



string OverviewTypeSelectionName =>
	WindowOverview?.Caption?.RegexMatchIfSuccess(@"\(([^\)]*)\)")?.Groups?[1]?.Value;

Parse.IOverviewEntry[] ListRatOverviewEntry => WindowOverview?.ListView?.Entry?.Where(entry =>
		(entry?.MainIconIsRed ?? false)	)
						//  ?.OrderBy(entry => .AttackPriorityIndex(entry))
				?.OrderBy(entry => entry?.Name?.RegexMatchSuccessIgnoreCase(@"battery|tower|sentry|web|strain|splinter|render|raider")) //Frigate
				?.OrderBy(entry => entry?.Name?.RegexMatchSuccessIgnoreCase(@"coreli|centi|alvi|pithi|corpii|gistii")) //Frigate
				?.OrderBy(entry => entry?.Name?.RegexMatchSuccessIgnoreCase(@"corelior|centior|alvior|pithior|corpior|gistior")) //Destroyer
				?.OrderBy(entry => entry?.Name?.RegexMatchSuccessIgnoreCase(@"corelum|centum|alvum|pithum|corpum|gistum")) //Cruiser
				?.OrderBy(entry => entry?.Name?.RegexMatchSuccessIgnoreCase(@"corelatis|centatis|alvatis|pithatis|copatis|gistatis")) //Battlecruiser
				?.OrderBy(entry => entry?.Name?.RegexMatchSuccessIgnoreCase(@"core\s|centus|alvus|pith\s|corpus|gist\s")) //Battleship
				?.ThenBy(entry => entry?.DistanceMax ?? int.MaxValue)						 
				?.ToArray();	
Parse.IOverviewEntry[] ListCelestialObjects => WindowOverview?.ListView?.Entry
	?.Where(entry => entry?.Name?.RegexMatchSuccessIgnoreCase(celestialOrbit) ?? false)
	?.OrderBy(entry => entry?.DistanceMax ?? int.MaxValue)
	?.ToArray();
	
Parse.IOverviewEntry[] listOverviewDreadCheck => WindowOverview?.ListView?.Entry
	?.Where(entry => entry?.Name?.RegexMatchSuccess(runFromRats) ?? true)
	.ToArray();

Parse.IOverviewEntry[] listOverviewEntryFriends =>
	WindowOverview?.ListView?.Entry
	?.Where(entry => entry?.ListBackgroundColor?.Any(IsFriendBackgroundColor) ?? false)
	?.ToArray();


Parse.IOverviewEntry[] listOverviewEntryEnemy =>
	WindowOverview?.ListView?.Entry
	?.Where(entry => entry?.ListBackgroundColor?.Any(IsEnemyBackgroundColor) ?? false)
	?.ToArray();
	
EWarTypeEnum[] listEWarPriorityGroupTeamplate =
{
	EWarTypeEnum.WarpDisrupt, EWarTypeEnum.WarpScramble, EWarTypeEnum.ECM, EWarTypeEnum.Web, EWarTypeEnum.EnergyNeut,EWarTypeEnum.EnergyVampire,
};

Parse.IOverviewEntry[] EWarToAttack =>
	WindowOverview?.ListView?.Entry?
		.Where(entry => entry != null && (!entry.EWarType?.IsNullOrEmpty() ?? false) &&  listEWarPriorityGroupTeamplate.Intersect(entry.EWarType).Any())
		?.ToArray();	
	
DroneViewEntryGroup DronesInBayListEntry =>
	WindowDrones?.ListView?.Entry?.OfType<DroneViewEntryGroup>()?.FirstOrDefault(Entry => null != Entry?.Caption?.Text?.RegexMatchIfSuccess(@"Drones in bay", RegexOptions.IgnoreCase));

DroneViewEntryGroup DronesInSpaceListEntry =>
	WindowDrones?.ListView?.Entry?.OfType<DroneViewEntryGroup>()?.FirstOrDefault(Entry => null != Entry?.Caption?.Text?.RegexMatchIfSuccess(@"Drones in Local Space", RegexOptions.IgnoreCase));

int?	DronesInSpaceCount => DronesInSpaceListEntry?.Caption?.Text?.AsDroneLabel()?.Status?.TryParseInt();
int?	DronesInBayCount => DronesInBayListEntry?.Caption?.Text?.AsDroneLabel()?.Status?.TryParseInt();

public bool ReadyForManeuverNot =>
	Measurement?.ShipUi?.Indication?.LabelText?.Any(indicationLabel =>
		(indicationLabel?.Text).RegexMatchSuccessIgnoreCase("warp|docking")) ?? false;

public bool ReadyForManeuver => !ReadyForManeuverNot && !(Measurement?.IsDocked ?? true);

Sanderling.Interface.MemoryStruct.IListEntry	WindowInventoryItem	=>
	WindowInventory?.SelectedRightInventory?.ListView?.Entry?.FirstOrDefault();


WindowChatChannel chatLocal =>
	 Sanderling.MemoryMeasurementParsed?.Value?.WindowChatChannel
	 ?.FirstOrDefault(windowChat => windowChat?.Caption?.RegexMatchSuccessIgnoreCase("local") ?? false);

//    assuming that own character is always visible in local
public bool hostileOrNeutralsInLocal => 1 != chatLocal?.ParticipantView?.Entry?.Count(IsNeutralOrEnemy);


void ClickMenuEntryOnMenuRoot(IUIElement MenuRoot, string MenuEntryRegexPattern)
{
	Sanderling.MouseClickRight(MenuRoot);
	
	var Menu = Measurement?.Menu?.FirstOrDefault();
	
	var	MenuEntry = Menu?.EntryFirstMatchingRegexPattern(MenuEntryRegexPattern, RegexOptions.IgnoreCase);
	
	Sanderling.MouseClickLeft(MenuEntry);
}

void EnsureWindowInventoryOpen()
{
	if (null != WindowInventory)
		return;

	Host.Log("open Inventory.");
	Sanderling.MouseClickLeft(Measurement?.Neocom?.InventoryButton);
	Host.Delay(1111);
}
void EnsureWindowInventoryOpenActiveShip()
{
	EnsureWindowInventoryOpen();

	var inventoryActiveShip = WindowInventory?.ActiveShipEntry;

	if (!(inventoryActiveShip?.IsSelected ?? false))
		Sanderling.MouseClickLeft(inventoryActiveShip);


}


//	sample label text: Intensive Reprocessing Array <color=#66FFFFFF>1,123 m</color //bla bla??
string InventoryContainerLabelRegexPatternFromContainerName(string containerName) =>
	@"^\s*" + Regex.Escape(containerName) + @"\s*($|\<)";

void InInventoryUnloadItems() => InInventoryUnloadItemsTo(UnloadDestContainerName);

void InInventoryUnloadItemsTo(string DestinationContainerName)
{
	Host.Log("unload items to '" + DestinationContainerName + "'.");

	EnsureWindowInventoryOpenActiveShip();

	for (;;)
	{
		var oreHoldListItem = WindowInventory?.SelectedRightInventory?.ListView?.Entry?.ToArray();

		var oreHoldItem = oreHoldListItem?.FirstOrDefault();

		if(null == oreHoldItem)
			break;    //    0 items in OreHold

		if(1 < oreHoldListItem?.Length)
			ClickMenuEntryOnMenuRoot(oreHoldItem, @"select\s*all");

		var DestinationContainerLabelRegexPattern =
			InventoryContainerLabelRegexPatternFromContainerName(DestinationContainerName);

		var DestinationContainer =
			WindowInventory?.LeftTreeListEntry?.SelectMany(entry => new[] { entry }.Concat(entry.EnumerateChildNodeTransitive()))
			?.FirstOrDefault(entry => entry?.Text?.RegexMatchSuccessIgnoreCase(DestinationContainerLabelRegexPattern) ?? false);

		if (null == DestinationContainer)
			Host.Log("error: Inventory entry labeled '" + DestinationContainerName + "' not found");

		Sanderling.MouseDragAndDrop(oreHoldItem, DestinationContainer);
	}
}

bool InitiateWarpToMiningSite()	=>
	InitiateDockToOrWarpToLocationInSolarSystemMenu("asteroid belts", PickNextMiningSiteFromSystemMenu);

MemoryStruct.IMenuEntry PickNextMiningSiteFromSystemMenu(IReadOnlyList<MemoryStruct.IMenuEntry> availableMenuEntries)
{
	Host.Log("I am seeing " + availableMenuEntries?.Count.ToString() + " mining sites to choose from.");

	var nextSite =
		availableMenuEntries
		?.OrderBy(menuEntry => visitedLocations.ToList().IndexOf(menuEntry?.Text))
		?.FirstOrDefault();

	Host.Log("I pick '" + nextSite?.Text + "' as next mining site, based on the intent to rotate through the mining sites and recorded previous locations.");
	return nextSite;
}

bool InitiateDockToOrWarpToLocationInSolarSystemMenu(
	string submenuLabel,
	Func<IReadOnlyList<MemoryStruct.IMenuEntry>, MemoryStruct.IMenuEntry> pickPreferredDestination = null)
{
	Host.Log("Attempt to initiate dock to or warp to menu entry in submenu '" + submenuLabel + "'");
	
	var listSurroundingsButton = Measurement?.InfoPanelCurrentSystem?.ListSurroundingsButton;
	
	Sanderling.MouseClickRight(listSurroundingsButton);

	var submenuEntry = Measurement?.Menu?.FirstOrDefault()?.EntryFirstMatchingRegexPattern("^" + submenuLabel + "$", RegexOptions.IgnoreCase);

	if(null == submenuEntry)
	{
		Host.Log("Submenu '" + submenuLabel + "' not found in the solar system menu.");
		return true;
	}

	Sanderling.MouseClickLeft(submenuEntry);

	var submenu = Measurement?.Menu?.ElementAtOrDefault(1);

	var destinationMenuEntry = pickPreferredDestination?.Invoke(submenu?.Entry?.ToList()) ?? submenu?.Entry?.FirstOrDefault();

	if(destinationMenuEntry == null)
	{
		Host.Log("Failed to open submenu '" + submenuLabel + "' in the solar system menu.");
		return true;
	}

	Sanderling.MouseClickRight(destinationMenuEntry);
var menuResult = Measurement?.Menu?.ElementAtOrDefault(2);
	var menuResultWarp =menuResult?.Entry.ToArray();
	var menuResultSelectWarpMenu = menuResultWarp?[1];
	
	Sanderling.MouseClickRight(menuResultSelectWarpMenu);
	
	var menuResultats = Measurement?.Menu?.ElementAtOrDefault(3);
	var menuResultWarpDestination = menuResultats?.Entry.ToArray();

		Host.Log("initiating  warp on '" + destinationMenuEntry?.Text + "'");
	ClickMenuEntryOnMenuRoot(menuResultWarpDestination[3], "within 30 km");
		Host.Log("no suitable menu entry found on '" + destinationMenuEntry?.Text + "'");
	return true;
}
void LockTarget ()
{	
	Sanderling.KeyDown(lockTargetKeyCode);
	Sanderling.MouseClickLeft(ListRatOverviewEntry?.FirstOrDefault(entry => !((entry?.MeTargeted ?? false) || (entry?.MeTargeting ?? false))));
	Sanderling.KeyUp(lockTargetKeyCode);

}
void UnlockTarget()
{
	var targetSelected = Measurement?.Target?.FirstOrDefault(target => target?.IsSelected ?? false);
		Sanderling.MouseClickRight(targetSelected);
		Sanderling.MouseClickLeft(MenuEntryUnLockTarget);	
		Host.Log("this is not a target");
		
}
void Undock()
{
	while(Measurement?.IsDocked ?? true)
	{
		Sanderling.MouseClickLeft(Measurement?.WindowStation?.FirstOrDefault()?.UndockButton);
		Host.Log("waiting for undocking to complete.");
		Host.Delay(8000);
	}

	Host.Delay(4444);
	Sanderling.InvalidateMeasurement();
}

void ModuleMeasureAllTooltip()
{
	Host.Log("measure tooltips of all modules.");

	for (;;)
	{
		var NextModule = Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule?.FirstOrDefault(m => null == m?.TooltipLast);

		if(null == NextModule)
			break;

		Host.Log("measure module.");
		//	take multiple measurements of module tooltip to reduce risk to keep bad read tooltip.
		Sanderling.MouseMove(NextModule);
		Sanderling.WaitForMeasurement();
		Sanderling.MouseMove(NextModule);
	}
}

void ActivateHardenerExecute()
{
	var	SubsetModuleHardener =
		Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule
		?.Where(module => module?.TooltipLast?.Value?.IsHardener ?? false);

	var	SubsetModuleToToggle =
		SubsetModuleHardener
		?.Where(module => !(module?.RampActive ?? false));

	foreach (var Module in SubsetModuleToToggle.EmptyIfNull())
		ModuleToggle(Module);
}
void ActivateArmorRepairerExecute()
{
	var	SubsetModuleArmorRepairer =
		Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule
		?.Where(module => module?.TooltipLast?.Value?.IsArmorRepairer ?? false);

	var	SubsetModuleToToggle =
		SubsetModuleArmorRepairer
		?.Where(module => !(module?.RampActive ?? false));

	foreach (var Module in SubsetModuleToToggle.EmptyIfNull())
		ModuleToggle(Module);
}
void StopArmorRepairer()
{
	var	SubsetModuleArmorRepairer =
		Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule
		?.Where(module => module?.TooltipLast?.Value?.IsArmorRepairer ?? false);

	var	SubsetModuleToToggle =
		SubsetModuleArmorRepairer
		?.Where(module => (module?.RampActive ?? false));

	foreach (var Module in SubsetModuleToToggle.EmptyIfNull())
		ModuleToggle(Module);
}
void ActivateWeaponExecute()
{
	var	SubsetModuleWeapon =
		Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule
		?.Where(module => module?.TooltipLast?.Value?.IsWeapon ?? false);

	var	SubsetModuleToToggle =
		SubsetModuleWeapon
		?.Where(module => !(module?.RampActive ?? false));

	foreach (var Module in SubsetModuleToToggle.EmptyIfNull())
		ModuleToggle(Module);
}
void ActivateAfterburnerExecute()
{
	var	SubsetModuleAfterburner =
		Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule
		?.Where(module => module?.TooltipLast?.Value?.IsAfterburner ?? false);

	var	SubsetModuleToToggle =
		SubsetModuleAfterburner
		?.Where(module => !(module?.RampActive ?? false));

	foreach (var Module in SubsetModuleToToggle.EmptyIfNull())
		ModuleToggle(Module);
}
void StopAfterburner()
{
	
	var	SubsetModuleAfterburner =
		Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule
		?.Where(module => module?.TooltipLast?.Value?.IsAfterburner ?? false);

	var	SubsetModuleToToggle =
		SubsetModuleAfterburner
		?.Where(module => (module?.RampActive ?? false));

	foreach (var Module in SubsetModuleToToggle.EmptyIfNull())
	{	ModuleToggle(Module); 		Host.Log("stop afterburner to warp faster"); }
}
void ModuleToggle(Sanderling.Accumulation.IShipUiModule Module)
{
	var ToggleKey = Module?.TooltipLast?.Value?.ToggleKey;

	Host.Log("toggle module using " + (null == ToggleKey ? "mouse" : Module?.TooltipLast?.Value?.ToggleKeyTextLabel?.Text));

	if(null == ToggleKey)
		Sanderling.MouseClickLeft(Module);
	else
		Sanderling.KeyboardPressCombined(ToggleKey);
}


void MemoryUpdate()
{
	RetreatUpdate();
	UpdateLocationRecord();
}


bool MeasurementEmergencyWarpOutEnter =>
	!(Measurement?.IsDocked ?? false) && !(EmergencyWarpOutHitpointPercent < ArmorHpPercent);

void RetreatUpdate()
{
	RetreatReasonTemporary = (RetreatOnNeutralOrHostileInLocal && hostileOrNeutralsInLocal) || (listOverviewDreadCheck?.Length >0) || (listOverviewEntryEnemy?.Length >0)  ? "hostile or neutral in local" : null;

	if (!MeasurementEmergencyWarpOutEnter)
		return;

	//	measure multiple times to avoid being scared off by noise from a single measurement. 
	Sanderling.InvalidateMeasurement();

	if (!MeasurementEmergencyWarpOutEnter)
		return;

	RetreatReasonPermanent = "Armor hp";
}
void UpdateLocationRecord()
{
	//	I am not interested in locations which are only close during warp.
	if(Measurement?.ShipUi?.Indication?.ManeuverType == ShipManeuverTypeEnum.Warp)
		return;

	// Purpose of recording locations is to prioritize our next destination when warping to mining site.

	var currentSystemLocationLabelText =
		Measurement?.InfoPanelCurrentSystem?.ExpandedContent?.LabelText
		?.OrderByCenterVerticalDown()?.FirstOrDefault()?.Text;

	if(currentSystemLocationLabelText == null)
		return;

	// 2018-03 observed label text: <url=showinfo:15//40088644 alt='Nearest'>Amsen V - Asteroid Belt 1</url>

	var currentLocationName = RegexExtension.RemoveXmlTag(currentSystemLocationLabelText)?.Trim();

	var	lastRecordedLocation = visitedLocations.LastOrDefault();

	if(lastRecordedLocation == currentLocationName)
		return;

	visitedLocations.Enqueue(currentLocationName);
	Host.Log("Recorded transition from location '" + lastRecordedLocation + "' to location '" + currentLocationName + "'");

	if(100 < visitedLocations.Count)
		visitedLocations.Dequeue();
}

// Orbit asteroid at 10km  Orbit("Crokite") will orbit first asteroid named Crokite at 10 km, stolen from forum :d
void Orbit(string whatToOrbit,string distance="30 km")
{
var ToOrbit=Measurement?.WindowOverview?.FirstOrDefault()?.ListView?.Entry?.Where(entry => entry?.Name?.RegexMatchSuccessIgnoreCase(whatToOrbit) ?? false)?.ToArray();
ClickMenuEntryOnPatternMenuRoot(ToOrbit.FirstOrDefault(),"Orbit",distance);
}
// Stolen and modified from MineOre.cs
// modified to click on Submenu if SubMenuEntryRegexPattern not null
void ClickMenuEntryOnPatternMenuRoot(IUIElement MenuRoot, string MenuEntryRegexPattern,string SubMenuEntryRegexPattern=null)
{
	Sanderling.MouseClickRight(MenuRoot);
	var Menu = Sanderling?.MemoryMeasurementParsed?.Value?.Menu?.FirstOrDefault();
	var	MenuEntry = Menu?.EntryFirstMatchingRegexPattern(MenuEntryRegexPattern, RegexOptions.IgnoreCase);
	Sanderling.MouseClickLeft(MenuEntry);
	if(SubMenuEntryRegexPattern!=null) {
	// Using the API explorer when we click on the top menu we get another menu that has more options
	// So skip the MenuRoot and click on Submenu
	var subMenu=Sanderling?.MemoryMeasurementParsed?.Value?.Menu?.Skip(1).First();
	var subMenuEntry = subMenu?.EntryFirstMatchingRegexPattern(SubMenuEntryRegexPattern, RegexOptions.IgnoreCase);
	Sanderling.MouseClickLeft(subMenuEntry);
	}
}


Func<object>	TakeAnomaly()
{

var probeScannerWindow = Measurement?.WindowProbeScanner?.FirstOrDefault();



var scanActuallyAnomaly = probeScannerWindow?.ScanResultView?.Entry?.FirstOrDefault(ActuallyAnomaly);

var UndesiredAnomaly = probeScannerWindow?.ScanResultView?.Entry?.FirstOrDefault(IgnoreAnomaly);

var scanResultCombatSite = probeScannerWindow?.ScanResultView?.Entry?.FirstOrDefault(AnomalySuitableGeneral);
				
Host.Log("take anomaly start");
if (probeScannerWindow == null)
	Sanderling.KeyboardPressCombined(new[]{ VirtualKeyCode.LMENU, VirtualKeyCode.VK_P });
if (null != scanActuallyAnomaly)
{
	 ClickMenuEntryOnMenuRoot(scanActuallyAnomaly, "Ignore Result");
	 return TakeAnomaly;
}

if (null != UndesiredAnomaly)
{
	
	ClickMenuEntryOnMenuRoot(UndesiredAnomaly, "Ignore Result");
	return TakeAnomaly;
}

if (null == scanResultCombatSite)
Host.Log("    I don't have the named anomaly, review the bot for taking the asteroids or just wait ");

if ((null != scanResultCombatSite) && (null == UndesiredAnomaly))
	{

	Sanderling.MouseClickRight(scanResultCombatSite);
	var menuResult = Measurement?.Menu?.ToList();
	if (null == menuResult)
		{	Host.Log("    ignore result  ?? just cheking  "); ClickMenuEntryOnMenuRoot(scanResultCombatSite, "Ignore Result"); }

	var menuResultWarp = menuResult?[0].Entry.ToArray();
	var menuResultSelectWarpMenu = menuResultWarp?[1];
	
	Sanderling.MouseClickLeft(menuResultSelectWarpMenu);
	
	var menuResultats = Measurement?.Menu?.ToList();
	var menuResultWarpDestination = menuResultats?[1].Entry.ToArray();

	Host.Log("warping to anomaly  ");
	ClickMenuEntryOnMenuRoot(menuResultWarpDestination[4], "within 50 km");
	
	return MainStep;

	}
		return MainStep;
}
void Orbitkeyboard()
{
//Orbit(celestialOrbit); // to use for orbit celestial objects from overview 
Sanderling.KeyDown(VirtualKeyCode.VK_W);

	if (0 < ListCelestialObjects?.Length )
		Sanderling.MouseClickLeft(ListCelestialObjects?.FirstOrDefault());
	if (0 ==  ListCelestialObjects?.Length )
		{Sanderling.MouseClickLeft(ListRatOverviewEntry?.FirstOrDefault(entry => (entry?.MainIconIsRed ?? false)));}

Sanderling.KeyUp(VirtualKeyCode.VK_W);

ActivateAfterburnerExecute();
	Host.Delay(1111);
	Host.Log("1 sec");
}


 bool AnomalySuitableGeneral(MemoryStruct.IListEntry scanResult) =>
			scanResult?.CellValueFromColumnHeader(AnomalyToTakeColumnHeader)?.RegexMatchSuccessIgnoreCase(AnomalyToTake) ?? false;

 bool ActuallyAnomaly(MemoryStruct.IListEntry scanResult) =>
		scanResult?.CellValueFromColumnHeader("Distance")?.RegexMatchSuccessIgnoreCase("km") ?? false;

 bool IgnoreAnomaly(MemoryStruct.IListEntry scanResult) =>
scanResult?.CellValueFromColumnHeader(IgnoreColumnheader)?.RegexMatchSuccessIgnoreCase(IgnoreAnomalyName) ?? false;

bool IsEnemyBackgroundColor(ColorORGB color) =>
	color.OMilli == 500 && color.RMilli == 750 && color.GMilli == 0 && color.BMilli == 600;


bool IsFriendBackgroundColor(ColorORGB color) =>
	(color.OMilli == 500 && color.RMilli == 0 && color.GMilli == 150 && color.BMilli == 600) || (color.OMilli == 500 && color.RMilli == 100 && color.GMilli == 600 && color.BMilli == 100);

bool IsNeutralOrEnemy(IChatParticipantEntry participantEntry) =>
   !(participantEntry?.FlagIcon?.Any(flagIcon =>
	 new[] { "good standing", "excellent standing", "Pilot is in your (fleet|corporation|alliance)", "Pilot is an ally in one or more of your wars", }
	 .Any(goodStandingText =>
		flagIcon?.HintText?.RegexMatchSuccessIgnoreCase(goodStandingText) ?? false)) ?? false);

