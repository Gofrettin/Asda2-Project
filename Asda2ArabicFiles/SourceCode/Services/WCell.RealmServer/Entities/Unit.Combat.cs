using System;
using WCell.Constants;
using WCell.Constants.NPCs;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.Core.Timers;
using WCell.RealmServer.Formulas;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Items;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Modifiers;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Effects;
using WCell.Util;
using WCell.Util.NLog;

namespace WCell.RealmServer.Entities
{
    public partial class Unit
    {
        #region Global Variables
        /// <summary>
        /// Used to determine melee distance
        /// </summary>
        public static float DefaultMeleeAttackRange = 1f;

        /// <summary>
        /// Used to determine ranged attack distance
        /// </summary>
        public static float DefaultRangedAttackRange = 20f;

        /// <summary>
        /// Time in milliseconds until a Player can leave combat
        /// </summary>
        public static int PvPDeactivationDelay = 6000;
        #endregion

        public float SplinterEffect;
        public int SplinterEffectChange;
        /// <summary>
        /// Times our melee and ranged attacks
        /// </summary>
        protected TimerEntry m_attackTimer;
        /// <summary>
        /// whether this Unit is currently in Combat-mode (effects regeneration etc).
        /// </summary>
        protected bool m_isInCombat;

        /// <summary>
        /// whether this Unit is currently actively fighting.
        /// </summary>
        protected bool m_isFighting;

        protected DateTime m_lastCombatTime;

        protected DamageAction m_DamageAction;

        protected int m_extraAttacks;

        /// <summary>
        /// The Environment.TickCount of your last strikes
        /// </summary>
        protected int m_lastStrike, m_lastOffhandStrike;

        protected Spell m_AutorepeatSpell;

        /// <summary>
        /// The last time when this Unit was still actively Fighting
        /// </summary>
        public DateTime LastCombatTime
        {
            get { return m_lastCombatTime; }
            set { m_lastCombatTime = value; }
        }

        public int MillisSinceLastCombatAction
        {
            get { return (DateTime.Now - m_lastCombatTime).ToMilliSecondsInt(); }
        }

        /// <summary>
        /// While in combat, this method will reset the current swing delay (swing timer is reset)
        /// </summary>
        public void ResetSwingDelay()
        {
            m_lastCombatTime = m_lastUpdateTime;
        }

        public void CancelPendingAbility()
        {
            if (m_spellCast != null && m_spellCast.IsPending)
            {
                m_spellCast.Cancel(SpellFailedReason.DontReport);
            }
        }

        /// <summary>
        /// The spell that is currently being triggered automatically by the CombatTimer
        /// </summary>
        public Spell AutorepeatSpell
        {
            get { return m_AutorepeatSpell; }
            set
            {
                if (m_AutorepeatSpell != value)
                {
                    m_AutorepeatSpell = value;
                    if (value != null)
                    {
                        if (value.IsRangedAbility)
                        {
                            SheathType = SheathType.Ranged;
                        }
                    }
                    else
                    {
                        SheathType = SheathType.Melee;
                    }
                }
            }
        }

        /// <summary>
        /// Whether this Unit is currently attacking with a ranged weapon
        /// </summary>
        public bool IsUsingRangedWeapon
        {
            get { return m_AutorepeatSpell != null && m_AutorepeatSpell.IsRangedAbility; }
        }

        /// <summary>
        /// Amount of extra attacks to hit on next thit
        /// </summary>
        public int ExtraAttacks
        {
            get { return m_extraAttacks; }
            set { m_extraAttacks = value; }
        }

        /// <summary>
        /// Adds damage mods to the given AttackAction
        /// </summary>
        public virtual void OnAttack(DamageAction action)
        {
            for (var i = AttackEventHandlers.Count - 1; i >= 0; i--)
            {
                var mod = AttackEventHandlers[i];
                mod.OnAttack(action);
            }
        }

        /// <summary>
        /// Adds damage mods to the given AttackAction
        /// </summary>
        public virtual void OnDefend(DamageAction action)
        {
            IsSitting = false;
            for (var i = AttackEventHandlers.Count - 1; i >= 0; i--)
            {
                var mod = AttackEventHandlers[i];
                mod.OnDefend(action);
            }
        }

