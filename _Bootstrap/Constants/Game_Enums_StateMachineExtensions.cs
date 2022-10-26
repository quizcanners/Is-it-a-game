using QuizCanners.IsItGame.StateMachine;
using QuizCanners.Utils;
using System;
using UnityEngine;

namespace QuizCanners.IsItGame
{
    public static class StateMachineExtensions
    {
        public static void Enter(this Game.Enums.GameState state)
        {
            var stateType = state.GetStateType();
            if (stateType != null)
            {
                if (Activator.CreateInstance(stateType) is not GameState.Base cast)
                {
                    Debug.LogError("Couldn't cast {0} to {1}".F(stateType, nameof(GameState.Base)));
                }
                else
                    GameState.Machine.Enter(cast);
            }
        }

        public static void Exit(this Game.Enums.GameState state)
        {
            var stateType = state.GetStateType();
            if (stateType != null)
                GameState.Machine.Exit(stateType);
        }

        public static Type GetStateType(this Game.Enums.GameState state)
        {
            switch (state)
            {
                case Game.Enums.GameState.Bootstrap: return typeof(GameState.Bootstrap);
                case Game.Enums.GameState.MainMenu: return typeof(GameState.MainMenu);
                case Game.Enums.GameState.GamePlay: return typeof(GameState.LoadGameScene);
                case Game.Enums.GameState.RayTracingScene: return typeof(GameState.RayTracingScene);
                case Game.Enums.GameState.Gyroscope: return typeof(GameState.Gyroscope);
                case Game.Enums.GameState.SpaceEffect: return typeof(GameState.SpaceEffect);
                case Game.Enums.GameState.Paused: return typeof(GameState.Paused);
                default: Debug.LogError(QcLog.CaseNotImplemented(state, context: "Get State Type")); return null; //"State {0} not linked to a type".F(state)); return null;
            }
        }
    }
}