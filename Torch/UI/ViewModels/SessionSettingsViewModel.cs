using System.Collections.Generic;
using System.ComponentModel;
using Torch.UI.Views;
using VRage.Game;
using VRage.Library.Utils;

namespace Torch.UI.ViewModels
{
    public class SessionSettingsViewModel : ViewModel
    {
        private readonly MyObjectBuilder_SessionSettings _settings;

        public SessionSettingsViewModel(MyObjectBuilder_SessionSettings settings)
        {
            _settings = settings;
        }

        [Display(Description = "The type of the game mode.", Name = "Game Mode", GroupName = "Others")]
        public MyGameModeEnum GameMode { get => _settings.GameMode; set => SetValue(ref _settings.GameMode, value); }

        [Display(Description = "The type of the game online mode.", Name = "Online Mode", GroupName = "Others")]
        public MyOnlineModeEnum OnlineMode { get => _settings.OnlineMode; set => SetValue(ref _settings.OnlineMode, value); }

        [Display(Description = "The multiplier for character inventory size.", Name = "Character Inventory Size", GroupName = "Multipliers")]
        public float CharacterInventorySizeMultiplier { get => _settings.InventorySizeMultiplier; set => SetValue(ref _settings.InventorySizeMultiplier, value); }

        [Display(Description = "The multiplier for block inventory size.", Name = "Block Inventory Size", GroupName = "Multipliers")]
        public float BlockInventorySizeMultiplier { get => _settings.BlocksInventorySizeMultiplier; set => SetValue(ref _settings.BlocksInventorySizeMultiplier, value); }

        [Display(Description = "The multiplier for assembler speed.", Name = "Assembler Speed", GroupName = "Multipliers")]
        public float AssemblerSpeedMultiplier { get => _settings.AssemblerSpeedMultiplier; set => SetValue(ref _settings.AssemblerSpeedMultiplier, value); }

        [Display(Description = "The multiplier for assembler efficiency.", Name = "Assembler Efficiency", GroupName = "Multipliers")]
        public float AssemblerEfficiencyMultiplier { get => _settings.AssemblerEfficiencyMultiplier; set => SetValue(ref _settings.AssemblerEfficiencyMultiplier, value); }

        [Display(Description = "The multiplier for refinery speed.", Name = "Refinery Speed", GroupName = "Multipliers")]
        public float RefinerySpeedMultiplier { get => _settings.RefinerySpeedMultiplier; set => SetValue(ref _settings.RefinerySpeedMultiplier, value); }

        [Display(Description = "The maximum number of connected players.", Name = "Max Players", GroupName = "Players")]
        public short MaxPlayers { get => _settings.MaxPlayers; set => SetValue(ref _settings.MaxPlayers, value); }

        [Display(Description = "The maximum number of existing floating objects.", Name = "Max Floating Objects", GroupName = "Environment")]
        public short MaxFloatingObjects { get => _settings.MaxFloatingObjects; set => SetValue(ref _settings.MaxFloatingObjects, value); }

        [Display(Description = "The maximum number of backup saves.", Name = "Max Backup Saves", GroupName = "Others")]
        public short MaxBackupSaves { get => _settings.MaxBackupSaves; set => SetValue(ref _settings.MaxBackupSaves, value); }

        [Display(Description = "The maximum number of blocks in one grid.", Name = "Max Grid Blocks", GroupName = "Block Limits")]
        public int MaxGridSize { get => _settings.MaxGridSize; set => SetValue(ref _settings.MaxGridSize, value); }

        [Display(Description = "The maximum number of blocks per player.", Name = "Max Blocks per Player", GroupName = "Block Limits")]
        public int MaxBlocksPerPlayer { get => _settings.MaxBlocksPerPlayer; set => SetValue(ref _settings.MaxBlocksPerPlayer, value); }