        /// <summary>
        /// Adds damage mods to the given AttackAction
        /// </summary>
        public virtual int AddHealingModsToAction(int healValue, SpellEffect effect, DamageSchool school)
        {
            return healValue;
        }

        internal DamageAction GetUnusedAction()
        {
            if (m_DamageAction == null || m_DamageAction.ReferenceCount > 0)
            {
                return new DamageAction(this);
            }
            return m_DamageAction;
        }

        /// <summary>
        /// Whether this unit has an ability pending for the given weapon (Heroic Strike for melee, Poison Dart for throwing, Stun Shot for ranged weapons etc)
        /// </summary>
        public bool UsesPendingAbility(IAsda2Weapon weapon)
        {
            return m_spellCast != null && m_spellCast.IsPending && m_spellCast.GetWeapon() == weapon;
        }

        #region Standard Attack

        /// <summary>
        /// Strike using mainhand weapon
        /// </summary>
        public void Strike()
        {
            Strike(MainWeapon);
        }

        /// <summary>
        /// Strike using given weapon
        /// </summary>
        public void Strike(IAsda2Weapon weapon)
        {
            var action = GetUnusedAction();
            var target = m_target;

            Strike(weapon, action, target);
        }

        /// <summary>
        /// Strike the target using mainhand weapon
        /// </summary>
        public void Strike(Unit target)
        {
            Strike(MainWeapon, target);
        }

        /// <summary>
        /// Strike the target using given weapon
        /// </summary>
        public void Strike(IAsda2Weapon weapon, Unit target)
        {
            Strike(weapon, GetUnusedAction(), target);
        }

        public void Strike(DamageAction action, Unit target)
        {
            Strike(MainWeapon, action, target);
        }

        /// <summary>
        /// Do a single attack using the given weapon and action on the target
        /// </summary>
        public void Strike(IAsda2Weapon weapon, DamageAction action, Unit target)
        {
            IsInCombat = true;
            if (UsesPendingAbility(weapon))
            {
                m_spellCast.Perform();
            }
            else
            {
                Strike(weapon, action, target, null);
            }
        }

        /// <summary>
        /// Do a single attack on the target using given weapon and ability.
        /// </summary>
        public ProcHitFlags Strike(IAsda2Weapon weapon, Unit target, SpellCast ability)
        {
            return Strike(weapon, GetUnusedAction(), target, ability);
        }

        /// <summary>
        /// Do a single attack on the target using given weapon, ability and action.
        /// </summary>
        public ProcHitFlags Strike(IAsda2Weapon weapon, DamageAction action, Unit target, SpellCast ability)
        {
            ProcHitFlags procHitFlags = ProcHitFlags.None;

            EnsureContext();
            if (!IsAlive)
            {
                return procHitFlags;
            }

            if (!target.IsInContext || !target.IsAlive)
            {
                return procHitFlags;
            }

            if (weapon == null)
            {
                log.Info("Trying to strike without weapon: " + this);
                return procHitFlags;
            }

            //if (IsMovementControlled)
            //{
            //    // stop running when landing a hit
            //    m_Movement.Stop();
            //}

            target.IsInCombat = true;

            action.Victim = target;
            action.Attacker = this;
            action.Weapon = weapon;

            if (ability != null)
            {
                action.Schools = ability.Spell.SchoolMask;
                action.SpellEffect = ability.Spell.Effects[0];

                // calc damage
                GetWeaponDamage(action, weapon, ability);
                procHitFlags = action.DoAttack();
                if (ability.Spell.AttributesExC.HasFlag(SpellAttributesExC.RequiresTwoWeapons) && m_offhandWeapon != null)
                {
                    // also strike with offhand
                    action.Reset(this, target, m_offhandWeapon);
                    GetWeaponDamage(action, m_offhandWeapon, ability);
                    procHitFlags |= action.DoAttack();
                    m_lastOffhandStrike = Environment.TickCount;
                }
            }
            else
            {
                // no combat ability
                m_extraAttacks += 1;
                do
                {
                    // calc damage
                    GetWeaponDamage(action, weapon, null);

                    action.Schools = weapon.Damages.AllSchools();
                    if (action.Schools == DamageSchoolMask.None)
                    {
                        action.Schools = DamageSchoolMask.Physical;
                    }

                    // normal attack
                    action.DoAttack();
                } while (--m_extraAttacks > 0);
            }
            action.OnFinished();
            return procHitFlags;
        }

