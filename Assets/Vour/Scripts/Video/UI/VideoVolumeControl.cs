using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CrizGames.Vour
{
    public class VideoVolumeControl : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private VideoController controls;
        private Image image;
        private Button button;
        private CanvasGroup audioSliderGroup;
        private RectTransform audioSliderContainerT;
        private Slider audioSlider;

        [Space]
        [SerializeField] private Sprite audioOnIcon;
        [SerializeField] private Sprite audioOffIcon;
        [Space]
        [SerializeField] private float sliderHeight = 200f;
        [Space]
        [SerializeField] private float smoothTime = 0.1f;
        [Space]
        [SerializeField] private bool sliderAlwaysVisible = false;

        private float targetAlpha, alphaVel;
        private float targetSliderHeight, sliderHeightVel;

        private bool pointerHovering = false;


        private void Start()
        {
            button = GetComponentInChildren<Button>();
            image = button.GetComponent<Image>();
            controls = GetComponentInParent<VideoController>();
            audioSliderGroup = GetComponentInChildren<CanvasGroup>();
            audioSliderContainerT = audioSliderGroup.GetComponent<RectTransform>();
            audioSlider = audioSliderGroup.GetComponentInChildren<Slider>();

            button.onClick.AddListener(ToggleAudioMute);

            controls.OnVideoLoaded.AddListener(OnVideoLoaded);
            controls.OnAudioVolumeChanged.AddListener(audioSlider.SetValueWithoutNotify);
            controls.OnAudioMuteStateChanged.AddListener(OnAudioMuteStateChanged);

            audioSlider.onValueChanged.AddListener(controls.SetAudioVolume);

            // Set start values
            controls.OnVideoLoaded.AddListener(() => audioSlider.SetValueWithoutNotify(controls.audioVolume));

            OnAudioMuteStateChanged(controls.isAudioMuted);

            if (!sliderAlwaysVisible)
                SetSliderVisibility(0f);
            targetSliderHeight = sliderHeight;
        }

        private void Update()
        {
            if (sliderAlwaysVisible)
                return;

            if (audioSliderGroup.alpha != targetAlpha)
            {
                SetSliderVisibility(Mathf.SmoothDamp(audioSliderGroup.alpha, targetAlpha, ref alphaVel, smoothTime));

                float sliderHeight = Mathf.SmoothDamp(audioSliderContainerT.sizeDelta.y, targetSliderHeight, ref sliderHeightVel, smoothTime);
                audioSliderContainerT.sizeDelta = new Vector2(audioSliderContainerT.sizeDelta.x, sliderHeight);
            }
        }

        private void OnVideoLoaded()
        {
            button.interactable = controls.hasAudio;
            audioSliderGroup.gameObject.SetActive(button.interactable);

            if (!button.interactable)
                Debug.Log("Video has no audio tracks. Volume control was disabled.");
        }

        private void ToggleAudioMute()
        {
            controls.SetAudioMute(!controls.isAudioMuted);
        }

        private void SetSliderVisibility(float alpha)
        {
            audioSliderGroup.alpha = alpha;
            audioSliderGroup.interactable = alpha > 0.5f;
            audioSliderGroup.blocksRaycasts = alpha > 0.5f;
        }

        private void OnAudioMuteStateChanged(bool muted)
        {
            image.sprite = muted ? audioOffIcon : audioOnIcon;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (sliderAlwaysVisible)
                return;

            pointerHovering = true;
            targetAlpha = 1f;
            targetSliderHeight = sliderHeight;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (sliderAlwaysVisible)
                return;

            pointerHovering = false;
            Invoke(nameof(DelayedPointerExit), 0.4f);
        }

        private void DelayedPointerExit()
        {
            // Pointer not at slider or sth.
            if (!pointerHovering)
            {
                targetAlpha = 0f;
                targetSliderHeight = 0f;
            }
        }
    }
}