/*V 1.1.02 and last(unless bugs) This bot ratting anomaly and/or asteroids; warp to asteroids at 0km and anomaly at 50, take the loot (or not, by ship type). Planning time by session or by time before DT, logout at the end of first timespan and stop bot. Protection from bumping 

fixs:
havens - solved
tethering -solved
*both in tests
GUIDE: ( and you have example at each variable in settings)
 fill in the values:
 -retreat from neutrals 
 -if you like to do anomalies or asteroids
 -the timers
 -name anomalies etc etc
 -the name of your tab for ratting
 -how many afterburners/hardeners/omni/armor repairers you have
 -and if some of them are activated in permanence or not
 -your home bookmark
 -container into station for unload
 -the name of your omni 
 -the value for your emergency warp out ( only for armor supported)
 -the value of your armor hp to start the repairer
 -the value of your offload cargo ( you can have a real big last wreck with more than 100m3), default value is 75% now
 -and the limit of offload count
 -make reds background in overview ( see forum my post) https://forum.botengine.org/t/ratting-bot-anomaly-and-or-asteroids-with-sanderling/1206
 -hide the empty wrecks ( see photo on forum, my post)
********** ATTENTION !! ######################
! DO NOT CHAT when you bot !! ( better hide the windows, except local, look on the photo on forum hot to do that in a manner 70%safe
! IF you warp/dock etc manual is better to stop the bot
! Some features could not work on older versions of sanderling!!
! The smaller value between DT and session timer is taken in account ( and you can see this value in stats)
! attention, because the local can do some false allarms (some "friends" had some bad standings even if he is blue)
##### GOOD POINTS ########
+ protection from bumpings ( tested 2-3 times)
+ you can review your setting when you are docked , stop the bot and made changes 
+ the window local chat must be biggest possible, if in you system are 50 persons and your window have enter for 10 persons  you are kill meat
+ the belts are taken in order
+ loot wreck, you can order in overview after mainicon, main Icon rats FIRST (collidables and wrecks are at the end). Also if you want only faction/officers set smaller offload capacity.yf you dont wanna loot, simply change the wreck name with ... (ex: mlqfkjhfbzeirfhjdbskdhizrkfjhihugizrzkjfvldkfvblkj , because this name didn't exist in game at all)
+ the symbol ♦ from types is for some drifters who appear near stations and asteroid belts
+ right before undock it check for reds on local; if they are present you stay in station until they gone(time + the undock delay)
+ if you want to dock ASAP when times up, comment the line 876 and unncomment 877 ( lines after bool MeasurementEmergencyWarpOutEnter). Else this it happens when times up AND sitefinished = true
###### BAD POINTS ##########
(-) It doesn't distinguish ewar web/scramble etc from targetpainting ( for example)
(-) It doesnt take in account what participants are hidden in local chat  ( so localchat is scrollable)( so avoid crowded systems)
###################
Thx to:
Viir
Terpla
pikacuq
the others from https://forum.botengine.org/ who contribued with or without their ideas /knowledges /code lines to create this bot/script.They are the owners and my contribution is insignifiante
License inherited from sanderling https://github.com/Arcitectus/Sanderling/blob/master/LICENSE and under same conditions
############
*/

using BotSharp.ToScript.Extension;
using Parse = Sanderling.Parse;
using MemoryStruct = Sanderling.Interface.MemoryStruct;
//	begin of configuration section ->
var RetreatOnNeutralOrHostileInLocal =true;   // true or false :warp to RetreatBookmark when a neutral or hostile is visible in local.
var RattingAnomaly = true;	// true or false:	when this is set to true, you take anomaly
var RattingAsteroids = true;	// true or false:	when this is set to true, you take asteroids
// SESSION/DT TImers
var minutesToDT = 15; //value in minutes before the DT of server ( already -1min than real DT of server)
var hoursToDT = 1;//value in h before the DT of server
var hoursToSession = 5; //wanna play for 5 hours
var minutesToSession = 11;// wanna play for 15 min
////
var MinimDelayUndock = 2;//in seconds 
var MaximDelayUndock = 45;//in seconds
var LimitOffloadCount = 100;// when you reach this limit, you dock
/////settings anomaly
string AnomalyToTakeColumnHeader = "name";  // the column header from table ex : name
string AnomalyToTake = ""; // his name , ex:  "forsaken hub" or " combat"
string IgnoreAnomalyName = "";// what anomaly to ignore : haven|Belt|asteroid|drone|forlorn|rally|sanctum|blood hub|serpentis hub|hidden|port|den
string IgnoreColumnheader = "Name";//the head  of anomaly to ignore
// you have to run from this rats:
string runFromRats = "♦|Titan|Dreadnought|Autothysian";// you run from him
//celestial to orbit
string celestialOrbit = ""; //ex: broken|pirate gate
string CelestialToAvoid = ""; // ex: Chemical Factory //this one make difference between haven rock and gas
// wrecks commander etc
string commanderNameWreck = "wreck";// if you dont wanna loot, then you change the name with  any name impossible to exist ingame
// ex: "Commander|Dark Blood|true|Shadow Serpentis|Dread Gurista|Domination Saint|Gurista Distributor|Sentient|Overseer|Spearhead|Dread Guristas|Estamel|Vepas|Thon|Kaikka|True Sansha|Chelm|Vizan|Selynne|Brokara|Dark Blood|Draclira|Ahremen|Raysere|Tairei|Cormack|Setele|Tuvan|Brynn|Domination|Tobias|Gotan|Hakim|Mizuro|wreck
////////
//ratting tab, fill in with your own ratting tab
string rattingTab = ""; // ex: combat
int DroneNumber = 5;// set number of drones in space; ex: 5
int TargetCountMax = 2; //target numbers; ex: 4
//set  hardeners, repairer, set true if you want to run them all time, if not, there is set StartArmorRepairerHitPoints
var ActivateHardener = true;// true or false ;true for activated permanent
var ActivateOmni = true; //true or false ; true for activated permanent
var ActivateArmorRepairer = true; // true or false ; true for activated permanent
string OmniSup = ""; // ex: Omnidirectional Tracking Link I
// 	Number of ArmorRepairers and afterburners; Thx Terpla. Both armor repairers are managed in same time, if you have 2. 
const int ArmorRepairsCount = 1; // how many you have
const int AfterburnersCount = 1;// how many you have
// 	Number of Afterburners, Thx Terpla. Also be carefull, I cannot manage 1 afterburner and 1 MWD in same time. this function is to be sure I have measurements in measure all modules tooltip
//fill in carefull also the bot will keep measuring 
const int HardenersCount = 0; // how many you have
const int OmniCount = 0; // how many you have
//	warpout emergency armor
var EmergencyWarpOutHitpointPercent = 40; // ex : 60 ; you warp home if your armor hp % is smaller that this value
var StartArmorRepairerHitPoints = 95; // armor hp value in % , when it starts armor repairer
//
bool returnDronesToBayOnRetreat = true;    // true or false ; when set to true, bot will attempt to dock back the drones before retreating
//	Bookmark of location where ore should be unloaded.
string UnloadBookmark = ""; // your  home bookmark
//	Name of the container to unload to as shown in inventory.
string UnloadDestContainerName = "Item Hangar"; //supposed it is Item Hangar
//	Bookmark of place to retreat to to prevent ship loss.
string RetreatBookmark = UnloadBookmark;
//register the visited locations
Queue<string> visitedLocations = new Queue<string>();
//diverses
var lockTargetKeyCode = VirtualKeyCode.LCONTROL;// lock target
var targetLockedKeyCode = VirtualKeyCode.SHIFT;//locked target
var orbitKeyCode = VirtualKeyCode.VK_W;// if you changed the default key
var attackDrones = VirtualKeyCode.VK_F;// if you changed the default key
var EnterOffloadOreHoldFillPercent = 75;//	percentage of ore hold fill level at which to enter the offload process and warp home.
const string StatusStringFromDroneEntryTextRegexPattern = @"\((.*)\)";
static public string StatusStringFromDroneEntryText(this string droneEntryText) => droneEntryText?.RegexMatchIfSuccess(StatusStringFromDroneEntryTextRegexPattern)?.Groups[1]?.Value?.RemoveXmlTag()?.Trim();
var startSession = DateTime.Now; // alternative: DateTime.UtcNow;
var playSession = DateTime.UtcNow.AddHours(hoursToSession).AddMinutes(minutesToSession);
var dateAndTime = DateTime.UtcNow;
var date = dateAndTime.Date;
var eveRealServerDT =date.AddHours(11).AddMinutes(-1);
//	<- end of configuration section
var OnePush = true;
Func<object> BotStopActivity = () => null;
Func<object> NextActivity = MainStep;
for(;;)
{
MemoryUpdate();
Host.Log(
	"Session started at: " +  startSession.ToString(" HH:mm") +// alternative (" dd/MM/yyyy HH:mm") 
	" ; armor.hp: " + ArmorHpPercent + "%" +
	" ; retreat: " +(chatLocal?.ParticipantView?.Entry?.Count(IsNeutralOrEnemy)-1)+ " # "  + RetreatReason + 
	" ; overview.rats: " + ListRatOverviewEntry?.Length +
	" ; drones in space( from total): " + DronesInSpaceCount + "(" +(DronesInSpaceCount + DronesInBayCount)+ ")"+
	" ; targeted rats :  " + Measurement?.Target?.Length+
	" ; cargo percent : " + OreHoldFillPercent + "%" +
	" ; Logout in (d/h/ m) : "  + TimeSpan.FromMinutes(logoutgame).ToString(@"dd\:hh\:mm")+
	" ; current offload count (max limit): " + OffloadCount + "("+ LimitOffloadCount+")" +
	" ; nextAct  : " + NextActivity?.Method?.Name);
CloseModalUIElement();
if(Measurement?.WindowOther != null)
    CloseWindowOther();
if(Measurement?.WindowTelecom != null)
    CloseWindowTelecom();
if (Tethering)
StopArmorRepairer();
if (Measurement?.IsDocked ?? false)
    MainStep();
if(0 < RetreatReason?.Length && !(Measurement?.IsDocked ?? false))
{
	if (listOverviewDreadCheck?.Length > 0)
	{		Host.Log("I'm a chicken and I'm run from dread");
	if (RattingAsteroids)
     {
        StopAfterburner();
        ActivateArmorRepairerExecute();
        InitiateWarpToMiningSite();
     }
	}
	Host.Log("Tactical retreat !! Better be a living cicken than a roasted bull! ");
	//Console.Beep(500, 200);// he will beep alot
	StopAfterburner();
	ActivateArmorRepairerExecute();
	 if (Measurement?.ShipUi?.Indication?.ManeuverType == ShipManeuverTypeEnum.Orbit)
	{
	 ClickMenuEntryOnPatternMenuRoot(Measurement?.InfoPanelCurrentSystem?.ListSurroundingsButton, UnloadBookmark, "align");
	}
	if (ReadyForManeuver
	&&(!returnDronesToBayOnRetreat || null == WindowDrones
	|| (returnDronesToBayOnRetreat && 0 == DronesInSpaceCount)))
	{
	Host.Log("Picard : Yes, I warping home( I know ... I know ... Is an Miracle!!) ");
	 ClickMenuEntryOnPatternMenuRoot(Measurement?.InfoPanelCurrentSystem?.ListSurroundingsButton, UnloadBookmark, "dock");
	}
	else 
		{ DroneEnsureInBay();}
	continue;
}
NextActivity = NextActivity?.Invoke() as Func<object>;
if(BotStopActivity == NextActivity)
	break;	
if(null == NextActivity)
	NextActivity = MainStep;
Host.Delay(1111);
}