        [Display(Description = "The total number of Performance Cost Units in the world.", Name = "World PCU", GroupName = "Block Limits")]
        public int TotalPCU { get => _settings.TotalPCU; set => SetValue(ref _settings.TotalPCU, value); }

        [Display(Description = "The maximum number of existing factions in the world.", Name = "Max Factions Count", GroupName = "Block Limits")]
        public int MaxFactionsCount { get => _settings.MaxFactionsCount; set => SetValue(ref _settings.MaxFactionsCount, value); }

        [Display(Description = "Defines block limits mode.", Name = "Block Limits Mode", GroupName = "Block Limits")]
        public MyBlockLimitsEnabledEnum BlockLimitsEnabled { get => _settings.BlockLimitsEnabled; set => SetValue(ref _settings.BlockLimitsEnabled, value); }

        [Display(Description = "Enables possibility to remove grid remotely from the world by an author.", Name = "Enable Remote Grid Removal", GroupName = "Others")]
        public bool EnableRemoteBlockRemoval { get => _settings.EnableRemoteBlockRemoval; set => SetValue(ref _settings.EnableRemoteBlockRemoval, value); }

        [Display(Description = "Defines hostility of the environment.", Name = "Environment Hostility", GroupName = "Environment")]
        public MyEnvironmentHostilityEnum EnvironmentHostility { get => _settings.EnvironmentHostility; set => SetValue(ref _settings.EnvironmentHostility, value); }

        [Display(Description = "Enables auto healing of the character.", Name = "Auto Healing", GroupName = "Players")]
        public bool AutoHealing { get => _settings.AutoHealing; set => SetValue(ref _settings.AutoHealing, value); }

        [Display(Description = "Enables copy and paste feature.", Name = "Enable Copy & Paste", GroupName = "Players")]
        public bool EnableCopyPaste { get => _settings.EnableCopyPaste; set => SetValue(ref _settings.EnableCopyPaste, value); }

        [Display(Description = "Enables weapons.", Name = "Enable Weapons", GroupName = "Others")]
        public bool WeaponsEnabled { get => _settings.WeaponsEnabled; set => SetValue(ref _settings.WeaponsEnabled, value); }

        [Display(Description = "", Name = "Show Player Names on HUD", GroupName = "Players")]
        public bool ShowPlayerNamesOnHud { get => _settings.ShowPlayerNamesOnHud; set => SetValue(ref _settings.ShowPlayerNamesOnHud, value); }

        [Display(Description = "Enables thruster damage.", Name = "Enable Thruster Damage", GroupName = "Others")]
        public bool ThrusterDamage { get => _settings.ThrusterDamage; set => SetValue(ref _settings.ThrusterDamage, value); }

        [Display(Description = "Enables spawning of cargo ships.", Name = "Enable Cargo Ships", GroupName = "NPCs")]
        public bool CargoShipsEnabled { get => _settings.CargoShipsEnabled; set => SetValue(ref _settings.CargoShipsEnabled, value); }

        [Display(Description = "Enables spectator camera.", Name = "Enable Spectator Camera", GroupName = "Others")]
        public bool EnableSpectator { get => _settings.EnableSpectator; set => SetValue(ref _settings.EnableSpectator, value); }

        /// <summary>
        ///     Size of the edge of the world area cube.
        ///     Don't use directly, as it is error-prone (it's km instead of m and edge size instead of half-extent)
        ///     Rather use MyEntities.WorldHalfExtent()
        /// </summary>
        [Display(Description = "Defines the size of the world.", Name = "World Size [km]", GroupName = "Environment")]
        public int WorldSizeKm { get => _settings.WorldSizeKm; set => SetValue(ref _settings.WorldSizeKm, value); }

        [Display(Description = "When enabled respawn ship is removed after player logout.", Name = "Remove Respawn Ships on Logoff", GroupName = "Others")]
        public bool RespawnShipDelete { get => _settings.RespawnShipDelete; set => SetValue(ref _settings.RespawnShipDelete, value); }

