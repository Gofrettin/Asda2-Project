﻿using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Spells.Auras.Misc
{
  /// <summary>
  /// Heals the wearer for EffectValue % when dealing damage
  /// </summary>
  public class LifeLeechPercentAuraHandler : AttackEventEffectHandler
  {
    public override void OnBeforeAttack(DamageAction action)
    {
    }

    public override void OnAttack(DamageAction action)
    {
      Owner.Heal(action.GetDamagePercent(EffectValue), m_aura.CasterUnit, m_spellEffect);
    }

    public override void OnDefend(DamageAction action)
    {
    }
  }
}