namespace EasySave.ObserverNameSpace
{
    interface IObservable
    {
        void attach(IObserver observer);
        void detach(IObserver observer);
        void notify(string type);
    }
}
