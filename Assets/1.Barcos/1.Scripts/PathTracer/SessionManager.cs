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
        public static SessionManager Instance;
        [Tooltip("Load Session Data for testing from local resources")]
        public bool useTestSession;
        [Tooltip("The sessionID for API request")]
        public string sessionID;
        [HideInInspector]
        public Observable<NavigatorAgent> focusAgent;
        public Observable<float> navigationTimeVelocity = new() { Value = 1f };
        public Observable<float> pathLineWidth = new();
        public Color startPathColor, endPathColor;
        public bool isNavigating = false;
        public List<NavigatorAgent> navAgents = new();
        public GameObject navigatorPlaceholder;
        public OrbitCamera orbitCamera;
        LineRenderer linePathRenderer;

        void Awake()
        {
            Instance = this;
            linePathRenderer = GetComponent<LineRenderer>();
            pathLineWidth.OnChanged += delegate ()
            {
                linePathRenderer.startWidth = pathLineWidth.Value;
                linePathRenderer.endWidth = pathLineWidth.Value;
            };
        }

        async void Start()
        {
            var myNavigatorAgent = useTestSession ? NavigatorAgent.GetNavigatorAgentTest() : await NavigatorAgent.GetNavigatorAgentBySessionID(sessionID);
            NavegationStart(myNavigatorAgent);
        }

        void Update()
        {
            if (isNavigating)
            {
                for (var x = 0; x < navAgents.Count; x++)
                {
                    Navigate(navAgents[x]);
                }
            }
        }

        void NavegationStart(NavigatorAgent navigatorAgent)
        {
            focusAgent.Value = navigatorAgent;
            navigatorAgent.navT.position = navigatorAgent.positions[0];
            navigatorAgent.navT.LookAt(navigatorAgent.positions[1]);
            orbitCamera.Target = navigatorAgent.navT.GetComponent<FocusPoint>();
            orbitCamera.enabled = true;
            navAgents.Add(navigatorAgent);
            isNavigating = true;
        }

        void Navigate(NavigatorAgent navigatorAgent)
        {
            var isTimeSpeedPositive = navigationTimeVelocity.Value > 0;
            var targetPosition = isTimeSpeedPositive ? navigatorAgent.GetTargetPosition() : navigatorAgent.GetPreviousTargetPosition();
            var targetRecord = isTimeSpeedPositive ? navigatorAgent.GetTargetPathRecord() : navigatorAgent.GetPreviousPathRecord();
            var speed = (10 + targetRecord.speed) * Mathf.Abs(navigationTimeVelocity.Value);
            var navT = navigatorAgent.navT;
            var targetNext = navigatorAgent.targetNext;
            var maxTargets = navigatorAgent.maxTargets;
            //TODO: Use Timestamp between points if we want to Lerp at real time
            navT.position = Vector3.MoveTowards(navT.position, targetPosition, speed * Time.deltaTime);
            if (navT.position == targetPosition)
            {
                if (isTimeSpeedPositive && targetNext + 1 < maxTargets)
                {
                    navigatorAgent.traveledDistanceTotal += navigatorAgent.mySessionData.records[targetNext].geoDist;
                    navigatorAgent.targetNext++;
                    navT.LookAt(navigatorAgent.positions[navigatorAgent.targetNext]);
                }
                else if (!isTimeSpeedPositive && navigatorAgent.targetNext - 1 > 0)
                {
                    navigatorAgent.traveledDistanceTotal -= navigatorAgent.mySessionData.records[targetNext].geoDist;
                    navigatorAgent.targetNext--;
                    // Apply delay to avoid LookAt to actual position while time rewind
                    UniTask.DelayFrame(10).ContinueWith(delegate () { navT.LookAt(navigatorAgent.positions[navigatorAgent.targetNext]); }).Forget();
                }
            }
        }

        // void DrawPath(SessionData sessionData)
        // {
        //     linePathRenderer.positionCount = sessionData.records.Count;
        //     linePathRenderer.startColor = startPathColor;
        //     linePathRenderer.endColor = endPathColor;
        //     for (var x = 0; x < sessionData.records.Count; x++)
        //     {
        //         linePathRenderer.SetPosition(x, sessionData.records[x].position);
        //     }
        // }

    }
}