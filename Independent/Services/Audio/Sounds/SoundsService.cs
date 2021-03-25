using QuizCanners.Inspect;
using UnityEngine;
using System.Collections.Generic;
using QuizCanners.Utils;

namespace QuizCanners.IsItGame {

    [ExecuteAlways]
    public class SoundsService : IsItGameServiceBase, INeedAttention
    {
        public AudioSource source;

        public EnumeratedSounds Sounds;

        private readonly Dictionary<IigEnum_SoundEffects, Gate.Double> _playTime = new();

        private readonly List<SoundRequest> _requests = new();

        private readonly PlayerPrefValue.Bool _wantSounds = new("WantSounds", defaultValue: true);
        private readonly PlayerPrefValue.Float _soundEffectsVolume = new("SoundsVolume", defaultValue: 1f);

        public bool WantSound 
        {
            get => _wantSounds.GetValue();
            set 
            {
                _wantSounds.SetValue(value);
                SetDirty();
            }
        }

        private struct SoundRequest 
        {
            public IigEnum_SoundEffects Effect;
            public double DspTime;
            public float VolumeScale;

            public bool IsTimeToPlay => (DspTime - AudioSettings.dspTime) < 0.02f;
        }

        public void Play(AudioClip clip, float clipVolume = 1) => source.PlayOneShot(clip, volumeScale: clipVolume * _soundEffectsVolume.GetValue());
        

        public void Play(IigEnum_SoundEffects eff, float minGap, float clipVolume) 
        {
            var lastPlayed = _playTime.GetOrCreate(eff);

            if (lastPlayed.TryChange(Time.realtimeSinceStartup, changeTreshold: minGap)) 
            {
                if (!Sounds) 
                {
                    Debug.LogError(QcLog.IsNull(Sounds, context: "Play")); 
                }

                var ass = Sounds.Get(eff);

                if (ass)
                {
                    Play(ass, clipVolume);
                }
            }
        }

        public void PlayDelaed(IigEnum_SoundEffects eff, float delay, float clipVolume = 1)
        {
            var req = new SoundRequest()
            {
                Effect = eff,
                DspTime = AudioSettings.dspTime + delay,
                VolumeScale = clipVolume,
            };

            _requests.Add(req);
        }

        public void Reset()
        {
            source = GetComponent<AudioSource>();
            if (!source)
            {
                source = gameObject.AddComponent<AudioSource>();
            }
        }

        public void LateUpdate()
        {
            if (_requests.Count > 0) 
            {
                for (int i=_requests.Count-1; i>=0; i--) 
                {
                    var req = _requests[i];
                    if (req.IsTimeToPlay) 
                    {
                        _requests.RemoveAt(i);
                        req.Effect.Play(clipVolume: req.VolumeScale);
                    }
                }
            }
        }

        #region Inspector

        public override string InspectedCategory => Utils.Singleton.Categories.SCENE_MGMT;

        private int _inspectedStuff = -1;

        private IigEnum_SoundEffects _debugSound;
        public override void Inspect()
        {
            base.Inspect();

            if (!source) 
            {
                "No Audio Source".PegiLabel().writeWarning().nl();
                "Find or Add".PegiLabel().Click().nl().OnChanged(Reset);
            }

            if (_inspectedStuff == -1)
            {
                _soundEffectsVolume.Nested_Inspect();

                if (Application.isPlaying)
                {
                    if ("Sound".PegiLabel().editEnum(ref _debugSound) | icon.Play.Click().nl())
                        _debugSound.Play();
                }
                else
                {
                    pegi.nl();
                    "Can test in Play Mode only".PegiLabel().writeHint();
                }
            }

            "Sounds".PegiLabel().edit_enter_Inspect(ref Sounds, ref  _inspectedStuff, 0).nl();
        }

        public override string NeedAttention()
        {
            if (!Sounds)
                return "No Sounds Scriptable Object";

            return base.NeedAttention();
        }
        #endregion

    }

    public enum IigEnum_SoundEffects
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
    }

    public static class SoundEffectsExtension 
    {
        public static void Play(this IigEnum_SoundEffects eff, float minGap = 0.04f, float clipVolume = 1)
            => Singleton.Try<SoundsService>(serv => serv.Play(eff, minGap: minGap, clipVolume: clipVolume));

    }

    [PEGI_Inspector_Override(typeof(SoundsService))] internal class AudioControllerDrawer : PEGI_Inspector_Override { }

}
