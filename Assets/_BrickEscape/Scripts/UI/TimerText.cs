using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace BrickEscape
{
    public class TimerText : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _timeText;

        private DateTime _endTime;
        private Coroutine _routine;

        private void Start()
        {
            StartCountdown24h();
        }
        public void StartCountdown24h()
        {
            _endTime = DateTime.Now.AddHours(24);

            if (_routine != null)
                StopCoroutine(_routine);

            _routine = StartCoroutine(UpdateTimerRoutine());
        }

        IEnumerator UpdateTimerRoutine()
        {
            while (true)
            {
                UpdateTime();

                // chờ đến phút tiếp theo cho chuẩn
                float secondsToNextMinute = 60 - DateTime.Now.Second;
                yield return new WaitForSecondsRealtime(secondsToNextMinute);
            }
        }

        void UpdateTime()
        {
            TimeSpan remain = _endTime - DateTime.Now;

            if (remain.TotalSeconds <= 0)
            {
                _timeText.text = "00h 00m";
                OnTimeEnd();
                StopCoroutine(_routine);
                return;
            }

            int hours = (int)remain.TotalHours;
            int minutes = remain.Minutes;

            _timeText.text = $"{hours:00}h {minutes:00}m";
        }

        void OnTimeEnd()
        {
            Debug.Log("Countdown finished!");
            // TODO: reset daily / reward / quest
        }
    }
}
