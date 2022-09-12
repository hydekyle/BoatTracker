using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace PathTracer
{
    [Serializable]
    public class NavigatorAgent
    {
        public Transform navT;
        public SessionData mySessionData;
        public List<Vector3> positions = new();
        public float traveledDistanceTotal = 0f;
        public int maxTargets, targetNext = 1;
        public int navigationIndex = 0, navigationIndexPrev = 0, navigationIndexNext = 1;

        public NavigatorAgent(SessionData sessionData)
        {
            this.mySessionData = sessionData;
            maxTargets = mySessionData.records.Count;
            var newGO = GameObject.Instantiate(SessionManager.Instance.navigatorPlaceholder);
            navT = newGO.transform;
            CalculateAllRecordPosition();
        }

        /// <summary> Calculate positions from records to start from Vector3.zero  </summary>
        void CalculateAllRecordPosition()
        {
            var startPoint = new Vector3(mySessionData.records[0].lat * Mathf.Pow(10, 6), 0, mySessionData.records[0].lng * Mathf.Pow(10, 6)) / 10;
            foreach (var record in mySessionData.records)
            {
                positions.Add(new Vector3(record.lat * Mathf.Pow(10, 6), 0, record.lng * Mathf.Pow(10, 6)) / 10 - startPoint);
            }
        }

        public static async UniTask<NavigatorAgent> GetNavigatorAgentBySessionID(string sessionID)
        {
            var endpoint = "https://developkanarasports.herokuapp.com/getSession?session_id=" + sessionID;
            var webRequest = UnityWebRequest.Get(endpoint);
            await webRequest.SendWebRequest();
            if (webRequest.result == UnityWebRequest.Result.ConnectionError) throw new Exception("Connection Error");
            var session = JsonUtility.FromJson<SessionData>(webRequest.downloadHandler.text);
            var navAgent = new NavigatorAgent(session);
            return navAgent;
        }

        public static NavigatorAgent GetNavigatorAgentTest()
        {
            TextAsset targetFile = Resources.Load<TextAsset>("data-points");
            var session = JsonUtility.FromJson<SessionData>(targetFile.text);
            var navAgent = new NavigatorAgent(session);
            return navAgent;
        }

        public PathRecord GetTargetPathRecord()
        {
            return mySessionData.records[targetNext];
        }

        public PathRecord GetPreviousPathRecord()
        {
            return mySessionData.records[targetNext - 1];
        }

        public Vector3 GetTargetPosition()
        {
            return positions[targetNext];
        }

        public Vector3 GetPreviousTargetPosition()
        {
            return positions[targetNext - 1];
        }
    }

    [Serializable]
    public struct SessionData
    {
        public string _id,
        device,
        device_user_id,
        end_timestamp,
        firestore_user_id;
        public GeolocationData geolocation;
        public List<PathRecord> records;
    }

    [Serializable]
    public struct GeolocationData
    {
        public float lat_center,
        lat_max,
        lat_mean,
        lat_min,
        lng_center,
        lng_max,
        lng_mean,
        lng_min;
    }

    [Serializable]
    public struct PathRecord
    {
        public string timestamp;
        public float geoDist,
        geoTime,
        lat,
        lng,
        speed,
        speedInKnots,
        twa,
        vmg;
    }
}
