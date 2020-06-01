using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EasySave.ModelNameSpace.Backup
{
    class MirrorBackup : AbstractBackup
    {
        public MirrorBackup(Dictionary<string, string> backupInformation)
        {
            this.Name = backupInformation["name"];
            this.Source = backupInformation["source"];
            this.Target = backupInformation["target"];
            this.LogsDir = backupInformation["logs"];

            //split the string to get a list of extensions or business softwares
            this.PriorityExtensions = backupInformation["priorityExt"].Split(';').ToList();
            this.BusinessSoftware = backupInformation["businessSoft"].Split(';').ToList();
            this.CryptExtensions = backupInformation["cryptExt"].Split(';').ToList();
        }

        //get the source files and copy them in target dir
        public override void save(string sourceDir, string targetDir, string mirrorDir, string logsDir, bool priorityFile)
        {
            //get info of source directory
            DirectoryInfo sourceDirInfo = new DirectoryInfo(sourceDir);

            //if the target directory doesn't exist, create it.
            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }

            //copy the priority files
            if (priorityFile)
            {
                foreach (string fileExtension in this.PriorityExtensions)
                {
                    if (fileExtension != "")
                    {
                        //get the files in the directory and copy them to the target dir
                        foreach (FileInfo file in sourceDirInfo.GetFiles("*" + fileExtension))
                        {
                            this.copy(file, targetDir);
                        }
                    }
                }
            }

            //copy the not priority files
            else
            {
                foreach (FileInfo file in sourceDirInfo.GetFiles())
                {
                    if (!(this.PriorityExtensions.Contains(file.Extension, StringComparer.OrdinalIgnoreCase)))
                    {
                        this.copy(file, targetDir);
                    }
                }
            }

            //copy the files of all the sub dirs (recursive method)
            this.subDir(sourceDirInfo, "", targetDir, logsDir, priorityFile);
        }

        //count the number of file to save and their total size
        public override int countFiles(string sourceDir, string mirrorDir)
        {
            int nbEligibleFiles = 0;

            DirectoryInfo Directory = new DirectoryInfo(sourceDir);

            //increment the total size and the count of elilible files for each file
            foreach (FileInfo file in Directory.GetFiles())
            {
                this.TotalSize += file.Length;
                nbEligibleFiles++;
            }
            //increment the total size and the count of elilible files for each file in sub dirs
            foreach (DirectoryInfo sourceSubDir in Directory.GetDirectories())
            {
                int nbEligibleSubsFiles = 0;
                nbEligibleSubsFiles = this.countFiles(sourceSubDir.FullName, "");
                nbEligibleFiles = nbEligibleFiles + nbEligibleSubsFiles;
            }

            return nbEligibleFiles;
        }

    }
}
