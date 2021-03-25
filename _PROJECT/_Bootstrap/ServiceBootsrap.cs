using QuizCanners.Inspect;
using System;

namespace QuizCanners.IsItGame.Services
{
    [Serializable]
    public class ServiceBootsrap : IPEGI
    {
       // public NetworkTimeService NetworkTime = new NetworkTimeService();

        public void Inspect()
        {
             Utils.Singleton.Collector.Inspect();
        }
    }
}