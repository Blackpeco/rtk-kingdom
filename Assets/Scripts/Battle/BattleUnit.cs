using UnityEngine;

namespace TsOnline
{
    /// <summary>
    /// Runtime combatant. Stats are a mutable copy of <see cref="UnitDefinition.baseStats"/>.
    /// </summary>
    public class BattleUnit : MonoBehaviour
    {
        public UnitDefinition definition;
        public bool isPlayer;
        public UnitStats stats;
        public int currentHp;
        public int currentSp;
        public bool isDefending;
        public ElementStatusKind status;
        public int statusTurnsLeft;
        public SpriteRenderer body;

        public string DisplayName
        {
            get
            {
                if (definition == null)
                    return name;
                if (!string.IsNullOrEmpty(definition.displayNameThai))
                    return definition.displayName + " / " + definition.displayNameThai;
                return definition.displayName;
            }
        }

        public string ShortName
        {
            get { return definition != null ? definition.displayName : name; }
        }

        public ElementType Element
        {
            get { return definition != null ? definition.element : ElementType.None; }
        }

        public bool IsAlive
        {
            get { return currentHp > 0; }
        }

        public int TurnAgi
        {
            get
            {
                int agi = stats.agi;
                if (status == ElementStatusKind.EarthAgiDown)
                    agi = Mathf.RoundToInt(agi * (1f - StatusEffectSystem.EarthAgiPenalty));
                return agi;
            }
        }

        public SkillDefinition[] Skills
        {
            get
            {
                if (definition == null || definition.startingSkills == null)
                    return new SkillDefinition[0];
                return definition.startingSkills;
            }
        }

        public void Init(UnitDefinition def, bool playerSide)
        {
            definition = def;
            isPlayer = playerSide;
            stats = def != null ? def.baseStats : new UnitStats(1, 0, 1, 1, 0, 1);
            currentHp = Mathf.Max(1, stats.hp);
            currentSp = Mathf.Max(0, stats.sp);
            isDefending = false;
            status = ElementStatusKind.None;
            statusTurnsLeft = 0;
            if (def != null)
                name = (playerSide ? "P_" : "E_") + def.displayName;
        }

        public void TakeDamage(int amount)
        {
            if (amount < 0)
                amount = 0;
            currentHp -= amount;
            if (currentHp < 0)
                currentHp = 0;
        }

        public void HealHp(int amount)
        {
            if (amount < 0)
                amount = 0;
            if (status == ElementStatusKind.Burn)
                amount = Mathf.RoundToInt(amount * (1f - StatusEffectSystem.BurnHealPenalty));
            currentHp += amount;
            if (currentHp > stats.hp)
                currentHp = stats.hp;
        }

        public bool TrySpendSp(int cost)
        {
            if (cost <= 0)
                return true;
            if (currentSp < cost)
                return false;
            currentSp -= cost;
            return true;
        }

        /// <summary>
        /// Evaluate Wind skip while the status is still active, then tick durations.
        /// A 1-turn Wind status therefore still produces exactly one skip check.
        /// </summary>
        public bool BeginTurn(System.Action<string> log)
        {
            bool skip = RollSkipTurn();
            TickStatusAtTurnStart(log);
            return skip;
        }

        public void TickStatusAtTurnStart(System.Action<string> log)
        {
            if (status == ElementStatusKind.None)
                return;

            if (status == ElementStatusKind.Burn && IsAlive)
            {
                int burn = Mathf.Max(1, Mathf.RoundToInt(stats.hp * StatusEffectSystem.BurnHpPercentPerTurn));
                TakeDamage(burn);
                if (log != null)
                    log(ShortName + " ถูกเผาไหม้ -" + burn + " HP");
            }

            statusTurnsLeft--;
            if (statusTurnsLeft <= 0)
            {
                if (log != null)
                    log(ShortName + " สถานะธาตุหมดลง");
                status = ElementStatusKind.None;
                statusTurnsLeft = 0;
            }
        }

        public bool RollSkipTurn()
        {
            return StatusTurnFlow.ShouldSkipTurn(status, null);
        }

        [ContextMenu("QA: Force Wind skip (1 turn)")]
        public void DebugForceWindSkip()
        {
            ApplyStatus(ElementStatusKind.WindSkip, 1);
            StatusTurnFlow.ForceNextSkipRoll = 0f;
        }

        public void ApplyStatus(ElementStatusKind kind, int turns)
        {
            status = kind;
            statusTurnsLeft = turns;
        }

        public Color ElementColor()
        {
            switch (Element)
            {
                case ElementType.Earth: return new Color(0.72f, 0.53f, 0.24f);
                case ElementType.Water: return new Color(0.22f, 0.52f, 0.82f);
                case ElementType.Fire: return new Color(0.86f, 0.24f, 0.18f);
                case ElementType.Wind: return new Color(0.40f, 0.82f, 0.55f);
                default: return Color.gray;
            }
        }
    }
}
