using EasySave.ObserverNameSpace;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace EasySave.ModelNameSpace.Server
{
    class Server : IObservable
    {
        private List<IObserver> _observers = new List<IObserver>();

        public List<ClientConnection> ClientsList { get; set; } = new List<ClientConnection>();
        public List<ClientConnection> ClientsListToDelete { get; set; } = new List<ClientConnection>();

        //notify the connection state to the thread
        public ManualResetEvent connectionDone = new ManualResetEvent(false);

        public Server()
        {

        }

        //listen to clients trying to connect
        public void startServer()
        {
            //establish the local endpoint for the socket 
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, 1337);

            //create a TCP/IP socket  
            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            //bind the socket to the local endpoint and listen for incoming connections  
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(100);

                while (true)
                {
                    //set the event to non signaled state  
                    this.connectionDone.Reset();
                    
                    //start an asynchronous socket to listen for connections  
                    listener.BeginAccept(new AsyncCallback(acceptCallback), listener);

                    //wait until a connection is made before continuing  
                    this.connectionDone.WaitOne();

                    //the connection is set, we can set the backup list
                    this.notify("IHM");
                }

            }
            catch (Exception)
            {

            }
        }

        //manage the connection
        public void acceptCallback(IAsyncResult ar)
        {
            //signal the main thread to continue 
            this.connectionDone.Set();

            //get the socket that handles the client request  
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            //create the client connection object  
            ClientConnection state = new ClientConnection();
            state.workSocket = handler;
            this.ClientsList.Add(state);
        }

        //send method 
        private void send(Socket handler, string data)
        {

            //convert the string data to byte data using ASCII encoding. 
            byte[] byteData = Encoding.ASCII.GetBytes(data);
            
            //begin sending the data to the remote device
            try
            {
                handler.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(sendCallback), handler);
            }
            catch (SocketException)
            {
                //if a client disconnects stop the socket 
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }
            /*if the server tries to send an element after the socket has been stopped
            we add the client to the list which will remove the clients at the end of the sending*/
            catch (ObjectDisposedException)
            {
                foreach (ClientConnection clientConnection in this.ClientsList)
                {
                    if (handler == clientConnection.workSocket && !this.ClientsListToDelete.Contains(clientConnection))
                    {
                        this.ClientsListToDelete.Add(clientConnection);
                    }
                }
            }
        }

        //continue sending asynchronously 
        private void sendCallback(IAsyncResult ar)
        {
            try
            {
                //retrieve the socket from the client connection object
                Socket handler = (Socket)ar.AsyncState;

                //complete sending the data to the remote device
                int bytesSent = handler.EndSend(ar);
            }
            catch (Exception)
            {

            }
        }

        //call the send method when view model is notified of an data update 
        public void callSend(string data)
        {
            //call the send method for each client connected
            foreach (ClientConnection clientConnection in this.ClientsList)
            {
                send(clientConnection.workSocket, data);
            }

            //wait the end of foreach and delete the clients disconnected of the list to stop trying to send them data
            if (this.ClientsListToDelete.Count > 0)
            {
                lock (this.ClientsList)
                {
                    foreach (ClientConnection clientConnection in this.ClientsListToDelete)
                    {
                        if (this.ClientsList.Contains(clientConnection))
                        {
                            this.ClientsList.Remove(clientConnection);
                        }
                    }
                }
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

        //retrieve the last updated backup list to send it 
        public void notify(string type)
        {
            foreach (var observer in _observers)
            {
                observer.updateBackupList();
            }
        }
    }
}
