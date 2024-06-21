using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CrizGames.Vour
{
    public class VideoPlayButton : MonoBehaviour
    {
        private VideoController controls;
        private Image image;
        private Button button;

        [SerializeField] private Sprite playIcon;
        [SerializeField] private Sprite pauseIcon;
        [SerializeField] private Sprite replayIcon;


        private void Awake()
        {
            button = GetComponent<Button>();
            image = GetComponent<Image>();
            controls = GetComponentInParent<VideoController>();

            button.onClick.AddListener(controls.TogglePlayingState);

            controls.OnPlayStateChanged.AddListener(OnPlayStateChanged);
        }

        private void OnPlayStateChanged(bool playing)
        {
            if (!playing && controls.hasVideoEnded)
                image.sprite = replayIcon;
            else
                image.sprite = playing ? pauseIcon : playIcon;
        }
    }
}