        [Display(Description = "", Name = "Reset Ownership", GroupName = "Players")]
        public bool ResetOwnership { get => _settings.ResetOwnership; set => SetValue(ref _settings.ResetOwnership, value); }

        [Display(Description = "The multiplier for welder speed.", Name = "Welder Speed", GroupName = "Multipliers")]
        public float WelderSpeedMultiplier { get => _settings.WelderSpeedMultiplier; set => SetValue(ref _settings.WelderSpeedMultiplier, value); }

        [Display(Description = "The multiplier for grinder speed.", Name = "Grinder Speed", GroupName = "Multipliers")]
        public float GrinderSpeedMultiplier { get => _settings.GrinderSpeedMultiplier; set => SetValue(ref _settings.GrinderSpeedMultiplier, value); }

        [Display(Description = "Enables realistic sounds.", Name = "Enable Realistic Sound", GroupName = "Environment")]
        public bool RealisticSound { get => _settings.RealisticSound; set => SetValue(ref _settings.RealisticSound, value); }

        [Display(Description = "The multiplier for hacking speed.", Name = "Hacking Speed", GroupName = "Multipliers")]
        public float HackSpeedMultiplier { get => _settings.HackSpeedMultiplier; set => SetValue(ref _settings.HackSpeedMultiplier, value); }

        [Display(Description = "Enables permanent death.", Name = "Permanent Death", GroupName = "Players")]
        public bool? PermanentDeath { get => _settings.PermanentDeath; set => SetValue(ref _settings.PermanentDeath, value); }

        [Display(Description = "Defines autosave interval.", Name = "Autosave Interval [mins]", GroupName = "Others")]
        public uint AutoSaveInMinutes { get => _settings.AutoSaveInMinutes; set => SetValue(ref _settings.AutoSaveInMinutes, value); }

        [Display(Description = "Enables saving from the menu.", Name = "Enable Saving from Menu", GroupName = "Others")]
        public bool EnableSaving { get => _settings.EnableSaving; set => SetValue(ref _settings.EnableSaving, value); }

        [Display(Description = "Enables respawn screen.", Name = "Enable Respawn Screen in the Game", GroupName = "Players")]
        public bool StartInRespawnScreen { get => _settings.StartInRespawnScreen; set => SetValue(ref _settings.StartInRespawnScreen, value); }

        [Display(Description = "Enables research.", Name = "Enable Research", GroupName = "Players")]
        public bool EnableResearch { get => _settings.EnableResearch; set => SetValue(ref _settings.EnableResearch, value); }

        [Display(Description = "Enables Good.bot hints.", Name = "Enable Good.bot hints", GroupName = "Players")]
        public bool EnableGoodBotHints { get => _settings.EnableGoodBotHints; set => SetValue(ref _settings.EnableGoodBotHints, value); }

        [Display(Description = "Enables infinite ammunition in survival game mode.", Name = "Enable Infinite Ammunition in Survival", GroupName = "Others")]
        public bool InfiniteAmmo { get => _settings.InfiniteAmmo; set => SetValue(ref _settings.InfiniteAmmo, value); }

        [Display(Description = "Enables drop containers (unknown signals).", Name = "Enable Drop Containers", GroupName = "Others")]
        public bool EnableContainerDrops { get => _settings.EnableContainerDrops; set => SetValue(ref _settings.EnableContainerDrops, value); }

        [Display(Description = "The multiplier for respawn ship timer.", Name = "Respawn Ship Time Multiplier", GroupName = "Players")]
        public float SpawnShipTimeMultiplier { get => _settings.SpawnShipTimeMultiplier; set => SetValue(ref _settings.SpawnShipTimeMultiplier, value); }

        [Display(Description = "Defines density of the procedurally generated content.", Name = "Procedural Density", GroupName = "Environment")]
        public float ProceduralDensity { get => _settings.ProceduralDensity; set => SetValue(ref _settings.ProceduralDensity, value); }

