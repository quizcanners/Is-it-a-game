using QuizCanners.IsItGame.Develop;
using QuizCanners.IsItGame.StateMachine;
using QuizCanners.Utils;
using System;
using UnityEngine;

namespace QuizCanners.IsItGame
{
    public enum IigEnum_UiView
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
    }

    public enum IigEnum_Scene
    {
        None = -1,
        GameScene = 0,
        RayTracing = 1,
        SpaceEffect = 2,
        NodeCommander = 3,
    }


    public enum IigEnum_GameState
    {
        Bootstrap = 0,
        RayTracingScene = 1,
        GamePlay = 2,
        Gyroscope = 3,
        MainMenu = 4,
        SpaceEffect = 5,
    }

    public static class StateMachineExtensions
    {
        private static Manager Machine => GameController.instance.StateMachine;

        public static void Enter(this IigEnum_GameState state)
        {
            var stateType = state.GetStateType();
            if (stateType != null)
            {
                var cast = Activator.CreateInstance(stateType) as BaseState;
                if (cast == null)
                {
                    Debug.LogError("Couldn't cast {0} to {1}".F(stateType, nameof(BaseState)));
                }
                else
                    Machine.Enter(cast);
            }
        }

        public static void Exit(this IigEnum_GameState state)
        {
            var stateType = state.GetStateType();
            if (stateType != null)
                Machine.Exit(stateType);
        }

        public static Type GetStateType(this IigEnum_GameState state)
        {
            switch (state)
            {
                case IigEnum_GameState.Bootstrap: return typeof(BootstrapState);
                case IigEnum_GameState.MainMenu: return typeof(MainMenuState);
                case IigEnum_GameState.GamePlay: return typeof(GamePlayInitializationState);
                case IigEnum_GameState.RayTracingScene: return typeof(RayTracingSceneState);
                case IigEnum_GameState.Gyroscope: return typeof(GyroscopeState);
                case IigEnum_GameState.SpaceEffect: return typeof(SpaceEffectState);
                default: Debug.LogError(QcLog.CaseNotImplemented(state, context: "Get State Type")); return null; //"State {0} not linked to a type".F(state)); return null;
            }
        }
    }
}