        /// <summary>
        /// Returns random damage for the given weapon
        /// </summary>
        public void GetWeaponDamage(DamageAction action, IAsda2Weapon weapon, SpellCast usedAbility, int targetNo = 0)
        {
            int damage;
            if (weapon == m_offhandWeapon)
            {
                damage = Utility.Random((int)MinOffHandDamage, (int)MaxOffHandDamage + 1);
            }
            else
            {
                /*if (weapon.IsRanged)
                {
                    damage = Utility.Random((int)MinRangedDamage, (int)MaxRangedDamage + 1);
                }
                else
                {*/
                damage = Utility.Random((int)MinDamage, (int)MaxDamage);
                //}
            }

            if (this is NPC)
            {
                damage = (int)(damage * NPCMgr.DefaultNPCDamageFactor + 0.999999f);
            }

            if (usedAbility != null && usedAbility.IsCasting)
            {
                // get damage modifiers from spell
                var multiplier = 0;

                foreach (var effectHandler in usedAbility.Handlers)
                {
                    if (effectHandler.Effect.IsStrikeEffectFlat)
                    {
                        damage += effectHandler.CalcDamageValue(targetNo);
                    }
                    else if (effectHandler.Effect.IsStrikeEffectPct)
                    {
                        multiplier += effectHandler.CalcDamageValue(targetNo);
                    }
                }
                if (multiplier > 0)
                {
                    action.Damage = (damage * multiplier + 50) / 100;
                }
                else
                {
                    action.Damage = damage;
                }

                foreach (var effectHandler in usedAbility.Handlers)
                {
                    if (effectHandler is WeaponDamageEffectHandler)
                    {
                        ((WeaponDamageEffectHandler)effectHandler).OnHit(action);
                    }
                }
            }
            else
            {
                action.Damage = damage;
            }
        }
        #endregion

        #region Spells
        /// <summary>
        /// Does spell-damage to this Unit
        /// </summary>
        public DamageAction DealSpellDamage(Unit attacker, SpellEffect effect, int dmg, bool addDamageBonuses = true, bool mayCrit = true, bool forceCrit = false, bool clearAction = true)
		{
			EnsureContext();
			if (!IsAlive)
			{
				return null;
			}
			if (attacker != null && !attacker.IsInContext)
			{
				attacker = null;
			}
			if (attacker is NPC)
			{
				dmg = (int)(dmg * NPCMgr.DefaultNPCDamageFactor + 0.999999f);
			}

			DamageSchool school;
			if (effect != null)
			{
				school = GetLeastResistantSchool(effect.Spell);
			}
			else
			{
				school = DamageSchool.Physical;
			}

			if (IsEvading || IsImmune(school) || IsInvulnerable || !IsAlive)
			{
				// cannot deal damage to this guy
				return null;
			}

			var action = GetUnusedAction();

			// reset values
			action.Attacker = attacker;
			action.HitFlags = 0;
			action.VictimState = 0;
			action.Weapon = null;

			if (effect != null)
			{
				// Some kind of spell is involved
				action.UsedSchool = school;
				action.Schools = effect.Spell.SchoolMask;
				action.IsDot = effect.IsPeriodic;
			}
			else
			{
				// pure white melee damage
				action.UsedSchool = DamageSchool.Physical;
				action.Schools = DamageSchoolMask.Physical;
				action.IsDot = false;
			}

			action.Damage = dmg;
		    var deffence = (Asda2MagicDefence + Asda2Defence)/2;
		    var atackerChr = action.Attacker as Character;
            if (atackerChr != null)
            {
                switch (atackerChr.Archetype.ClassId)
                {
                        case ClassId.OHS:
                        deffence = Asda2Defence;
                        break;
                        case ClassId.THS:
                        deffence = Asda2Defence;
                        break;
                        case ClassId.Spear:
                        deffence = Asda2Defence;
                        break;
                        case ClassId.Bow:
                        deffence = Asda2Defence;
                        break;
                        case ClassId.Crossbow:
                        deffence = Asda2Defence;
                        break;
                        case ClassId.Balista:
                        deffence = Asda2Defence;
                        break;
                        case ClassId.AtackMage:
                        deffence = Asda2MagicDefence;
                        break;
                        case ClassId.HealMage:
                        deffence = Asda2MagicDefence;
                        break;
                        case ClassId.SupportMage:
                        deffence = Asda2MagicDefence;
                        break;
                }
            }
            action.ResistPct = DamageAction.CalcResistPrc(deffence, action.Damage, GetResistChancePct(this, action.UsedSchool));
			action.Absorbed = 0;
			action.SpellEffect = effect;
            

			action.Victim = this;
			if (attacker != null)
			{
				attacker.DeathPrevention++;
			}

			DeathPrevention++;
			try
			{
				if (attacker != null)
				{
					// the damage is caused by someone else (i.e. not environmental damage etc)

                    var random = Utility.Random(1, 10000);
                    var hitChance = action.CalcHitChance();
                    if (random > hitChance)
                    {
                        // missed the target
                        action.Miss();
                    }
                    else
                    {
                        var block = action.CalcBlockChance();
                        if (random > (hitChance - block))
                        {
                            // block
                            action.Block();
                        }
                        else
                        {
                            var critical = action.CalcCritChance();
                            if (forceCrit || random > (hitChance - critical - block))
                            {
                                // critical hit
                                action.StrikeCritical();
                            }
                        }
                        if (addDamageBonuses)
                        {
                            action.AddDamageMods();
                        }
                    }

					// add mods and call events
					
					OnDefend(action);
					attacker.OnAttack(action);
				}

				action.Resisted = (int)Math.Round(action.Damage * action.ResistPct / 100);

				DoRawDamage(action);
			}
			finally
			{
				DeathPrevention--;
				if (attacker != null)
				{
					attacker.DeathPrevention--;
				}
                if(clearAction)
                    action.OnFinished();
			}
            if (clearAction)
                return null;
            return action;
        }

