﻿using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Effects
{
  public class SetNumberOfTalentGroupsHandler : SpellEffectHandler
  {
    public SetNumberOfTalentGroupsHandler(SpellCast cast, SpellEffect effect)
      : base(cast, effect)
    {
    }

    public override void Apply()
    {
      int num = Effect.BasePoints + 1;
      Character casterObject = Cast.CasterObject as Character;
      if(casterObject == null)
        return;
      casterObject.Talents.SpecProfileCount = num;
    }
  }
}