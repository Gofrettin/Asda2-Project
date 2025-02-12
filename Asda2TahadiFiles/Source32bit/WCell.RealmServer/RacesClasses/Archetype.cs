﻿using NLog;
using System;
using System.Collections.Generic;
using WCell.Constants;
using WCell.Constants.Misc;
using WCell.Constants.World;
using WCell.Core;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Items;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Spells;
using WCell.Util.Data;
using WCell.Util.Graphics;

namespace WCell.RealmServer.RacesClasses
{
  /// <summary>
  /// An Archetype is the combination of a Race and a Class to define a distinct persona, eg. Orc Warrior, Human Paladin etc
  /// </summary>
  [DataHolder]
  public class Archetype : IDataHolder
  {
    [NotPersistent]
    public readonly LevelStatInfo[] LevelStats = new LevelStatInfo[RealmServerConfiguration.MaxCharacterLevel];

    /// <summary>All initial spells of this Archetype</summary>
    [NotPersistent]public readonly List<Spell> Spells = new List<Spell>();

    /// <summary>All initial items for males of this Archetype</summary>
    [NotPersistent]public readonly List<ItemStack> MaleItems = new List<ItemStack>();

    /// <summary>All initial items for females of this Archetype</summary>
    [NotPersistent]public readonly List<ItemStack> FemaleItems = new List<ItemStack>();

    [NotPersistent]public byte[] ActionButtons = ActionButton.CreateEmptyActionButtonBar();
    public ClassId ClassId;
    public RaceId RaceId;

    /// <summary>The starting position for the given race.</summary>
    public Vector3 StartPosition;

    public float StartOrientation;

    /// <summary>The starting map for the given race.</summary>
    public MapId StartMapId;

    /// <summary>The starting zone for the given race.</summary>
    public ZoneId StartZoneId;

    [NotPersistent]public IWorldZoneLocation StartLocation;
    [NotPersistent]public BaseClass Class;
    [NotPersistent]public BaseRace Race;
    [NotPersistent]public ChatLanguage[] SpokenLanguages;

    public LevelStatInfo FirstLevelStats { get; internal set; }

    public void FinalizeDataHolder()
    {
      Race = ArchetypeMgr.GetRace(RaceId);
      Class = ArchetypeMgr.GetClass(ClassId);
      if(Class != null)
      {
        BaseRace race = Race;
      }

      Archetype[] archetypeArray = ArchetypeMgr.Archetypes[(uint) ClassId];
      if(archetypeArray == null)
        ArchetypeMgr.Archetypes[(uint) ClassId] =
          archetypeArray = new Archetype[WCellConstants.RaceTypeLength];
      StartLocation =
        new WorldZoneLocation(StartMapId, StartPosition, StartZoneId);
      if(StartLocation.Map == null)
        LogManager.GetCurrentClassLogger().Warn("Failed to initialize Archetype \"" + this +
                                                "\" - StartMap does not exist: " + StartMapId);
      else
        archetypeArray[(uint) RaceId] = this;
    }

    public LevelStatInfo GetLevelStats(uint level)
    {
      if(level >= LevelStats.Length)
        level = (uint) (LevelStats.Length - 1);
      return LevelStats[level - 1U];
    }

    public List<ItemStack> GetInitialItems(GenderType gender)
    {
      if(gender == GenderType.Female)
        return FemaleItems;
      return MaleItems;
    }

    /// <summary>Gets the BaseStrength at a specific level.</summary>
    /// <param name="level">the level to get the BaseStrength for</param>
    /// <returns>BaseStrength amount</returns>
    public int GetStrength(int level)
    {
      LevelStatInfo levelStat = LevelStats[level - 1];
      if(levelStat == null)
        return 0;
      return levelStat.Strength;
    }

    /// <summary>Gets the BaseAgility at a specific level.</summary>
    /// <param name="level">the level to get the BaseAgility for</param>
    /// <returns>BaseAgility amount</returns>
    public int GetAgility(int level)
    {
      LevelStatInfo levelStat = LevelStats[level - 1];
      if(levelStat == null)
        return 0;
      return levelStat.Agility;
    }

    /// <summary>Gets the BaseStamina at a specific level.</summary>
    /// <param name="level">the level to get the BaseStamina for</param>
    /// <returns>BaseStamina amount</returns>
    public int GetStamina(int level)
    {
      LevelStatInfo levelStat = LevelStats[level - 1];
      if(levelStat == null)
        return 0;
      return levelStat.Stamina;
    }

    /// <summary>Gets the BaseIntellect at a specific level.</summary>
    /// <param name="level">the level to get the BaseIntellect for</param>
    /// <returns>the BaseIntellect amount</returns>
    public int GetIntellect(int level)
    {
      LevelStatInfo levelStat = LevelStats[level - 1];
      if(levelStat == null)
        return 0;
      return levelStat.Intellect;
    }

    /// <summary>Gets the BaseSpirit at a specific level.</summary>
    /// <param name="level">the level to get the BaseSpirit for</param>
    /// <returns>the BaseSpirit amount</returns>
    public int GetSpirit(int level)
    {
      LevelStatInfo levelStat = LevelStats[level - 1];
      if(levelStat == null)
        return 0;
      return levelStat.Spirit;
    }

    public override string ToString()
    {
      return Race + " " + Class;
    }
  }
}