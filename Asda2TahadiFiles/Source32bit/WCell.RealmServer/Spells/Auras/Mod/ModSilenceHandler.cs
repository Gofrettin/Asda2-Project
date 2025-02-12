﻿using WCell.Constants.Spells;

namespace WCell.RealmServer.Spells.Auras.Mod
{
  /// <summary>
  /// Prevents carrier from attacking or using "physical abilities"
  /// </summary>
  public class ModSilenceHandler : AuraEffectHandler
  {
    protected override void Apply()
    {
      Owner.IncMechanicCount(SpellMechanic.Silenced, false);
    }

    protected override void Remove(bool cancelled)
    {
      Owner.DecMechanicCount(SpellMechanic.Silenced, false);
    }
  }
}