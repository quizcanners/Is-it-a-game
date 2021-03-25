using QuizCanners.Inspect;
using System.Collections.Generic;
using UnityEngine;
using System;
using QuizCanners.Utils;
using QuizCanners.Lerp;

namespace QuizCanners.IsItGame
{
    [ExecuteAlways]
    public class MusicService : IsItGameServiceBase, INeedAttention
    {
        public EnumeratedMusic Music;
        public AudioSource sourceA;
        public AudioSource sourceB;
     
        [NonSerialized] private AudioClip _targetClip;
        [NonSerialized] private float _startTargetClipAt;
        [NonSerialized] private bool _playSilence;
        [NonSerialized] private float _transitionProgress;
        [NonSerialized] private float _currentVolumeScale = 1;
        [NonSerialized] private bool _activeIsA;
        [NonSerialized] private int _latestRequestVersion;
        [NonSerialized] private bool _applicationPaused;
        [NonSerialized] private IigEnum_Music currentlyPlaying = IigEnum_Music.None;
        [NonSerialized] private float _targetVolumeScale = 1;

        private readonly PlayerPrefValue.Bool _musicIsOn = new("WantMusic", defaultValue: true);
        private readonly PlayerPrefValue.Float _musicVolume = new("MusicVolume", defaultValue: 0.5f);

        private readonly Dictionary<string, float> _perSongPlaybeckResume = new();

        public float Volume
        {
            get => _musicIsOn.GetValue() ? _musicVolume.GetValue() : 0;
            set 
            {
                _musicVolume.SetValue(value);
                SetDirty();
            }
        }
        public void Play(IigEnum_Music music, bool skipTransition = false)
        {
            currentlyPlaying = music;
            Play(Music.Get(music), skipTransition: skipTransition);
        }

        private void Play(MusicClipData data, bool skipTransition = false) 
        {
            _latestRequestVersion += 1;
            int thisRequest = _latestRequestVersion;
            _playSilence = true;

            if (data == null) 
            {
                Debug.LogError("Clip Data is null");
                return;
            }

            StartCoroutine(data.GetClipAsync(onComplete: clip => 
            {
                if (_latestRequestVersion > thisRequest)
                {
                    return;
                }

                _targetVolumeScale = data.Volume;

                float startAt = (data.AlwaysStartFromBeginning || !clip) ? 0 : _perSongPlaybeckResume.TryGet(clip.name);

                Play(clip, skipTransition: skipTransition, startAt: startAt);
            }));
        }

        private void Play(AudioClip clip, bool skipTransition = false, float startAt = 0)
        {
            _latestRequestVersion += 1;
            _targetClip = clip;
            _startTargetClipAt = startAt;
            _playSilence = !clip;
            if (skipTransition)
            {
                Flip();
                _transitionProgress = 1;
            }
        }

        public AudioSource ActiveSource => _activeIsA ? sourceA : sourceB;
        public AudioSource FadingSource => _activeIsA ? sourceB : sourceA;

        private void Flip()
        {
            // Save Where we stopped playing
            var curClip = ActiveSource.clip;

            if (curClip) 
            {
                _perSongPlaybeckResume[curClip.name] = ActiveSource.time;
            }

            // Flip Sources
            _activeIsA = !_activeIsA;
            _transitionProgress = 1 - _transitionProgress;

            // Set Up new Active Source
            ActiveSource.clip = _targetClip;

            if (_targetClip)
            {
                ActiveSource.time = _startTargetClipAt;
                ActiveSource.loop = true;
                if (_targetClip.loadState != AudioDataLoadState.Loaded)
                {
                    ActiveSource.Pause();
                    ActiveSource.PlayScheduled(AudioSettings.dspTime + 3);
                }
                else
                {
                    ActiveSource.Play();
                }
            }
           
            // Clear to avoid Repeat checks
            _targetClip = null;
        }

        private void OnApplicationPause(bool pause)
        {
            _applicationPaused = pause;
        }

