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

            public enum UiSoundEffects
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
                BoltGun_Shoot = 24,
            }

            public enum WorldSoundEffects
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
}
