﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.Video;
using static CrizGames.Vour.Location;

namespace CrizGames.Vour
{
    public class VideoPoint : PopupPoint
    {
        [SerializeField] private RawImage videoImg;
        private RenderTexture renderTexture;

        private VideoController controller;
        private VideoUIController uiController;

        public VideoClip video;
        public VideoPlayer videoPlayer;
        public string videoURL;
        public string streamingAssetsVidPath;
        public VideoLocationType videoLocationType;
        public bool loopVideo = true;
        public bool videoUI = true;
        public bool videoUIAudioVolume = true;
        public bool videoUILoopButton = true;
        [Range(0f, 1f)]
        public float videoVolume = 1f;

        public override Transform panelParent => transform;

        private void Awake()
        {
            panel.gameObject.SetActive(false);
            
            this.CreateVideoPlayer();
        }

        public override void InitPanel()
        {
            panel.localScale = new Vector3(0, 0, 1);
            panel.gameObject.SetActive(false);
            
            VideoUtils.SetupVideoPanel(
                panel,
                ref controller,
                ref uiController,
                videoPlayer,
                SetTex,
                SetupLoadedVideo,
                UpdateSize,
                GetVideoSize,
                videoUI,
                videoUIAudioVolume,
                videoUILoopButton);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            
            videoPlayer.Prepare();
        }

        private void OnDisable()
        {
            videoPlayer.Stop();

            if (renderTexture != null)
            {
                renderTexture.DiscardContents();
                renderTexture.Release();
                renderTexture = null;
            }
        }

        public override void Interact()
        {
            base.Interact();

            if (open)
                videoPlayer.GoToFirstFrame();
            controller.SetPlayingState(open);
        }

        private void GetVideoSize(UnityAction<int, int> callback)
        {
            VideoUtils.GetVideoSize(video, videoPlayer, videoLocationType, callback, callbackTextureSize => controller.OnVideoLoaded.AddListener(callbackTextureSize));
        }

        private void SetupLoadedVideo()
        {
            videoPlayer.SetupRenderTexture(ref renderTexture);

            SetTex(renderTexture);

            controller.SetAudioVolume(videoVolume);
        }

        private void SetTex(Texture tex)
        {
            videoImg.texture = tex;

            if (tex.GetType() == typeof(RenderTexture))
                videoPlayer.targetTexture = (RenderTexture)tex;
        }

        protected override Transform GetPanel()
        {
            return transform.GetChild(2);
        }
    }
}