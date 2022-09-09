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
        public TMP_Text dateText, speedText, nmText, vmgText;
        public RectTransform pointerStart, pointerFinish;
        public Transform myRunnerAvatar;
        public List<Image> timebackArrows, timeforwardArrows;
        public Image arrowCompass;
        public float refreshRateTime = 1f;
        public float compassSpeed = 5f;
        float lastTick = 0f;
        bool isActive = false;
        PathRecord targetRecord;
        Observable<int> timeSpeed = new Observable<int>() { Value = 1 };

        void Start()
        {
            sessionManager.actualTarget.OnChangedValues += delegate (PathRecord previousTargetRecord, PathRecord actualTargetRecord)
            {
                isActive = true;
                targetRecord = actualTargetRecord;
            };
            timeSpeed.OnChanged += OnTimeSpeedChanged;
            OnTimeSpeedChanged();
        }

        void Update()
        {
            UpdateNavigationUI();
        }

        void UpdateNavigationUI()
        {
            if (!isActive) return;
            // Every frame update
            UIRotateCompassArrowByNavigator(sessionManager.navT);
            UIUpdateNavigatorPosition();
            // Time delay update (to avoid displayed data changing too fast)
            if (Time.time < lastTick + refreshRateTime) return;
            speedText.text = targetRecord.speedInKnots.ToString("0.0");
            nmText.text = (sessionManager.traveledDistanceTotal / 1000).ToString("0.00");
            vmgText.text = targetRecord.vmg.ToString("0.00");
            try
            {
                dateText.text = targetRecord.timestamp;
            }
            catch
            {
                Debug.LogWarningFormat("Timestamp format from session data is invalid: {0}", targetRecord.timestamp);
            }
            lastTick = Time.time;
        }

        void OnTimeSpeedChanged()
        {
            sessionManager.navigationVelocity.Value = 10 * timeSpeed.Value;
            UISetTimeArrowColorByTimespeed(sessionManager.navigationVelocity.Value);
        }

        void UISetTimeArrowColorByTimespeed(float timeSpeed)
        {
            timebackArrows[2].color = timeSpeed <= -3 ? Color.yellow : Color.white;
            timebackArrows[1].color = timeSpeed <= -2 ? Color.yellow : Color.white;
            timebackArrows[0].color = timeSpeed <= -1 ? Color.yellow : Color.white;
            timeforwardArrows[0].color = timeSpeed >= 1 ? Color.yellow : Color.white;
            timeforwardArrows[1].color = timeSpeed >= 2 ? Color.yellow : Color.white;
            timeforwardArrows[2].color = timeSpeed >= 3 ? Color.yellow : Color.white;
        }

        /// <summary> Set position in timeline for avatar UI </summary>
        void UIUpdateNavigatorPosition()
        {
            var lerpValue = (float)sessionManager.targetNext / (float)sessionManager.maxTargets;
            var targetPos = Vector3.Lerp(pointerStart.position, pointerFinish.position, lerpValue);
            myRunnerAvatar.position = Vector3.MoveTowards(myRunnerAvatar.position, targetPos, Time.deltaTime * 100);
        }

        void UIRotateCompassArrowByNavigator(Transform navigator)
        {
            arrowCompass.transform.localRotation = Quaternion.Lerp(arrowCompass.transform.localRotation, Quaternion.Euler(0, 0, navigator.transform.rotation.eulerAngles.y), Time.deltaTime * compassSpeed);
        }

        public void BTN_TimeForward()
        {
            if (timeSpeed.Value < 3) timeSpeed.Value++;
            if (timeSpeed.Value == 0) BTN_TimeForward();
        }

        public void BTN_TimeBack()
        {
            if (timeSpeed.Value > -3) timeSpeed.Value--;
            if (timeSpeed.Value == 0) BTN_TimeBack();
        }
    }

}