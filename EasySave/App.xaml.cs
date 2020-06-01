using System.Diagnostics;
using System.Windows;

namespace EasySave
{
    /// <summary>
    /// Logique d'interaction pour App.xaml
    /// </summary>
    public partial class App : Application
    {   
        //single instance
        protected override void OnStartup(StartupEventArgs e)
        {
            if (AnotherInstanceExists())
            {
                //just shutdown the current application, if any instance found  
                App.Current.Shutdown(); 
            }             

            base.OnStartup(e);
        }

        private bool AnotherInstanceExists()
        {
            //obtains a new Process component and associates it with the currently active process.
            Process currentRunningProcess = Process.GetCurrentProcess();

            //get the list of active processes 
            Process[] listOfProcs = Process.GetProcessesByName(currentRunningProcess.ProcessName);

            //checks if the process of the easySave program is running on the list
            foreach (Process proc in listOfProcs)
            {
                if ((proc.MainModule.FileName == currentRunningProcess.MainModule.FileName) && (proc.Id != currentRunningProcess.Id))
                {
                    MessageBox.Show("EasySave is already running.", "Error");
                    return true;
                }
                    
            }
            return false;
        }
    }


   
}
