#if UNITY_EDITOR
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

public class ReportSectionInputBufferTests
{
    [Test]
    public void IB_01_AddInputThenSingleFrameUpdate_IncrementsStoredInputAgeByExactlyOne()
    {
        InputBuffer buffer = new InputBuffer();
        InputObject input = new InputObject(InputCommand.LeftPunch, Key.I);

        buffer.AddInput(input);
        buffer.UpdateFrameCounter();

        Assert.That(buffer.GetInputAt(0), Is.SameAs(input));
        Assert.That(input.GetFrame().GetFrameNumber(), Is.EqualTo(1));
    }

    [Test]
    public void IB_02_AttackInputAtExpiryThreshold_IsRemovedByExpiredInputSweep()
    {
        InputBuffer buffer = new InputBuffer();
        InputObject attackInput = new InputObject(InputCommand.RightPunch, Key.O);

        buffer.AddInput(attackInput);

        for (int i = 0; i < 8; i++)
        {
            buffer.UpdateFrameCounter();
        }

        buffer.RemoveExpiredInputs();

        Assert.That(buffer.Count(), Is.EqualTo(0));
        Assert.That(buffer.Contains(attackInput), Is.False);
    }

    [Test]
    public void IB_03_RemoveSpecificInputObject_LeavesUnrelatedEntriesIntact()
    {
        InputBuffer buffer = new InputBuffer();
        InputObject matchingInput = new InputObject(InputCommand.LeftKick, Key.K);
        InputObject unrelatedInput = new InputObject(InputCommand.RightKick, Key.L);

        buffer.AddInput(matchingInput);
        buffer.AddInput(unrelatedInput);

        buffer.Remove(matchingInput);

        Assert.That(buffer.Count(), Is.EqualTo(1));
        Assert.That(buffer.Contains(matchingInput), Is.False);
        Assert.That(buffer.Contains(unrelatedInput), Is.True);
        Assert.That(buffer.GetInputAt(0).GetInputCommand(), Is.EqualTo(InputCommand.RightKick));
    }
}

public class ReportSectionFrameCounterTests
{
    [Test]
    public void FC_01_GetFrameNumberReturnsZeroImmediatelyAfterConstruction()
    {
        FrameCounter counter = new FrameCounter();

        Assert.That(counter.GetFrameNumber(), Is.EqualTo(0));
    }

    [Test]
    public void FC_02_UpdateFrameCalledNTimes_MakesGetFrameNumberReturnN()
    {
        FrameCounter counter = new FrameCounter();
        int expectedFrames = 5;

        for (int i = 0; i < expectedFrames; i++)
        {
            counter.UpdateFrame();
        }

        Assert.That(counter.GetFrameNumber(), Is.EqualTo(expectedFrames));
    }

    [Test]
    public void FC_03_ResetFrameReturnsCounterToZeroRegardlessOfPriorState()
    {
        FrameCounter counter = new FrameCounter();

        for (int i = 0; i < 3; i++)
        {
            counter.UpdateFrame();
        }

        counter.ResetFrame();

        Assert.That(counter.GetFrameNumber(), Is.EqualTo(0));
    }
}

public class ReportSectionStateManagerTests
{
    [Test]
    public void SM_01_CanToggleStateReturnsFalseAndBitmaskRemainsUnchangedWhenBlockingStateIsActive()
    {
        StateManager stateManager = new StateManager();
        stateManager.AddState(PlayerStates.Attacking);

        PlayerStates before = CodebaseTestHelpers.GetField<PlayerStates>(stateManager, "playerStates");
        bool canEnterWalking = stateManager.CanToggleState(PlayerStates.Walking);
        PlayerStates after = CodebaseTestHelpers.GetField<PlayerStates>(stateManager, "playerStates");

        Assert.That(canEnterWalking, Is.False);
        Assert.That(after, Is.EqualTo(before));
    }

    [Test]
    public void SM_02_EnterStateClearsOverrideStatesBeforeSettingNewState()
    {
        StateManager stateManager = new StateManager();
        stateManager.AddState(PlayerStates.Idle);
        stateManager.AddState(PlayerStates.Walking);

        stateManager.EnterState(PlayerStates.Dashing);

        PlayerStates bitmask = CodebaseTestHelpers.GetField<PlayerStates>(stateManager, "playerStates");

        Assert.That(bitmask.HasFlag(PlayerStates.Dashing), Is.True);
        Assert.That(bitmask.HasFlag(PlayerStates.Idle), Is.False);
        Assert.That(bitmask.HasFlag(PlayerStates.Walking), Is.False);
    }

    [Test]
    public void SM_03_ExitStateAppliesConfiguredExitStateAfterDeparture()
    {
        StateManager stateManager = new StateManager();
        stateManager.AddState(PlayerStates.Idle);
        stateManager.EnterState(PlayerStates.Crouching);

        stateManager.ExitState(PlayerStates.Crouching);

        PlayerStates bitmask = CodebaseTestHelpers.GetField<PlayerStates>(stateManager, "playerStates");

        Assert.That(bitmask.HasFlag(PlayerStates.Crouching), Is.False);
        Assert.That(bitmask.HasFlag(PlayerStates.Rising), Is.True);
    }
}

public class ReportSectionCombatExecutorTests
{
    private class CombatExecutorTestCollidable : ICollidable
    {
        private readonly int playerId;
        private readonly CombatData combatData;
        private readonly PlayerStates activeStates;

        public CombatExecutorTestCollidable(int playerId, CombatData combatData, PlayerStates activeStates = PlayerStates.None)
        {
            this.playerId = playerId;
            this.combatData = combatData;
            this.activeStates = activeStates;
        }

