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

        void OnTimeSpeedChanged()
        {
            sessionManager.navigationVelocity.Value = 10 * timeSpeed.Value;
            timebackArrows[2].color = timeSpeed.Value <= -3 ? Color.yellow : Color.white;
            timebackArrows[1].color = timeSpeed.Value <= -2 ? Color.yellow : Color.white;
            timebackArrows[0].color = timeSpeed.Value <= -1 ? Color.yellow : Color.white;
            timeforwardArrows[0].color = timeSpeed.Value >= 1 ? Color.yellow : Color.white;
            timeforwardArrows[1].color = timeSpeed.Value >= 2 ? Color.yellow : Color.white;
            timeforwardArrows[2].color = timeSpeed.Value >= 3 ? Color.yellow : Color.white;
        }

        void Update()
        {
            UpdateNavigationUI();
        }

        void UpdateNavigationUI()
        {
            if (!isActive) return;
            // Every frame update
            RotateCompassArrowByNavigator(sessionManager.navT);
            MoveAvatarRunner();
            // Time delay update (to avoid displayed data changing too fast)
            if (Time.time < lastTick + refreshRateTime) return;
            speedText.text = targetRecord.speedInKnots.ToString("0.0");
            nmText.text = (sessionManager.traveledDistanceTotal / 1000).ToString("0");
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

        void MoveAvatarRunner()
        {
            var lerpValue = (float)sessionManager.targetNext / (float)sessionManager.maxTargets;
            var targetPos = Vector3.Lerp(pointerStart.position, pointerFinish.position, lerpValue);
            myRunnerAvatar.position = Vector3.MoveTowards(myRunnerAvatar.position, targetPos, Time.deltaTime * 100);
        }

        void RotateCompassArrowByNavigator(Transform navigator)
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