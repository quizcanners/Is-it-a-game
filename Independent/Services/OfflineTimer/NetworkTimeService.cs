using QuizCanners.Inspect;
using QuizCanners.Utils;
using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.Networking;

namespace QuizCanners.IsItGame
{
    public class NetworkTimeService : Singleton.ClassBase, IPEGI
    {
        private const string SERVER = "http://www.google.com/";

        private DateTime _serverTime;
        private UnityWebRequestAsyncOperation _request;

        private float _unityTimeWhenRequestSent;
        private float UnityTime => UnityEngine.Time.realtimeSinceStartup;

        private bool _isTimeValid;
        public bool IsTimeValid
        {
            get
            {
                CheckRequest();
                return _isTimeValid;
            }
            private set => _isTimeValid = value;
        }

        public DateTime Time
        {
            get
            {
                CheckRequest();

                if (!IsTimeValid)
                {
                    SendRequest();

                    Debug.LogError("Time was returned before it was requested");

                    return DateTime.UtcNow;
                }

                return _serverTime;
            }
        }

        public void SendRequest()
        {
            if (_request == null)
            {
                _request = UnityWebRequest.Get(SERVER).SendWebRequest();
                _unityTimeWhenRequestSent = UnityTime;
            }
            else
            {
                CheckRequest();
            }
        }

        public void Inspect()
        {
            if (IsTimeValid)
                "Last Network Time: {0}".F(Time.ToString()).PegiLabel().write();
            else
                "Is Invalid".PegiLabel().write();

            if (_request != null)
                icon.Wait.draw(_request.webRequest.result.ToString()); //.nl();
            else
                "Request Time".PegiLabel().Click(() =>
                {
                    _request = null;
                    SendRequest();
                });

            pegi.nl();

         
            
        }

        private void CheckRequest()
        {
            if (_request != null)
            {
                if (_request.isDone)
                {
                    if (_request.webRequest.result == UnityWebRequest.Result.Success)
                    {
                        string netTime = _request.webRequest.GetResponseHeader("date");
                        var time = DateTime.ParseExact(netTime,
                                        "ddd, dd MMM yyyy HH:mm:ss 'GMT'",
                                        CultureInfo.InvariantCulture.DateTimeFormat,
                                        DateTimeStyles.AssumeUniversal).ToUniversalTime();

                        _serverTime = time;
                        IsTimeValid = true;
                    }
                    else
                    {
                        IsTimeValid = false;
                    }

                    _request = null;
                }
            }
            else if ((UnityTime - _unityTimeWhenRequestSent) > 100)
            {
                SendRequest();
            }
        }
    }
}