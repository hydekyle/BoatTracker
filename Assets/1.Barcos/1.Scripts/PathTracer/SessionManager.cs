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
        [Tooltip("Load Session Data for testing from local resources")]
        public bool useTestSession;
        [Tooltip("The sessionID for API request")]
        public string sessionID;
        public Transform navT;
        public Color startPathColor, endPathColor;
        public Observable<float> navigationVelocity = new() { Value = 1f };
        public Observable<float> pathLineWidth = new();
        [HideInInspector]
        public Observable<PathRecord> actualTarget;
        [HideInInspector]
        public int maxTargets, targetNext;
        public float traveledDistanceTotal = 0f;
        int navigationIndex = 0, navigationIndexPrev = 0, navigationIndexNext = 1;
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
            mySessionData = useTestSession ? SessionData.GetTestSessionData() : await SessionData.GetSessionDataBySessionID(sessionID);
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
            maxTargets = sessionData.records.Count;
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
                    traveledDistanceTotal += actualTarget.Value.geoDist;
                    targetNext++;
                    navT.LookAt(mySessionData.records[targetNext].position);
                }
                else if (!isSpeedPositive && targetNext - 1 > 0)
                {
                    traveledDistanceTotal -= actualTarget.Value.geoDist;
                    targetNext--;
                    // Apply delay to avoid LookAt to actual position while time rewind
                    UniTask.DelayFrame(10).ContinueWith(delegate () { navT.LookAt(mySessionData.records[targetNext].position); }).Forget();
                }
            }
        }

        void DrawPath(SessionData sessionData)
        {
            linePathRenderer.positionCount = sessionData.records.Count;
            linePathRenderer.startColor = startPathColor;
            linePathRenderer.endColor = endPathColor;
            for (var x = 0; x < sessionData.records.Count; x++)
            {
                linePathRenderer.SetPosition(x, sessionData.records[x].position);
            }
        }

    }
}