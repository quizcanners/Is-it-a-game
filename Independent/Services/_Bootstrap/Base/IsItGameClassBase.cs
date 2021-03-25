using QuizCanners.Inspect;

namespace QuizCanners.IsItGame
{
    public class IsItGameClassBase 
    {
        private GameController Mgmt => GameController.instance;
        protected Services.ServiceBootsrap GameServices => Mgmt.Services;
        protected EntityPrototypes GamePrototypes => Mgmt.EntityPrototypes;
        protected PersistentGameStateData GameEntities => Mgmt.PersistentProgressData;
        protected StateMachine.Manager GameState => Mgmt.StateMachine;
    }
}
