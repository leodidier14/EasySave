using EasySave.ObserverNameSpace;
using EasySave.ViewModelNameSpace;
using System.Collections.Generic;
using System.Windows;

namespace EasySave
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IObservable
    {
        private List<IObserver> _observers = new List<IObserver>();

        public MainWindow()
        {
            InitializeComponent();

            this.attach(DataContext as ViewModel);
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
            foreach (var observer in _observers)
            {
                observer.updateClose();
            }
        }

        private void closeWindow(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.notify("close");
        }
    }
}