int? ShieldHpPercent => ShipUi?.HitpointsAndEnergy?.Shield / 10;
int? ArmorHpPercent => ShipUi?.HitpointsAndEnergy?.Armor / 10;
bool DefenseExit =>
    (Measurement?.IsDocked ?? false) ||
    !(0 < ListRatOverviewEntry?.Length);
bool DefenseEnter =>
    !DefenseExit;
string RetreatReasonTemporary = null;
string RetreatReasonPermanent = null;
string RetreatReason => RetreatReasonPermanent ?? RetreatReasonTemporary;
int? LastCheckOreHoldFillPercent = null;
int OffloadCount = 0;
bool OreHoldFilledForOffload => Math.Max(0, Math.Min(100, EnterOffloadOreHoldFillPercent)) <= OreHoldFillPercent;
Func<object> MainStep()
{
    if (Measurement?.IsDocked ?? false)
    {
        ReviewSettings();	
        if (0 < RetreatReasonPermanent?.Length || OffloadCount > LimitOffloadCount
		|| logoutme==true || reasonCapsule == true)
        {
        Host.Log("permanent retreat or you are in capsule = bot stop");
        	Sanderling.KeyboardPressCombined(new[]{ VirtualKeyCode.LMENU, VirtualKeyCode.SHIFT, VirtualKeyCode.VK_Q});
		Host.Delay(1111);
			if (reasonDrones == true) 
			{ Host.Log(" Until you refill your drones = bot stop");
				return BotStopActivity;
			}
		return BotStopActivity; 
		}
		InInventoryUnloadItems();
        if (0 < RetreatReason?.Length)
			{ Host.Log("Hostiles on local, but I'm safe into Station ... taking a nap");
			Host.Delay(4111);
			return MainStep;
			}
		Random rnd = new Random();
		int DelayTime = rnd.Next(MinimDelayUndock, MaximDelayUndock);
			Host.Log("Keep your horses for :  " + DelayTime+ " s ");
			Host.Delay( DelayTime*1000);
        if (null == RetreatReason?.Length)
             Undock();
        else 
            return MainStep;
    }
    if (ReadyForManeuver)
    {
        DroneEnsureInBay(); 
        ModuleMeasureAllTooltip(); 
        Host.Log("Refreshing news: I'm ready for rats");
        if (0 == DronesInSpaceCount && 0 == ListRatOverviewEntry?.Length)
        {
			if (ReadyForManeuver)
            {
                if (OreHoldFilledForOffload)
                {
                    ClickMenuEntryOnPatternMenuRoot(Measurement?.InfoPanelCurrentSystem?.ListSurroundingsButton, UnloadBookmark, "dock");
                    return MainStep;
                }
				if ((!OreHoldFilledForOffload) && (0 <= ListRatOverviewEntry?.Length)
					&& (listOverviewCommanderWreck?.Length > 0)
					&& (ListCelestialObjects?.Length > 0))
				return InBeltMineStep;
                if (RattingAnomaly)
                {
						Host.Log("I would like to spin around rocks");
                    return TakeAnomaly;
                }
                if (RattingAsteroids)
                {
						Host.Log("Maybe some Asteroids bring some cool rats? :)");
                    InitiateWarpToMiningSite();
                    return MainStep;
                }
            }
        }

    }
//	ModuleMeasureAllTooltip(); //if you want again an measurement, uncomment
	if (ActivateHardener)
		ActivateHardenerExecute();
	if (ActivateOmni)	
		ActivateOmniExecute();
	return InBeltMineStep;
}
void CloseModalUIElement()
{
    var ButtonClose =
        ModalUIElement?.ButtonText?.FirstOrDefault(button => (button?.Text).RegexMatchSuccessIgnoreCase("close|no|ok"));
    Sanderling.MouseClickLeft(ButtonClose);
}
void CloseWindowTelecom()
{
    var WindowTelecom = Measurement?.WindowTelecom?.FirstOrDefault(w => (w?.Caption.RegexMatchSuccessIgnoreCase("Information") ?? false));
    var CloseButton = WindowTelecom?.ButtonText?.FirstOrDefault(text => text.Text.RegexMatchSuccessIgnoreCase("Close"));
    var HavenTelecom = Measurement?.WindowTelecom?.FirstOrDefault()?.LabelText?.FirstOrDefault(text => (text?.Text.RegexMatchSuccessIgnoreCase("Ship Computer") ?? false));
    if (CloseButton != null)
        Sanderling.MouseClickLeft(CloseButton);
}
public void CloseWindowOther()//thx Terpla
{
    var windowOther = Sanderling?.MemoryMeasurementParsed?.Value?.WindowOther?.FirstOrDefault();
    //	if close button not visible then move mouse to the our window
    if (!windowOther?.HeaderButtonsVisible ?? false)
        Sanderling.MouseMove(windowOther.LabelText.FirstOrDefault());

    Sanderling.InvalidateMeasurement(); //	make sure we have new measurement
    if (windowOther?.HeaderButton != null)
    {
        //	we have 3 buttons and looking with HintText "Close"
        var closeButton = windowOther.HeaderButton?.FirstOrDefault(x => x.HintText == "Close");
        if (closeButton != null)
            Sanderling.MouseClickLeft(closeButton);
    }
}
void DroneLaunch()
{
    Host.Log("Launching my Vipers");
    Sanderling.MouseClickRight(DronesInBayListEntry);
    Sanderling.MouseClickLeft(Menu?.FirstOrDefault()?.EntryFirstMatchingRegexPattern("launch", RegexOptions.IgnoreCase));
}
void DroneEnsureInBay()
{
    if (null == WindowDrones || DronesInSpaceCount==0)
        return;
     else
    DroneReturnToBay();
    Host.Delay(4444);
}
void DroneReturnToBay()
{
    Host.Log("Inconceivable to forget my Vipers here");
    //Sanderling.MouseClickRight(DronesInSpaceListEntry);
    //Sanderling.MouseClickLeft(Menu?.FirstOrDefault()?.EntryFirstMatchingRegexPattern("return.*bay", RegexOptions.IgnoreCase));
     Sanderling.KeyboardPressCombined(new[]{ targetLockedKeyCode, VirtualKeyCode.VK_R });//if you like 
}
var NoMoreRats = false;
Func<object> DefenseStep()
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
    var setDroneInLocalSpace = droneListView?.Entry?.OfType<DroneViewEntryItem>()
        ?.Where(drone => droneGroupInLocalSpace?.RegionCenter()?.B < drone?.RegionCenter()?.B)
        ?.ToArray();
    var droneInLocalSpaceSetStatus =
        setDroneInLocalSpace?.Select(drone => drone?.LabelText?.Select(label => label?.Text?.StatusStringFromDroneEntryText()))?.ConcatNullable()?.WhereNotDefault()?.Distinct()?.ToArray();
    var droneInLocalSpaceIdle =
        droneInLocalSpaceSetStatus?.Any(droneStatus => droneStatus.RegexMatchSuccessIgnoreCase("idle")) ?? false;
    var droneGroupInBay = droneGroupWithNameMatchingPattern("bay");
    if (ActivateArmorRepairer == true || ArmorHpPercent < StartArmorRepairerHitPoints)
    {
        Host.Log("Armor integrity < "  + StartArmorRepairerHitPoints + "%");
        ActivateArmorRepairerExecute();
    }
    if (ArmorHpPercent > StartArmorRepairerHitPoints && ActivateArmorRepairer == false)
    { StopArmorRepairer(); }
    if (DefenseExit)
    { 
	return null; 
	}
    if (Measurement?.ShipUi?.Indication?.ManeuverType != ShipManeuverTypeEnum.Orbit)
		return InBeltMineStep;	
    if (null == targetSelected)
        LockTarget();
    if (0 < DronesInBayCount && DronesInSpaceCount < DroneNumber)
        DroneLaunch();
    if (!(0 < DronesInSpaceCount))
        DroneLaunch();
    if (null != targetSelected)
    {
        if (shouldAttackTarget)
        {
		if (Measurement?.Target?.FirstOrDefault(target => target?.IsSelected ?? false).DistanceMax < WeaponRange)
            ActivateWeaponExecute();
		if (droneInLocalSpaceIdle && (Measurement?.Target?.Length > 0))
			{
				Sanderling.KeyboardPress(attackDrones);
				Host.Log("Vipers message: Sir! Yes Sir! We engage the target");
			}
        }
        else
            UnlockTarget();
    }
    if (Measurement?.Target?.Length < TargetCountMax && 1 < ListRatOverviewEntry?.Count())
        LockTarget();
    if (EWarToAttack?.Count() > 0)//thx pikacuq
    {
        var EWarSelected = EWarToAttack?.FirstOrDefault(target => target?.IsSelected ?? false);
        var EWarLocked = EWarToAttack?.FirstOrDefault(target => target?.MeTargeted ?? false);
        if (EWarLocked == null)
        {
            Sanderling.KeyDown(lockTargetKeyCode);
            Sanderling.MouseClickLeft(EWarToAttack?.FirstOrDefault(entry => !((entry?.MeTargeted ?? false))));
            Sanderling.KeyUp(lockTargetKeyCode);
        }
        else if (EWarSelected == null)
        { Sanderling.MouseClickLeft(EWarToAttack?.FirstOrDefault()); }
        else if ( null != EWarSelected && droneInLocalSpaceIdle)
        {
            //Host.Log("drones change to ewar target"); // it was for debug
            Sanderling.KeyboardPress(attackDrones);
            Host.Log("Some nasty rats, engaging them ");
        }
    }
    if (0 == ListRatOverviewEntry?.Count())
    {
	StopAfterburner();
	DroneEnsureInBay();
        NoMoreRats = true;
    }
    return DefenseStep;
}
var SiteFinished = false;
Func<object> InBeltMineStep()
{
    if (RattingAnomaly && (0 < listOverviewEntryFriends?.Length || ListCelestialToAvoid?.Length > 0 ) && 0 < ListRatOverviewEntry?.Length && ReadyForManeuver )
	{            
        if (  ListCelestialToAvoid?.Length > 0)
	    	{
	            Sanderling.KeyboardPressCombined(new[] { VirtualKeyCode.LMENU, VirtualKeyCode.VK_P });
	            Host.Log("Gas Haven, better run!!");
	            ClickMenuEntryOnPatternMenuRoot(Measurement?.InfoPanelCurrentSystem?.ListSurroundingsButton, UnloadBookmark, "warp");
	        }
	
		if (Measurement?.ShipUi?.Indication?.ManeuverType != ShipManeuverTypeEnum.Orbit)
   	    {
		Host.Log("Presence of friends on site! I'm doing you a favor and ignore this anomaly and a second favor to take another one(anomaly)");
		ActivateArmorRepairerExecute();//to be sure I stay alive, rats can target me
        return TakeAnomaly;
		}
	}
    if ((ReadyForManeuver) && (Measurement?.ShipUi?.Indication?.ManeuverType != ShipManeuverTypeEnum.Orbit) && (0 < ListRatOverviewEntry?.Length))
   {
        Orbitkeyboard();
        if (DefenseEnter)
        {
            return DefenseStep;
        }
    }
    EnsureWindowInventoryOpen();
        if ((!OreHoldFilledForOffload) && 0 == ListRatOverviewEntry?.Length && 0 < listOverviewCommanderWreck?.Length && ReadyForManeuver)
	{
        if(!(listOverviewCommanderWreck?.FirstOrDefault()?.DistanceMax > 16000))
            StopAfterburner();
        else 		
           ActivateAfterburnerExecute();
        WreckLoot();  
        LootingCargo();
  
	}
    else if (( OreHoldFilledForOffload || 0 == listOverviewCommanderWreck?.Length ) && 0 == ListRatOverviewEntry?.Length && ReadyForManeuver)
 	{
        if (AnomalyToTake == "haven" && 0 == ListRatOverviewEntry?.Length && NoMoreRats == false && 0 < ListCelestialObjects?.Length)
            {
            Host.Log("I'm in Heaven, waiting my rats :d :))");
                while( 0 == ListRatOverviewEntry?.Length)
                {
                Host.Delay(1111);
                    return InBeltMineStep;
                }
            }
        Host.Log("Im coolest! Site finished! "); 
		SiteFinished = true;	
        return MainStep;
	}
    if (!ReadyForManeuver)
        return MainStep;
    return InBeltMineStep;
}
void WreckLoot()
{  
    if (OnePush == true && (listOverviewCommanderWreck?.FirstOrDefault()?.DistanceMax > 100) )
     { 
        ClickMenuEntryOnMenuRoot(listOverviewCommanderWreck?.FirstOrDefault(), "open cargo");
        OnePush = false;
     }
}
void LootingCargo ()
{
    var LootButton = Measurement?.WindowInventory?[0]?.ButtonText?.FirstOrDefault(text => text.Text.RegexMatchSuccessIgnoreCase("Loot All"));
    if (LootButton != null)
     {  
        Sanderling.MouseClickLeft(LootButton);
        OnePush = true;
     }
}
Sanderling.Parse.IMemoryMeasurement Measurement =>
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
Sanderling.Parse.IWindowOverview WindowOverview =>
    Measurement?.WindowOverview?.FirstOrDefault();