        public float GetBaseCritChance(DamageSchool dmgSchool, Spell spell, IAsda2Weapon weapon)
        {
            var character = this as Character;
            if (character != null)
            {
                var chr = character;
                return chr.CritChanceMeleePct;
            }
            var npc = this as NPC;
            if (npc != null)
            {
                switch (npc.Entry.Rank)
                {
                    case CreatureRank.Normal:
                        return 200;
                    case CreatureRank.Elite:
                        return 1000;
                    case CreatureRank.Boss:
                        return 1500;
                    case CreatureRank.WorldBoss:
                        return 2000;
                }
            }
            return 100;
        }

        /// <summary>
        /// Calculates this Unit's chance to resist the given school.
        /// Value is between 0 and 100
        /// </summary>
        public float GetResistChancePct(Unit attacker, DamageSchool school)
        {
            int attackerLevel;
            var res = GetResistance(school);
            if (attacker != null)
            {
                attackerLevel = Math.Max(1, attacker.Level);
                res -= attacker.GetTargetResistanceMod(school);
            }
            else
            {
                attackerLevel = 1;
            }

            res = Math.Max(0, res);

            var resist = (res / (attackerLevel * 5f)) * 0.75f;

            if (resist > 75)
            {
                resist = 75f;
            }

            if (resist < 0)
            {
                resist = 0;
            }

            return resist;
        }

        /// <summary>
        /// Returns percent * 100 of chance to dodge
        /// Modified by expertise.
        /// </summary>
        public int CalcDodgeChance(WorldObject attacker)
        {
            float dodgeChance;

            if (this is Character)
            {
                var def = (Character)this;
                dodgeChance = def.DodgeChance;
            }
            else
            {
                dodgeChance = 5;
            }

            if (attacker is Character)
            {
                var atk = (Character)attacker;
                dodgeChance -= atk.Expertise * 0.25f;
                dodgeChance += atk.IntMods[(int)StatModifierInt.TargetDodgesAttackChance];
            }

            dodgeChance *= 100;
            return (int)dodgeChance;
        }

