﻿using System;
using System.Collections.Generic;
using WCell.Constants.Misc;
using WCell.Constants.Spells;
using WCell.Core.Paths;
using WCell.RealmServer.Content;
using WCell.RealmServer.NPCs.Spawns;
using WCell.Util.Data;
using WCell.Util.Graphics;

namespace WCell.RealmServer.NPCs
{
  [DataHolder]
  [Serializable]
  public class WaypointEntry : IDataHolder, IPathVertex, IHasPosition
  {
    public static readonly LinkedList<WaypointEntry> EmptyList = new LinkedList<WaypointEntry>();
    public uint Flags;
    public EmoteType Emote;
    public SpellId SpellId;
    public uint ArriveDisplayId;
    public uint LeaveDisplayId;
    [NotPersistent]public NPCSpawnEntry SpawnEntry;
    [NotPersistent]public LinkedListNode<WaypointEntry> Node;

    public uint SpawnId { get; set; }

    /// <summary>Id of this waypoint in the chain</summary>
    public uint Id { get; set; }

    public Vector3 Position { get; set; }

    public float Orientation { get; set; }

    /// <summary>Time to wait at this point in milliseconds</summary>
    public uint WaitTime { get; set; }

    public float GetDistanceToNext()
    {
      throw new NotImplementedException("Not implemented yet.");
    }

    public void FinalizeDataHolder()
    {
      SpawnEntry = NPCMgr.GetSpawnEntry(SpawnId);
      if(SpawnEntry == null)
      {
        ContentMgr.OnInvalidDBData("{0} had an invalid SpawnId.", (object) this);
      }
      else
      {
        bool flag = false;
        for(LinkedListNode<WaypointEntry> node = SpawnEntry.Waypoints.First;
          node != null;
          node = node.Next)
        {
          if(node.Value.Id > Id)
          {
            Node = node.List.AddBefore(node, this);
            flag = true;
            break;
          }

          if((int) node.Value.Id == (int) Id)
          {
            ContentMgr.OnInvalidDBData("Found multiple Waypoints with the same Id {0} for SpawnEntry {1}",
              (object) Id, (object) SpawnEntry);
            return;
          }
        }

        if(flag)
          return;
        SpawnEntry.HasDefaultWaypoints = false;
        Node = SpawnEntry.Waypoints.AddLast(this);
      }
    }

    public override string ToString()
    {
      return string.Format("NPCWaypoint {0} {1}", SpawnId, Id);
    }

    public static IEnumerable<WaypointEntry> GetAllDataHolders()
    {
      List<WaypointEntry> waypointEntryList = new List<WaypointEntry>(NPCMgr.SpawnEntries.Length * 10);
      foreach(NPCSpawnEntry spawnEntry in NPCMgr.SpawnEntries)
      {
        if(spawnEntry != null && spawnEntry.Waypoints != null)
          waypointEntryList.AddRange(spawnEntry.Waypoints);
      }

      return waypointEntryList;
    }
  }
}