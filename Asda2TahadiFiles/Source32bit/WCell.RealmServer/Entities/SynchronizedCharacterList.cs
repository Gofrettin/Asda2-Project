﻿using System;
using System.Collections.Generic;
using WCell.Constants.Factions;
using WCell.Core.Network;
using WCell.RealmServer.Network;
using WCell.Util.Collections;

namespace WCell.RealmServer.Entities
{
  public class SynchronizedCharacterList : SynchronizedList<Character>, ICharacterSet, IPacketReceiver
  {
    public SynchronizedCharacterList(FactionGroup group, ICollection<Character> chrs)
      : base(chrs)
    {
      FactionGroup = group;
    }

    public SynchronizedCharacterList(FactionGroup group)
      : base(5)
    {
      FactionGroup = group;
    }

    public SynchronizedCharacterList(int capacity, FactionGroup group)
      : base(capacity)
    {
      FactionGroup = group;
    }

    public FactionGroup FactionGroup { get; protected set; }

    public int CharacterCount
    {
      get { return Count; }
    }

    /// <summary>Threadsafe iteration</summary>
    /// <param name="callback"></param>
    public void ForeachCharacter(Action<Character> callback)
    {
      EnterLock();
      try
      {
        for(int index = Count - 1; index >= 0; --index)
        {
          Character character = this[index];
          callback(character);
          if(!character.IsInWorld)
            RemoveUnlocked(index);
        }
      }
      finally
      {
        ExitLock();
      }
    }

    /// <summary>Creates a Copy of the set</summary>
    public Character[] GetAllCharacters()
    {
      return ToArray();
    }

    public bool IsRussianClient { get; set; }

    public Locale Locale { get; set; }

    public void Send(RealmPacketOut packet, bool addEnd = false)
    {
      byte[] finalizedPacket = packet.GetFinalizedPacket();
      ForeachCharacter(chr => chr.Send(finalizedPacket));
    }
  }
}