Sanderling.Parse.IWindowInventory WindowInventory =>
    Measurement?.WindowInventory?.FirstOrDefault();
IWindowDroneView WindowDrones =>
    Measurement?.WindowDroneView?.FirstOrDefault();
Tab OverviewTabActive =>
	Measurement?.WindowOverview?.FirstOrDefault()?.PresetTab
	?.OrderByDescending(tab => tab?.LabelColorOpacityMilli ?? 1500)
	?.FirstOrDefault();
Tab combatTab => WindowOverview?.PresetTab
	?.OrderByDescending(tab => tab?.Label.Text.RegexMatchSuccessIgnoreCase(rattingTab))
	?.FirstOrDefault();
var inventoryActiveShip = WindowInventory?.ActiveShipEntry;
var inventoryActiveShipEntry = WindowInventory?.ActiveShipEntry;
var ShipHasHold = inventoryActiveShipEntry?.TreeEntryFromCargoSpaceType(ShipCargoSpaceTypeEnum.General) != null;
var hasHold = ShipHasHold;
ITreeViewEntry InventoryActiveShipContainer
{
    get
    {
        var hasHold = ShipHasHold;
        return
        WindowInventory?.ActiveShipEntry?.TreeEntryFromCargoSpaceType( hasHold ? ShipCargoSpaceTypeEnum.OreHold : ShipCargoSpaceTypeEnum.General);
    }
}
//var OreHoldCapacityMilliMax =0;
IInventoryCapacityGauge OreHoldCapacityMilli =>
    (InventoryActiveShipContainer?.IsSelected ?? false) ? WindowInventory?.SelectedRightInventoryCapacityMilli : null;
