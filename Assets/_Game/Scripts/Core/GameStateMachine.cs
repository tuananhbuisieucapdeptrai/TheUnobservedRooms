using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnobservedRooms.Core
{
    public sealed class GameStateMachine : MonoBehaviour
    {
        private static readonly IReadOnlyDictionary<GameFlowState, HashSet<GameFlowState>> Allowed =
            new Dictionary<GameFlowState, HashSet<GameFlowState>>
            {
                [GameFlowState.Boot] = new() { GameFlowState.Generate },
                [GameFlowState.Generate] = new() { GameFlowState.Briefing, GameFlowState.Results },
                [GameFlowState.Briefing] = new() { GameFlowState.Explore, GameFlowState.Paused },
                [GameFlowState.Explore] = new() { GameFlowState.Pursuit, GameFlowState.Exit, GameFlowState.Collapse, GameFlowState.Results, GameFlowState.Paused },
                [GameFlowState.Pursuit] = new() { GameFlowState.Explore, GameFlowState.Exit, GameFlowState.Collapse, GameFlowState.Results, GameFlowState.Paused },
                [GameFlowState.Exit] = new() { GameFlowState.Results },
                [GameFlowState.Collapse] = new() { GameFlowState.Results },
                [GameFlowState.Results] = new() { GameFlowState.Generate },
                [GameFlowState.Paused] = new() { GameFlowState.Briefing, GameFlowState.Explore, GameFlowState.Pursuit }
            };

        public GameFlowState Current { get; private set; } = GameFlowState.Boot;
        public GameFlowState BeforePause { get; private set; } = GameFlowState.Boot;
        public event Action<GameFlowState, GameFlowState> Changed;

        public bool CanEnter(GameFlowState next) => next != Current && Allowed.TryGetValue(Current, out var set) && set.Contains(next);

        public bool TryEnter(GameFlowState next)
        {
            if (!CanEnter(next))
            {
                Debug.LogWarning($"Rejected game-state transition {Current} -> {next}.");
                return false;
            }

            var previous = Current;
            if (next == GameFlowState.Paused) BeforePause = Current;
            Current = next;
            Time.timeScale = next == GameFlowState.Paused ? 0f : 1f;
            Changed?.Invoke(previous, next);
            return true;
        }

        public bool Resume() => Current == GameFlowState.Paused && TryEnter(BeforePause);
    }
}
