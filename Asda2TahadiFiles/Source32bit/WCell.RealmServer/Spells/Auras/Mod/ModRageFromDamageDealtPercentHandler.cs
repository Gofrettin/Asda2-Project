﻿using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Mod
{
  /// <summary>TODO: Only used for WarriorArmsEndlessRage</summary>
  public class ModRageFromDamageDealtPercentHandler : AuraEffectHandler
  {
    protected override void Apply()
    {
      Character owner = Owner as Character;
    }

    protected override void Remove(bool cancelled)
    {
      Character owner = Owner as Character;
    }
  }
}