int? OreHoldFillPercent => OreHoldCapacityMilli?.Max > 0 ? ((int?)((OreHoldCapacityMilli?.Used * 100) / OreHoldCapacityMilli?.Max )) : 0 ;
var reasonCapsule  = false;
Sanderling.Accumulation.IShipUiModule[] SetModuleWeapon =>
	Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule?.Where(module => module?.TooltipLast?.Value?.IsWeapon ?? false)?.ToArray();
int?		WeaponRange => SetModuleWeapon?.Select(module =>
	module?.TooltipLast?.Value?.RangeOptimal ?? module?.TooltipLast?.Value?.RangeMax ?? module?.TooltipLast?.Value?.RangeWithin ?? 0)?.DefaultIfEmpty(0)?.Min();
string OverviewTypeSelectionName =>
    WindowOverview?.Caption?.RegexMatchIfSuccess(@"\(([^\)]*)\)")?.Groups?[1]?.Value;
Parse.IOverviewEntry[] ListRatOverviewEntry => WindowOverview?.ListView?.Entry?.Where(entry =>
    (entry?.MainIconIsRed ?? false))
    ?.OrderBy(entry => entry?.Name?.RegexMatchSuccessIgnoreCase(@"battery|tower|sentry|web|strain|splinter|render|raider|friar|reaver")) //Frigate
    ?.OrderBy(entry => entry?.Name?.RegexMatchSuccessIgnoreCase(@"coreli|centi|alvi|pithi|corpii|gistii|cleric|engraver")) //Frigate
    ?.OrderBy(entry => entry?.Name?.RegexMatchSuccessIgnoreCase(@"corelior|centior|alvior|pithior|corpior|gistior")) //Destroyer
    ?.OrderBy(entry => entry?.Name?.RegexMatchSuccessIgnoreCase(@"corelum|centum|alvum|pithum|corpum|gistum|prophet")) //Cruiser
    ?.OrderBy(entry => entry?.Name?.RegexMatchSuccessIgnoreCase(@"corelatis|centatis|alvatis|pithatis|corpatis|gistatis|apostle")) //Battlecruiser
    ?.OrderBy(entry => entry?.Name?.RegexMatchSuccessIgnoreCase(@"core\s|centus|alvus|pith\s|corpus|gist\s")) //Battleship
    ?.ThenBy(entry => entry?.DistanceMax ?? int.MaxValue)
    ?.ToArray();
Parse.IOverviewEntry[] ListCelestialObjects => WindowOverview?.ListView?.Entry
    ?.Where(entry => entry?.Name?.RegexMatchSuccessIgnoreCase(celestialOrbit) ?? false)
    ?.OrderBy(entry => entry?.DistanceMax ?? int.MaxValue)
    ?.ToArray();
Parse.IOverviewEntry[] ListCelestialToAvoid => WindowOverview?.ListView?.Entry
    ?.Where(entry => entry?.Name?.RegexMatchSuccessIgnoreCase(CelestialToAvoid ) ?? false)
    ?.OrderBy(entry => entry?.DistanceMax ?? int.MaxValue)
    ?.ToArray();
Parse.IOverviewEntry[] listOverviewDreadCheck => WindowOverview?.ListView?.Entry
    ?.Where(entry => (entry?.Name?.RegexMatchSuccess(runFromRats) ?? true) || (entry?.Type?.RegexMatchSuccess(runFromRats) ?? true))
    .ToArray();
Parse.IOverviewEntry[] listOverviewEntryFriends =>
    WindowOverview?.ListView?.Entry
    ?.Where(entry => entry?.ListBackgroundColor?.Any(IsFriendBackgroundColor) ?? false)
    ?.ToArray();
Parse.IOverviewEntry[] listOverviewEntryEnemy =>
    WindowOverview?.ListView?.Entry
    ?.Where(entry => entry?.ListBackgroundColor?.Any(IsEnemyBackgroundColor) ?? false)
    ?.ToArray();
