using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace EasySave.ModelNameSpace.Backup
{
    sealed class Logs
    {
        private static Logs instance = null;
        private string serializeObj;
        private static readonly object padlock = new object();

        //method to get instance for Singleton
        public static Logs GetInstance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new Logs();
                    }
                    return instance;
                }
            }
        }

        //create one daily logs with information from all the backups of the day.
        public void createDailyLogs(AbstractBackup backup, string logsDir)
        {
            string dailyLogsDir = logsDir + @"/DailyLogs/";

            if (!Directory.Exists(dailyLogsDir))
            {
                Directory.CreateDirectory(dailyLogsDir);
            }

            //only one thread at a time can write when a backup is finished
            lock (this)
            {
                //serialize properties of the JsonObject (AbstractBackup)
                this.serializeObj = JsonConvert.SerializeObject(backup, Formatting.Indented) + Environment.NewLine;
                
                //write in the file
                File.AppendAllText(dailyLogsDir + @"DailyLogs_" + DateTime.Now.ToString("dd-MM-yyyy") + ".json", this.serializeObj);
            }
        }

        //for each backup on realtime log is created
        public void createRealTimeLogs(Dictionary<string, string> jsonAttribute, string name, string logsDir)
        {
            string realTimeLogsDir = logsDir + @"/RealTimeLogs/";
            string file = name + ".json";

            if (!Directory.Exists(realTimeLogsDir))
            {
                Directory.CreateDirectory(realTimeLogsDir);
            }

            //serialize the information backup dictionnary 
            this.serializeObj = JsonConvert.SerializeObject(jsonAttribute, Formatting.Indented) + Environment.NewLine;

            //write in the file
            File.AppendAllText(realTimeLogsDir + file, this.serializeObj);
        }
    }
}
