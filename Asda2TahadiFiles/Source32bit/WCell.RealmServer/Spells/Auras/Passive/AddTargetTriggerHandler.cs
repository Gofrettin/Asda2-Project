﻿namespace WCell.RealmServer.Spells.Auras.Handlers
{
  /// <summary>
  /// test
  /// Gives EffectValue% to trigger another Spell
  /// </summary>
  public class AddTargetTriggerHandler : AuraEffectHandler
  {
    protected override void Apply()
    {
      Owner.Spells.TargetTriggers.Add(this);
    }

    protected override void Remove(bool cancelled)
    {
      Owner.Spells.TargetTriggers.Remove(this);
    }
  }
}