        [Display(Description = "Defines unique starting seed for the procedurally generated content.", Name = "Procedural Seed", GroupName = "Environment")]
        public int ProceduralSeed { get => _settings.ProceduralSeed; set => SetValue(ref _settings.ProceduralSeed, value); }

        [Display(Description = "Enables destruction feature for the blocks.", Name = "Enable Destructible Blocks", GroupName = "Environment")]
        public bool DestructibleBlocks { get => _settings.DestructibleBlocks; set => SetValue(ref _settings.DestructibleBlocks, value); }

        [Display(Description = "Enables in game scripts.", Name = "Enable Ingame Scripts", GroupName = "Others")]
        public bool EnableIngameScripts { get => _settings.EnableIngameScripts; set => SetValue(ref _settings.EnableIngameScripts, value); }

        [Display(Description = "", Name = "Flora Density Multiplier", GroupName = "Environment")]
        public float FloraDensityMultiplier { get => _settings.FloraDensityMultiplier; set => SetValue(ref _settings.FloraDensityMultiplier, value); }

        [Display(Description = "Enables tool shake feature.", Name = "Enable Tool Shake", GroupName = "Players")]
        [DefaultValue(false)]
        public bool EnableToolShake { get => _settings.EnableToolShake; set => SetValue(ref _settings.EnableToolShake, value); }

        [Display(Description = "", Name = "Voxel Generator Version", GroupName = "Environment")]
        public int VoxelGeneratorVersion { get => _settings.VoxelGeneratorVersion; set => SetValue(ref _settings.VoxelGeneratorVersion, value); }

        [Display(Description = "Enables oxygen in the world.", Name = "Enable Oxygen", GroupName = "Environment")]
        public bool EnableOxygen { get => _settings.EnableOxygen; set => SetValue(ref _settings.EnableOxygen, value); }

        [Display(Description = "Enables airtightness in the world.", Name = "Enable Airtightness", GroupName = "Environment")]
        public bool EnableOxygenPressurization { get => _settings.EnableOxygenPressurization; set => SetValue(ref _settings.EnableOxygenPressurization, value); }

        [Display(Description = "Enables 3rd person camera.", Name = "Enable 3rd Person Camera", GroupName = "Players")]
        public bool Enable3rdPersonView { get => _settings.Enable3rdPersonView; set => SetValue(ref _settings.Enable3rdPersonView, value); }

        [Display(Description = "Enables random encounters in the world.", Name = "Enable Encounters", GroupName = "NPCs")]
        public bool EnableEncounters { get => _settings.EnableEncounters; set => SetValue(ref _settings.EnableEncounters, value); }

        [Display(Description = "Enables possibility of converting grid to station.", Name = "Enable Convert to Station", GroupName = "Others")]
        public bool EnableConvertToStation { get => _settings.EnableConvertToStation; set => SetValue(ref _settings.EnableConvertToStation, value); }

        [Display(Description = "Enables possibility of station grid inside voxel.", Name = "Enable Station Grid with Voxel", GroupName = "Environment")]
        public bool StationVoxelSupport { get => _settings.StationVoxelSupport; set => SetValue(ref _settings.StationVoxelSupport, value); }

        [Display(Description = "Enables sun rotation.", Name = "Enable Sun Rotation", GroupName = "Environment")]
        public bool EnableSunRotation { get => _settings.EnableSunRotation; set => SetValue(ref _settings.EnableSunRotation, value); }

        [Display(Description = "Enables respawn ships.", Name = "Enable Respawn Ships", GroupName = "Others")]
        public bool EnableRespawnShips { get => _settings.EnableRespawnShips; set => SetValue(ref _settings.EnableRespawnShips, value); }

        [Display(Description = "", Name = "Physics Iterations", GroupName = "Environment")]
        public int PhysicsIterations { get => _settings.PhysicsIterations; set => SetValue(ref _settings.PhysicsIterations, value); }