// this is for ewar - not used for the momment
EWarTypeEnum[] listEWarPriorityGroupTeamplate =
{
    EWarTypeEnum.WarpDisrupt, EWarTypeEnum.WarpScramble, EWarTypeEnum.ECM, EWarTypeEnum.Web, EWarTypeEnum.EnergyNeut, EWarTypeEnum.EnergyVampire, EWarTypeEnum.TrackingDisrupt
};
Parse.IOverviewEntry[] EWarToAttack =>
    WindowOverview?.ListView?.Entry
	?.Where(entry => entry != null && (!entry?.EWarType?.IsNullOrEmpty() ?? false) && (entry?.EWarType).Any())
	?.ToArray(); 
Parse.IOverviewEntry[] listOverviewCommanderWreck =>
    WindowOverview?.ListView?.Entry
    ?.Where(entry => entry?.Name?.RegexMatchSuccessIgnoreCase(commanderNameWreck) ?? true)
    ?.OrderBy(entry => entry?.Name?.RegexMatchSuccessIgnoreCase(@"Commander|Dark Blood|true|Shadow Serpentis|Dread Gurista|Domination Saint|Gurista Distributor|Sentient|Overseer|Spearhead|Dread Guristas|Estamel|Vepas|Thon|Kaikka|True Sansha|Chelm|Vizan|Selynne|Brokara|Dark Blood|Draclira|Ahremen|Raysere|Tairei|Cormack|Setele|Tuvan|Brynn|Domination|Tobias|Gotan|Hakim|Mizuro")) //Battleship
    ?.OrderBy(entry => entry?.Name?.RegexMatchSuccessIgnoreCase(@"core\s|corpatis|centus|alvus|pith\s|corpus|gist\s")) //Battleship
    ?.OrderBy(entry => entry?.Name?.RegexMatchSuccessIgnoreCase(@"corelatis|centatis|alvatis|pithatis|corpatis|gistatis|apostle")) //Battlecruiser
    ?.OrderBy(entry => entry?.Name?.RegexMatchSuccessIgnoreCase(@"corelum|centum|alvum|pithum|corpum|gistum|prophet")) //Cruiser
    ?.OrderBy(entry => entry?.Name?.RegexMatchSuccessIgnoreCase(@"corelior|centior|alvior|pithior|corpior|gistior")) //Destroyer
    ?.OrderBy(entry => entry?.Name?.RegexMatchSuccessIgnoreCase(@"coreli|centi|alvi|pithi|corpii|gistii|cleric|engraver")) //Frigate
//    ?.OrderBy(entry => entry?.DistanceMax ?? int.MaxValue)
    .ToArray();
DroneViewEntryGroup DronesInBayListEntry =>
    WindowDrones?.ListView?.Entry?.OfType<DroneViewEntryGroup>()?.FirstOrDefault(Entry => null != Entry?.Caption?.Text?.RegexMatchIfSuccess(@"Drones in bay", RegexOptions.IgnoreCase));
DroneViewEntryGroup DronesInSpaceListEntry =>
    WindowDrones?.ListView?.Entry?.OfType<DroneViewEntryGroup>()?.FirstOrDefault(Entry => null != Entry?.Caption?.Text?.RegexMatchIfSuccess(@"Drones in Local Space", RegexOptions.IgnoreCase));
int? DronesInSpaceCount => DronesInSpaceListEntry?.Caption?.Text?.AsDroneLabel()?.Status?.TryParseInt();
int? DronesInBayCount => DronesInBayListEntry?.Caption?.Text?.AsDroneLabel()?.Status?.TryParseInt();
public bool Tethering =>
    Measurement?.ShipUi?.EWarElement?.Any(EwarElement => (EwarElement?.EWarType).RegexMatchSuccess("tethering")) ?? false;
public bool ReadyForManeuverNot =>
    Measurement?.ShipUi?.Indication?.LabelText?.Any(indicationLabel =>
        (indicationLabel?.Text).RegexMatchSuccessIgnoreCase("warp|docking")) ?? false;
public bool ReadyForManeuver => !ReadyForManeuverNot && !(Measurement?.IsDocked ?? true);
Sanderling.Interface.MemoryStruct.IListEntry WindowInventoryItem =>
    WindowInventory?.SelectedRightInventory?.ListView?.Entry?.FirstOrDefault();
WindowChatChannel chatLocal =>
     Sanderling.MemoryMeasurementParsed?.Value?.WindowChatChannel
     ?.FirstOrDefault(windowChat => windowChat?.Caption?.RegexMatchSuccessIgnoreCase("local") ?? false);
