﻿using System.Collections.Generic;
using System.Linq;
using EnumsNET;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.Computation.Common.Builders.Modifiers;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Common.Builders.Values;
using PoESkillTree.GameModel.Skills;

namespace PoESkillTree.Computation.Parsing.SkillParsers
{
    public class ActiveSkillLevelParser : IPartialSkillParser
    {
        private readonly IBuilderFactories _builderFactories;
        private readonly IMetaStatBuilders _metaStatBuilders;
        private readonly IModifierBuilder _modifierBuilder = new ModifierBuilder();

        private List<Modifier> _modifiers;
        private SkillPreParseResult _preParseResult;

        public ActiveSkillLevelParser(IBuilderFactories builderFactories, IMetaStatBuilders metaStatBuilders)
            => (_builderFactories, _metaStatBuilders) = (builderFactories, metaStatBuilders);

        public PartialSkillParseResult Parse(Skill skill, SkillPreParseResult preParseResult)
        {
            _modifiers = new List<Modifier>();
            _preParseResult = preParseResult;
            var level = preParseResult.LevelDefinition;

            if (level.DamageEffectiveness is double effectiveness)
            {
                AddModifier(_metaStatBuilders.DamageBaseAddEffectiveness, Form.TotalOverride, effectiveness);
            }
            if (level.DamageMultiplier is double multiplier)
            {
                AddModifier(_metaStatBuilders.DamageBaseSetEffectiveness, Form.TotalOverride, multiplier);
            }
            if (level.CriticalStrikeChance is double crit &&
                preParseResult.HitDamageSource is DamageSource hitDamageSource)
            {
                AddModifier(_builderFactories.ActionBuilders.CriticalStrike.Chance.With(hitDamageSource),
                    Form.BaseSet, crit);
            }
            if (level.ManaCost is int cost)
            {
                AddModifier(_builderFactories.StatBuilders.Pool.From(Pool.Mana).Cost, Form.BaseSet, cost);
                ParseReservation(cost);
            }
            if (level.Cooldown is int cooldown)
            {
                AddModifier(_builderFactories.StatBuilders.Cooldown, Form.BaseSet, cooldown);
            }

            var result = new PartialSkillParseResult(_modifiers, new UntranslatedStat[0]);
            _modifiers = null;
            _preParseResult = preParseResult;
            return result;
        }

        private void ParseReservation(int cost)
        {
            var activeSkillTypes = _preParseResult.SkillDefinition.ActiveSkill.ActiveSkillTypes.ToList();
            if (!activeSkillTypes.Contains(ActiveSkillType.ManaCostIsReservation))
                return;

            var isPercentage = activeSkillTypes.Contains(ActiveSkillType.ManaCostIsPercentage);
            var skillBuilder = _builderFactories.SkillBuilders.FromId(_preParseResult.SkillDefinition.Id);

            AddModifier(skillBuilder.Reservation, Form.BaseSet, cost, requiresMainSkill: false);

            foreach (var pool in Enums.GetValues<Pool>())
            {
                var poolBuilder = _builderFactories.StatBuilders.Pool.From(pool);
                var value = skillBuilder.Reservation.Value;
                if (isPercentage)
                {
                    value = value.AsPercentage * poolBuilder.Value;
                }
                AddModifier(poolBuilder.Reservation, Form.BaseAdd, value,
                    skillBuilder.ReservationPool.Value.Eq((double) pool));
            }
        }

        private void AddModifier(IStatBuilder stat, Form form, double value, bool requiresMainSkill = true)
        {
            var condition = requiresMainSkill ? _preParseResult.IsMainSkill.IsSet : null;
            AddModifier(stat, form, _builderFactories.ValueBuilders.Create(value), condition);
        }

        private void AddModifier(IStatBuilder stat, Form form, IValueBuilder value, IConditionBuilder condition)
        {
            var builder = _modifierBuilder
                .WithStat(stat)
                .WithForm(_builderFactories.FormBuilders.From(form))
                .WithValue(value);
            if (condition != null)
                builder = builder.WithCondition(condition);
            var intermediateModifier = builder.Build();
            _modifiers.AddRange(intermediateModifier.Build(_preParseResult.GlobalSource, Entity.Character));
        }
    }
}