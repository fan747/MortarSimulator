using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Other
{
    public class Logger
    {
        private string logFilePath;
        private List<double> _distances;


        public Logger(string path)
        {
            logFilePath = path;
            _distances = new List<double>();
            File.Create(logFilePath);          
        }

        public void LogMessageDistance(string logString, string stackTrace, LogType type)
        {
            string[] logStringArray = logString.Split(' ');
            List<double> distanceList = new List<double>();

            foreach (string i in logStringArray)
            {
                float distance;

                if (float.TryParse(i,out distance))
                {
                    distanceList.Add(Math.Floor(distance));
                }
            }

            if(distanceList.Count != 0 && distanceList[0] % 50 == 0 && !_distances.Contains(distanceList[0]))
            {
                _distances.Add(distanceList[0]);

                using (StreamWriter writer = new StreamWriter(logFilePath, true))
                {
                    writer.WriteLine($"{distanceList[0]} m: {distanceList[1]} MIL");
                }
            }
        }
    }
}
