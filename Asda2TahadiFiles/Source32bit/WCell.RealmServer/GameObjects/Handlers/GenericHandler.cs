﻿using NLog;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.GameObjects.Handlers
{
  /// <summary>GO Type 5</summary>
  public class GenericHandler : GameObjectHandler
  {
    private static readonly Logger log = LogManager.GetCurrentClassLogger();

    public override bool Use(Character user)
    {
      GOEntry entry = m_go.Entry;
      return true;
    }
  }
}