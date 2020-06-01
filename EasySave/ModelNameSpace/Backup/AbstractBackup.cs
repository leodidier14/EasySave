using EasySave.ObserverNameSpace;
using EasySave.ViewModelNameSpace;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Linq;

namespace EasySave.ModelNameSpace.Backup
{
    //JsonObject create a json object with jsonproperty to serialize it in the createHistory method
    [JsonObject(MemberSerialization.OptIn)]
    public abstract class AbstractBackup : INotifyPropertyChanged, IObservable
    {
        private bool _isPausedBs = false;
        private bool _isPaused = false;
        private bool _eventPauseState = false;
        private bool _isChecked = true;
        private bool _isStopped = false;
        private bool _isRunned = false;
        private int _progress = 0;
        private int encryptionTime = 0;
        private string _currentFile = "";
        private string _remainingFiles = "";
        
        [JsonProperty]
        public string Name { get; set; }
        [JsonProperty]
        public string Source { get; set; }
        [JsonProperty]
        public string Target { get; set; }
        public string Mirror { get; set; } = "";
        public string LogsDir { get; set; } = "";
        [JsonProperty]
        public string BackupDate { get; set; }
        [JsonProperty]
        public long TotalSize { get; set; }
        [JsonProperty]
        public string TransactionTime { get; set; }
        [JsonProperty]
        public string EncryptionTime { get; set; }       

        public List<string> PriorityExtensions { get; set; }
        public List<string> CryptExtensions { get; set; }
        public List<string> BusinessSoftware { get; set; }

        private List<IObserver> _observers = new List<IObserver>();

        //stock jsonAttrributes for the real time logs
        public Dictionary<string, string> jsonAttributes = new Dictionary<string, string>();

        public AutoResetEvent pauseEvent = new AutoResetEvent(false);

        public Cryptosoft.Cryptosoft cryptosoft = new Cryptosoft.Cryptosoft();

        public event PropertyChangedEventHandler PropertyChanged;

        //abstract method implement in mirror class et differential class to save files 
        public abstract void save(string sourceDir, string targetDir, string mirrorDir, string logsDir, bool priorityFile);

        //abstract method implement to count the eligible files and the total size of these before launch save in viewModel
        public abstract int countFiles(string sourceDir, string mirrorDir);

        //use cryptosoft to copy and encrypt eligible file
        public void cryptosoftCopy(FileInfo file, string targetDir)
        {
            string sourceFile= file.FullName;
            string targetFile = Path.Combine(targetDir, file.Name);

            this.cryptosoft.encrypt(sourceFile, targetFile);

            //Exit code is the return of cryptosoft program = stopwatch
            encryptionTime += this.cryptosoft.encrypt(sourceFile, targetFile);

            //set the Last Write Time of the file with the encryption date of the file so as not to corrupt a potential differential backup
            new FileInfo(targetFile).LastWriteTime = File.GetLastWriteTime(file.FullName);
        }

        //copy a file in target dir
        public void copy(FileInfo file, string targetDir)
        {
            string targetFile = Path.Combine(targetDir, file.Name);
            this.CurrentFile = file.FullName;
            long maxFileSize = 0;

            //notify the viewModel that the backup has progressed  
            this.notify("IHM");

            //while business software is running, pause copying 
            while (this.CheckBusinessSoftware())
            {
                this.IsPausedBs = true;
                this.pauseEvent.WaitOne(1000);
                this.IsPausedBs = false;
            }

            //if IsPaused is set to true in viewModel, pause copying
            if (this.IsPaused)
            {
                this.EventPauseState = true;
                //pauseEvent wait a set before resume
                this.pauseEvent.WaitOne();
            }

            try
            {
                maxFileSize = long.Parse(ViewModel.MaxFileSize) * 1000;
            }

            //the maximum maxFileSize is 1TO
            catch (OverflowException)
            {
                maxFileSize = 1000000000000;
            }

            //check if the file length correspond to max file size parameted
            if (file.Length > maxFileSize)
            {
                //if yes, autorize only one thread to copy 
                ViewModel.maxFileSizeMutex.WaitOne();

                //check if the file extension should be encrypted 
                if (this.CryptExtensions.Contains(file.Extension, StringComparer.OrdinalIgnoreCase))
                {
                    this.cryptosoftCopy(file, targetDir);
                }
                else
                {                  
                    file.CopyTo(targetFile, true);
                }

                //copy of a file having a max size is finished, release the mutex to continue the copy of the other files 
                ViewModel.maxFileSizeMutex.ReleaseMutex();
            }
            else
            {
                if (this.CryptExtensions.Contains(file.Extension, StringComparer.OrdinalIgnoreCase))
                {
                    this.cryptosoftCopy(file, targetDir);
                }
                else
                {
                    file.CopyTo(targetFile, true);
                }

            }
            //write in realtimelogs = progress
            realTimeLogs(file);       
        }

