namespace DataExtractor.Tool.Services;

using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins.Cache;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Auth.OAuth2;
using DataExtractor.Tool.Dto;
using Mutagen.Bethesda.Plugins.Cache.Internals.Implementations;
using Noggog;

public class DataExtractorConfig
{
    public static readonly HashSet<int> AllowedLabelIds = new() {67, 90, 295, 419, 421, 447, 1173 };

    public static readonly Dictionary<string, string> StatusTranslationMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["1st Pass Done"] = "First passed",
        ["1st Passed"] = "First passed",
        ["2nd Pass Done"] = "Needs finalization",
        ["2nd Passed"] = "Needs finalization",
        ["Needs: Polishing"] = "Needs finalization",
        ["In Progress"] = "Work in Progress",
        ["Claimed"] = "Assigned",
        ["Needs: Rework"] = "Redo",
        ["Needs: Extra Work"] = "Needs more work",
        ["Needs: Fixing"] = "Needs more work",
        ["Not Open"] = "Not started"
    };

    public static readonly Dictionary<string, string> LocationCategoryMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["LocTypeDwelling"] = "Interior",
        ["LocTypeDungeon"] = "Dungeon",
        ["LocTypeSettlement"] = "Settlement",
    };

    public static readonly Dictionary<string, string> LocationTypeMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["LocTypeAnimalDen"] = "Animal Den",
        ["LocTypeHouse"] = "House",
        ["LocTypeCastle"] = "Castle",
        ["LocSetCave"] = "Cave",
        ["LocTypeMine"] = "Mine",
        ["LocTypeFarm"] = "Farm",
        ["LocSetOutdoor"] = "Exterior",
        ["LocTypeShip"] = "Ship",
        ["LocTypeMilitaryFort"] = "Imperial Fort",
        ["LocTypeBanditCamp"] = "Bandit Camp",
        ["BSKLocTypeAyleid"] = "Ayleid Ruin",
        ["BSKLocSetTombColovian"] = "Colovian Tomb",
        ["BSKLocSetTombNibenese"] = "Nibenese Tomb",
        ["BSKLocSetFortRuin"] = "Fort Ruin",
        ["BSKLocTypeMinotaurCamp"] = "Minotaur Camp",
        ["BSKLocTypeOgreDen"] = "Ogre Den",
        ["BSKLocTypeUndead"] = "Undead",
        ["BSKLocTypeGoblinDen"] = "Goblin Den",
        ["CYRLocSetAkaviriRuin"] = "Akaviri Ruin",
    };
}