using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityObservables;

namespace PathTracer
{
    public class SessionManager : MonoBehaviour
    {
        public string sessionID;
        public Transform navT;
        public Color startColor, endColor;
        public Observable<float> navigationVelocity = new() { Value = 1f };
        public Observable<float> pathLineWidth = new();
        public Observable<PathRecord> actualTarget;
        int navigationIndex = 0, navigationIndexPrev = 0, navigationIndexNext = 1;
        int targetNext;
        bool isNavigating = false;
        LineRenderer linePathRenderer;
        SessionData mySessionData;

        void Awake()
        {
            linePathRenderer = GetComponent<LineRenderer>();
            pathLineWidth.OnChanged += delegate ()
            {
                linePathRenderer.startWidth = pathLineWidth.Value;
                linePathRenderer.endWidth = pathLineWidth.Value;
            };
        }

        async void Start()
        {
            mySessionData = await SessionData.GetSessionDataBySessionID(sessionID);
            //DrawPath(mySessionData);
            NavegationStart(mySessionData);
        }

        void Update()
        {
            if (isNavigating) Navigate();
        }

        public PathRecord GetActualTarget()
        {
            return mySessionData.records[targetNext];
        }

        public PathRecord GetPreviousTarget()
        {
            return mySessionData.records[targetNext - 1];
        }

        void NavegationStart(SessionData sessionData)
        {
            navT.position = sessionData.records[0].position;
            navT.LookAt(sessionData.records[1].position);
            targetNext = 1;
            isNavigating = true;
        }

        void Navigate()
        {
            var isSpeedPositive = navigationVelocity.Value > 0;
            var targetRecord = isSpeedPositive ? GetActualTarget() : GetPreviousTarget();
            actualTarget.Value = targetRecord;
            var speed = (10 + targetRecord.speed) * Mathf.Abs(navigationVelocity.Value);
            //TODO: Use Timestamp between points if we want to Lerp at real time
            navT.position = Vector3.MoveTowards(navT.position, targetRecord.position, speed * Time.deltaTime);
            if (navT.position == targetRecord.position)
            {
                if (isSpeedPositive && targetNext + 1 < mySessionData.records.Count)
                {
                    targetNext++;
                    navT.LookAt(mySessionData.records[targetNext].position);
                }
                else if (!isSpeedPositive && targetNext - 1 > 0)
                {
                    targetNext--;
                    // Apply delay to avoid LookAt to actual position while time rewind
                    UniTask.DelayFrame(10).ContinueWith(delegate () { navT.LookAt(mySessionData.records[targetNext].position); }).Forget();
                }
            }
        }

        void DrawPath(SessionData sessionData)
        {
            linePathRenderer.positionCount = sessionData.records.Count;
            linePathRenderer.startColor = startColor;
            linePathRenderer.endColor = endColor;
            for (var x = 0; x < sessionData.records.Count; x++)
            {
                linePathRenderer.SetPosition(x, sessionData.records[x].position);
            }
        }

    }
}