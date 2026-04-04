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

        HitOutcome outcome = Evaluate(combatData, data.HitboxEntry, data.Defender);
        CombatResult result = BuildResult(outcome, combatData);

        Debug.Log($"[CombatExecutor] P{data.Attacker.PlayerId} → P{data.Defender.PlayerId} | " +
                $"Outcome: {outcome} | Damage: {result.Damage} | Stun: {result.StunFrames}");

        data.Defender.ReceiveCombatResult(result);
    }

    private HitOutcome Evaluate(CombatData combatData, CombatHitboxEntry hitboxEntry, ICollidable defender)
    {
        AttackHeight height = hitboxEntry.AttackHeight;
        bool isStandGuarding = defender.HasState(PlayerStates.StandGuarding);
        bool isCrouchGuarding = defender.HasState(PlayerStates.CrouchGuarding);
        bool isCrouching = defender.HasState(PlayerStates.Crouching);
        bool isAttacking = defender.HasState(PlayerStates.Attacking);

        if (!combatData.Blockable)
        {
            if (isAttacking) return HitOutcome.CounterHit;
            return HitOutcome.NormalHit;
        }

        if (height == AttackHeight.High && isCrouching)
        {
            return HitOutcome.Whiff;
        }

        switch (height)
        {
            case AttackHeight.High:
                if (isStandGuarding) return HitOutcome.Blocked;
                break;

            case AttackHeight.Mid:
                if (isStandGuarding) return HitOutcome.Blocked;
                break;

            case AttackHeight.Low:
                if (isCrouchGuarding) return HitOutcome.Blocked;
                break;

            case AttackHeight.SpecialMid:
                if (isStandGuarding || isCrouchGuarding) return HitOutcome.Blocked;
                break;
        }

        if (isAttacking) return HitOutcome.CounterHit;

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