using QuizCanners.IsItGame.StateMachine;
using QuizCanners.Utils;
using System;
using UnityEngine;

namespace QuizCanners.IsItGame
{
    public partial class Game
    {
        public partial class Enums
        {

            public enum View
            {
                None = 0,
                LoadingView = 1,
                MainMenu = 2,
                ErrorSorry = 3,
                RayTracingView = 4,
                PlayerNameEdit = 5,
                SelectUser = 6,
                Settings = 7,
                Gyroscope = 8,
                SpaceEffect = 9,
                BackToMainMenuButton = 10,
                SceneLoading = 11,
                IdleGameView = 12,
                Inventory = 13,
            }

            public enum Scene
            {
                None = -1,
                GameScene = 0,
                RayTracing = 1,
                SpaceEffect = 2,
                NodeCommander = 3,
            }

            public enum GameState
            {
                Bootstrap = 0,
                RayTracingScene = 1,
                GamePlay = 2,
                Gyroscope = 3,
                MainMenu = 4,
                SpaceEffect = 5,
            }
        }
    }
    public static class StateMachineExtensions
    {
        private static GameState.MachineManager Machine => Singleton.Get<Singleton_GameController>().StateMachine;

        public static void Enter(this Game.Enums.GameState state)
        {
            var stateType = state.GetStateType();
            if (stateType != null)
            {
                var cast = Activator.CreateInstance(stateType) as GameState.Base;
                if (cast == null)
                {
                    Debug.LogError("Couldn't cast {0} to {1}".F(stateType, nameof(GameState.Base)));
                }
                else
                    Machine.Enter(cast);
            }
        }

        public static void Exit(this Game.Enums.GameState state)
        {
            var stateType = state.GetStateType();
            if (stateType != null)
                Machine.Exit(stateType);
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
                default: Debug.LogError(QcLog.CaseNotImplemented(state, context: "Get State Type")); return null; //"State {0} not linked to a type".F(state)); return null;
            }
        }
    }
}
