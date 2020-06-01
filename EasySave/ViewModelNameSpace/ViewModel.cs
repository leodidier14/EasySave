using EasySave.ModelNameSpace;
using EasySave.ModelNameSpace.Backup;
using EasySave.ModelNameSpace.Factory;
using EasySave.ModelNameSpace.Server;
using EasySave.ObserverNameSpace;
using EasySave.ViewModelNameSpace.Commands;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace EasySave.ViewModelNameSpace
{
    class ViewModel : INotifyPropertyChanged, IObserver
    {
        private string _name = "";
        private string _sourcePath = "";
        private string _mirrorPath = "";
        private string _targetPath = "";
        private bool _listIsEmpty = true;
        private bool _listFinishedIsEmpty = true;
        private string _logsPath = @"Logs/";
        private string _visibility = "Hidden";
        private string _messageText = "";
        private string _messageColor = "";
        private string _language = "en";
        private string _priorityField = "";
        private string _priorityExt = "";
        private string _cryptField = "";
        private string _cryptExt = "";
        private string _businessSoftField = "";
        private string _businessSoft = "";
        private string _maxFileSizeField = "";
        private static string _maxFileSize = "1000000";

        public ObservableCollection<AbstractBackup> BackupList { get; set; }
        public ObservableCollection<string> TranslationList { get; set; }
        public ObservableCollection<string> PriorityExtList { get; set; }
        public ObservableCollection<string> BusinessSoftList { get; set; }
        public ObservableCollection<string> CryptExtList { get; set; }

        public Dictionary<string, string> backupInformation = new Dictionary<string, string>();
        public Dictionary<string, Thread> threadDictionary = new Dictionary<string, Thread>();

        public event PropertyChangedEventHandler PropertyChanged;
        public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;

        public MainFactory BackupFactory { get; set; }
        public Checker checker;
        public Server server;
        public Thread serverThread;

        public static Mutex maxFileSizeMutex = new Mutex();
        private Barrier priorityBarrier;

        public OpenDirCmd OpenDirCmd { get; set; }
        public AddCmd AddCmd { get; set; }
        public LaunchCmd LaunchCmd { get; set; }
        public BackupModeCmd BackupModeCmd { get; set; }
        public RemoveBackupCmd RemoveBackupCmd { get; set; }
        public PauseCmd PauseCmd { get; set; }
        public ResumeCmd ResumeCmd { get; set; }
        public StopCmd StopCmd { get; set; }
        public SwitchLanguageCmd SwitchLanguageCmd { get; set; }
        public RemovePriorityExtCmd RemovePriorityExtCmd { get; set; }
        public RemoveBusinessSoftCmd RemoveBusinessSoftCmd { get; set; }
        public RemoveCryptExtCmd RemoveCryptExtCmd { get; set; }

        public ViewModel()
        {
            this.PriorityExtList = new ObservableCollection<string> { ".txt" };
            this.BusinessSoftList = new ObservableCollection<string>() { "WINWORD" };
            this.CryptExtList = new ObservableCollection<string>() { ".txt" };
            this.BackupList = new ObservableCollection<AbstractBackup>();
            this.TranslationList = new ObservableCollection<string>();

            this.OpenDirCmd = new OpenDirCmd(this);
            this.AddCmd = new AddCmd(this);
            this.LaunchCmd = new LaunchCmd(this);
            this.BackupModeCmd = new BackupModeCmd(this);
            this.StopCmd = new StopCmd(this);
            this.PauseCmd = new PauseCmd(this);
            this.ResumeCmd = new ResumeCmd(this);
            this.SwitchLanguageCmd = new SwitchLanguageCmd(this);
            this.RemoveBackupCmd = new RemoveBackupCmd(this);
            this.RemovePriorityExtCmd = new RemovePriorityExtCmd(this);
            this.RemoveBusinessSoftCmd = new RemoveBusinessSoftCmd(this);
            this.RemoveCryptExtCmd = new RemoveCryptExtCmd(this);

            this.priorityBarrier = new Barrier(0);

            this.switchLanguage("default");

            this.BackupFactory = new MirrorFactory();

            checker = new Checker(this);

            server = new Server();
            server.attach(this);
            serverThread = new Thread(() => server.startServer());
            serverThread.Name = "serverThread";
            serverThread.Start();
        }

        //change the backup mode
        public void backupMode(string mode)
        {
            if (mode == "mirror")
            {
                //hide the mirror field
                this.Visibility = "Hidden";
                this.resetFields();
                this.messageConsole(this.TranslationList[47], true);

                //spectify the factory type to use 
                this.BackupFactory = new MirrorFactory();
            }

            else
            {
                //show the mirror field
                this.Visibility = "Visible";
                this.resetFields();
                this.messageConsole(this.TranslationList[48], true);

                //spectify the factory type to use 
                this.BackupFactory = new DifferentialFactory();
            }
        }

        //add backup in list 
        public void addBackup()
        {

            //check if user can add a backup
            if (!checker.checkIfCanAdd())
            {
                return;
            }

            //check if the list of priority extensions is empty and if not transform it into a string
            if (this.PriorityExtList.Any())
            {
                foreach (string extention in this.PriorityExtList)
                {
                    this.PriorityExt += extention + ";";
                }
                this.PriorityExt = this.PriorityExt.Remove(this.PriorityExt.Length - 1, 1);
            }

            //check if the list of crypt extensions is empty and if not transform it into a string
            if (this.CryptExtList.Any())
            {
                foreach (string extention in this.CryptExtList)
                {
                    this.CryptExt += extention + ";";
                }
                this.CryptExt = this.CryptExt.Remove(this.CryptExt.Length - 1, 1);
            }

            //check if the list of business software is empty and if not transform it into a string
            if (this.BusinessSoftList.Any())
            {
                foreach (string software in this.BusinessSoftList)
                {
                    this.BusinessSoft += software + ";";
                }
                this.BusinessSoft = this.BusinessSoft.Remove(this.BusinessSoft.Length - 1, 1);
            }

            //add backup information to the dictionary
            this.backupInformation.Add("name", this.Name);
            this.backupInformation.Add("source", this.SourcePath);
            this.backupInformation.Add("target", this.TargetPath);
            this.backupInformation.Add("mirror", this.MirrorPath);
            this.backupInformation.Add("logs", this.LogsPath);
            this.backupInformation.Add("priorityExt", this.PriorityExt);
            this.backupInformation.Add("businessSoft", this.BusinessSoft);
            this.backupInformation.Add("cryptExt", this.CryptExt);

            //calls the factory create method by passing the backup information
            AbstractBackup abstractBackup = this.BackupFactory.createBackup(backupInformation);

            //add the created backup to the list of backups to run
            this.BackupList.Add(abstractBackup);

            //the backup is now observed
            abstractBackup.attach(this);

            //call this method to notify the server that the backup list has been updated.
            this.updateBackupList();

            messageConsole(this.Name + this.TranslationList[32], true);

            this.ListIsEmpty = false;
            this.resetFields();
            this.backupInformation.Clear();

            this.PriorityExt = "";
            this.CryptExt = "";
            this.BusinessSoft = "";
        }

        //launch the selected backup(s)
        public void run()
        {
            //check if at least one backup can be started. If not, quit run method
            if (!checker.checkIfBackupCanBeRunned())
            {
                return;
            }

            //check if list contains element
            if (BackupList.Any())
            {
                Thread threadBackup;
                foreach (AbstractBackup backup in BackupList)
                {
                    //check is the backup is checked to run it
                    if (backup.IsChecked)
                    {
                        /*add a participant to the priority barrier. 
                        it is used in the model to wait until all launched backups 
                        have finished copying their priority files before copying the non-priority ones*/
                        this.priorityBarrier.AddParticipant();

                        //call the launchBackup method in a thread to launch the checked backup
                        threadBackup = new Thread(() => backup.launchBackup(priorityBarrier));

                        //add this thread in a dictionnary
                        threadBackup.Name = backup.Name;
                        threadDictionary.Add(threadBackup.Name, threadBackup);

                        //start the thread
                        threadBackup.Start();

                        backup.IsRunned = true;
                        backup.IsChecked = false;
                    }
                }
            }

            else
            {
                messageConsole(TranslationList[24], false);
            }
        }

        //resume a paused backup
        public void resume(string backupName)
        {
            foreach (AbstractBackup backup in BackupList)
            {
                if (backup.Name == backupName)
                {
                    //the set method of AutoResetEvent frees the paused thread
                    if (backup.EventPauseState)
                    {
                        backup.IsPaused = false;
                        backup.pauseEvent.Set();
                        backup.EventPauseState = false;
                    }


                }
            }
        }

        //pause a runned backup
        public void pause(string backupName)
        {
            foreach (AbstractBackup backup in BackupList)
            {
                if (backup.Name == backupName)
                {
                    //set the IsPaused boolean to true to prevent the model from pausing the thread
                    backup.IsPaused = true;
                }
            }
        }

        //stop a runned or a paused backup
        public void stop(string backupName)
        {
            //search for the corresponding thread in the dictionary to abort it
            if (threadDictionary.ContainsKey(backupName))
            {
                threadDictionary[backupName].Abort();
            }

            foreach (AbstractBackup backup in BackupList)
            {
                if (backup.Name == backupName)
                {
                    backup.IsStopped = true;
                    backup.IsPaused = false;
                    backup.IsPausedBs = false;
                    this.ListFinishedIsEmpty = false;
                }
            }
        }

        //remove selected backup(s) from the list 
        public void removeSelected()
        {
            //check if backups are available for removing
            if (!checker.checkIfBackupCanBeRunned())
            {
                return;
            }

            if (!this.ListIsEmpty)
            {
                /*a problem happens if we remove an item from collection while iterating over it. 
                to fix this problem, iterate over a copy of the collection whose count does not change*/
                var fixedSize = BackupList.ToArray();
                foreach (var backup in fixedSize)
                {
                    if (backup.IsChecked)
                    {
                        BackupList.Remove(backup);
                    }
                };
                messageConsole(this.TranslationList[49], true);
            }
            else
            {
                messageConsole(this.TranslationList[36], false);
            }

            if (BackupList.Any())
            {
                this.ListIsEmpty = false;
            }
            else
            {
                this.ListIsEmpty = true;
            }
            //call this method to notify the server that the backup list has been updated.
            this.updateBackupList();
        }

        //remove selected backup(s) from the list 
        public void removeFinished()
        {
            /*a problem happens if we remove an item from collection while iterating over it. 
            to fix this problem, iterate over a copy of the collection whose count does not change*/
            var fixedSizeBackûp = BackupList.ToArray();
            foreach (var backup in fixedSizeBackûp)
            {
                if (backup.Progress == 100 || backup.IsStopped)
                {
                    BackupList.Remove(backup);

                    //remove thread finish from the threadDictionary
                    var fixedSizeThread = threadDictionary.ToArray();
                    foreach (KeyValuePair<string, Thread> thread in fixedSizeThread)
                    {
                        if (thread.Key == backup.Name)
                        {
                            threadDictionary.Remove(thread.Key);
                        }
                    };
                }
            };

            this.ListFinishedIsEmpty = true;

            if (BackupList.Any())
            {
                this.ListIsEmpty = false;
            }
            else
            {
                this.ListIsEmpty = true;
            }

            messageConsole(this.TranslationList[46], true);

            //call this method to notify the server that the backup list has been updated.
            this.updateBackupList();
        }

        //reset the backup information fields
        public void resetFields()
        {
            this.Name = "";
            this.SourcePath = "";
            this.MirrorPath = "";
            this.TargetPath = "";
        }

        //display an error or succes message in a textbox
        public void messageConsole(string msg, bool success)
        {
            if (success)
            {
                this.MessageText = msg;
                this.MessageColor = "Green";
            }
            else
            {
                this.MessageText = msg;
                this.MessageColor = "Red";
            }
        }

        //switch language to french or english
        public async void switchLanguage(string language)
        {
            this.LanguageValue = language;
            this.TranslationList.Clear();

            await Task.Run(() =>
            {
                //call the static method switchLanguage to add the elements of the returned list on the translation list
                foreach (string value in Language.switchLanguage(language))
                {
                    this.TranslationList.Add(value);
                }
            });
            if (language == "default")
            {
                this.messageConsole("Welcome to EasySave !", true);
            }
            else
            {
                this.messageConsole(this.TranslationList[37], true);
            }

        }

        //add a priority extension to the list      
        public void addPriorityExt()
        {
            //extension can only contain alphanumeric characters
            Regex regex = new Regex("^[a-zA-Z0-9]+$");

            //check if the field is filled in
            if (this.PriorityField == "")
            {
                messageConsole(this.TranslationList[38], false);
                return;
            }

            //check if the extension matches with the regex
            if (regex.IsMatch(this.PriorityField))
            {
                //check if the extension is already on list and if not add it
                if (!this.PriorityExtList.Contains("." + this.PriorityField, StringComparer.OrdinalIgnoreCase))
                {
                    this.PriorityExtList.Add("." + this.PriorityField);
                    messageConsole(this.TranslationList[39], true);
                }
                else
                {
                    messageConsole(this.TranslationList[40], false);
                }
                this.PriorityField = "";
            }
            else
            {
                messageConsole(this.TranslationList[41], false);
            }
        }

        //remove a priority extension from the list 
        public void removePriorityExt(string priorityExt)
        {
            this.PriorityExtList.Remove(priorityExt);
        }

        //add a crypt extension to the list 
        public void addCryptExt()
        {
            //extension can only contain alphanumeric characters
            Regex regex = new Regex("^[a-zA-Z0-9]+$");

            //check if the field is filled in
            if (this.CryptField == "")
            {
                messageConsole(this.TranslationList[38], false);
                return;
            }

            //check if the extension matches with the regex
            if (regex.IsMatch(this.CryptField))
            {
                //check if the extension is already on list and if not add it
                if (!this.CryptExtList.Contains("." + this.CryptField, StringComparer.OrdinalIgnoreCase))
                {
                    this.CryptExtList.Add("." + this.CryptField);
                    messageConsole(this.TranslationList[39], true);
                }
                else
                {
                    messageConsole(this.TranslationList[40], false);
                }
                this.CryptField = "";
            }
            else
            {
                messageConsole(this.TranslationList[41], false);
            }
        }

        //remove a crypt extension from the list 
        public void removeCryptExt(string cryptExt)
        {
            this.CryptExtList.Remove(cryptExt);
        }

        //add a business software to the list
        public void addBusinessSoft()
        {
            //check if the field is filled in
            if (this.BusinessSoftField == "")
            {
                messageConsole(this.TranslationList[38], false);
                return;
            }

            //check if the software is already on list and if not add it
            if (!this.BusinessSoftList.Contains(this.BusinessSoftField))
            {
                this.BusinessSoftList.Add(this.BusinessSoftField);
                messageConsole(this.TranslationList[42], true);
            }
            else
            {
                messageConsole(this.TranslationList[43], false);
            }

            this.BusinessSoftField = "";
        }

        //remove a business software from the list 
        public void removeBusinessSoft(string businessSoft)
        {
            this.BusinessSoftList.Remove(businessSoft);
        }

        //change the maximum file size for a transfer    
        public void addMaxFileSize()
        {
            //check if the field is filled in
            if (this.MaxFileSizeField == "")
            {
                messageConsole(this.TranslationList[38], false);
                return;
            }

            //check if the value is a valid number before change it
            if (this.MaxFileSizeField.All(char.IsDigit))
            {
                messageConsole(this.TranslationList[45], true);
                MaxFileSize = this.MaxFileSizeField;
            }
            else
            {
                messageConsole(this.TranslationList[44], false);
            }

            this.MaxFileSizeField = "";

        }

        /*update the progress of one backup on the remote client.
        when the progress change in model, the view model is notified to launch this method*/
        public void updateProgress(AbstractBackup abstractBackup)
        {
            //contain the information about the progress of the backup
            Dictionary<string, string> backup = new Dictionary<string, string> { { "Name", abstractBackup.Name }, { "Source", abstractBackup.Source }, { "Target", abstractBackup.Target }, { "Progress", abstractBackup.Progress.ToString() }, { "CurrentFile", abstractBackup.CurrentFile } };

            //serializes the dictionary
            string jsonSerializedObj;
            jsonSerializedObj = JsonConvert.SerializeObject(backup) + "<ONE>";

            //send it to the server sending method
            server.callSend(jsonSerializedObj);
        }

        /*update all the backup list on the remote client.
         this method is called in this view model when the backup list is updated*/
        public void updateBackupList()
        {
            List<Dictionary<string, string>> backupLListDic = new List<Dictionary<string, string>>();

            //one dictionary = one backup, the list of dictionnary = all backups
            foreach (AbstractBackup backup in this.BackupList)
            {
                backupLListDic.Add(new Dictionary<string, string> { { "Name", backup.Name }, { "Source", backup.Source }, { "Target", backup.Target }, { "Progress", backup.Progress.ToString() }, { "CurrentFile", backup.CurrentFile } });
            }

            //serializes the list of dictionaries 
            string jsonSerializedObj;
            jsonSerializedObj = JsonConvert.SerializeObject(backupLListDic) + "<ALL>";

            //send it to the server sending method
            server.callSend(jsonSerializedObj);
        }

        //call when one or more backups are finished
        public void updateFinishedBackup()
        {
            this.ListFinishedIsEmpty = false;
        }

        //abort threads when the window is closed
        public void updateClose()
        {
            //abort server thread when the window is closed
            serverThread.Abort();

            foreach (AbstractBackup backup in BackupList)
            {
                //abort paused thread when we close the window 
                if (backup.IsPaused)
                {
                    this.threadDictionary[backup.Name].Abort();
                }
            }
        }

        //notifies a client that a static property value has changed      
        public static void onPropertyChangedStatic(string propertyName)
        {
            StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));
        }

        //notifies a client that a property value has changed  
        public void onPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        //---------------------------Properties ----------------------------------//

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                if (value == _name) return;
                _name = value;
                onPropertyChanged("Name");
            }
        }
        public string SourcePath
        {
            get
            {
                return _sourcePath;
            }
            set
            {
                if (value == _sourcePath) return;
                _sourcePath = value;
                onPropertyChanged("SourcePath");
            }
        }
        public string MirrorPath
        {
            get
            {
                return _mirrorPath;
            }
            set
            {
                if (value == _mirrorPath) return;
                _mirrorPath = value;
                onPropertyChanged("MirrorPath");
            }
        }
        public string TargetPath
        {
            get
            {
                return _targetPath;
            }
            set
            {
                if (value == _targetPath) return;
                _targetPath = value;
                onPropertyChanged("TargetPath");
            }
        }
        public bool ListIsEmpty
        {
            get
            {
                return _listIsEmpty;
            }
            set
            {
                if (value == _listIsEmpty) return;
                _listIsEmpty = value;
                onPropertyChanged("ListIsEmpty");
            }
        }
        public bool ListFinishedIsEmpty
        {
            get
            {
                return _listFinishedIsEmpty;
            }
            set
            {
                if (value == _listFinishedIsEmpty) return;
                _listFinishedIsEmpty = value;
                onPropertyChanged("ListFinishedIsEmpty");
            }
        }
        public string LogsPath
        {
            get
            {
                return _logsPath;
            }
            set
            {
                if (value == _logsPath) return;
                _logsPath = value;
                onPropertyChanged("LogsPath");
            }
        }
        public string Visibility
        {
            get
            {
                return _visibility;
            }
            set
            {
                if (value == _visibility) return;
                _visibility = value;
                onPropertyChanged("Visibility");
            }
        }
        public string MessageText
        {
            get
            {
                return _messageText;
            }
            set
            {
                if (value == _messageText) return;
                _messageText = value;
                onPropertyChanged("MessageText");
            }
        }
        public string MessageColor
        {
            get
            {
                return _messageColor;
            }
            set
            {
                if (value == _messageColor) return;
                _messageColor = value;
                onPropertyChanged("MessageColor");
            }
        }
        public string LanguageValue
        {
            get
            {
                return _language;
            }
            set
            {
                if (value == _language) return;
                _language = value;
                onPropertyChanged("LanguageValue");
            }
        }
        public string PriorityField
        {
            get
            {
                return _priorityField;
            }
            set
            {
                if (value == _priorityField) return;
                _priorityField = value;
                onPropertyChanged("PriorityField");
            }
        }
        public string PriorityExt
        {
            get
            {
                return _priorityExt;
            }
            set
            {
                if (value == _priorityExt) return;
                _priorityExt = value;
                onPropertyChanged("PriorityExt");
            }
        }
        public string CryptField
        {
            get
            {
                return _cryptField;
            }
            set
            {
                if (value == _cryptField) return;
                _cryptField = value;
                onPropertyChanged("CryptField");
            }
        }
        public string CryptExt
        {
            get
            {
                return _cryptExt;
            }
            set
            {
                if (value == _cryptExt) return;
                _cryptExt = value;
                onPropertyChanged("CryptExt");
            }
        }
        public string BusinessSoftField
        {
            get
            {
                return _businessSoftField;
            }
            set
            {
                if (value == _businessSoftField) return;
                _businessSoftField = value;
                onPropertyChanged("BusinessSoftField");
            }
        }
        public string BusinessSoft
        {
            get
            {
                return _businessSoft;
            }
            set
            {
                if (value == _businessSoft) return;
                _businessSoft = value;
                onPropertyChanged("BusinessSoft");
            }
        }
        public string MaxFileSizeField
        {
            get
            {
                return _maxFileSizeField;
            }
            set
            {
                if (value == _maxFileSizeField) return;
                _maxFileSizeField = value;
                onPropertyChanged("MaxFileSizeField");
            }
        }
        public static string MaxFileSize
        {
            get
            {
                return _maxFileSize;
            }
            set
            {
                if (value == _maxFileSize) return;
                _maxFileSize = value;
                onPropertyChangedStatic("MaxFileSize");
            }
        }
    }
}