//    assuming that own character is always visible in local
public bool hostileOrNeutralsInLocal => 1 < chatLocal?.ParticipantView?.Entry?.Count(IsNeutralOrEnemy);
void ClickMenuEntryOnMenuRoot(IUIElement MenuRoot, string MenuEntryRegexPattern)
{
    Sanderling.MouseClickRight(MenuRoot);
    var Menu = Measurement?.Menu?.FirstOrDefault();
    var MenuEntry = Menu?.EntryFirstMatchingRegexPattern(MenuEntryRegexPattern, RegexOptions.IgnoreCase);
    Sanderling.MouseClickLeft(MenuEntry);
}
void EnsureWindowInventoryOpen()
{
    if (null != WindowInventory)
        return;
   // Host.Log("open Inventory.");
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
//	sample label text: Intensive Reprocessing Array <color=#66FFFFFF>1,123 m</color //
string InventoryContainerLabelRegexPatternFromContainerName(string containerName) =>
    @"^\s*" + Regex.Escape(containerName) + @"\s*($|\<)";
void InInventoryUnloadItems() => InInventoryUnloadItemsTo(UnloadDestContainerName);
void InInventoryUnloadItemsTo(string DestinationContainerName)
{
   Host.Log("unload items to '" + DestinationContainerName + "'.");
    EnsureWindowInventoryOpenActiveShip();
    for (; ; )
    {
        var oreHoldListItem = WindowInventory?.SelectedRightInventory?.ListView?.Entry?.ToArray();
        var oreHoldItem = oreHoldListItem?.FirstOrDefault();
        if (null == oreHoldItem)
            break;    //    0 items in Cargo
        if (1 < oreHoldListItem?.Length)
            ClickMenuEntryOnMenuRoot(oreHoldItem, @"select\s*all");
        var DestinationContainerLabelRegexPattern =
            InventoryContainerLabelRegexPatternFromContainerName(DestinationContainerName);
        var DestinationContainer =
            WindowInventory?.LeftTreeListEntry?.SelectMany(entry => new[] { entry }.Concat(entry.EnumerateChildNodeTransitive()))
            ?.FirstOrDefault(entry => entry?.Text?.RegexMatchSuccessIgnoreCase(DestinationContainerLabelRegexPattern) ?? false);
        if (null == DestinationContainer)
            Host.Log("Houston, we have a problem: '" + DestinationContainerName + "' not found");
        Sanderling.MouseDragAndDrop(oreHoldItem, DestinationContainer);
    }
}
bool InitiateWarpToMiningSite()	=>
	InitiateDockToOrWarpToLocationInSolarSystemMenu("asteroid belts", PickNextMiningSiteFromSystemMenu);
MemoryStruct.IMenuEntry PickNextMiningSiteFromSystemMenu(IReadOnlyList<MemoryStruct.IMenuEntry> availableMenuEntries)
{
	Host.Log("Hubble says he saw  " + availableMenuEntries?.Count.ToString() + " mining sites to choose from.");
	var nextSite =
		availableMenuEntries
		?.OrderBy(menuEntry => visitedLocations.ToList().IndexOf(menuEntry?.Text))
		?.FirstOrDefault();
	Host.Log("I pick in order '" + nextSite?.Text + "' as next mining site. You like it or not, c'est la vie :p");
	return nextSite;
}
bool InitiateDockToOrWarpToLocationInSolarSystemMenu(
	string submenuLabel,
	Func<IReadOnlyList<MemoryStruct.IMenuEntry>, MemoryStruct.IMenuEntry> pickPreferredDestination = null)
{
	Host.Log("Preparing engines for '" + submenuLabel + "'");
	var listSurroundingsButton = Measurement?.InfoPanelCurrentSystem?.ListSurroundingsButton;
	Sanderling.MouseClickRight(listSurroundingsButton);
	var submenuEntry = Measurement?.Menu?.FirstOrDefault()?.EntryFirstMatchingRegexPattern("^" + submenuLabel + "$", RegexOptions.IgnoreCase);
	if(null == submenuEntry)
	{
		Host.Log("Failure on telemetry systems: Submenu '" + submenuLabel + "' not found.");
		return true;
	}
	Sanderling.MouseClickLeft(submenuEntry);
	var submenu = Measurement?.Menu?.ElementAtOrDefault(1);
	var destinationMenuEntry = pickPreferredDestination?.Invoke(submenu?.Entry?.ToList()) ?? submenu?.Entry?.FirstOrDefault();
	if(destinationMenuEntry == null)
	{
		Host.Log("My fingers failed to open submenu '" + submenuLabel + "' in the solar system menu.");
		return true;
	}
	Sanderling.MouseClickLeft(destinationMenuEntry);
	var actionsMenu = Measurement?.Menu?.ElementAtOrDefault(2);
	if(destinationMenuEntry == null)
	{
		Host.Log("I'm drunk? Failed to open actions menu for '" + destinationMenuEntry.Text + "' in the solar system menu.");
		return true;
	}
	var menuResultaction = actionsMenu?.Entry.ToArray();
	var menuResultSelectWarpMenu= menuResultaction?[1];
	var maneuverMenuEntry = menuResultSelectWarpMenu;
	if (maneuverMenuEntry?.Text != "Warp to Within")
	{
		return true;
	}
	if (maneuverMenuEntry?.Text == "Warp to Within")
	{
		Host.Log("Prepare your engines for '" + maneuverMenuEntry.Text + "' on '" + destinationMenuEntry?.Text + "'");
		Sanderling.MouseClickRight(maneuverMenuEntry);
		var menuResultats = Measurement?.Menu?.ElementAtOrDefault(3);
		var menuResultWarpDestination = menuResultats?.Entry.ToArray();
		if (menuResultWarpDestination[0].Text !=  "Within 0 m")
		{
		Host.Log("Failed to open the kinder egg '" + destinationMenuEntry.Text + "' in the solar system menu.");
		return true;
		}
		else
		{
		Host.Log("'Engines' to Picard:initiating  warp on '" + destinationMenuEntry?.Text + "'");
		ClickMenuEntryOnMenuRoot(menuResultWarpDestination[0], "within 0 m");
   		Host.Delay(8000);
		return false;
		}		
	}
	Host.Log("no suitable menu entry found on '" + destinationMenuEntry?.Text + "'");
	return true;
}
void LockTarget()
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
    Host.Log("sorry, this is not a target");
}
void Undock()
{
    while (Measurement?.IsDocked ?? true)
    {
        Sanderling.MouseClickLeft(Measurement?.WindowStation?.FirstOrDefault()?.UndockButton);
        Host.Log("waiting for undocking to complete.");
        Host.Delay(4826);
    }
    Sanderling.KeyboardPressCombined(new[]{ VirtualKeyCode.LCONTROL, VirtualKeyCode.SPACE});
    Host.Delay(8444);
    Sanderling.InvalidateMeasurement();
}
void ModuleMeasureAllTooltip()
{
	Host.Log("Your modules have to be somewhere");
		var armorRapairCount = Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule.Count(m => m?.TooltipLast?.Value?.IsArmorRepairer ?? false);
		var afterburnersCount = Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule.Count((module => (module?.TooltipLast?.Value?.IsAfterburner ?? false) || (module?.TooltipLast?.Value?.IsMicroWarpDrive?? false)));
		var hardenersCount = Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule.Count(m => m?.TooltipLast?.Value?.IsHardener ?? false);
		var omniCount  = Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule.Count(module => module?.TooltipLast?.Value?.LabelText?.Any(
					label => label?.Text?.RegexMatchSuccess(OmniSup, System.Text.RegularExpressions.RegexOptions.IgnoreCase) ?? false) ?? false);
	while( (armorRapairCount < ArmorRepairsCount) || (afterburnersCount <  AfterburnersCount)
			|| (hardenersCount <  HardenersCount)|| (omniCount <  OmniCount)	)
	{
		if(Sanderling.MemoryMeasurementParsed?.Value?.IsDocked ?? false)
			break;
		foreach(var NextModule in Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule)
		{
			if(null == NextModule)
				break;
			Host.Log("This circuit is all right? let's measure this module");
			//	take multiple measurements of module tooltip to reduce risk to keep bad read tooltip.
			Sanderling.MouseMove(NextModule);
			Sanderling.WaitForMeasurement();
			Sanderling.MouseMove(NextModule);
		}		
	omniCount  = Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule.Count(module => module?.TooltipLast?.Value?.LabelText?.Any(
					label => label?.Text?.RegexMatchSuccess(OmniSup, System.Text.RegularExpressions.RegexOptions.IgnoreCase) ?? false) ?? false);
		hardenersCount = Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule.Count(m => m?.TooltipLast?.Value?.IsHardener ?? false);
		armorRapairCount = Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule.Count(m => m?.TooltipLast?.Value?.IsArmorRepairer ?? false);
		afterburnersCount = Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule.Count((module => (module?.TooltipLast?.Value?.IsAfterburner ?? false) || (module?.TooltipLast?.Value?.IsMicroWarpDrive?? false)));
		Host.Log(  " Armor Repair count = " + armorRapairCount + "; Afterburners count = " + afterburnersCount + " ;     Hardeners count = " + hardenersCount + " ;  Omni count = " + omniCount + " " );
	}
}
void ActivateHardenerExecute()
{
    var SubsetModuleHardener =
        Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule
        ?.Where(module => module?.TooltipLast?.Value?.IsHardener ?? false);
     var SubsetModuleToToggle =
        SubsetModuleHardener
        ?.Where(module => !(module?.RampActive ?? false));
    if ( SubsetModuleHardener.Count()>0)
    foreach (var Module in SubsetModuleToToggle.EmptyIfNull())
        ModuleToggle(Module);
}
void ActivateArmorRepairerExecute()
{
    var SubsetModuleArmorRepairer =
        Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule
        ?.Where(module => module?.TooltipLast?.Value?.IsArmorRepairer ?? false);
    var SubsetModuleToToggle =
        SubsetModuleArmorRepairer
        ?.Where(module => !(module?.RampActive ?? false));
    if ( SubsetModuleArmorRepairer.Count()>0)
    foreach (var Module in SubsetModuleToToggle.EmptyIfNull())
        ModuleToggle(Module);
}
void StopArmorRepairer()
{
    var SubsetModuleArmorRepairer =
        Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule
        ?.Where(module => module?.TooltipLast?.Value?.IsArmorRepairer ?? false);
    var SubsetModuleToToggle =
        SubsetModuleArmorRepairer
        ?.Where(module => (module?.RampActive ?? false));
    if ( SubsetModuleArmorRepairer.Count()>0)
    foreach (var Module in SubsetModuleToToggle.EmptyIfNull())
        ModuleToggle(Module);
}
void ActivateWeaponExecute()
{
    var SubsetModuleWeapon =
        Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule
        ?.Where(module => module?.TooltipLast?.Value?.IsWeapon ?? false);
    var SubsetModuleToToggle =
        SubsetModuleWeapon
        ?.Where(module => !(module?.RampActive ?? false));
    foreach (var Module in SubsetModuleToToggle.EmptyIfNull())
        ModuleToggle(Module);
}
void ActivateAfterburnerExecute()
{
    var SubsetModuleAfterburner =
        Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule
        ?.Where(module => (module?.TooltipLast?.Value?.IsAfterburner ?? false) || (module?.TooltipLast?.Value?.IsMicroWarpDrive?? false));
    var SubsetModuleToToggle =
        SubsetModuleAfterburner
        ?.Where(module => !(module?.RampActive ?? false));
    if ( SubsetModuleAfterburner.Count()>0)
    foreach (var Module in SubsetModuleToToggle.EmptyIfNull())
        ModuleToggle(Module);
}
void StopAfterburner()
{
    var SubsetModuleAfterburner =
        Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule
        ?.Where(module => (module?.TooltipLast?.Value?.IsAfterburner ?? false) || (module?.TooltipLast?.Value?.IsMicroWarpDrive?? false));
    var SubsetModuleToToggle =
        SubsetModuleAfterburner
        ?.Where(module => (module?.RampActive ?? false));
    if ( SubsetModuleAfterburner.Count()>0)
    foreach (var Module in SubsetModuleToToggle.EmptyIfNull())
    {
		ModuleToggle(Module); 
	}
}
void ActivateOmniExecute()
{
    var SubsetModuleOmni =
		Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule?.Where(module => module?.TooltipLast?.Value?.LabelText?.Any(
		label => label?.Text?.RegexMatchSuccess(OmniSup, System.Text.RegularExpressions.RegexOptions.IgnoreCase) ?? false) ?? false);
    var SubsetModuleToToggle =
        SubsetModuleOmni
        ?.Where(module => !(module?.RampActive ?? false));
    if ( SubsetModuleOmni.Count()>0)
    foreach (var Module in SubsetModuleToToggle.EmptyIfNull())
        ModuleToggle(Module);
}
void ModuleToggle(Sanderling.Accumulation.IShipUiModule Module)
{
    var ToggleKey = Module?.TooltipLast?.Value?.ToggleKey;
    Host.Log("toggle module using " + (null == ToggleKey ? "mouse" : Module?.TooltipLast?.Value?.ToggleKeyTextLabel?.Text));
    if (null == ToggleKey)
        Sanderling.MouseClickLeft(Module);
    else
        Sanderling.KeyboardPressCombined(ToggleKey);
}
void MemoryUpdate()
{
 	RetreatUpdate();
	UpdateLocationRecord();
	OffloadCountUpdate();
	Timers ();
}
var logoutme= false;
var eveNextServerDT = DateTime.UtcNow;
var logoutgame = (eveRealServerDT-DateTime.UtcNow ).TotalMinutes;
void Timers ()
{
var now = DateTime.UtcNow;
var CloseGameSession = (playSession - now).TotalMinutes;
if (now.Day ==eveRealServerDT.Day && now.TimeOfDay > eveRealServerDT.TimeOfDay )
	{	
		eveNextServerDT = eveRealServerDT.AddDays(1);
	}
 if (now.TimeOfDay < eveRealServerDT.TimeOfDay  && ( now.Day>= eveRealServerDT.Day  )) 
	{
		eveNextServerDT = eveRealServerDT.AddDays(0);
	}
var eveSafeDT = eveNextServerDT.AddHours(-hoursToDT).AddMinutes(-minutesToDT);
var CloseGameDT = (eveSafeDT - now).TotalMinutes;
var LogoutGame = Math.Min(CloseGameDT,CloseGameSession);
if (playSession !=DateTime.UtcNow)
{
logoutgame = LogoutGame;
}
	if (LogoutGame<0) 
	{	
	logoutme = true;
		Host.Log("logoutgame, time elapsed " + logoutme + " ");
	}
}
bool MeasurementEmergencyWarpOutEnter =>
    !(Measurement?.IsDocked ?? false) && (!(EmergencyWarpOutHitpointPercent < ArmorHpPercent)  || ((0 < ListRatOverviewEntry?.Length) && (listOverviewEntryFriends?.Length > 0) && (listOverviewEntryFriends?.FirstOrDefault()?.DistanceMax < 50)));
