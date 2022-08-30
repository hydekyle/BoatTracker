using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace PathTracer
{
    [Serializable]
    public class SessionData
    {
        public string _id,
        device,
        device_user_id,
        end_timestamp,
        firestore_user_id;
        public GeolocationData geolocation;
        public List<PathRecord> records;

        /// <summary> Calculates and stores in each record the Vector3 position for Unity </summary>
        public void PrecalculateAllRecordPosition()
        {
            var startPoint = new Vector3(records[0].lat * Mathf.Pow(10, 6), 0, records[0].lng * Mathf.Pow(10, 6)) / 10;
            foreach (var record in records)
            {
                record.position = new Vector3(record.lat * Mathf.Pow(10, 6), 0, record.lng * Mathf.Pow(10, 6)) / 10 - startPoint;
            }
        }

        public static async UniTask<SessionData> GetSessionDataBySessionID(string sessionID)
        {
            // var endpoint = "https://developkanarasports.herokuapp.com/getSession?session_id=" + sessionID;
            // var webRequest = UnityWebRequest.Get(endpoint);
            // await webRequest.SendWebRequest();
            // if (webRequest.result == UnityWebRequest.Result.ConnectionError) throw new Exception("Connection Error");
            // var session = JsonUtility.FromJson<SessionData>(webRequest.downloadHandler.text);
            // session.PrecalculateAllRecordPosition();
            // return session;

            //TODO: Use JSON from server instead
            await UniTask.Delay(1);
            TextAsset targetFile = Resources.Load<TextAsset>("data-points");
            var session = JsonUtility.FromJson<SessionData>(targetFile.text);
            session.PrecalculateAllRecordPosition();
            return session;
        }
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
    public class PathRecord
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
        public Vector3 position; // This field is for caching LatLng as Vector3
    }
}
