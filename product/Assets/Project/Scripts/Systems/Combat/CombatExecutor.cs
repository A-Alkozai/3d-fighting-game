using UnityEngine;

public class CombatExecutor
{
    public void ProcessHit(HitCollisionData data)
    {
        CombatData combatData = data.Attacker.GetCombatData();
        if (combatData == null)
        {
            Debug.LogWarning("[CombatExecutor] No combat data for attacker's current move");
            return;
        }

        HitOutcome outcome = Evaluate(combatData, data.Defender);
        CombatResult result = BuildResult(outcome, combatData);

        Debug.Log($"[CombatExecutor] P{data.Attacker.PlayerId} → P{data.Defender.PlayerId} | " +
                  $"Outcome: {outcome} | Damage: {result.Damage} | Stun: {result.StunFrames}");

        data.Defender.ReceiveCombatResult(result);
    }

    private HitOutcome Evaluate(CombatData combatData, ICollidable defender)
    {
        AttackHeight height = combatData.AttackHeight;
        bool isCrouching = defender.HasState(PlayerStates.Crouching);
        bool isGuarding = defender.HasState(PlayerStates.Guarding);
        bool isIdle = defender.HasState(PlayerStates.Idle);
        bool isAttacking = defender.HasState(PlayerStates.Attacking);

        Debug.Log($"[CombatExecutor] Evaluate: height={height}, idle={isIdle}, " +
                $"crouching={isCrouching}, guarding={isGuarding}, attacking={isAttacking}");

        // High attacks whiff over crouching opponents (regardless of guard)
        if (height == AttackHeight.High && isCrouching)
        {
            return HitOutcome.Whiff;
        }

        // Block checks
        if (defender.HasState(PlayerStates.Guarding))
        {
            // Unblockable moves ignore guard entirely
            if (!combatData.Blockable)
            {
                return HitOutcome.NormalHit;
            }

            switch (height)
            {
                case AttackHeight.High:
                    if (!isCrouching) return HitOutcome.Blocked;
                    break;

                case AttackHeight.Mid:
                    if (!isCrouching) return HitOutcome.Blocked;
                    break;

                case AttackHeight.Low:
                    if (isCrouching) return HitOutcome.Blocked;
                    break;

                case AttackHeight.SpecialMid:
                    return HitOutcome.Blocked;
            }

            // If we reach here: wrong guard type
            // Standing guard vs Low → hit goes through
            // Crouch guard vs Mid → hit goes through
            // Falls through to normal/counter hit check below
        }

        // Counter hit: defender was attacking
        if (defender.HasState(PlayerStates.Attacking))
        {
            return HitOutcome.CounterHit;
        }

        return HitOutcome.NormalHit;
    }

    private CombatResult BuildResult(HitOutcome outcome, CombatData combatData)
    {
        switch (outcome)
        {
            case HitOutcome.NormalHit:
                return new CombatResult(
                    outcome,
                    combatData.Damage,
                    combatData.HitStunFrames,
                    combatData.Knockback,
                    combatData.HitEffect
                );

            case HitOutcome.CounterHit:
                return new CombatResult(
                    outcome,
                    combatData.CounterHitDamage,
                    (int)(combatData.HitStunFrames * 1.5f),
                    combatData.Knockback * 1.2f,
                    combatData.CounterHitEffect
                );

            case HitOutcome.Blocked:
                return new CombatResult(
                    outcome,
                    0,
                    combatData.BlockStunFrames,
                    combatData.Knockback * 0.5f,
                    HitEffect.None
                );

            case HitOutcome.Whiff:
                return new CombatResult(
                    outcome,
                    0,
                    0,
                    0f,
                    HitEffect.None
                );

            default:
                return new CombatResult(outcome, 0, 0, 0f, HitEffect.None);
        }
    }
}