        /// <summary>
        /// Returns percent * 100 of chance to parry.
        /// Modified by expertise.
        /// </summary>
        /// <returns></returns>
        public int CalcParryChance(Unit attacker)
        {
            var parryChance = ParryChance;

            if (attacker is Character)
            {
                var atk = (Character)attacker;
                parryChance -= atk.Expertise * 0.25f;
            }

            parryChance *= 100;
            return (int)parryChance;
        }

        /// <summary>
        /// Modified by victim's resilience
        /// </summary>
        /// <param name="dmg"></param>
        /// <param name="victim"></param>
        /// <param name="effect"></param>
        /// <returns></returns>
        public virtual float CalcCritDamage(float dmg, Unit victim, SpellEffect effect)
        {
            /*if (effect != null)
            {
                return (int)(dmg * 1.5f);
            }*/
            var multiplier = 200;
            multiplier -= (int)victim.GetResiliencePct();
            multiplier += victim.GetIntMod(StatModifierInt.CritDamageBonusPct) + CritDamageBonusPrc;
            return (dmg * multiplier + 50) / 100;
        }


        /// <summary>
        /// whether this Unit resists a debuff (independent on resistances)
        /// </summary>
        public bool CheckDebuffResist(int attackerLevel, DamageSchool school)
        {
            if (Utility.Random(0, 100) <
                GetDebuffResistance(school) -
                GetAttackerSpellHitChanceMod(school))
                return true;

            return false;
        }
        #endregion

        #region Combat
        /// <summary>
        /// whether this Unit is currently in Combat.
        /// If it is actively fighting (rather than being forced into CombatMode),
        /// IsFighting must be true.
        /// </summary>
        public bool IsInCombat
        {
            get { return m_isInCombat; }
            set
            {
                if (m_isInCombat == value) return;

                if (m_isInCombat = value)
                {
                    UnitFlags |= UnitFlags.Combat;

                    StandState = StandState.Stand;

                    // remove non-combat Auras
                    m_auras.RemoveByFlag(AuraInterruptFlags.OnStartAttack);

                    // cancel non-combat spellcasts
                    if (m_spellCast != null)
                    {
                        var spell = m_spellCast.Spell;
                        if (spell != null &&
                            spell.RequiresCasterOutOfCombat)
                        {
                            m_spellCast.Cancel();
                        }
                    }

                    // Pet entered combat, so will it's master.
                    if (HasMaster)
                    {
                        Master.IsInCombat = true;
                    }

                    OnEnterCombat();
                    m_attackTimer.Start(1);
                }
                else
                {
                    if (this is NPC)
                    {
                        // NPCs cannot remain in combat-mode without fighting
                        IsFighting = false;
                    }
                    CancelPendingAbility();
                    UnitFlags &= ~UnitFlags.Combat;
                    m_attackTimer.Stop();

                    OnLeaveCombat();
                }

                this.UpdatePowerRegen();
            }
        }


        /// <summary>
        /// Indicates whether this Unit is currently trying to swing at its target.
        /// If <c>IsInCombat</c> is set but Unit is not fighting, it will leave Combat mode after <c>CombatDeactivationDelay</c> without combat activity.
        /// </summary>
        public bool IsFighting
        {
            get { return m_isFighting; }
            set
            {
                if (m_isFighting != value)
                {
#pragma warning disable 665
                    if (m_isFighting = value)
#pragma warning restore 665
                    {
                        if (m_target == null)
                        {
                            /*LogUtil.ErrorException(new Exception(string.Format(
                                "{0} ({1}) is trying to fight without valid Target.",
                                this, m_brain != null ? m_brain.State.ToString() : "")));*/
                            m_isFighting = false;
                            return;
                        }
                        Dismount();

                        if (this is NPC)
                        {
                            // NPCs are always in combat mode when starting to attack
                            IsInCombat = true;
                        }
                        else
                        {
                            m_attackTimer.Start(1);
                        }
                        /*var chr = this as Character;
                        if (chr != null)
                        {
                            Asda2CombatHandler.StartAtackResponse(chr, m_target, 0);
                        }*/
                        //m_lastFightTime = Environment.TickCount;

                        //CombatTick(0);
                    }
                    else
                    {
                        CancelPendingAbility();
                        // var chr = this as Character;
                        var npc = this as NPC;
                        if (npc != null)
                        {
                            //npc.Brain.EnterDefaultState();
                            npc.Brain.StopCurrentAction();
                            //npc.Target = null;
                        }
                        /*if(chr !=null)
                            Asda2MovmentHandler.SendStartMoveCommonToOneClienResponset(chr,chr.Client,true);*/
                        //CombatHandler.SendCombatStop(this, m_target, 0);
                        CheckCombatState();
                    }
                }
            }
        }

