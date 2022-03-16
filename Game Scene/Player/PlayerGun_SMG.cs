using PainterTool;
using QuizCanners.Inspect;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuizCanners.IsItGame
{
    [Serializable]
    public class PlayerGun_SMG : IPEGI
    {
        public PlaytimePainter_BrushConfigScriptableObject brushConfig;

        [SerializeField] private pegi.EnterExitContext context = new();
        public void Inspect()
        {
            using (context.StartContext())
            {
                "Brush Config".PegiLabel().Edit_Enter_Inspect(ref brushConfig).Nl();
            }
        }
    }
}
