using QuizCanners.Inspect;
using QuizCanners.IsItGame.UI;
using QuizCanners.Utils;
using UnityEngine;

namespace QuizCanners.IsItGame
{
    [ExecuteAlways]
    public class GameController : MonoBehaviour, IPEGI
    {
        public const string PROJECT_NAME = "Is It A Game";

        public static GameController instance;

        public StateMachine.Manager StateMachine = new();
        public Services.ServiceBootsrap Services = new();

        [Header("Scriptable Objects")]
        public EntityPrototypes EntityPrototypes;
        public PersistentGameStateData PersistentProgressData;

        #region Inspector

        private readonly pegi.EnterExitContext _context = new();
        public void Inspect()
        {
            using (_context.StartContext())
            {
                if (!_context.IsAnyEntered)
                    "GAME CONTROLLER".PegiLabel(pegi.Styles.ListLabel).write();

                pegi.nl();

                "Services".PegiLabel().enter_Inspect(Services).nl();  // Game Flow & Data independent logic

                if (Application.isPlaying)
                    "State Machine".PegiLabel().enter_Inspect(StateMachine).nl();  // Game Flow logic.

                "Persistable Progress Data".PegiLabel().edit_enter_Inspect(ref PersistentProgressData).nl();  // Game Data that changes from run to run
                "Prototypes".PegiLabel().edit_enter_Inspect(ref EntityPrototypes).nl();  // Game Data that stays persistent

                "Utils".PegiLabel().isEntered().nl().If_Entered(QcUtils.InspectAllUtils);

                if (_context.IsAnyEntered == false && Application.isPlaying)
                    Singleton.Try<UiViewService>(s => s.InspectCurrentView(),
                        onFailed: () => "No {0} found".F(nameof(UiViewService)));
            }
        }
        #endregion

        void Update()
        {
            StateMachine.ManagedUpdate();

            if (Input.GetKey(KeyCode.Escape))
                Application.Quit();
        }

        void LateUpdate() => StateMachine.ManagedLateUpdate();

        void OnEnable()
        {
            instance = this;
            StateMachine.ManagedOnEnable();

            if (PersistentProgressData)
                PersistentProgressData.Load();
        }

        void OnDisable()
        {
            StateMachine.ManagedOnDisable();
            if (PersistentProgressData)
                PersistentProgressData.Save();
        }

        void Awake() 
        {
            QcDebug.ForceDebugOption(); // To have inspector without building IsDebug
            QcLog.LogHandler.SavingLogs = true;
        }
    }

    [PEGI_Inspector_Override(typeof(GameController))] internal class GameManagerDrawer : PEGI_Inspector_Override { }
}