        /// <summary>
        /// Tries to land a mainhand hit + maybe offhand hit on the current Target
        /// </summary>
        protected virtual void CombatTick(int timeElapsed)
        {
            // if currently casting a spell, skip this
            if (IsUsingSpell && !m_spellCast.IsPending)
            {
                m_attackTimer.Start(DamageAction.DefaultCombatTickDelay);
                return;
            }

            if (!CheckCombatState())
            {
                if (m_isInCombat)
                {
                    // if still in combat - check soon again
                    m_attackTimer.Start(DamageAction.DefaultCombatTickDelay);
                }
                return;
            }

            if (!CanDoHarm || !CanMelee)
            {
                m_attackTimer.Start(DamageAction.DefaultCombatTickDelay);
                return;
            }

            var target = m_target;
            if (target == null || target.IsDead)
            {
                IsFighting = false;
                if (this is NPC)
                    Movement.MayMove = true;
                return;
            }
            var now = Environment.TickCount;

            var isRanged = IsUsingRangedWeapon;
            var mainHandDelay = m_lastStrike + (isRanged ? RangedAttackTime : MainHandAttackTime) - now;

            var strikeReady = mainHandDelay <= 0;

            // try to strike
            if (strikeReady)
            {
                if (this is NPC)
                    Movement.MayMove = true;
                var distanceSq = GetDistanceSq(target);
                var mainWeapon = isRanged ? m_RangedWeapon : m_mainWeapon;
                if (mainWeapon != null)
                {
                    if (IsInAttackRangeSq(mainWeapon, target, distanceSq))
                    {
                        // close enough
                        if (m_AutorepeatSpell != null)
                        {
                            // Auto-shot (only when not running)
                            if (!IsMoving)
                            {
                                SpellCast.TargetFlags = SpellTargetFlags.Unit;
                                SpellCast.SelectedTarget = target;
                                SpellCast.Start(m_AutorepeatSpell, false);
                                m_lastStrike = now;
                                mainHandDelay += RangedAttackTime;
                            }
                        }
                        else
                        {
                            var chr = this as Character;
                            if (chr != null)
                            {
                                chr.IsMoving = false;
                            }
                            Strike(mainWeapon);
                            m_lastStrike = now;
                            mainHandDelay += MainHandAttackTime;
                            if (this is NPC)
                                Movement.MayMove = false;
                        }
                    }
                    else
                    {
                        // too far away
                        if (UsesPendingAbility(mainWeapon))
                        {
                            // ability is pending -> Need to cancel
                            m_spellCast.Cancel(SpellFailedReason.OutOfRange);
                        }

                        // no pending ability
                        if (this is Character)
                        {
                            CombatHandler.SendAttackSwingNotInRange(this as Character);
                        }
                        else if (this is NPC)
                        {
                            Brain.OnCombatTargetOutOfRange();
                        }
                    }
                }
            }





            m_attackTimer.Start(mainHandDelay <= 0 ? 1000 : mainHandDelay);
        }

        /// <summary>
        /// Checks whether the Unit can attack.
        /// Also deactivates combat mode, if unit has left combat for long enough.
        /// </summary>
        protected virtual bool CheckCombatState()
        {
            if (m_comboTarget != null && (!m_comboTarget.IsInContext || !m_comboTarget.IsAlive))
            {
                ResetComboPoints();
            }

            if (m_target == null || !CanHarm(m_target))
            {
                IsFighting = false;
            }
            else if (!CanSee(m_target))
            {
                //if (this is Character)
                //{
                //	CharacterHandler.SendClearTarget(((Character)this), m_target);
                //}
                Target = null;
                IsFighting = false;
            }
            else if (!CanDoHarm)
            {
                return false;
            }

            return m_isFighting;
        }

