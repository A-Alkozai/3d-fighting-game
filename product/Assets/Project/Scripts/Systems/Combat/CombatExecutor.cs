using UnityEngine;

// Evaluates a hit: determines outcome (hit/block/whiff/counter), calculates damage and stun, sends result to defender
public class CombatExecutor
{
    // Entry point - called by HitCollisionExecutor when a hitbox overlaps a hurtbox
    public void ProcessHit(HitCollisionData data)
    {
        CombatData combatData = data.Attacker.GetCombatData();
        if (combatData == null)
        {
            Debug.LogWarning("[CombatExecutor] No combat data for attacker's current move");
            return;
        }

        CombatHitboxEntry entry = data.HitboxEntry;
        if (entry == null)
        {
            Debug.LogWarning("[CombatExecutor] No hitbox entry for this hit");
            return;
        }

        // Determine what happened: normal hit, counter hit, blocked, or whiffed
        HitOutcome outcome = Evaluate(combatData, entry, data.Defender);

        // Build the result with appropriate damage/stun values based on outcome
        CombatResult result = BuildResult(outcome, entry);

        Debug.Log($"[CombatExecutor] P{data.Attacker.PlayerId} → P{data.Defender.PlayerId} | " +
                  $"Outcome: {outcome} | Damage: {result.Damage} | Stun: {result.StunFrames}");

        // Send the result to the defender so they take damage/enter stun
        data.Defender.ReceiveCombatResult(result);
    }

    // Check defender's state to determine if the hit lands, is blocked, or whiffs
    private HitOutcome Evaluate(CombatData combatData, CombatHitboxEntry entry, ICollidable defender)
    {
        AttackHeight height = entry.AttackHeight;
        bool isStandGuarding = defender.HasState(PlayerStates.StandGuarding);
        bool isCrouchGuarding = defender.HasState(PlayerStates.CrouchGuarding);
        bool isCrouching = defender.HasState(PlayerStates.Crouching);
        bool isAttacking = defender.HasState(PlayerStates.Attacking);

        // Unblockable attacks always hit
        if (!combatData.Blockable)
        {
            if (isAttacking) return HitOutcome.CounterHit;
            return HitOutcome.NormalHit;
        }

        // High attacks whiff entirely against crouching opponents
        if (height == AttackHeight.High && isCrouching)
        {
            return HitOutcome.Whiff;
        }

        // Check if the defender's guard matches the attack height
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

        // If defender was mid-attack, they get counter hit (extra damage/stun)
        if (isAttacking) return HitOutcome.CounterHit;

        return HitOutcome.NormalHit;
    }

    // Create a CombatResult with damage/stun/knockback values based on the outcome type
    private CombatResult BuildResult(HitOutcome outcome, CombatHitboxEntry entry)
    {
        switch (outcome)
        {
            case HitOutcome.NormalHit:
                return new CombatResult(
                    outcome,
                    entry.Damage,
                    entry.HitStunFrames,
                    entry.Knockback,
                    entry.HitEffect
                );

            case HitOutcome.CounterHit:
                // Counter hits deal more damage and 1.5x stun frames
                return new CombatResult(
                    outcome,
                    entry.CounterHitDamage,
                    (int)(entry.HitStunFrames * 1.5f),
                    entry.Knockback * 1.2f,
                    entry.CounterHitEffect
                );

            case HitOutcome.Blocked:
                // No damage, shorter stun, reduced knockback
                return new CombatResult(
                    outcome,
                    0,
                    entry.BlockStunFrames,
                    entry.Knockback * 0.5f,
                    HitEffect.None
                );

            case HitOutcome.Whiff:
                // Nothing happens - attack missed entirely
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