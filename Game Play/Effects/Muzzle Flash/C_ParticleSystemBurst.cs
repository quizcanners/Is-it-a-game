using QuizCanners.Inspect;
using QuizCanners.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace QuizCanners.IsItGame.Develop
{
    public class C_ParticleSystemBurst : MonoBehaviour, IPEGI
    {
        [SerializeField] private List<SystemConfig> _systems;


        [Serializable]
        private class SystemConfig : IPEGI_ListInspect, IPEGI
        {
            public int count = 1;
            public ParticleSystem _system;

            public void Emit() 
            {
                if (_system)
                    _system.Emit(count);
            }

            #region Inspector

            public override string ToString() => "{0} {1}".F(_system ? _system.gameObject.name : "NULL", count > 1 ? "x{0}".F(count) : "");

            public void Inspect()
            {
                "Count".PegiLabel().Edit(ref count).Nl();
            }

            public void InspectInList(ref int edited, int index)
            {
                if (!_system) 
                {
                    "System".PegiLabel().Edit(ref _system);
                    return;
                }

                if (Application.isPlaying && Icon.Play.Click())
                    Emit();

                if (_system.emission.enabled)
                {
                    "System {0} has Emission enabled".PegiLabel().WriteWarning().Nl();

                    if ("Stop Emitter".PegiLabel().Click())
                    {
                        var em = _system.emission;
                        em.enabled = false;
                    }
                }

                ToString().PegiLabel(style: pegi.Styles.BaldText).Write();

                if (Icon.Enter.Click()) 
                    edited = index;

                pegi.ClickHighlight(_system);
                
            }
            #endregion

            public SystemConfig(ParticleSystem system) 
            {
                _system = system;
                if (system.emission.burstCount > 0)
                {
                    count = (int)system.emission.GetBurst(0).count.constant;
                }
                else
                    count = 1;
            }
        }


        public void Emit() 
        {
            foreach (var s in _systems) 
            {
                s.Emit();
            }
        }

        pegi.CollectionInspectorMeta _collection = new("Particle Systems");

        public void Inspect()
        {
            "Emit".PegiLabel().Click().Nl().OnChanged(Emit);

            if ("Refresh Particle Systems".PegiLabel().ClickConfirm(confirmationTag: "ClPs").Nl()) 
            {
                _systems.Clear();
                var cmps = GetComponentsInChildren<ParticleSystem>();

                foreach (var c in cmps)
                {
                    _systems.Add(new SystemConfig(c));
                }
            }

            _collection.Edit_List(_systems).Nl();

            
        }
    }

    [PEGI_Inspector_Override(typeof(C_ParticleSystemBurst))]
    internal class C_MuzzleFlashDrawer : PEGI_Inspector_Override { }
}