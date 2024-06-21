using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace CrizGames.Vour
{
    public class VideoTimeTexts : MonoBehaviour
    {
        private VideoController controls;

        [SerializeField] private TextMeshProUGUI currentTimeText;
        [SerializeField] private TextMeshProUGUI finalTimeText;

        private bool gotTimes = false;

        private void Start()
        {
            controls = GetComponentInParent<VideoController>();

            controls.OnInit.AddListener(() => gotTimes = false);
        }

        private void LateUpdate()
        {
            // Wait while times are not available (video with URL)
            if (!gotTimes && !float.IsNaN(controls.videoTimeLength))
            {
                finalTimeText.text = SecondsToTimeString(controls.videoTimeLength);
                gotTimes = true;
            }

            if (gotTimes)
                currentTimeText.text = SecondsToTimeString(controls.currentVideoTime);
        }

        private string SecondsToTimeString(float seconds)
        {
            if (float.IsNaN(seconds))
                return "0:00";

            string format = @"m\:ss";
            if (seconds >= 600)
                format = @"mm\:ss";
            else if (seconds >= 3600)
                format = @"hh\:mm\:ss";
            return System.TimeSpan.FromSeconds(seconds).ToString(format);
        }
    }
}