//  !(Measurement?.IsDocked ?? false) && (logoutme == true || !(EmergencyWarpOutHitpointPercent < ArmorHpPercent)  || ((0 < ListRatOverviewEntry?.Length) && (listOverviewEntryFriends?.Length > 0) && (listOverviewEntryFriends?.FirstOrDefault()?.DistanceMax < 50)));


void RetreatUpdate()
{
    RetreatReasonTemporary = (RetreatOnNeutralOrHostileInLocal && hostileOrNeutralsInLocal)
	|| (listOverviewDreadCheck?.Length > 0) || (listOverviewEntryEnemy?.Length > 0) || reasonDrones == true 
	|| (logoutme == true && SiteFinished ==true ) || reasonCapsule == true  ? "Capsule or reds in local or session time elapsed" : null;
    if (!MeasurementEmergencyWarpOutEnter)
        return;
    //	measure multiple times to avoid being scared off by noise from a single measurement. 
    Sanderling.InvalidateMeasurement();
    if (!MeasurementEmergencyWarpOutEnter)
        return;
    RetreatReasonPermanent = "They messed my Armor hp, I am bumped or session is expired??";
}
void UpdateLocationRecord()
{
    //	I am not interested in locations which are only close during warp.
    if (Measurement?.ShipUi?.Indication?.ManeuverType == ShipManeuverTypeEnum.Warp)
        return;
    // Purpose of recording locations is to prioritize our next destination when warping to mining site.
    var currentSystemLocationLabelText =
        Measurement?.InfoPanelCurrentSystem?.ExpandedContent?.LabelText
        ?.OrderByCenterVerticalDown()?.FirstOrDefault()?.Text;
    if (currentSystemLocationLabelText == null)
        return;
    // 2018-03 observed label text: <url=showinfo:15//40088644 alt='Nearest'>Amsen V - Asteroid Belt 1</url>
    var currentLocationName = RegexExtension.RemoveXmlTag(currentSystemLocationLabelText)?.Trim();
    var lastRecordedLocation = visitedLocations.LastOrDefault();
    if (lastRecordedLocation == currentLocationName)
        return;
    visitedLocations.Enqueue(currentLocationName);
    Host.Log("Recorded transition from location '" + lastRecordedLocation + "' to location '" + currentLocationName + "'");
    if (100 < visitedLocations.Count)
        visitedLocations.Dequeue();
}
// Orbit asteroid at 30km  Orbit("Crokite") will orbit first asteroid named Crokite at 10 km, stolen from forum :d
void Orbit(string whatToOrbit, string distance = "30 km")
{
    var ToOrbit = Measurement?.WindowOverview?.FirstOrDefault()?.ListView?.Entry?.Where(entry => entry?.Name?.RegexMatchSuccessIgnoreCase(whatToOrbit) ?? false)?.ToArray();
    ClickMenuEntryOnPatternMenuRoot(ToOrbit.FirstOrDefault(), "Orbit", distance);
}
// Stolen and modified from MineOre.cs
// modified to click on Submenu if SubMenuEntryRegexPattern not null
void ClickMenuEntryOnPatternMenuRoot(IUIElement MenuRoot, string MenuEntryRegexPattern, string SubMenuEntryRegexPattern = null)
{
    Sanderling.MouseClickRight(MenuRoot);
    var Menu = Sanderling?.MemoryMeasurementParsed?.Value?.Menu?.FirstOrDefault();
    var MenuEntry = Menu?.EntryFirstMatchingRegexPattern(MenuEntryRegexPattern, RegexOptions.IgnoreCase);
    Sanderling.MouseClickLeft(MenuEntry);
    if (SubMenuEntryRegexPattern != null)
    {
        // Using the API explorer when we click on the top menu we get another menu that has more options
        // So skip the MenuRoot and click on Submenu
		//var subMenu = Sanderling?.MemoryMeasurementParsed?.Value?.Menu?.Skip(1).First();
		var subMenu = Sanderling?.MemoryMeasurementParsed?.Value?.Menu?.ElementAtOrDefault(1);
        var subMenuEntry = subMenu?.EntryFirstMatchingRegexPattern(SubMenuEntryRegexPattern, RegexOptions.IgnoreCase);
        Sanderling.MouseClickLeft(subMenuEntry);
    }
}
var reasonDrones = false;
Func<object> TakeAnomaly()
{
    var probeScannerWindow = Measurement?.WindowProbeScanner?.FirstOrDefault();
    var scanActuallyAnomaly = probeScannerWindow?.ScanResultView?.Entry?.FirstOrDefault(ActuallyAnomaly);
    var UndesiredAnomaly = probeScannerWindow?.ScanResultView?.Entry?.FirstOrDefault(IgnoreAnomaly);
    var scanResultCombatSite = probeScannerWindow?.ScanResultView?.Entry?.FirstOrDefault(AnomalySuitableGeneral);
    Host.Log("Telemetry instruments: working at ignoring anomalies :) be patient");
   if ( (DronesInSpaceCount + DronesInBayCount ) < DroneNumber)
	{
	reasonDrones = true;
	}
   if (combatTab != OverviewTabActive)
	{ 
	Sanderling.MouseClickLeft(combatTab);
		Host.Delay(1111);
	}
    if (probeScannerWindow == null)
        Sanderling.KeyboardPressCombined(new[] { VirtualKeyCode.LMENU, VirtualKeyCode.VK_P });
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
      if (null == scanResultCombatSite && !Tethering)
    {
        Host.Log(" Hubble: no more anomalies! If you dont like the asteroids then admire the Space from a tethering zone. ");
	 ClickMenuEntryOnPatternMenuRoot(Measurement?.InfoPanelCurrentSystem?.ListSurroundingsButton, UnloadBookmark, "0 m|approach");
    }
    if ((null != scanResultCombatSite) && (null == UndesiredAnomaly))
    {
        Sanderling.MouseClickRight(scanResultCombatSite);
        var menuResult = Measurement?.Menu?.ToList();
        if (null == menuResult)
        { Host.Log(" Telemety calibration: not expected resultats. "); return TakeAnomaly; }
		else
		{
        var menuResultWarp = menuResult?[0].Entry.ToArray();
        var menuResultSelectWarpMenu = menuResultWarp?[1];
        Sanderling.MouseClickLeft(menuResultSelectWarpMenu);
        var menuResultats = Measurement?.Menu?.ToList();
		if (Measurement?.Menu?.ToList() ? [1].Entry.ToArray()[4].Text !=  "Within 50 km")
			{ 
			return TakeAnomaly;
			}
			else
			{        
			var menuResultWarpDestination = Measurement?.Menu?.ToList() ? [1].Entry.ToArray();
			Host.Log(" Hooray, warping to anomaly  ");
			ClickMenuEntryOnMenuRoot(menuResultWarpDestination[4], "within 50 km");
                NoMoreRats = false;
            if (probeScannerWindow != null)
			Sanderling.KeyboardPressCombined(new[] { VirtualKeyCode.LMENU, VirtualKeyCode.VK_P });
			}
		}
        return MainStep;
    }
    return MainStep;
}
void Orbitkeyboard()
{
    //Orbit(celestialOrbit); // to use for orbit celestial objects from overview 
    Sanderling.KeyDown(orbitKeyCode);

    if (0 < ListCelestialObjects?.Length)
        Sanderling.MouseClickLeft(ListCelestialObjects?.FirstOrDefault());
    if (0 == ListCelestialObjects?.Length)
    { Sanderling.MouseClickLeft(ListRatOverviewEntry?.FirstOrDefault(entry => (entry?.MainIconIsRed ?? false))); }
    Sanderling.KeyUp(orbitKeyCode);
    ActivateAfterburnerExecute();
    Host.Delay(1111);
    Host.Log("I smell roses ... better to Orbit arround here");
}
void OffloadCountUpdate()
{
    var CapsuleType = WindowInventory?.LeftTreeListEntry?.SelectMany(entry => new[] { entry }.Concat(entry.EnumerateChildNodeTransitive()))
            ?.FirstOrDefault(entry => entry?.Text?.RegexMatchSuccessIgnoreCase("Capsule") ?? false);
								
    if (null !=CapsuleType )
    {
    reasonCapsule = true;
    Host.Log("reason capsule "+reasonCapsule + " ");
    }
    var OreHoldFillPercentSynced = OreHoldFillPercent;
    if (!OreHoldFillPercentSynced.HasValue)
        return;
    if (0 == OreHoldFillPercentSynced && OreHoldFillPercentSynced < LastCheckOreHoldFillPercent)
        ++OffloadCount;
    LastCheckOreHoldFillPercent = OreHoldFillPercentSynced;
}
void ReviewSettings()
{
    Host.Log("Review Settings: ");
    Host.Log("- retreat on neutrals :  " + RetreatOnNeutralOrHostileInLocal + " ; ");
        Host.Log("- ratting asteroids :  " + RattingAsteroids + " ; ");
            Host.Log("- ratting anomaly :  " + RattingAnomaly + " ; ");
                Host.Log("- anomaly name to take:  " + AnomalyToTake + " ; ");
                    Host.Log("- next DT (Eve Time) :  " + eveNextServerDT.ToString(" dd/MM/yyyy hh:mm:ss")+ " (-1 min); ");
                        Host.Log("- closer logout(d:h:m) :  " + TimeSpan.FromMinutes(logoutgame).ToString(@"dd\:hh\:mm")+ " , taken in account when you finish a site (or change settings); ");
                            Host.Log("- bookmark home: " + RetreatBookmark + " ; ");
                                Host.Log("- offload limit :  " + LimitOffloadCount + " ; ");
                                    Host.Log("- delay undock min(max) :  " + MinimDelayUndock + "(" + MaximDelayUndock + " ); ");
                                    Host.Log("  End of review.");
}

