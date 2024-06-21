using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Video;

namespace CrizGames.Vour
{
    [RequireComponent(typeof(VideoController))]
    public class VideoUIController : MonoBehaviour
    {
        private VideoController controller;

        [SerializeField] private GameObject audioVolume;
        [SerializeField] private GameObject loopButton;
        [Space]
        [SerializeField] private GameObject uiContainerOverride;

        private GameObject UIContainer => uiContainerOverride ? uiContainerOverride : gameObject;


        protected virtual void Awake()
        {
            controller = GetComponent<VideoController>();
        }

        private void Start()
        {
            //if (Utils.InVR())
            //    GetComponent<Canvas>().worldCamera = EventSystem.current.GetComponent<VourUIInput>().eventCamera;
        }

        public void EnableUI(VideoPlayer videoPlayer, bool enableAudioVolume, bool enableLoopButton)
        {
            UIContainer.SetActive(true);
            audioVolume.SetActive(enableAudioVolume);
            loopButton.SetActive(enableLoopButton);

            if(controller == null)
                controller = GetComponent<VideoController>();

            if (Application.isPlaying)
                controller.Init(videoPlayer);
        }

        public void DisableUI()
        {
            UIContainer.SetActive(false);
        }
    }
}