        /// <summary>
        /// Resets the attack timer to delay the next strike by the current weapon delay,
        /// if Unit is fighting.
        /// </summary>
        public void ResetAttackTimer()
        {
            if (m_isFighting)
            {
                // figure out delay
                int delay;
                if (m_offhandWeapon != null)
                {
                    delay = Math.Min(MainHandAttackTime, OffHandAttackTime);
                }
                else
                {
                    delay = MainHandAttackTime;
                }

                if (m_RangedWeapon != null && m_RangedWeapon.IsRanged)
                {
                    delay = Math.Min(delay, RangedAttackTime);
                }

                // start
                m_attackTimer.Start(delay);
            }
            else
            {
                m_attackTimer.Start(DamageAction.DefaultCombatTickDelay);
            }
        }

        /// <summary>
        /// Is called whenever this Unit enters Combat mode
        /// </summary>
        protected virtual void OnEnterCombat()
        {
            SheathType = IsUsingRangedWeapon ? SheathType.Ranged : SheathType.Melee;
            StandState = StandState.Stand;
            m_lastCombatTime = m_lastUpdateTime;
            if (m_brain != null)
            {
                m_brain.OnEnterCombat();
            }
        }

        /// <summary>
        /// Is called whenever this Unit leaves Combat mode
        /// </summary>
        protected virtual void OnLeaveCombat()
        {
            //SheathType = SheathType.None;
            ResetComboPoints();
            if (m_brain != null)
            {
                m_brain.OnLeaveCombat();
            }
        }

        #region OnDamageReceived
        /// <summary>
        /// Is called whenever this Unit receives any kind of damage
        /// 
        /// TODO: There is a small chance with each hit by your weapon that it will lose 1 durability point.
        /// TODO: There is a small chance with each spell cast that you will lose 1 durability point to your weapon. 
        /// TODO: There is a small chance with each hit absorbed by your armor that it will lose 1 durability point.
        /// </summary>
        protected internal virtual void OnDamageAction(IDamageAction action)
        {
            IsSitting = false;
            if (action is DamageAction && action.Attacker != null)
            {
                var aaction = (DamageAction)action;
                
                if (action.ActualDamage <= 0)
                {
                    return;
                }

                if (!action.IsDot)
                {
                    var attacker = action.Attacker;
                    var weapon = action.Weapon;
                    var weaponAttack = weapon != null && !attacker.IsPvPing;

                    // Remove damage-sensitive Auras
                    m_auras.RemoveByFlag(AuraInterruptFlags.OnDamage);
                    attacker.m_lastCombatTime = attacker.m_lastUpdateTime;
                    
                    // stand up when hit
                    StandState = StandState.Stand;
                    
                    if (IsAlive)
                    {
                        // aggro'd -> Enter combat mode and update combat-time
                        IsInCombat = true;
                        m_lastCombatTime = m_lastUpdateTime;
                    }
                }

                TriggerProcOnDamageReceived(action);
            }
        }

        void TriggerProcOnDamageReceived(IDamageAction action)
        {
            var procHitFlags = action.IsCritical ? ProcHitFlags.CriticalHit : ProcHitFlags.None;
            var victimProcFlags = ProcTriggerFlags.ReceivedAnyDamage;

            if (action.IsDot)
            {
                action.Attacker.Proc(ProcTriggerFlags.DonePeriodicDamageOrHeal, this, action, true, procHitFlags);
                victimProcFlags |= ProcTriggerFlags.ReceivedPeriodicDamageOrHeal;
            }

            Proc(victimProcFlags, action.Attacker, action, true, procHitFlags);
        }
        #endregion
        #endregion

        #region Range Checks
        public float GetAttackRange(IAsda2Weapon weapon, Unit target)
        {
            return weapon.MaxRange + CombatReach + target.CombatReach;
        }

        public float GetBaseAttackRange(Unit target)
        {
            return MaxAttackRange + target.CombatReach;
        }

        public bool IsInBaseAttackRange(Unit target)
        {
            var distSq = GetDistanceSq(target);
            var range = GetBaseAttackRange(target);
            return distSq <= range * range;
        }

        public bool IsInBaseAttackRangeSq(Unit target, float distSq)
        {
            var range = GetBaseAttackRange(target);
            return distSq <= range * range;
        }

