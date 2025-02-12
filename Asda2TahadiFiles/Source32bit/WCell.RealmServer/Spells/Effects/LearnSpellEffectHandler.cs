﻿using NLog;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Spells.Effects
{
  public class LearnSpellEffectHandler : SpellEffectHandler
  {
    private static Logger log = LogManager.GetCurrentClassLogger();
    private Spell toLearn;

    public LearnSpellEffectHandler(SpellCast cast, SpellEffect effect)
      : base(cast, effect)
    {
    }

    public override SpellFailedReason Initialize()
    {
      if((toLearn = Effect.TriggerSpell) == null)
      {
        if(m_cast.CasterItem != null && m_cast.CasterItem.Template.TeachSpell != null)
        {
          toLearn = m_cast.CasterItem.Template.TeachSpell.Spell;
        }
        else
        {
          log.Warn("Learn-Spell {0} has invalid Spell to be taught: {1}",
            Effect.Spell, Effect.TriggerSpellId);
          return SpellFailedReason.Error;
        }
      }

      return SpellFailedReason.Ok;
    }

    public override SpellFailedReason InitializeTarget(WorldObject target)
    {
      return ((Unit) target).Spells.Contains(toLearn.Id)
        ? SpellFailedReason.SpellLearned
        : SpellFailedReason.Ok;
    }

    protected override void Apply(WorldObject target, ref DamageAction[] actions)
    {
      ((Unit) target).Spells.AddSpell(toLearn);
    }

    public override ObjectTypes TargetType
    {
      get { return ObjectTypes.Player; }
    }
  }
}