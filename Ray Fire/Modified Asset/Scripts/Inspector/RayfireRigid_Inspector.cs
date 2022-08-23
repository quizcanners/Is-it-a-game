using QuizCanners.Inspect;
using QuizCanners.Utils;

namespace RayFire
{

    public partial class RayfireRigid : IPEGI
    {
        public void Inspect()
        {
            "Im {0}".F(nameof(RayfireRigid)).PegiLabel().Write().Nl();

            "Integrity: {0} %".F(AmountIntegrity).PegiLabel().Write().Nl();
        }
    }
}
