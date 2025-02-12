﻿using WCell.Constants.Misc;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Spells.Auras;

namespace WCell.RealmServer.Auras.Effects
{
  /// <summary>Reduces victim armor by the given value in %</summary>
  public class ModArmorPenetrationHandler : AuraEffectHandler
  {
    protected override void Apply()
    {
      Character owner = Owner as Character;
      if(owner == null)
        return;
      owner.ModCombatRating(CombatRating.ArmorPenetration, EffectValue);
    }

    protected override void Remove(bool cancelled)
    {
      Character owner = Owner as Character;
      if(owner == null)
        return;
      owner.ModCombatRating(CombatRating.ArmorPenetration, -EffectValue);
    }
  }
}