using PainterTool;
using QuizCanners.Inspect;
using QuizCanners.Utils;
using UnityEngine;

namespace QuizCanners.IsItGame.Develop
{

    [CreateAssetMenu(fileName = FILE_NAME, menuName = QcUnity.SO_CREATE_MENU + Singleton_GameController.PROJECT_NAME + "/Config/" + FILE_NAME)]
    public class SO_ChornobaivkaSettings : ScriptableObject, IPEGI
    {
        public const string FILE_NAME = "Chornobaivka Settings";

        [SerializeField] public PlaytimePainter_BrushConfigScriptableObject DismembermentBrush;
        [SerializeField] public PlaytimePainter_BrushConfigScriptableObject GibletsSplatterBrush;
        [SerializeField] private Color _bloodColor = new Color(0.5f, 0.1f, 0.1f, 1);
        private readonly ShaderProperty.ColorFloat4Value BLOOD_COLOR = new("_qc_BloodColor", new Color(0.5f, 0.1f, 0.1f, 1));




        public Color BloodColor 
        {
            get => _bloodColor;
            set 
            {
                _bloodColor = value;
                BLOOD_COLOR.GlobalValue = value;
            }
        }

        public void UpdateShaderParameters() 
        {
            BloodColor = _bloodColor;
        }


        private pegi.EnterExitContext _context = new();

        public void Inspect()
        {
            var changes = pegi.ChangeTrackStart();

            using (_context.StartContext())
            {
                "Dismemberment Brush".PegiLabel().Edit_Enter_Inspect(ref DismembermentBrush).Nl();
                "Giblets Splatter Brush".PegiLabel().Edit_Enter_Inspect(ref GibletsSplatterBrush).Nl();

                if (_context.IsAnyEntered == false)
                {
                    "_qc_BloodColor".PegiLabel().Edit(ref _bloodColor).Nl();
                }
            }

            if (changes)
            {
                UpdateShaderParameters();
            }
        }
    }
}