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
                PauseMenu = 13,
            }

            public enum Scene
            {
                None = -1,
                GameScene = 0,
                RayTracing = 1,
                SpaceEffect = 2,
                NodeCommander = 3,
                MainMenu = 4,
                Terrain = 5,
            }

            public enum GameState
            {
                Bootstrap = 0,
                RayTracingScene = 1,
                GamePlay = 2,
                Gyroscope = 3,
                MainMenu = 4,
                SpaceEffect = 5,
                Paused = 6,
            }

            public enum SoundEffects
            {
                None = 0,
                Click = 1,
                PressDown = 2,
                MouseLeave = 3,
                Tab = 4,
                Coins = 5,
                Process = 6,
                ProcessFinal = 7,
                Ice = 8,
                Scratch = 9,
                ItemPurchase = 10,
                MouseEnter = 11,
                MouseExit = 12,
                HoldElement = 13,
                Shot = 14,
                DefaultSurfaceImpact = 15,
                BodyImpact = 16,
                BodyExplosion = 17,
                BlotDrop = 18,
                ArmorImpact = 19,
                BulletFlyBy = 20,
                Explosion = 21,
                Explosion_Gory = 22,
                Explosion_Near = 23,
            }

            public enum Music
            {
                None = 0,
                MainMenu = 1,
                Loading = 2,
                Combat = 3,
                Reward = 4,
                Exploration = 5,
                PauseMenu = 6,
            }

            public enum PhisicalSimulation 
            {
                Active,
                Paused
            }
        }
    }

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