bool AnomalySuitableGeneral(MemoryStruct.IListEntry scanResult) =>
    scanResult?.CellValueFromColumnHeader(AnomalyToTakeColumnHeader)?.RegexMatchSuccessIgnoreCase(AnomalyToTake) ?? false;
bool ActuallyAnomaly(MemoryStruct.IListEntry scanResult) =>
       scanResult?.CellValueFromColumnHeader("Distance")?.RegexMatchSuccessIgnoreCase("km") ?? false;
bool IgnoreAnomaly(MemoryStruct.IListEntry scanResult) =>
scanResult?.CellValueFromColumnHeader(IgnoreColumnheader)?.RegexMatchSuccessIgnoreCase(IgnoreAnomalyName) ?? false;
bool IsEnemyBackgroundColor(ColorORGB color) =>
    color.OMilli == 500 && color.RMilli == 750 && color.GMilli == 0 && color.BMilli == 0;
bool IsFriendBackgroundColor(ColorORGB color) =>
    (color.OMilli == 500 && color.RMilli == 0 && color.GMilli == 150 && color.BMilli == 600) || (color.OMilli == 500 && color.RMilli == 100 && color.GMilli == 600 && color.BMilli == 100);
bool IsNeutralOrEnemy(IChatParticipantEntry participantEntry) =>
   !(participantEntry?.FlagIcon?.Any(flagIcon =>
     new[] { "good standing", "excellent standing", "Pilot is in your (fleet|corporation|alliance)", "Pilot is an ally in one or more of your wars", }
     .Any(goodStandingText =>
        flagIcon?.HintText?.RegexMatchSuccessIgnoreCase(goodStandingText) ?? false)) ?? false);

