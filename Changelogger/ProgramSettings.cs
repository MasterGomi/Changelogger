using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Changelogger
{
    class ProgramSettings
    {
        //Settings (pre-filled with defaults for if no settings file exists)
        public static string RecordFolder = "/__ChangeloggerRecords/";
        public static string RecordNameFormat = "d-M-yyyy hh-mm tt";
        public static string MasterRecordFileName = "__MasterRecord.dat";

        private static readonly string _settingsRelFilePath = "./ChangeloggerSettings.dat";

        //TODO: Add custom settings for each folder (A settings file in the records folder) that can include things like exclusions

        public static void ReadSettings()
        {
            FileInfo settingsFile = new FileInfo(_settingsRelFilePath);
            if (!File.Exists(_settingsRelFilePath))
            {
                StreamWriter file = new StreamWriter(settingsFile.Create());
                file.WriteLine("RecordFolder: " + RecordFolder);
                file.WriteLine("RecordNameFormat: " + RecordNameFormat);
                file.WriteLine("MasterRecordFileName: " + MasterRecordFileName);
                file.Close();
            }
            else
            {
                StreamReader file = new StreamReader(_settingsRelFilePath);
                RecordFolder = file.ReadLine().Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries)[1].Trim();
                RecordNameFormat = file.ReadLine().Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries)[1].Trim();
                MasterRecordFileName = file.ReadLine().Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries)[1].Trim();
                file.Close();
            }
        }
    }
}