        private void Update()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (TryEnterIfStateChanged())
            {
                IigEnum_Music song = currentlyPlaying;
                if (GameStateMachine.TryChangeFallback(ref song, fallbackValue: IigEnum_Music.None)) 
                    Play(song);
            }

            float lerpSoundTo = _applicationPaused ? 0 : (_playSilence ? 0 : Volume * _targetVolumeScale);

            float volumeChangeSpeed = _applicationPaused ? 2 : 0.5f;

            bool changingVolume = LerpUtils.IsLerpingBySpeed(ref _currentVolumeScale, lerpSoundTo, volumeChangeSpeed);

            if (LerpUtils.IsLerpingBySpeed(ref _transitionProgress, 1, 1f) || changingVolume)
            {
                ActiveSource.volume = _transitionProgress * _currentVolumeScale;
                FadingSource.volume = (1 - _transitionProgress) * _currentVolumeScale;
                
                if (_currentVolumeScale == 0) 
                {
                    FadingSource.clip = null;
                    FadingSource.Pause();

                    ActiveSource.clip = null;
                    ActiveSource.Pause();

                } else if (Mathf.Approximately(_transitionProgress, 1)) 
                {
                    FadingSource.clip = null;
                    FadingSource.Pause();
                }
            }

            if (_targetClip) 
            {
                if (_targetClip == ActiveSource.clip) 
                {
                    _targetClip = null;

                } else if (_targetClip == FadingSource.clip)
                {
                    Flip();
                }
                else
                {
                    if (_transitionProgress >= 1) 
                    {
                        Flip();
                    }
                }
            }
        }

        #region Inspector
        public override string InspectedCategory => Utils.Singleton.Categories.SCENE_MGMT;


        private IigEnum_Music _debugCoreMusic = IigEnum_Music.None;
        private int _inspectedStuff = -1;
        public override void Inspect()
        {
            base.Inspect();

            if (_inspectedStuff == -1) 
            {
                if ("Song To Play".PegiLabel().editEnum(ref _debugCoreMusic) | icon.Play.Click().nl())
                    _debugCoreMusic.Play();

                "Volume".PegiLabel(50).edit(ref _targetVolumeScale, 0, 2).nl();

                if (!sourceA && !sourceB) 
                {
                    "No Audio Sources Assigned".PegiLabel().writeWarning();

                    "Create Audio Sources".PegiLabel().Click().nl().OnChanged(() =>
                    {
                        var obj = new GameObject("Source A", typeof(AudioSource));
                        obj.transform.SetParent(transform);
                        sourceA = obj.GetComponent<AudioSource>();
                        sourceA.playOnAwake = false;

                        obj = new GameObject("Source B", typeof(AudioSource));
                        obj.transform.SetParent(transform);
                        sourceB = obj.GetComponent<AudioSource>();
                        sourceB.playOnAwake = false;
                    });
                } else if (Application.isPlaying) 
                {
                    "Active Playing: {0} Clip: {1}".F(ActiveSource.isPlaying, ActiveSource.clip).PegiLabel().nl();
                    "Fading Playing: {0} Clip: {1}".F(FadingSource.isPlaying, FadingSource.clip).PegiLabel().nl();
                }
            }

            "Music".PegiLabel().edit_enter_Inspect(ref Music, ref _inspectedStuff, 0).nl();
        }

        public override string NeedAttention()
        {
            if (!Music)
                return "No Music Scriptable Object";

            return base.NeedAttention();
        }
        #endregion
    }

    public enum IigEnum_Music
    {
        None = 0,
        MainMenu = 1,
        Loading = 2,
        Combat = 3,
        Reward = 4,
    }

    public static class CoreMusicExtension
    {
        public static void Play(this IigEnum_Music music, bool skipTransition = false) =>
            Singleton.Try<MusicService>(s => s.Play(music, skipTransition: skipTransition));
    }

    [PEGI_Inspector_Override(typeof(MusicService))] internal class MusicServiceDrawer : PEGI_Inspector_Override { }
}
