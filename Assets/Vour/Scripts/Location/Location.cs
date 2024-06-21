using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.Video;

namespace CrizGames.Vour
{
    public class Location : MonoBehaviour
    {
        public enum LocationType
        {
            Empty,
            Image,
            Video,
            Scene
        }

        public enum DisplayType
        {
            _2D,
            _3D,
            _180,
            _180_3D,
            _360,
            _3603D,
        }

        public enum Layout3D
        {
            OverUnder,
            SideBySide
        }

        public enum VideoLocationType
        {
            Local,
            StreamingAssets,
            URL
        }

        public LocationType locationType;
        public DisplayType displayType;
        public Layout3D layout3D;
        public Texture2D texture;
        public VideoClip video;
        public VideoPlayer videoPlayer;
        public string videoURL;
        public string streamingAssetsVidPath;
        public VideoLocationType videoLocationType;

        [Tooltip("Scale image/video to fullscreen height")]
        public bool scaleToFullscreen = false;

        public bool lockCamera = false;
        public bool loopVideo = true;
        public bool videoUI = false;
        public bool videoUIAudioVolume = true;
        public bool videoUILoopButton = true;
        [Range(0f, 1f)] public float videoVolume = 1f;

        public SceneReference scene;

        public Vector3 rotOffset;

        private TeleportPoint[] _teleportPoints;

        /// <summary>
        /// Init is called from LocationManager because the GameObject of Location is inactive
        /// </summary>
        public void Init()
        {
            // Create a video player object for the video location
            if (locationType.IsVideo())
                this.CreateVideoPlayer();

            _teleportPoints = GetComponentsInChildren<TeleportPoint>(true);
        }

        /// <summary>
        /// Set data of this location to the corresponding LocationBase
        /// </summary>
        public void SetData()
        {
            LocationManager.GetManager().SetDataToLocationView(this);
            
            if (PlayerBase.Instance != null)
                PlayerBase.Instance.OnNewLocation(this);
        }

        /// <summary>
        /// Smart preload.
        /// Preload video locations linked to this location via teleport points
        /// </summary>
        public void PreloadLinkedVideos()
        {
            foreach (var t in _teleportPoints)
            {
                if (t.targetLocation != null && t.targetLocation.locationType.IsVideo() && !t.targetLocation.videoPlayer.isPrepared)
                    t.targetLocation.videoPlayer.Prepare();
            }
        }
        
        /// <summary>
        /// Unloads all video locations the player didn't jump to
        /// </summary>
        public void UnloadLinkedVideos(Location nextLocation)
        {
            foreach (var t in _teleportPoints)
            {
                if (t.targetLocation != null && t.targetLocation.locationType.IsVideo() && t.targetLocation != nextLocation)
                    t.targetLocation.videoPlayer.Stop();
            }

            bool nextHasLinkToThisLoc = nextLocation._teleportPoints.Any(t => t.targetLocation == this);
            if (locationType.IsVideo())
            {
                // Don't unload video if connected
                if (nextHasLinkToThisLoc)
                {
                    videoPlayer.Pause();
                    videoPlayer.time = 0;
                }
                // Unload video if not connected
                else
                    videoPlayer.Stop(); 
            }
        }

        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }

        private void OnApplicationQuit()
        {
            if (videoPlayer != null && videoPlayer.isPlaying)
                videoPlayer.Stop();
        }
    }
}