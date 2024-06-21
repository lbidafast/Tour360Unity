using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CrizGames.Vour
{
    public class VideoLoopButton : MonoBehaviour
    {
        private VideoController controls;
        private Image image;
        private Button button;

        [SerializeField] private Sprite loopOnIcon;
        [SerializeField] private Sprite loopOffIcon;


        private void Start()
        {
            image = GetComponent<Image>();
            button = GetComponent<Button>();
            controls = GetComponentInParent<VideoController>();

            button.onClick.AddListener(ToggleLoop);

            // Set start values
            SetLoopIcon(controls.isLooping);
        }

        private void ToggleLoop()
        {
            controls.SetLooping(!controls.isLooping);
            SetLoopIcon(controls.isLooping);
        }

        private void SetLoopIcon(bool on)
        {
            image.sprite = on ? loopOnIcon : loopOffIcon;
        }
    }
}