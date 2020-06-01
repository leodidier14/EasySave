using EasySave.ModelNameSpace.Backup;
using System.Collections.Generic;

namespace EasySave.ModelNameSpace.Factory
{
    abstract class MainFactory
    {
        //instantiate a backup
        protected abstract AbstractBackup setBackup(Dictionary<string, string> backupInformation);

        public AbstractBackup createBackup(Dictionary<string, string> backupInformation)
        {
            AbstractBackup abstractSave;

            abstractSave = this.setBackup(backupInformation);

            return abstractSave;
        }
    }
}