        //launch save method for each subdir to copy all the files
        public void subDir(DirectoryInfo sourceDirInfo, string mirrorDir, string targetDir, string logsDir, bool priorityFile)
        {
            foreach (DirectoryInfo subDir in sourceDirInfo.GetDirectories())
            {
                this.save(subDir.FullName, Path.Combine(targetDir, subDir.Name), Path.Combine(mirrorDir, subDir.Name), logsDir, priorityFile);
            }
        }

        //launch the checked backups on interface
        public void launchBackup(Barrier priorityBarrier)
        {
            try
            {
                //start stop watch transaction time
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                //copy the priority files
                this.save(this.Source, this.Target, this.Mirror, this.LogsDir, true);

                //wait until all launched backups have finished copying their priority files before copying the non-priority ones
                priorityBarrier.SignalAndWait();

                //remove all the participants of the barrier 
                if (priorityBarrier.ParticipantCount > 0)
                {
                    priorityBarrier.RemoveParticipants(priorityBarrier.ParticipantCount);
                }

                //copy the not priority files
                this.save(this.Source, this.Target, this.Mirror, this.LogsDir, false);

                this.TransactionTime = stopwatch.ElapsedMilliseconds + "ms";
                this.EncryptionTime = encryptionTime + "ms";
                this.BackupDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");

                //write the backup information in the daily logs
                Logs.GetInstance.createDailyLogs(this, this.LogsDir);

            }

            //when backup is stopped
            catch (ThreadAbortException)
            {
                //write in the daily log even if the backup is stopped
                Logs.GetInstance.createDailyLogs(this, this.LogsDir);

                //remove one participant because one thread is aborted
                if (priorityBarrier.ParticipantCount > 0)
                {
                    priorityBarrier.RemoveParticipant();
                }

                //if the stopped backup was using a mutex, release it. 
                try
                {
                    ViewModel.maxFileSizeMutex.ReleaseMutex();
                }

                catch (ApplicationException)
                {

                }
            }

            //if a differentia backup copy 0 file
            if(this.Progress == 0)
            {
                this.Progress = 100;
            }
            //notify the viewModel that the backup has progressed 
            this.notify("IHM");
            
            //notify viewModel that the backup is finished
            this.notify("finished");
        }

