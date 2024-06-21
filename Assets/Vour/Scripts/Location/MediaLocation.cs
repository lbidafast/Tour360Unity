using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using static CrizGames.Vour.Location;

namespace CrizGames.Vour
{
    public abstract class MediaLocation : LocationView
    {
        private static readonly int MainTex = Shader.PropertyToID("_MainTex");
        private static readonly int Layout = Shader.PropertyToID("_Layout");
        private static readonly int ImageType = Shader.PropertyToID("_ImageType");
        
        [SerializeField] private protected Material material;

        public int size = 4;

        public bool resizeWidth = false;

        public MeshRenderer rend;

        private bool Is3D => location.displayType.Is3D();
        private bool Is180 => location.displayType.Is180();
        private bool Is360 => location.displayType.Is360();

        public override void Init()
        {
            base.Init();
            
            rend.sharedMaterial = material;
        }

        protected virtual void SetMedia(Texture texture)
        {
            int layout = 0;
            if (Is3D)
                layout = location.layout3D == Layout3D.SideBySide ? 1 : 2;
            
            material.SetTexture(MainTex, texture);
            material.SetInt(Layout, layout);

            if (Is180 || Is360)
            {
                material.SetInt(ImageType, Is360 ? 0 : 1);
                // Rotate 360 sphere
                rend.transform.eulerAngles = rotOffset;
            }
        }

        protected void UpdateSize(Vector2 sourceSize)
        {
            Transform t = rend.transform;
            Vector3 scale = new Vector3(size, size, size);

            float fullscreenMultiplier = size;
            var cam = PlayerBase.Instance ? PlayerBase.Instance.cam : null;
            if (cam != null) // Screen height
                fullscreenMultiplier = cam.ScreenToWorldPoint(new Vector3(0, Screen.height, t.position.z)).y - cam.ScreenToWorldPoint(new Vector3(0, 0, t.position.z)).y;

            if (resizeWidth)
            {
                scale.x = sourceSize.x / sourceSize.y * scale.y;

#if UNITY_EDITOR
                if (Is3D && (!locationType.IsVideo() || Application.isPlaying || (video != null && location.videoLocationType == VideoLocationType.Local)))
#else
                if (Is3D)
#endif
                {
                    switch (location.layout3D)
                    {
                        case Layout3D.OverUnder:
                            scale.y /= 2;
                            break;

                        case Layout3D.SideBySide:
                            scale.x /= 2;
                            break;
                    }
                }
            }

            // if (Is360)
            //     scale.x *= -1f;
            // else 
            if(!Is360 && location.scaleToFullscreen)
                scale *= fullscreenMultiplier / size;

            t.localScale = scale;
        }
    }
}