        [Display(Description = "Defines interval of one rotation of the sun.", Name = "Sun Rotation Interval", GroupName = "Environment")]
        public float SunRotationIntervalMinutes { get => _settings.SunRotationIntervalMinutes; set => SetValue(ref _settings.SunRotationIntervalMinutes, value); }

        [Display(Description = "Enables jetpack.", Name = "Enable Jetpack", GroupName = "Players")]
        public bool EnableJetpack { get => _settings.EnableJetpack; set => SetValue(ref _settings.EnableJetpack, value); }

        [Display(Description = "Enables spawning with tools in the inventory.", Name = "Spawn with Tools", GroupName = "Players")]
        public bool SpawnWithTools { get => _settings.SpawnWithTools; set => SetValue(ref _settings.SpawnWithTools, value); }

        [Display(Description = "Enables voxel destructions.", Name = "Enable Voxel Destruction", GroupName = "Environment")]
        public bool EnableVoxelDestruction { get => _settings.EnableVoxelDestruction; set => SetValue(ref _settings.EnableVoxelDestruction, value); }

        [Display(Description = "Enables spawning of drones in the world.", Name = "Enable Drones", GroupName = "NPCs")]
        public bool EnableDrones { get => _settings.EnableDrones; set => SetValue(ref _settings.EnableDrones, value); }

        [Display(Description = "Enables spawning of wolves in the world.", Name = "Enable Wolves", GroupName = "NPCs")]
        public bool EnableWolfs { get => _settings.EnableWolfs; set => SetValue(ref _settings.EnableWolfs, value); }

        [Display(Description = "Enables spawning of spiders in the world.", Name = "Enable Spiders", GroupName = "NPCs")]
        public bool EnableSpiders { get => _settings.EnableSpiders; set => SetValue(ref _settings.EnableSpiders, value); }

        [Display(Name = "Block Type World Limits", GroupName = "Block Limits")]
        public Dictionary<string, short> BlockTypeLimits { get => _settings.BlockTypeLimits.Dictionary; set => SetValue(x => _settings.BlockTypeLimits.Dictionary = x, value); }

        [Display(Description = "Enables scripter role for administration.", Name = "Enable Scripter Role", GroupName = "Others")]
        public bool EnableScripterRole { get => _settings.EnableScripterRole; set => SetValue(ref _settings.EnableScripterRole, value); }

        [Display(Description = "Defines minimum respawn time for drop containers.", Name = "Min Drop Container Respawn Time", GroupName = "Others")]
        public int MinDropContainerRespawnTime { get => _settings.MinDropContainerRespawnTime; set => SetValue(ref _settings.MinDropContainerRespawnTime, value); }

        [Display(Description = "Defines maximum respawn time for drop containers.", Name = "Max Drop Container Respawn Time", GroupName = "Others")]
        public int MaxDropContainerRespawnTime { get => _settings.MaxDropContainerRespawnTime; set => SetValue(ref _settings.MaxDropContainerRespawnTime, value); }

        [Display(Description = "Enables friendly fire for turrets.", Name = "Enable Turrets Friendly Fire", GroupName = "Environment")]
        public bool EnableTurretsFriendlyFire { get => _settings.EnableTurretsFriendlyFire; set => SetValue(ref _settings.EnableTurretsFriendlyFire, value); }

        [Display(Description = "Enables sub-grid damage.", Name = "Enable Sub-Grid Damage", GroupName = "Environment")]
        public bool EnableSubgridDamage { get => _settings.EnableSubgridDamage; set => SetValue(ref _settings.EnableSubgridDamage, value); }

        [Display(Description = "Defines synchronization distance in multiplayer. High distance can slow down server drastically. Use with caution.", Name = "Sync Distance", GroupName = "Environment")]
        public int SyncDistance { get => _settings.SyncDistance; set => SetValue(ref _settings.SyncDistance, value); }