        //call the method to add jsonAttributes to the dictionary and launch the createHistory method 
        public void realTimeLogs(FileInfo file)
        {
            //to initialize eligibleFiles, remainingFiles and remainingFilesSize
            if (!this.jsonAttributes.ContainsKey("eligibleFiles"))
            {
                this.AddToJsonAttribute("eligibleFiles", this.countFiles(this.Source, this.Mirror).ToString());
                this.RemainingFiles = this.jsonAttributes["eligibleFiles"];
                this.AddToJsonAttribute("remainingFiles", this.RemainingFiles);
                this.AddToJsonAttribute("remainingFilesSize", this.TotalSize.ToString());
            }

            this.AddToJsonAttribute("writtingTime", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
            this.AddToJsonAttribute("currentFile", this.CurrentFile);
            this.AddToJsonAttribute("totalSize", this.TotalSize.ToString());
            this.RemainingFiles = (int.Parse(this.jsonAttributes["remainingFiles"]) - 1).ToString();
            this.AddToJsonAttribute("remainingFiles", this.RemainingFiles);
            this.AddToJsonAttribute("remainingFilesSize", (long.Parse(this.jsonAttributes["remainingFilesSize"]) - file.Length).ToString());

            try
            {
                this.Progress = (int)((this.TotalSize - long.Parse(this.jsonAttributes["remainingFilesSize"])) * 100 / this.TotalSize);
            }
            catch (DivideByZeroException)
            {
                this.Progress = 100;
            }

            this.AddToJsonAttribute("progress", this.Progress.ToString());

            //create the realtime with the jsonAttributes information
            Logs.GetInstance.createRealTimeLogs(this.jsonAttributes, this.Name, this.LogsDir);     
        }

        //check if one of the business software in the list is running on the pc
        public bool CheckBusinessSoftware()
        {
            foreach (string process in this.BusinessSoftware)
            {
                if (Process.GetProcessesByName(process).Length > 0)
                {
                    return true;
                }
            }
            return false;
        }

        //add jsonAttributes to the dictionary, if they already exist, replace them
        public void AddToJsonAttribute(string key, string value)
        {
            try
            {
                this.jsonAttributes.Add(key, value);
            }
            catch
            {
                this.jsonAttributes[key] = value;
            }
        }

        public void attach(IObserver observer)
        {
            this._observers.Add(observer);
        }
        public void detach(IObserver observer)
        {
            this._observers.Remove(observer);
        }
        public void notify(string type)
        {
            if (type == "IHM")
            {
                foreach (var observer in _observers)
                {
                    //update the progress of one backup on the remote client
                    observer.updateProgress(this);
                }
            }
            else
            {
                foreach (var observer in _observers)
                {
                    //call when one or more backups are finished
                    observer.updateFinishedBackup();
                }
            }

        }

        //notifies a client that a property value has changed
        public void onPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        //---------------------------Properties ----------------------------------//

        public bool IsChecked
        {
            get
            {
                return _isChecked;
            }
            set
            {
                if (value == _isChecked) return;
                _isChecked = value;
                this.onPropertyChanged("IsChecked");
            }
        }
        public bool IsRunned
        {
            get
            {
                return _isRunned;
            }
            set
            {
                if (value == _isRunned) return;
                _isRunned = value;
                this.onPropertyChanged("IsRunned");
            }
        }
        public bool IsStopped
        {
            get
            {
                return _isStopped;
            }
            set
            {
                if (value == _isStopped) return;
                _isStopped = value;
                this.onPropertyChanged("IsStopped");
            }
        }
        public bool IsPaused
        {
            get
            {
                return _isPaused;
            }
            set
            {
                if (value == _isPaused) return;
                _isPaused = value;
                this.onPropertyChanged("IsPaused");
            }
        }
        public bool EventPauseState
        {
            get
            {
                return _eventPauseState;
            }
            set
            {
                if (value == _eventPauseState) return;
                _eventPauseState = value;
                this.onPropertyChanged("EventPauseState");
            }
        }
        public bool IsPausedBs
        {
            get
            {
                return _isPausedBs;
            }
            set
            {
                if (value == _isPausedBs) return;
                _isPausedBs = value;
                this.onPropertyChanged("IsPausedBs");
            }
        }
        public int Progress
        {
            get
            {
                return _progress;
            }
            set
            {
                if (value == _progress) return;
                _progress = value;
                this.onPropertyChanged("Progress");
            }
        }
        public string CurrentFile
        {
            get
            {
                return _currentFile;
            }
            set
            {
                if (value == _currentFile) return;
                _currentFile = value;
                this.onPropertyChanged("CurrentFile");
            }
        }
        public string RemainingFiles
        {
            get
            {
                return _remainingFiles;
            }
            set
            {
                if (value == _remainingFiles) return;
                _remainingFiles = value;
                this.onPropertyChanged("RemainingFiles");
            }
        }

    }
}

