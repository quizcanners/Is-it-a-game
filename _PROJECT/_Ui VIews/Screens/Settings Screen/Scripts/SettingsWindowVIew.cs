using QuizCanners.Inspect;
using QuizCanners.Utils;

namespace QuizCanners.IsItGame.Develop
{
    public class SettingsWindowVIew : IsItGameBehaviourBase, IPEGI
    {
        public void Inspect()
        {
            pegi.nl();

            var s = Singleton.Try<SoundsService>(s =>
            {
                var snd = s.WantSound;
                if ("Sound".PegiLabel().toggleIcon(ref snd).nl())
                    Singleton.Get<SoundsService>().WantSound = s;
            }, onFailed: ()=> "No {0}".F(nameof(SoundsService)).PegiLabel().writeHint() , logOnServiceMissing: false);
        }
    }

    [PEGI_Inspector_Override(typeof(SettingsWindowVIew))]
    internal class SettingsWindowVIewDrawer : PEGI_Inspector_Override { }
}