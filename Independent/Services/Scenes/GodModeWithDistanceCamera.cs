using QuizCanners.Inspect;
using UnityEngine;


namespace QuizCanners.Utils
{
    public class GodModeWithDistanceCamera : GodModeConfigurable
    {
        [SerializeField] private Camera _distanceCamera;
        [SerializeField] private float _distantCameraFarClip;

        public override CameraClearFlags ClearFlags
        {
            get => _distanceCamera.clearFlags;
            set => _distanceCamera.clearFlags = value;
        }

        protected override void AdjsutCamera()
        {
            base.AdjsutCamera();
            var cam = MainCam;

            if (!_distanceCamera || !cam)
                return;

            var camTf = _mainCam.transform;
            var disttf = _distanceCamera.transform;

            disttf.position = camTf.position;
            disttf.rotation = camTf.rotation;

            _distanceCamera.nearClipPlane = cam.farClipPlane * 0.5f;
            _distanceCamera.farClipPlane = Mathf.Max( _distantCameraFarClip, cam.farClipPlane*2f);
        }

        public override void Inspect()
        {
            base.Inspect();

            pegi.nl();
            "Distance camera".PegiLabel().edit(ref _distanceCamera).nl();

            "Distant cutoff".PegiLabel().edit(ref _distantCameraFarClip).nl();
            if (MainCam) 
            {
                "Main Distance: ".F(MainCam.farClipPlane).PegiLabel().nl();
            }



        }

    }

    [PEGI_Inspector_Override(typeof(GodModeWithDistanceCamera))] internal class GodModeWithDistanceCameraDrawer : PEGI_Inspector_Override { }
}
