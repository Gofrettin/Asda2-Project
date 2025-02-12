﻿using System;
using WCell.Core;
using WCell.RealmServer.Entities;
using WCell.RealmServer.NPCs;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Spells
{
  public class SpellSummonPossessedHandler : SpellSummonHandler
  {
    public override NPC Summon(SpellCast cast, ref Vector3 targetLoc, NPCEntry entry)
    {
      NPC npc = base.Summon(cast, ref targetLoc, entry);
      if(cast.CasterChar != null)
      {
        cast.CasterChar.Summon = EntityId.Zero;
        npc.Summoner = null;
        npc.Master = cast.CasterChar;
        npc.AddMessage(() => cast.CasterChar.Possess(0, npc, true, false));
      }

      return npc;
    }
  }
}