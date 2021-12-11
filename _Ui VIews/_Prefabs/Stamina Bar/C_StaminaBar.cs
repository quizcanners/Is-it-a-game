using QuizCanners.Inspect;
using QuizCanners.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using QuizCanners.Lerp;


namespace QuizCanners {
    
    [ExecuteAlways]
    public class C_StaminaBar : MonoBehaviour, IPEGI {

        [SerializeField] protected Image _image;
        [SerializeField] protected TextMeshProUGUI _staminaCount;
        [SerializeField] protected AudioSource audioSource;
        [SerializeField] protected AudioClip onFillCrossTheCenter;
        [SerializeField] protected AudioClip onHitCrossTheCenter;
        [SerializeField] protected AudioClip onHitBelow;
        [SerializeField] protected AudioClip onHitAbove;
        [SerializeField] protected Graphic centralLine;
        [SerializeField] protected float fullRechargeDuration = 6;
        [SerializeField] protected int pointsMaximum = 8;

        private readonly ShaderProperty.FloatValue _staminaLineInShader = new("_NodeNotesStaminaPortion");
        private float _staminaLine = 1f;

        private bool Above => _staminaLine >= 0.5f;

        private readonly ShaderProperty.FloatValue _previousStaminaLineInShader = new("_NodeNotesStaminaPortion_Prev");
        private float _previousStaminaLine = 1f;

        private float showPreviousTimer;

        private readonly ShaderProperty.FloatValue _staminaCurve = new("_NodeNotes_StaminaCurve");
        [SerializeField] private float _staminaCurveValue = 3;

        public float StaminaCurve
        {
            get { return _staminaCurveValue; }
            set
            {
                var prev = StaminaPortion;

                _staminaCurveValue = value;

                StaminaPortion = prev;

                _staminaCurve.GlobalValue = _staminaCurveValue;
            }
        }

        private void Play(AudioClip clip, float pitch = 1)
        {
            if (clip)
            {
                audioSource.PlayOneShot(clip, pitch);
            }
        }

        // Start is called before the first frame update
        private void Reset()
        {
            _image = GetComponent<Image>();
        }
        
        public float StaminaPoints
        {
            get { return (StaminaPortion * pointsMaximum); }
            set
            { StaminaPortion = value / pointsMaximum; }
        }

        private float StaminaPortion
        {
            get
            {
                var above = _staminaLine >= 0.5f;

                float off = (above ?  _staminaLine - 0.5f : 0.5f - _staminaLine) * 2; // 1

                off = 1 - off; // 2

                float thickness = Mathf.Pow(off, 1 + StaminaCurve); // 3

                thickness *= 0.5f; // 4

                return above ? 1f - thickness : (thickness); // 5
            }

            set
            {
                var above = value > 0.5f;

                value = above ? 1f - value : value; //5

                value *= 2; // 4

                value = Mathf.Pow(value, 1f/(1f+ StaminaCurve)); //3

                value = 1 - value; // 2

                value *= 0.5f;

                value = (above ? value + 0.5f : 0.5f - value);

                _staminaLine = value;

            }
        }
        
        // Update is called once per frame
        private void Update()
        {

           

            if (Application.isPlaying)
            {

                bool above = Above;

                _staminaLine = Mathf.Clamp01(_staminaLine + Time.deltaTime / fullRechargeDuration);

                if (Above && !above)
                    audioSource.PlayOneShot(onFillCrossTheCenter);

                if (Input.GetKeyDown(KeyCode.Alpha1))
                    Use(1);
                if (Input.GetKeyDown(KeyCode.Alpha2))
                    Use(2);
                if (Input.GetKeyDown(KeyCode.Alpha3))
                    Use(5);
                
            }

            audioSource.pitch = _staminaLine;

            //StaminaCurve = 1 + Mathf.Pow(Mathf.Clamp01(_staminaLine * 2),2) * 5;

            _staminaLineInShader.GlobalValue = _staminaLine;
            
            if (showPreviousTimer > 0)
            {
                if (_staminaLine >= _previousStaminaLine)
                    showPreviousTimer = 0;

                showPreviousTimer -= Time.deltaTime;
            }
            else
            {
                LerpUtils.IsLerpingBySpeed(ref _previousStaminaLine, _staminaLine, 0.1f);
            }

            _previousStaminaLineInShader.GlobalValue = _previousStaminaLine;

            if (_staminaCount)
            {
                _staminaCount.text = ((int) StaminaPoints).ToString();
                var targetColor = Above
                    ? Color.LerpUnclamped(Color.yellow, Color.green, (_staminaLine - 0.5f) * 2)
                    : Color.LerpUnclamped(Color.magenta, Color.blue, _staminaLine * 2);

                _staminaCount.IsLerpingRgbBySpeed(targetColor, 5); // LerpUtils.LerpRgb()
                  
            }

            centralLine.TrySetAlpha(Above ? 1 : 0);
        }

        private void OnEnable()
        {
            _staminaCurve.GlobalValue = _staminaCurveValue;
        }

        public void Use(int cost)
        {

            var pnts = StaminaPoints;

            if (pnts >= cost)
            {
                var above = Above;

                _previousStaminaLine = _staminaLine;

                showPreviousTimer = 1f;

                StaminaPoints = pnts - cost;
                
                if (!above)
                    Play(onHitBelow, 0.5f + _staminaLine);
                else if (above && !Above)
                    Play(onHitCrossTheCenter, Mathf.Pow(_staminaLine*2.1f, 6));
                else
                    Play(onHitAbove, 1f + (1f-_staminaLine));

            }

        }

        private void InspectSkill(string skillName, int cost)
        {
            
            var points = (int)StaminaPoints;

            "{0} [{1} st]".F(skillName, cost).PegiLabel().Click().Nl().OnChanged(() =>
                {
                    if (points > cost)
                    {
                        Use(cost);
                    }
                });
            
        }

        public void Inspect()
        {
            pegi.EditorViewPegi.Lock_UnlockClick(gameObject);
            pegi.Nl();

            var curve = StaminaCurve;

            if ("Stamina Curve".PegiLabel().Edit(ref curve, 0, 10f).Nl())
                StaminaCurve = curve;

              

            "Stamina".PegiLabel(40).Edit01(ref _staminaLine).Nl();

            int points = (int)StaminaPoints;

            if ("Points".PegiLabel().EditDelayed(ref points).Nl())
                StaminaPoints = points;

            InspectSkill("Shoot", 1); pegi.Nl();

            InspectSkill("Kick", 2); pegi.Nl();

            InspectSkill("Spell", 5); pegi.Nl();

        }
    }
    
    [PEGI_Inspector_Override(typeof(C_StaminaBar))] internal class StaminaBarDrawer : PEGI_Inspector_Override { }


}