        public float GetMinAttackRange(IAsda2Weapon weapon, Unit target)
        {
            return weapon.MinRange;
        }

        /// <summary>
        /// Returns whether the given Object is in range of Main- or Extra (Ranged)- weapon
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public bool IsInAttackRange(Unit target)
        {
            var distSq = GetDistanceSq(target);
            return IsInAttackRangeSq(target, distSq);
        }

        /// <summary>
        /// Whether the suitable target is in reach to be attacked
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public bool CanReachForCombat(Unit target)
        {
            return CanMove || IsInAttackRange(target);
        }

        public bool IsInAttackRangeSq(Unit target, float distSq)
        {
            if (!CanMelee)
            {
                if (this is NPC)
                {
                    return IsInBaseAttackRangeSq(target, distSq);
                }
                return false;
            }
            if (UsesRangedWeapon)
            {
                if (IsInAttackRangeSq(m_RangedWeapon, target, distSq))
                {
                    return true;
                }
            }

            return IsInMeleeAttackRangeSq(m_mainWeapon, target, distSq);
        }

        public bool IsInMaxRange(Spell spell, WorldObject target)
        {
            var max = GetSpellMaxRange(spell, target);
            var distSq = GetDistanceSq(target);
            return distSq <= max * max;
        }

        public bool IsInSpellRange(Spell spell, WorldObject target)
        {
            var max = GetSpellMaxRange(spell, target);
            var distSq = GetDistanceSq(target);
            if (spell.Range.MinDist > 0)
            {
                var min = spell.Range.MinDist; //GetSpellMinRange(spell.Range.MinDist, target);
                if (distSq < min * min)
                {
                    return false;
                }
            }

            return distSq <= max * max;
        }

        /// <summary>
        /// Melee has no Min range
        /// </summary>
        public bool IsInMeleeAttackRangeSq(IAsda2Weapon weapon, Unit target, float distSq)
        {
            var max = GetAttackRange(weapon, target);
            return distSq <= max * max;
        }

        public bool IsInAttackRangeSq(IAsda2Weapon weapon, Unit target, float distSq)
        {
            var max = GetAttackRange(weapon, target);
            if (UsesPendingAbility(weapon))
            {
                max = GetSpellMaxRange(m_spellCast.Spell, max);
            }

            if (distSq <= max * max)
            {
                if (weapon.IsRanged)
                {
                    var min = GetMinAttackRange(weapon, target);
                    if (distSq < min * min)
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        public bool IsInRange(SimpleRange range, WorldObject obj)
        {
            var distSq = GetDistanceSq(obj);
            return (distSq <= range.MaxDist * range.MaxDist &&
                     (range.MinDist < 1 || distSq >= range.MinDist * range.MinDist));
        }

        public virtual float AggroBaseRange
        {
            get { return NPCEntry.AggroBaseRangeDefault /*+ CombatReach*/ + BoundingRadius; }
        }

        public virtual float GetAggroRange(Unit victim)
        {
            return Math.Max(AggroBaseRange + ((Level - victim.Level) * NPCEntry.AggroRangePerLevel), NPCEntry.AggroRangeMinDefault);
        }

        public float GetAggroRangeSq(Unit victim)
        {
            //if (unit.BelongsToPlayer)
            //{
            var range = GetAggroRange(victim);
            return range * range;
            //}
            //return AggroRangeNPCs * AggroRangeNPCs;
        }
        #endregion

        #region Combat and Hostility Checks
        /// <summary>
        /// Checks for hostility etc
        /// 
        /// TODO: Restrict interference in Duels
        /// </summary>
        public SpellFailedReason CanCastSpellOn(Unit target, Spell spell)
        {
            //return spell.IsHarmful == IsHostileWith(target) && !target.IsImmune(spell.Mechanic)
            //    && !target.IsImmune(spell.School);
            var canHarm = CanHarm(target);
            if ((canHarm && !spell.HasHarmfulEffects) || (!canHarm && !spell.HasBeneficialEffects))
            {
                return canHarm ? SpellFailedReason.TargetFriendly : SpellFailedReason.TargetEnemy;
            }
            return SpellFailedReason.Ok;
        }

        #endregion
    }
}