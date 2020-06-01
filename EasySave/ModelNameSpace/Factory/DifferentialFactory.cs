using EasySave.ModelNameSpace.Backup;
using System.Collections.Generic;

namespace EasySave.ModelNameSpace.Factory
{
    class DifferentialFactory : MainFactory
    {
        //instantiate a differential backup
        protected override AbstractBackup setBackup(Dictionary<string, string> backupInformation)
        {
            AbstractBackup diffSave;

            diffSave = new DifferentialBackup(backupInformation);

            return diffSave;
        }

    }
}
