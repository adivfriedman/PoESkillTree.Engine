﻿using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Engine.Computation.Common;
using PoESkillTree.Engine.Computation.Common.Builders;
using PoESkillTree.Engine.Computation.Common.Builders.Damage;
using PoESkillTree.Engine.Computation.Common.Builders.Equipment;
using PoESkillTree.Engine.Computation.Common.Builders.Modifiers;
using PoESkillTree.Engine.Computation.Common.Data;
using PoESkillTree.Engine.Computation.Data.Base;
using PoESkillTree.Engine.Computation.Data.Collections;
using PoESkillTree.Engine.GameModel;
using PoESkillTree.Engine.GameModel.Items;

namespace PoESkillTree.Engine.Computation.Data
{
    /// <inheritdoc />
    /// <summary>
    /// <see cref="IStatMatchers"/> implementation matching stat parts specifying conditions.
    /// </summary>
    public class ConditionMatchers : StatMatchersBase
    {
        private readonly IModifierBuilder _modifierBuilder;

        public ConditionMatchers(IBuilderFactories builderFactories, IModifierBuilder modifierBuilder)
            : base(builderFactories)
        {
            _modifierBuilder = modifierBuilder;
        }

        protected override IReadOnlyList<MatcherData> CreateCollection() =>
            new ConditionMatcherCollection(_modifierBuilder)
            {
                // actions
                // - generic
                { "if you('ve| have) ({ActionMatchers})( an enemy)? recently", Reference.AsAction.Recently },
                { "if you haven't ({ActionMatchers}) recently", Not(Reference.AsAction.Recently) },
                { "if you've ({ActionMatchers}) in the past # seconds", Reference.AsAction.InPastXSeconds(Value) },
                { "for # seconds on ({ActionMatchers})", Reference.AsAction.InPastXSeconds(Value) },
                { "on ({ActionMatchers}) for # seconds", Reference.AsAction.InPastXSeconds(Value) },
                {
                    "for # seconds on ({KeywordMatchers}) ({ActionMatchers})",
                    And(Condition.WithPart(References[0].AsKeyword), References[1].AsAction.InPastXSeconds(Value))
                },
                {
                    "(gain )?for # seconds when you ({ActionMatchers}) a rare or unique enemy",
                    And(Enemy.IsRareOrUnique, Reference.AsAction.InPastXSeconds(Value))
                },
                {
                    "(gain )?for # seconds when you ({ActionMatchers}) a unique enemy",
                    And(Enemy.IsUnique, Reference.AsAction.InPastXSeconds(Value))
                },
                {
                    "(gain )?for # seconds when you ({ActionMatchers}) an enemy",
                    Reference.AsAction.InPastXSeconds(Value)
                },
                { "when you ({ActionMatchers}) an enemy, for # seconds", Reference.AsAction.InPastXSeconds(Value) },
                { "for # seconds when ({ActionMatchers})", Reference.AsAction.By(Enemy).InPastXSeconds(Value) },
                // - kill
                {
                    "if you've killed a maimed enemy recently",
                    And(Kill.Recently, Buff.Maim.IsOn(Enemy))
                },
                {
                    "if you've killed a bleeding enemy recently",
                    And(Kill.Recently, Ailment.Bleed.IsOn(Enemy))
                },
                {
                    "if you've killed a cursed enemy recently",
                    And(Kill.Recently, Buffs(targets: Enemy).With(Keyword.Curse).Any())
                },
                {
                    "if you or your totems have killed recently",
                    Or(Kill.Recently, Kill.By(Entity.Totem).Recently)
                },
                {
                    "if you or your minions have killed recently",
                    Or(Kill.Recently, Kill.By(Entity.Minion).Recently)
                },
                // - hit
                { "if you('ve| have) hit them recently", Hit.Recently },
                { "if you('ve| have) been hit recently", Hit.By(Enemy).Recently },
                { "if you haven't been hit recently", Not(Hit.By(Enemy).Recently) },
                { "if you were damaged by a hit recently", Hit.By(Enemy).Recently },
                { "if you've taken no damage from hits recently", Not(Hit.By(Enemy).Recently) },
                { "if you weren't damaged by a hit recently", Not(Hit.By(Enemy).Recently) },
                // - critical strike
                { "if you've crit in the past # seconds", CriticalStrike.InPastXSeconds(Value) },
                // - block
                { "if you've blocked damage from a unique enemy recently", And(Block.Recently, Enemy.IsUnique) },
                {
                    "if you've blocked damage from a unique enemy in the past # seconds",
                    And(Block.InPastXSeconds(Value), Enemy.IsUnique)
                },
                // - other
                { "if you've taken a savage hit recently", Action.SavageHit.By(Enemy).Recently },
                { "if you've shattered an enemy recently", Action.Shatter.Recently },
                {
                    "for # seconds after spending( a total of)? # mana",
                    Action.SpendMana(Values[1]).InPastXSeconds(Values[0])
                },
                { "if you have consumed a corpse recently", Action.ConsumeCorpse.Recently },
                { "if you haven't taken damage recently", Not(Action.TakeDamage.Recently) },
                { "if a minion has been killed recently", Action.Die.By(Entity.Minion).Recently },
                { "while focussed", Action.Focus.Recently },
                { "for # seconds when you focus", Action.Focus.InPastXSeconds(Value) },
                // damage
                { "attacks have", Condition.With(DamageSource.Attack) },
                { "with attacks", Condition.With(DamageSource.Attack) },
                { "for attacks", Condition.With(DamageSource.Attack) },
                { "for spells", Condition.With(DamageSource.Spell) },
                { "(your )?spells have", Condition.With(DamageSource.Spell) },
                // - by item tag
                { "with weapons", AttackWith(Tags.Weapon) },
                { "(?<!this )weapon", AttackWith(Tags.Weapon) },
                { "with bows", AttackWith(Tags.Bow) },
                { "with a bow", AttackWith(Tags.Bow) },
                { "with arrow hits", AttackWith(Tags.Bow) },
                { "bow", AttackWith(Tags.Bow) },
                { "with swords", AttackWith(Tags.Sword) },
                { "to sword attacks", AttackWith(Tags.Sword) },
                { "with claws", AttackWith(Tags.Claw) },
                { "claw", AttackWith(Tags.Claw) },
                { "to claw attacks", AttackWith(Tags.Claw) },
                { "with daggers", AttackWith(Tags.Dagger) },
                { "to dagger attacks", AttackWith(Tags.Dagger) },
                { "with wands", AttackWith(Tags.Wand) },
                { "wand", AttackWith(Tags.Wand) },
                { "to wand attacks", AttackWith(Tags.Wand) },
                { "with axes", AttackWith(Tags.Axe) },
                { "to axe attacks", AttackWith(Tags.Axe) },
                { "with staves", AttackWith(Tags.Staff) },
                { "with a staff", AttackWith(Tags.Staff) },
                { "to staff attacks", AttackWith(Tags.Staff) },
                { "with ranged weapons", AttackWith(Tags.Ranged) },
                {
                    "with maces",
                    (Or(MainHandAttackWith(Tags.Mace), MainHandAttackWith(Tags.Sceptre)),
                        Or(OffHandAttackWith(Tags.Mace), OffHandAttackWith(Tags.Sceptre)))
                },
                {
                    "to mace attacks",
                    (Or(MainHandAttackWith(Tags.Mace), MainHandAttackWith(Tags.Sceptre)),
                        Or(OffHandAttackWith(Tags.Mace), OffHandAttackWith(Tags.Sceptre)))
                },
                { "with one handed weapons", AttackWith(Tags.OneHandWeapon) },
                {
                    "with one handed melee weapons",
                    (And(MainHandAttackWith(Tags.OneHandWeapon), Not(MainHand.Has(Tags.Ranged))),
                        And(OffHandAttackWith(Tags.OneHandWeapon), Not(OffHand.Has(Tags.Ranged))))
                },
                { "with two handed weapons", AttackWith(Tags.TwoHandWeapon) },
                {
                    "with two handed melee weapons",
                    And(MainHandAttackWith(Tags.TwoHandWeapon), Not(MainHand.Has(Tags.Ranged)))
                },
                { "with unarmed attacks", And(MainHandAttack, Not(MainHand.HasItem)) },
                // - by item slot
                { "with the main-hand weapon", MainHandAttack },
                { "with main hand", MainHandAttack },
                { "with off hand", OffHandAttack },
                {
                    "(attacks |hits )?with this weapon( deal| have)?",
                    (ModifierSourceIs(ItemSlot.MainHand).And(MainHandAttack),
                        ModifierSourceIs(ItemSlot.OffHand).And(OffHandAttack))
                },
                // - taken
                { "(?<!when you )take", Condition.DamageTaken },
                // equipment
                { "while unarmed", Not(MainHand.HasItem) },
                { "while wielding a staff", MainHand.Has(Tags.Staff) },
                { "while wielding a dagger", EitherHandHas(Tags.Dagger) },
                { "while wielding a bow", MainHand.Has(Tags.Bow) },
                { "while wielding a sword", EitherHandHas(Tags.Sword) },
                { "while wielding a claw", EitherHandHas(Tags.Claw) },
                { "while wielding an axe", EitherHandHas(Tags.Axe) },
                { "while wielding a mace", Or(EitherHandHas(Tags.Mace), EitherHandHas(Tags.Sceptre)) },
                { "while wielding a wand", EitherHandHas(Tags.Wand) },
                { "while wielding a melee weapon", And(EitherHandHas(Tags.Weapon), Not(MainHand.Has(Tags.Ranged))) },
                { "while wielding a one handed weapon", MainHand.Has(Tags.OneHandWeapon) },
                { "while wielding a two handed weapon", MainHand.Has(Tags.TwoHandWeapon) },
                { "while dual wielding", OffHand.Has(Tags.Weapon) },
                { "while holding a shield", OffHand.Has(Tags.Shield) },
                { "while dual wielding or holding a shield", Or(OffHand.Has(Tags.Weapon), OffHand.Has(Tags.Shield)) },
                { "with shields", OffHand.Has(Tags.Shield) },
                {
                    "from equipped shield",
                    And(Condition.BaseValueComesFrom(ItemSlot.OffHand), OffHand.Has(Tags.Shield))
                },
                { "with # corrupted items equipped", Equipment.Count(e => e.Corrupted.IsSet) >= Value },
                // stats
                // - pool
                { "(when|while) on low ({PoolStatMatchers})", Reference.AsPoolStat.IsLow },
                { "when not on low ({PoolStatMatchers})", Not(Reference.AsPoolStat.IsLow) },
                { "(when|while) on full ({PoolStatMatchers})", Reference.AsPoolStat.IsFull },
                { "while ({PoolStatMatchers}) is full", Reference.AsPoolStat.IsFull },
                { "while not on full ({PoolStatMatchers})", Not(Reference.AsPoolStat.IsFull) },
                { "if you have ({PoolStatMatchers})", Not(Reference.AsPoolStat.IsEmpty) },
                { "while no ({PoolStatMatchers}) is reserved", Reference.AsPoolStat.Reservation.Value <= 0 },
                { "if energy shield recharge has started recently", EnergyShield.Recharge.StartedRecently },
                { "while leeching ({PoolStatMatchers})", Reference.AsPoolStat.Leech.IsActive },
                // - charges
                { "(while|if) you have no ({ChargeTypeMatchers})", Reference.AsChargeType.Amount.Value <= 0 },
                { "(while|if) you have( an?)? ({ChargeTypeMatchers})", Reference.AsChargeType.Amount.Value > 0 },
                {
                    "(while|if) you have at least # ({ChargeTypeMatchers})",
                    Reference.AsChargeType.Amount.Value >= Value
                },
                {
                    "while (at maximum|on full) ({ChargeTypeMatchers})",
                    Reference.AsChargeType.Amount.Value >= Reference.AsChargeType.Amount.Maximum.Value
                },
                { "lose a ({ChargeTypeMatchers}) and", Reference.AsChargeType.Amount.Value > 0 },
                // - other
                {
                    "if you have # primordial (jewels|items socketed or equipped)",
                    Stat.PrimordialJewelsSocketed.Value >= Value
                },
                // - on enemy
                { "(against enemies )?that are on low life", Life.For(Enemy).IsLow },
                { "against enemies on low life", Life.For(Enemy).IsLow },
                { "(against enemies )?that are on full life", Life.For(Enemy).IsFull },
                { "against enemies on full life", Life.For(Enemy).IsFull },
                { "against rare and unique enemies", Enemy.IsRareOrUnique },
                { "against unique enemies", Enemy.IsUnique },
                { "if rare or unique", Enemy.IsRareOrUnique },
                { "if normal or magic", Not(Enemy.IsRareOrUnique) },
                { "while there is only one nearby enemy", Enemy.CountNearby.Eq(1) },
                { "if there are at least # nearby enemies", Enemy.CountNearby >= Value },
                { "at close range", Enemy.IsNearby },
                { "while a rare or unique enemy is nearby", And(Enemy.IsRareOrUnique, Enemy.IsNearby) },
                // buffs
                { "while you have ({BuffMatchers})", Reference.AsBuff.IsOn(Self) },
                { "while affected by ({SkillMatchers})", Reference.AsSkill.Buff.IsOn(Self) },
                { "while affected by a ({KeywordMatchers}) skill buff", Buffs(Self).With(Reference.AsKeyword).Any() },
                { "during onslaught", Buff.Onslaught.IsOn(Self) },
                { "while phasing", Buff.Phasing.IsOn(Self) },
                { "if you've ({BuffMatchers}) an enemy recently,?", Reference.AsBuff.InflictionAction.Recently },
                { "enemies you taunt( deal)?", And(For(Enemy), Buff.Taunt.IsOn(Self, Enemy)) },
                { "enemies ({BuffMatchers}) by you", And(For(Enemy), Reference.AsBuff.IsOn(Self, Enemy)) },
                { "enemies you curse( have)?", And(For(Enemy), Buffs(Self, Enemy).With(Keyword.Curse).Any()) },
                { "(against|from) blinded enemies", Buff.Blind.IsOn(Enemy) },
                { "from taunted enemies", Buff.Taunt.IsOn(Enemy) },
                {
                    "you and allies affected by your aura skills (have|deal)",
                    Or(For(Self), And(For(Ally), Buffs(targets: Ally).With(Keyword.Aura).Any()))
                },
                // ailments
                { "while( you are)? ({AilmentMatchers})", Reference.AsAilment.IsOn(Self) },
                { "(against|from) ({AilmentMatchers}) enemies", Reference.AsAilment.IsOn(Enemy) },
                {
                    "(against|from) ({AilmentMatchers}) or ({AilmentMatchers}) enemies",
                    Or(References[0].AsAilment.IsOn(Enemy), References[1].AsAilment.IsOn(Enemy))
                },
                {
                    "against frozen, shocked or ignited enemies",
                    Or(Ailment.Freeze.IsOn(Enemy), Ailment.Shock.IsOn(Enemy), Ailment.Ignite.IsOn(Enemy))
                },
                { "which are ({AilmentMatchers})", Reference.AsAilment.IsOn(Enemy) },
                {
                    "against enemies( that are)? affected by elemental ailments",
                    Ailment.Elemental.Any(a => a.IsOn(Enemy))
                },
                {
                    "against enemies( that are)? affected by no elemental ailments",
                    Not(Ailment.Elemental.Any(a => a.IsOn(Enemy)))
                },
                { "enemies chilled by supported skills( have)?", Ailment.Chill.IsOn(Enemy) },
                // ground effects
                { "while on consecrated ground", Ground.Consecrated.IsOn(Self) },
                // skills
                // - by keyword
                { "vaal( skill)?", With(Keyword.Vaal) },
                { "non-vaal skills deal", Not(With(Keyword.Vaal)) },
                { "with bow skills", And(MainHand.Has(Tags.Bow), With(Keyword.Bow)) },
                { "chaos skills have", With(Chaos) },
                { "spell skills have", With(Keyword.Spell) },
                { "(with|of|for) ({KeywordMatchers}) skills", With(Reference.AsKeyword) },
                { "(supported )?({KeywordMatchers}) skills (have|deal)", With(Reference.AsKeyword) },
                {
                    "({KeywordMatchers}) ({KeywordMatchers}) skills (have|deal)",
                    And(With(References[0].AsKeyword), With(References[1].AsKeyword))
                },
                { "caused by melee hits", Condition.WithPart(Keyword.Melee) },
                { "with melee damage", Condition.WithPart(Keyword.Melee) },
                // - by damage type
                { "with elemental skills", ElementalDamageTypes.Select(With).Aggregate((l, r) => l.Or(r)) },
                { "with ({DamageTypeMatchers}) skills", With(Reference.AsDamageType) },
                // - by single skill
                { "({SkillMatchers})('s)?", With(Reference.AsSkill) },
                { "({SkillMatchers}) (fires|has a|have a|has|deals|gain)", With(Reference.AsSkill) },
                { "dealt by ({SkillMatchers})", With(Reference.AsSkill) },
                {
                    "({SkillMatchers}) and ({SkillMatchers})",
                    Or(With(References[0].AsSkill), With(References[1].AsSkill))
                },
                { "while you have an? ({SkillMatchers})", Reference.AsSkill.Instances.Value > 0 },
                // - cast recently/in past x seconds
                { "if you've cast a spell recently,?", Skills[Keyword.Spell].Cast.Recently },
                { "if you've attacked recently,?", Skills[Keyword.Attack].Cast.Recently },
                { "if you've used a movement skill recently", Skills[Keyword.Movement].Cast.Recently },
                { "if you've used a minion skill recently", Minions.Cast.Recently },
                { "if you've used a warcry recently", Skills[Keyword.Warcry].Cast.Recently },
                { "if you've warcried recently", Skills[Keyword.Warcry].Cast.Recently },
                {
                    "if you've used a ({DamageTypeMatchers}) skill in the past # seconds",
                    Skills[Reference.AsDamageType].Cast.InPastXSeconds(Value)
                },
                { "if you summoned a golem in the past # seconds", Golems.Cast.InPastXSeconds(Value) },
                // - by skill part
                {
                    "(beams?|final wave|shockwaves?|cone|aftershock) (has a|deals?)",
                    Stat.MainSkillPart.Value.Eq(1)
                },
                // - other
                { "to enemies they're attached to", Flag.IsBrandAttachedToEnemy },
                { "to branded enemy", Flag.IsBrandAttachedToEnemy },
                {
                    "while you're in a blood bladestorm",
                    And(Flag.InBloodStance, Condition.Unique("Are you in a Bladestorm?"))
                },
                {
                    "sand bladestorms grant to you",
                    And(Flag.InSandStance, Condition.Unique("Are you in a Bladestorm?"))
                },
                {
                    "enemies maimed by this skill",
                    And(Condition.ModifierSourceIs(new ModifierSource.Local.Skill("BloodSandArmour")),
                        Buff.Maim.IsOn(Enemy), Flag.InBloodStance, For(Enemy))
                },
                // traps and mines
                { "with traps", With(Keyword.Trap) },
                { "skills used by traps have", With(Keyword.Trap) },
                { "with mines", With(Keyword.Mine) },
                { "traps and mines (deal|have a)", Or(With(Keyword.Trap), With(Keyword.Mine)) },
                { "for throwing traps", With(Keyword.Trap) },
                { "if you detonated mines recently", Skills.DetonateMines.Cast.Recently },
                { "if you've placed a mine or thrown a trap recently", Or(Traps.Cast.Recently, Mines.Cast.Recently) },
                // totems
                { "^totems", For(Entity.Totem) },
                { "totems (gain|have)", For(Entity.Totem) },
                { "you and your totems", Or(For(Self), For(Entity.Totem)) },
                { "(spells cast|attacks used|skills used) by totems (have a|have)", With(Keyword.Totem) },
                { "of totem skills that cast an aura", And(With(Keyword.Totem), With(Keyword.Aura)) },
                { "while you have a totem", Totems.CombinedInstances.Value > 0 },
                { "if you've summoned a totem recently", Totems.Cast.Recently },
                // minions
                { "minions", For(Entity.Minion) },
                { "minions (deal|have|gain)", For(Entity.Minion) },
                { "supported skills have minion", For(Entity.Minion) },
                { "you and your minions have", For(Entity.Minion).Or(For(Self)) },
                { "golems", And(For(Entity.Minion), With(Keyword.Golem)) },
                { "golems have", And(For(Entity.Minion), With(Keyword.Golem)) },
                { "spectres have", And(For(Entity.Minion), With(Skills.RaiseSpectre)) },
                { "skeletons deal", And(For(Entity.Minion), WithSkeletonSkills) },
                // flasks
                { "while using a flask", Equipment.IsAnyFlaskActive() },
                { "during any flask effect", Equipment.IsAnyFlaskActive() },
                // - mods on flasks are only added when the flask item is enabled
                { "during (flask )?effect", Condition.True },
                // jewel thresholds
                {
                    "with( at least)? # ({AttributeStatMatchers}) in radius",
                    PassiveTree.TotalInModifierSourceJewelRadius(Reference.AsStat) >= Value
                },
                {
                    "with # total ({AttributeStatMatchers}) and ({AttributeStatMatchers}) in radius",
                    (PassiveTree.TotalInModifierSourceJewelRadius(References[0].AsStat)
                     + PassiveTree.TotalInModifierSourceJewelRadius(References[1].AsStat))
                    >= Value
                },
                // passive tree
                { "marauder:", PassiveTree.ConnectsToClass(CharacterClass.Marauder).IsSet },
                { "duelist:", PassiveTree.ConnectsToClass(CharacterClass.Duelist).IsSet },
                { "ranger:", PassiveTree.ConnectsToClass(CharacterClass.Ranger).IsSet },
                { "shadow:", PassiveTree.ConnectsToClass(CharacterClass.Shadow).IsSet },
                { "witch:", PassiveTree.ConnectsToClass(CharacterClass.Witch).IsSet },
                { "templar:", PassiveTree.ConnectsToClass(CharacterClass.Templar).IsSet },
                { "scion:", PassiveTree.ConnectsToClass(CharacterClass.Scion).IsSet },
                // stance
                { "(while )?in blood stance", Flag.InBloodStance },
                { "(while )?in sand stance", Flag.InSandStance },
                // other
                { "enemies have", For(Enemy) },
                { "against targets they pierce", Projectile.PierceCount.Value >= 1 },
                { "while stationary", Flag.AlwaysStationary },
                { "while moving", Flag.AlwaysMoving },
                // unique
                {
                    "against burning enemies", Or(Ailment.Ignite.IsOn(Enemy), Condition.Unique("Is the Enemy Burning?"))
                },
                { "while leeching", Condition.Unique("Leech.IsActive") },
                {
                    "if you've killed an enemy affected by your damage over time recently",
                    Condition.Unique("Have you recently killed an Enemy affected by your Damage over Time?")
                },
                { "while you have energy shield", Condition.Unique("Do you have Energy Shield?") },
                {
                    "if you've taken fire damage from a hit recently",
                    Condition.Unique("Have you recently taken Fire Damage from a Hit?")
                },
                { "while you have at least one nearby ally", Condition.Unique("Is any ally nearby?") },
                { "while channelling", Condition.Unique("Are you currently channeling?") },
                { "while you are not losing rage", Condition.Unique("Are you currently losing rage?") },
                { "during soul gain prevention", Condition.Unique("SoulGainPrevention") },
                // support gem mod clarifications. Irrelevant for parsing.
                { "supported (skills|spells|attacks) (have|deal)", Condition.True },
                { "(from |with )?supported skills'?", Condition.True },
                { "a supported skill", Condition.True },
                { "supported attacks", Condition.True },
                { "supported attack skills", Condition.True },
                { "supported attack skills deal", Condition.True },
                { "of supported curse skills", Condition.True },
            };
    }
}