﻿namespace WCell.RealmServer.Spells.Auras.Mod
{
  public class ModDamageTakenHandler : AuraEffectHandler
  {
    protected override void Apply()
    {
      Owner.ModDamageTakenMod(m_spellEffect.MiscBitSet, EffectValue);
    }

    protected override void Remove(bool cancelled)
    {
      Owner.ModDamageTakenMod(m_spellEffect.MiscBitSet, -EffectValue);
    }
  }
}