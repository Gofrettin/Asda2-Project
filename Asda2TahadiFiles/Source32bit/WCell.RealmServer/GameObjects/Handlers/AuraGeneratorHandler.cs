﻿using NLog;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.GameObjects.Handlers
{
  /// <summary>GO Type 30</summary>
  public class AuraGeneratorHandler : GameObjectHandler
  {
    private static readonly Logger log = LogManager.GetCurrentClassLogger();

    public override bool Use(Character user)
    {
      GOEntry entry = m_go.Entry;
      return true;
    }
  }
}