        [Display(Description = "Defines render distance for clients in multiplayer. High distance can slow down client FPS. Values larger than SyncDistance may not work as expected.", Name = "View Distance", GroupName = "Environment")]
        public int ViewDistance { get => _settings.ViewDistance; set => SetValue(ref _settings.ViewDistance, value); }

        [Display(Description = "Enables experimental mode.", Name = "Experimental Mode", GroupName = "Others")]
        public bool ExperimentalMode { get => _settings.ExperimentalMode; set => SetValue(ref _settings.ExperimentalMode, value); }

        [Display(Description = "Enables adaptive simulation quality system. This system is useful if you have a lot of voxel deformations in the world and low simulation speed.", Name = "Adaptive Simulation Quality", GroupName = "Others")]
        public bool AdaptiveSimulationQuality { get => _settings.AdaptiveSimulationQuality; set => SetValue(ref _settings.AdaptiveSimulationQuality, value); }

        [Display(Description = "Enables voxel hand.", Name = "Enable voxel hand", GroupName = "Others")]
        public bool EnableVoxelHand { get => _settings.EnableVoxelHand; set => SetValue(ref _settings.EnableVoxelHand, value); }

        [Display(Description = "Enables trash removal system.", Name = "Trash Removal Enabled", GroupName = "Trash Removal")]
        public bool TrashRemovalEnabled { get => _settings.TrashRemovalEnabled; set => SetValue(ref _settings.TrashRemovalEnabled, value); }

        [Display(Description = "Defines flags for trash removal system.", Name = "Trash Removal Flags", GroupName = "Trash Removal")]
        public MyTrashRemovalFlags TrashFlagsValue { get => (MyTrashRemovalFlags)_settings.TrashFlagsValue; set => SetValue(ref _settings.TrashFlagsValue, (int)value); }

        [Display(Description = "Defines block count threshold for trash removal system.", Name = "Block Count Threshold", GroupName = "Trash Removal")]
        public int BlockCountThreshold { get => _settings.BlockCountThreshold; set => SetValue(ref _settings.BlockCountThreshold, value); }

        [Display(Description = "Defines player distance threshold for trash removal system.", Name = "Player Distance Threshold [m]", GroupName = "Trash Removal")]
        public float PlayerDistanceThreshold { get => _settings.PlayerDistanceThreshold; set => SetValue(ref _settings.PlayerDistanceThreshold, value); }

        [Display(Description = "By setting this, server will keep number of grids around this value. \n !WARNING! It ignores Powered and Fixed flags, Block Count and lowers Distance from player.\n Set to 0 to disable.", Name = "Optimal Grid Count", GroupName = "Trash Removal")]
        public int OptimalGridCount { get => _settings.OptimalGridCount; set => SetValue(ref _settings.OptimalGridCount, value); }

        [Display(Description = "Defines player inactivity threshold for trash removal system. \n !WARNING! This will remove all grids of the player.\n Set to 0 to disable.", Name = "Player Inactivity Threshold [hours]", GroupName = "Trash Removal")]
        public float PlayerInactivityThreshold { get => _settings.PlayerInactivityThreshold; set => SetValue(ref _settings.PlayerInactivityThreshold, value); }

        [Display(Description = "Defines character removal threshold for trash removal system. If player disconnects it will remove his character after this time.\n Set to 0 to disable.", Name = "Character Removal Threshold [mins]", GroupName = "Trash Removal")]
        public int PlayerCharacterRemovalThreshold { get => _settings.PlayerCharacterRemovalThreshold; set => SetValue(ref _settings.PlayerCharacterRemovalThreshold, value); }

        [Display(Description = "Sets optimal distance in meters when spawning new players near others.", Name = "Optimal Spawn Distance", GroupName = "Players")]
        public float OptimalSpawnDistance { get => _settings.OptimalSpawnDistance; set => SetValue(ref _settings.OptimalSpawnDistance, value); }