        public CombatResult LastResult { get; private set; }
        public int PlayerId => playerId;

        public List<CollisionBox> GetActiveHitboxes()
        {
            return new List<CollisionBox>();
        }

        public IEnumerable<CollisionBox> GetAllHurtboxes()
        {
            return new List<CollisionBox>();
        }

        public CollisionBox GetCollisionBox(string id)
        {
            return null;
        }

        public BodyCollider GetBodyCollider()
        {
            return null;
        }

        public Transform GetTransform()
        {
            return null;
        }

        public CombatData GetCombatData()
        {
            return combatData;
        }

        public CombatHitboxEntry GetActiveHitboxEntry(string hitboxId)
        {
            return null;
        }

        public bool HasState(PlayerStates state)
        {
            return (activeStates & state) != 0;
        }

        public void ReceiveCombatResult(CombatResult result)
        {
            LastResult = result;
        }

        public string GetCurrentMoveId()
        {
            return null;
        }
    }

    [Test]
    public void CE_01_NonGuardingDefender_ProducesNormalHitWithCorrectDamage()
    {
        CombatExecutor executor = new CombatExecutor();
        CombatHitboxEntry entry = CodebaseTestHelpers.MakeCombatHitboxEntry(attackHeight: "Mid", damage: 14, counterHitDamage: 21);
        CombatData combatData = CodebaseTestHelpers.MakeCombatData("jab", true, new List<CombatHitboxEntry> { entry });
        CombatExecutorTestCollidable attacker = new CombatExecutorTestCollidable(1, combatData);
        CombatExecutorTestCollidable defender = new CombatExecutorTestCollidable(2, null);
        HitCollisionData collision = new HitCollisionData(attacker, defender, null, null, entry);

        executor.ProcessHit(collision);

        Assert.That(defender.LastResult, Is.Not.Null);
        Assert.That(defender.LastResult.Outcome, Is.EqualTo(HitOutcome.NormalHit));
        Assert.That(defender.LastResult.Damage, Is.EqualTo(14));
    }

    [Test]
    public void CE_02_CorrectGuardCoverage_ProducesBlockedOutcome()
    {
        CombatExecutor executor = new CombatExecutor();
        CombatHitboxEntry entry = CodebaseTestHelpers.MakeCombatHitboxEntry(attackHeight: "Mid", damage: 18, counterHitDamage: 24);
        CombatData combatData = CodebaseTestHelpers.MakeCombatData("mid-strike", true, new List<CombatHitboxEntry> { entry });
        CombatExecutorTestCollidable attacker = new CombatExecutorTestCollidable(1, combatData);
        CombatExecutorTestCollidable defender = new CombatExecutorTestCollidable(2, null, PlayerStates.StandGuarding);
        HitCollisionData collision = new HitCollisionData(attacker, defender, null, null, entry);

        executor.ProcessHit(collision);

        Assert.That(defender.LastResult, Is.Not.Null);
        Assert.That(defender.LastResult.Outcome, Is.EqualTo(HitOutcome.Blocked));
        Assert.That(defender.LastResult.Damage, Is.EqualTo(0));
    }

    [Test]
    public void CE_03_DefenderMarkedAsAttacking_ProducesCounterHit()
    {
        CombatExecutor executor = new CombatExecutor();
        CombatHitboxEntry entry = CodebaseTestHelpers.MakeCombatHitboxEntry(attackHeight: "Mid", damage: 10, counterHitDamage: 16);
        CombatData combatData = CodebaseTestHelpers.MakeCombatData("counter-test", true, new List<CombatHitboxEntry> { entry });
        CombatExecutorTestCollidable attacker = new CombatExecutorTestCollidable(1, combatData);
        CombatExecutorTestCollidable defender = new CombatExecutorTestCollidable(2, null, PlayerStates.Attacking);
        HitCollisionData collision = new HitCollisionData(attacker, defender, null, null, entry);

        executor.ProcessHit(collision);

        Assert.That(defender.LastResult, Is.Not.Null);
        Assert.That(defender.LastResult.Outcome, Is.EqualTo(HitOutcome.CounterHit));
        Assert.That(defender.LastResult.Damage, Is.EqualTo(16));
    }
}

public class ReportSectionRuntimeInitialisationTests
{
    [Test]
    public void MD_01_InitialiseTotalFrames_ComputesExpectedIntegerFromClipLengthAndSpeed()
    {
        AnimationExecutor executor = CodebaseTestHelpers.CreateAnimationExecutorWithClip("timing-test", 0.5f);
        AnimationData data = CodebaseTestHelpers.MakeAnimationData("timing-test", "timing-test", false, 2f);

        try
        {
            data.InitialiseTotalFrames(executor);

            Assert.That(data.TotalFrames, Is.EqualTo(15));
        }
        finally
        {
            Object.DestroyImmediate(executor.gameObject);
        }
    }

    [Test]
    public void MD_02_InitialiseObjects_ParsesKnownInputCommandStringIntoEnum()
    {
        MoveData moveData = CodebaseTestHelpers.CreateInstance<MoveData>(new Dictionary<string, object>
        {
            ["id"] = "single-jab",
            ["moveName"] = "single-jab",
            ["description"] = "single-jab description",
            ["moveType"] = "attack",
            ["isLoop"] = false,
            ["inputDelay"] = 0,
            ["branchDelay"] = 0,
            ["inputSequence"] = new List<string> { "LeftPunch" },
            ["requiredStates"] = new List<string> { "Idle" }
        });

        moveData.InitialiseObjects();

        Assert.That(moveData.InputSequence.Count, Is.EqualTo(1));
        Assert.That(moveData.InputSequence[0], Is.EqualTo(InputCommand.LeftPunch));
    }
}
#endif
