using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace CrizGames.Vour
{
    public class VideoSlider : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private VideoController controls;
        private Slider slider;

        [SerializeField] private RectTransform background;
        [SerializeField] private RectTransform fillArea;
        [SerializeField] private RectTransform handle;
        private RectTransform handleParent;
        [Space]
        [SerializeField] private float handleHoverSize = 30f;
        private float handleNormalSize;
        [Space]
        [SerializeField] private float tweenSmoothing = 0.05f;
        [SerializeField] private float normalPadding = 6f;
        [SerializeField] private float hoverPadding = 0f;
        private float currentPadding, targetPadding, paddingTweenVel;
        private float currentHandleSize, targetHandleSize, handleTweenVel;


        private void Start()
        {
            slider = GetComponent<Slider>();
            controls = GetComponentInParent<VideoController>();
            handleParent = handle.parent.GetComponent<RectTransform>();

            controls.OnVideoProgress.AddListener(slider.SetValueWithoutNotify);
            slider.onValueChanged.AddListener(controls.SetVideoTime);

            // Set start values
            currentPadding = targetPadding = normalPadding;
            SetPadding(normalPadding);
            SetHandleSize(handleParent.rect.height - normalPadding * 2f);
            handleNormalSize = handle.rect.height;
        }

        private void Update()
        {
            if (currentPadding != targetPadding)
            {
                currentPadding = Mathf.SmoothDamp(currentPadding, targetPadding, ref paddingTweenVel, tweenSmoothing);
                currentHandleSize = Mathf.SmoothDamp(currentHandleSize, targetHandleSize, ref handleTweenVel, tweenSmoothing);
                SetPadding(currentPadding);
                SetHandleSize(currentHandleSize);
            }
        }

        private void SetPadding(float yPadding)
        {
            background.offsetMin = new Vector2(background.offsetMin.x, yPadding);
            background.offsetMax = new Vector2(background.offsetMax.x, -yPadding);
            fillArea.offsetMin = new Vector2(fillArea.offsetMin.x, yPadding);
            fillArea.offsetMax = new Vector2(fillArea.offsetMax.x, -yPadding);
        }

        private void SetHandleSize(float size)
        {
            float yPadding = (handleParent.rect.height - size) / 2f;
            handle.offsetMin = new Vector2(handle.offsetMin.x, yPadding);
            handle.offsetMax = new Vector2(handle.offsetMax.x, -yPadding);
            handle.sizeDelta = new Vector2(handle.rect.height, handle.sizeDelta.y);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            targetPadding = hoverPadding;
            targetHandleSize = handleHoverSize;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            targetPadding = normalPadding;
            targetHandleSize = handleNormalSize;
        }

        private void OnValidate()
        {
            if (handle == null)
                return;

            handleParent = handle.parent.GetComponent<RectTransform>();
            SetPadding(normalPadding);
            SetHandleSize(handleParent.rect.height - normalPadding * 2f);
        }
    }
}