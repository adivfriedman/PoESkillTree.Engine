﻿using System.Text.RegularExpressions;

namespace PoESkillTree.Engine.Computation.Parsing.SkillParsers
{
    public static class SkillStatIds
    {
        public const string IsAreaDamage = "is_area_damage";
        public const string IsAlwaysProjectile = "base_is_projectile";
        public const string SkillDotIsAreaDamage = "skill_dot_is_area_damage";
        public const string DealsSecondaryDamage = "display_skill_deals_secondary_damage";

        private const string DamageTypeRegex = "(physical|cold|fire|lightning|chaos)";

        public static readonly Regex HitDamageRegex =
            new Regex($"^(attack|spell|secondary)_(minimum|maximum)_base_{DamageTypeRegex}_damage$");

        public static readonly Regex DamageOverTimeRegex =
            new Regex($"^base_{DamageTypeRegex}_damage_to_deal_per_minute$");

        public static readonly Regex SkillDamageConversionRegex =
            new Regex($"^skill_{DamageTypeRegex}_damage_%_to_convert_to_{DamageTypeRegex}$");
    }
}