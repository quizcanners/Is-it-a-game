using QuizCanners.Inspect;
using QuizCanners.Utils;

namespace QuizCanners.IsItGame.Develop
{
    public class View_SettingsWindow : IsItGameBehaviourBase, IPEGI
    {
        public void Inspect()
        {
            pegi.Nl();

            var s = Singleton.Try<Singleton_WorldSounds>(s =>
            {
                var snd = s.WantSound;
                if ("Sound".PegiLabel().ToggleIcon(ref snd).Nl())
                    Singleton.Get<Singleton_WorldSounds>().WantSound = s;
            }, onFailed: ()=> "No {0}".F(nameof(Singleton_WorldSounds)).PegiLabel().Write_Hint() , logOnServiceMissing: false);
        }
    }

    [PEGI_Inspector_Override(typeof(View_SettingsWindow))]
    internal class SettingsWindowVIewDrawer : PEGI_Inspector_Override { }
}