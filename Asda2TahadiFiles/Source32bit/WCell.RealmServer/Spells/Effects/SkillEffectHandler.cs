﻿namespace WCell.RealmServer.Spells.Effects
{
  /// <summary>Uses a Skill (add skill checks here?)</summary>
  public class SkillEffectHandler : SpellEffectHandler
  {
    public SkillEffectHandler(SpellCast cast, SpellEffect effect)
      : base(cast, effect)
    {
    }

    public override void Apply()
    {
      int miscValue = Effect.MiscValue;
      int basePoints = Effect.BasePoints;
    }

    public override bool HasOwnTargets
    {
      get { return false; }
    }
  }
}