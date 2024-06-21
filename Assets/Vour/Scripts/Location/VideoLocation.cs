using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Video;

namespace CrizGames.Vour
{
    public class VideoLocation : MediaLocation
    {
        private RenderTexture renderTexture;

        public override void Init()
        {
            base.Init();

            gameObject.SetActive(true);

            if (Application.isPlaying)
            {
                if (location.videoUI)
                {
                    MainVideoUIController.Instance.EnableUI(location.videoPlayer, location.videoUIAudioVolume, location.videoUILoopButton);
                    MainVideoUIController.Instance.GetComponent<VideoController>().SetAudioVolume(location.videoVolume);
                }

                location.videoPlayer.Play();
            }
        }

        public virtual void OnDisable()
        {
            if (location == null || location.videoPlayer == null)
                return;

            if (renderTexture != null)
            {
                renderTexture.DiscardContents();
                renderTexture.Release();
                renderTexture = null;
            }

            if (location.videoUI)
                MainVideoUIController.Instance.DisableUI();
        }

        public override void UpdateLocation()
        {
#if UNITY_EDITOR
            // Set video in case the user changed it while playing
            if (Application.isPlaying)
            {
                location.Init();
                location.videoPlayer.Play();
            }
#endif

            // Set loading texture while video is loading
            if (Application.isPlaying)
            {
                if (!location.videoPlayer.isPrepared)
                {
                    SetMedia(VourSettings.Instance.loadingTexture);

                    // When video loaded
                    location.videoPlayer.prepareCompleted += SetupLoadedVideo;
                }
                else
                {
                    // When video is already loaded
                    SetupLoadedVideo(location.videoPlayer);
                }
            }
#if UNITY_EDITOR
            else
            {
                // Set a gray texture so you don't get a flashbang in your face
                Texture2D grayTex = new Texture2D(1, 1);
                grayTex.SetPixel(0, 0, Color.gray);
                grayTex.Apply();

                SetMedia(grayTex);
            }
#endif

            GetVideoSize((width, height) => UpdateSize(new Vector2(width, height)));
        }

        private void SetupLoadedVideo(VideoPlayer videoPlayer)
        {
            videoPlayer.SetupRenderTexture(ref renderTexture);

            videoPlayer.targetTexture = renderTexture;

            SetMedia(renderTexture);

            SetVideoVolume();
            
            IsReady = true;
        }

        private void GetVideoSize(UnityAction<int, int> callback)
        {
            VideoUtils.GetVideoSize(video, location.videoPlayer, location.videoLocationType, callback, callbackTextureSize => location.videoPlayer.prepareCompleted += (_) => callbackTextureSize());
        }

        private void SetVideoVolume()
        {
            if (location.videoPlayer == null)
                return;

            for (ushort i = 0; i < location.videoPlayer.audioTrackCount; i++)
                location.videoPlayer.SetDirectAudioVolume(i, location.videoVolume);
        }
    }
}