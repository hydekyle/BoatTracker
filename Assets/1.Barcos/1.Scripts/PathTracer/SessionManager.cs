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
        [Tooltip("Load multiples sessions at time")]
        public List<string> sessionIDs;
        [HideInInspector]
        public Observable<NavigatorAgent> focusAgent;
        public Observable<float> navigationTimeVelocity = new() { Value = 1f };
        public bool isNavigating = false;
        public List<NavigatorAgent> navAgents = new();
        public GameObject navigatorPlaceholder;
        public OrbitCamera orbitCamera;

        void Awake()
        {
            Instance = this;
        }

        async void Start()
        {
            var myNavigatorAgent = await NavigatorAgent.GetNavigatorListBySessionList(sessionIDs);
            NavegationStart(myNavigatorAgent);
        }

        void NavegationStart(List<NavigatorAgent> navigatorAgentList)
        {
            foreach (var navigatorAgent in navigatorAgentList)
            {
                navigatorAgent.navT.position = navigatorAgent.positions[0];
                navigatorAgent.navT.LookAt(navigatorAgent.positions[1]);
                orbitCamera.Target = navigatorAgent.navT.GetComponent<FocusPoint>();
                focusAgent.Value = navigatorAgent;
                navAgents.Add(navigatorAgent);
            }
            orbitCamera.enabled = true;
            isNavigating = true;
            NavigationRoutine().Forget();
        }

        async UniTaskVoid NavigationRoutine()
        {
            while (isNavigating)
            {
                for (var x = 0; x < navAgents.Count; x++)
                {
                    Navigate(navAgents[x]);
                }
                await UniTask.Yield(PlayerLoopTiming.Update);
            }
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

    }
}