        [Display(Description = "Enables automatic respawn at nearest available respawn point.", Name = "Enable Auto Respawn", GroupName = "Players")]
        public bool EnableAutoRespawn { get => _settings.EnableAutorespawn; set => SetValue(ref _settings.EnableAutorespawn, value); }

        [Display(Description = "The number of NPC factions generated on the start of the world.", Name = "NPC Factions Count", GroupName = "NPCs")]
        public int TradeFactionsCount { get => _settings.TradeFactionsCount; set => SetValue(ref _settings.TradeFactionsCount, value); }

        [Display(Description = "The inner radius [m] (center is in 0,0,0), where stations can spawn. Does not affect planet-bound stations (surface Outposts and Orbital stations).", Name = "Stations Inner Radius", GroupName = "NPCs")]
        public double StationsDistanceInnerRadius { get => _settings.StationsDistanceInnerRadius; set => SetValue(ref _settings.StationsDistanceInnerRadius, value); }

        [Display(Description = "The outer radius [m] (center is in 0,0,0), where stations can spawn. Does not affect planet-bound stations (surface Outposts and Orbital stations).", Name = "Stations Outer Radius Start", GroupName = "NPCs")]
        public double StationsDistanceOuterRadiusStart { get => _settings.StationsDistanceOuterRadiusStart; set => SetValue(ref _settings.StationsDistanceOuterRadiusStart, value); }

        [Display(Description = "The outer radius [m] (center is in 0,0,0), where stations can spawn. Does not affect planet-bound stations (surface Outposts and Orbital stations).", Name = "Stations Outer Radius End", GroupName = "NPCs")]
        public double StationsDistanceOuterRadiusEnd { get => _settings.StationsDistanceOuterRadiusEnd; set => SetValue(ref _settings.StationsDistanceOuterRadiusEnd, value); }

        [Display(Description = "Time period between two economy updates in seconds.", Name = "Economy tick time", GroupName = "NPCs")]
        public int EconomyTickInSeconds { get => _settings.EconomyTickInSeconds; set => SetValue(ref _settings.EconomyTickInSeconds, value); }

        [Display(Description = "If enabled bounty contracts will be available on stations.", Name = "Enable Bounty Contracts", GroupName = "Players")]
        public bool EnableBountyContracts { get => _settings.EnableBountyContracts; set => SetValue(ref _settings.EnableBountyContracts, value); }

        [Display(Description = "Resource deposits count coefficient for generated world content (voxel generator version > 2).", Name = "Deposits Count Coefficient", GroupName = "Environment")]
        public float DepositsCountCoefficient { get => _settings.DepositsCountCoefficient; set => SetValue(ref _settings.DepositsCountCoefficient, value); }

        [Display(Description = "Resource deposit size denominator for generated world content (voxel generator version > 2).", Name = "Deposit Size Denominator", GroupName = "Environment")]
        public float DepositSideDenominator { get => _settings.DepositSizeDenominator; set => SetValue(ref _settings.DepositSizeDenominator, value); }

        [Display(Description = "Enables economy features.", Name = "Enable Economy", GroupName = "NPCs")]
        public bool EnableEconomy { get => _settings.EnableEconomy; set => SetValue(ref _settings.EnableEconomy, value); }

        [Display(Description = "Enables system for voxel reverting.", Name = "Enable Voxel Reverting", GroupName = "Trash Removal")]
        public bool VoxelTrashRemovalEnabled { get => _settings.VoxelTrashRemovalEnabled; set => SetValue(ref _settings.VoxelTrashRemovalEnabled, value); }

        [Display(Description = "Allows super gridding exploit to be used.", Name = "Enable Supergridding", GroupName = "Others")]
        public bool EnableSupergridding { get => _settings.EnableSupergridding; set => SetValue(ref _settings.EnableSupergridding, value); }

        public static implicit operator MyObjectBuilder_SessionSettings(SessionSettingsViewModel viewModel)
        {
            return viewModel._settings;
        }
    }
}