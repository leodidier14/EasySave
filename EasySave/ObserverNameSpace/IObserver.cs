using EasySave.ModelNameSpace.Backup;

namespace EasySave.ObserverNameSpace
{
    public interface IObserver
    {
        void updateProgress(AbstractBackup abstractBackup);

        void updateBackupList();

        void updateFinishedBackup();

        void updateClose();
    }
}
