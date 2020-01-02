using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Changelogger
{
    public struct CustomFileInfo
    {
        public string FileName;
        public string ContainingFolder;
        public DateTime LastModified;

        public StreamReader ReadMasterInfo(StreamReader file) //Only reading from master file, which is stored differently that the generated reports
        {
            FileName = file.ReadLine();
            ContainingFolder = file.ReadLine();
            LastModified = DateTime.Parse(file.ReadLine());
            return file;
        }
        public StreamWriter WriteMasterInfo(StreamWriter file) //Only writing to master file, which is stored differently that the generated reports
        {
            file.WriteLine(FileName);
            file.WriteLine(ContainingFolder);
            file.WriteLine(LastModified.ToString());
            return file;
        }
        public StreamWriter WriteRecordInfo(StreamWriter file)
        {
            file.WriteLine("\t\t" + FileName);
            file.WriteLine("\t\t\tModified on: " + LastModified.ToString("d/M/yyyy h:mmtt"));
            return file;
        }
        public CustomFileInfo(FileInfo file)
        {
            FileName = file.Name;
            ContainingFolder = file.DirectoryName;
            LastModified = file.LastWriteTime;
        }
    }
    class LogRetriever
    {
        private List<CustomFileInfo> FullList = new List<CustomFileInfo>();
        private List<CustomFileInfo> MasterList = new List<CustomFileInfo>();
        
        public void ReadMasterList(string dirAddress)
        {
            if(!Directory.Exists(dirAddress + ProgramSettings.RecordFolder)) { Directory.CreateDirectory(dirAddress + ProgramSettings.RecordFolder); }
            StreamReader file = new StreamReader(File.Open(dirAddress + ProgramSettings.RecordFolder + ProgramSettings.MasterRecordFileName, FileMode.OpenOrCreate, FileAccess.Read));
            while (!file.EndOfStream)
            {
                CustomFileInfo currentRecord = new CustomFileInfo();
                file = currentRecord.ReadMasterInfo(file);
                MasterList.Add(currentRecord);
            }
            file.Close();
        }
        public void GenerateFullList(string targetDir)
        {
            List<FileInfo> files = new List<FileInfo>();
            DirectoryInfo currentDir = new DirectoryInfo(targetDir);
            files.AddRange(currentDir.GetFiles("*", SearchOption.AllDirectories));
            List<FileInfo> disqualified = new List<FileInfo>();
            foreach(FileInfo file in files)
            {
                if(file.FullName.Contains(ProgramSettings.RecordFolder.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries)[0])){ disqualified.Add(file); }
            }
            foreach(FileInfo f in disqualified) { files.Remove(f); }
            foreach(FileInfo f in files)
            {
                FullList.Add(new CustomFileInfo(f));
            }
        }
        public void SaveFullList(string dirAddress)
        {
            if(File.Exists(dirAddress + ProgramSettings.RecordFolder + ProgramSettings.MasterRecordFileName)) { File.Delete(dirAddress + ProgramSettings.RecordFolder + ProgramSettings.MasterRecordFileName); }
            StreamWriter file = new StreamWriter(File.Create(dirAddress + ProgramSettings.RecordFolder + ProgramSettings.MasterRecordFileName));
            foreach(CustomFileInfo f in FullList)
            {
                file = f.WriteMasterInfo(file);
            }
            file.Close();
        }
        public void HandleDiscrepancies(string dirAddress)
        {
            if(MasterList.Count == 0) {
                StreamWriter f = new StreamWriter(File.OpenWrite(dirAddress + ProgramSettings.RecordFolder + ProgramSettings.RecordNameFormat));
                f.WriteLine("New files:");
                f = WriteList(f, FullList, dirAddress);
                f.Close();
                return;
            }
            List<CustomFileInfo> UpdatedFiles = new List<CustomFileInfo>();
            List<int> masterRemoval = new List<int>();
            List<int> fullRemoval = new List<int>();
            foreach(CustomFileInfo f in FullList)
            {
                foreach(CustomFileInfo masterFile in MasterList)
                {
                    if(f.FileName == masterFile.FileName && f.ContainingFolder == masterFile.ContainingFolder)
                    {
                        if(f.LastModified.ToString("d/M/yyyy h:mm") != masterFile.LastModified.ToString("d/M/yyyy h:mm"))
                        {
                            UpdatedFiles.Add(f);
                        }
                        fullRemoval.Add(FullList.IndexOf(f));
                        masterRemoval.Add(MasterList.IndexOf(masterFile));
                    }
                }
            }
            for(int i = masterRemoval.Count - 1; i>=0; i--)
            {
                MasterList.RemoveAt(masterRemoval[i]);
            }
            for (int i = fullRemoval.Count - 1; i >= 0; i--)
            {
                FullList.RemoveAt(fullRemoval[i]);
            }
            StreamWriter file = new StreamWriter(File.OpenWrite(dirAddress + ProgramSettings.RecordFolder + DateTime.Now.ToString(ProgramSettings.RecordNameFormat) + ".txt"));
            if (FullList.Count != 0)
            {
                file.WriteLine("New files:");
                file = WriteList(file, FullList, dirAddress);
            }
            else if(MasterList.Count == 0)
            {
                file.WriteLine("No change");
                file.Close();
                return;
            }
            if (UpdatedFiles.Count != 0)
            {
                file.WriteLine("\n\nUpdated files:");
                file = WriteList(file, UpdatedFiles, dirAddress);
            }
            if (MasterList.Count != 0)
            {
                file.WriteLine("\n\nRemoved files:");
                file = WriteList(file, MasterList, dirAddress);
            }
            file.Close();
        }
        private StreamWriter WriteList(StreamWriter stream, List<CustomFileInfo> files, string dirAddress)
        {
            string currentFullDir = dirAddress;
            stream.WriteLine("\t./");
            foreach(CustomFileInfo f in files)
            {
                if(f.ContainingFolder != currentFullDir)
                {
                    currentFullDir = f.ContainingFolder;
                    stream.WriteLine("\n\t" + currentFullDir.Remove(0, dirAddress.Length));
                }
                stream = f.WriteRecordInfo(stream);
            }
            return stream;
        }
    }
}
