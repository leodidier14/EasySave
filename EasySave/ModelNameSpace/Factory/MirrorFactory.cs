using EasySave.ModelNameSpace.Backup;
using System.Collections.Generic;

namespace EasySave.ModelNameSpace.Factory
{
    class MirrorFactory : MainFactory
    {
        //instantiate a mirror backup
        protected override AbstractBackup setBackup(Dictionary<string, string> backupInformation)
        {
            AbstractBackup mirrorSave;

            mirrorSave = new MirrorBackup(backupInformation);

            return mirrorSave;
        }
    }
}
