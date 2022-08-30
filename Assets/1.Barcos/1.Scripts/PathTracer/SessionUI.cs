using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityObservables;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;

namespace PathTracer
{
    public class SessionUI : MonoBehaviour
    {
        public SessionManager sessionManager;
        public TMP_Text velocimeterText, temperatureText, dateText, timeSpeedText;
        public Image arrowCompass;
        public float refreshRateTime = 1f;
        public float compassSpeed = 5f;
        float lastTick = 0f;
        bool isActive = false;

        void Start()
        {
            sessionManager.actualTarget.OnChangedValues += delegate (PathRecord previousTarget, PathRecord actualTarget)
            {
                isActive = true;
            };
            sessionManager.navigationVelocity.OnChangedValues += delegate (float previousSpeed, float actualSpeed)
            {
                timeSpeedText.text = string.Concat("x", actualSpeed.ToString("0.0"));
            };
        }

        void Update()
        {
            arrowCompass.transform.rotation = Quaternion.Lerp(arrowCompass.transform.rotation, Quaternion.Euler(0, 0, sessionManager.navT.transform.rotation.eulerAngles.y), Time.deltaTime * compassSpeed);
            if (!isActive) return;
            if (Time.time < lastTick + refreshRateTime) return;
            velocimeterText.text = sessionManager.actualTarget.Value.speedInKnots.ToString("0.0");
            dateText.text = sessionManager.actualTarget.Value.timestamp.Split(' ')[1];
            //temperatureText.text = sessionManager.actualTarget.Value.speed.ToString("0.0");
            lastTick = Time.time;
        }

        public void BTN_TimeFaster()
        {
            sessionManager.navigationVelocity.Value += 10f;
        }

        public void BTN_TimeSlower()
        {
            sessionManager.navigationVelocity.Value -= 10f;
        }
    }

}