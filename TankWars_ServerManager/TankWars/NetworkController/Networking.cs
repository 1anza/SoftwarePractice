using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace NetworkUtil
{
    public static class Networking
    {
        /////////////////////////////////////////////////////////////////////////////////////////
        // Server-Side Code
        /////////////////////////////////////////////////////////////////////////////////////////

        ///
        /// <summary>
        /// Starts a TcpListener on the specified port and starts an event-loop to accept new clients.
        /// The event-loop is started with BeginAcceptSocket and uses AcceptNewClient as the callback.
        /// AcceptNewClient will continue the event-loop.
        /// </summary>
        /// <param name="toCall">The method to call when a new connection is made</param>
        /// <param name="port">The the port to listen on</param>
        public static TcpListener StartServer(Action<SocketState> toCall, int port)
        {

            //Create a new listener for the port
            TcpListener listener = new TcpListener(port);
            listener.Start();

            //Holds listener and toCall as objects to be passed into BeginAccept
            Tuple<TcpListener, Action<SocketState>> connection = Tuple.Create(listener, toCall);

            //start event loop to accept new clients
            listener.BeginAcceptSocket(AcceptNewClient, connection);

            return listener;
        }

        /// <summary>
        /// To be used as the callback for accepting a new client that was initiated by StartServer, and 
        /// continues an event-loop to accept additional clients.
        ///
        /// Uses EndAcceptSocket to finalize the connection and create a new SocketState. The SocketState's
        /// OnNetworkAction should be set to the delegate that was passed to StartServer.
        /// Then invokes the OnNetworkAction delegate with the new SocketState so the user can take action. 
        /// 
        /// If anything goes wrong during the connection process (such as the server being stopped externally), 
        /// the OnNetworkAction delegate should be invoked with a new SocketState with its ErrorOccurred flag set to true 
        /// and an appropriate message placed in its ErrorMessage field. The event-loop should not continue if
        /// an error occurs.
        ///
        /// If an error does not occur, after invoking OnNetworkAction with the new SocketState, an event-loop to accept 
        /// new clients should be continued by calling BeginAcceptSocket again with this method as the callback.
        /// </summary>
        /// <param name="ar">The object asynchronously passed via BeginAcceptSocket. It must contain a tuple with 
        /// 1) a delegate so the user can take action (a SocketState Action), and 2) the TcpListener</param>
        private static void AcceptNewClient(IAsyncResult ar)
        {

            Tuple<TcpListener, Action<SocketState>> asyncState = (Tuple<TcpListener, Action<SocketState>>)ar.AsyncState;
            Socket socket = (Socket)null;
            try
            {
                TcpListener listener = asyncState.Item1;
                socket = listener.EndAcceptSocket(ar);
                socket.NoDelay = true;
            }
            catch (Exception ex)
            {
                SocketState socketState = new SocketState(asyncState.Item2, socket)
                {
                    ErrorOccurred = true,
                    ErrorMessage = ex.ToString()
                };
                socketState.OnNetworkAction(socketState);
                return;
            }
            SocketState socketState1 = new SocketState(asyncState.Item2, socket);
            socketState1.OnNetworkAction(socketState1);
            try
            {
                asyncState.Item1.BeginAcceptSocket(new AsyncCallback(Networking.AcceptNewClient), (object)asyncState);
            }
            catch (Exception ex)
            {
                SocketState socketState2 = new SocketState(asyncState.Item2, socket)
                {
                    ErrorOccurred = true,
                    ErrorMessage = ex.ToString()
                };
                socketState2.OnNetworkAction(socketState2);
            }
        }

        /// <summary>
        /// Stops the given TcpListener.
        /// </summary>
        public static void StopServer(TcpListener listener)
        {
            listener.Stop();
        }

        /////////////////////////////////////////////////////////////////////////////////////////
        // Client-Side Code
        /////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Begins the asynchronous process of connecting to a server via BeginConnect, 
        /// and using ConnectedCallback as the method to finalize the connection once it's made.
        /// 
        /// If anything goes wrong during the connection process, toCall should be invoked 
        /// with a new SocketState with its ErrorOccurred flag set to true and an appropriate message 
        /// placed in its ErrorMessage field. Depending on when the error occurs, this should happen either
        /// in this method or in ConnectedCallback.
        ///
        /// This connection process should timeout and produce an error (as discussed above) 
        /// if a connection can't be established within 3 seconds of starting BeginConnect.
        /// 
        /// </summary>
        /// <param name="toCall">The action to take once the connection is open or an error occurs</param>
        /// <param name="hostName">The server to connect to</param>
        /// <param name="port">The port on which the server is listening</param>
        public static void ConnectToServer(Action<SocketState> toCall, string hostName, int port)
        {
            // TODO: This method is incomplete, but contains a starting point 
            //       for decoding a host address
            SocketState clientSocket = new SocketState((Action<SocketState>)null, (Socket)null);

            // Establish the remote endpoint for the socket.
            IPHostEntry ipHostInfo;
            IPAddress ipAddress = IPAddress.None;

            // Determine if the server address is a URL or an IP
            try
            {
                ipHostInfo = Dns.GetHostEntry(hostName);
                bool foundIPV4 = false;

                foreach (IPAddress addr in ipHostInfo.AddressList)
                    if (addr.AddressFamily != AddressFamily.InterNetworkV6)
                    {
                        foundIPV4 = true;
                        ipAddress = addr;
                        break;
                    }
                // Didn't find any IPV4 addresses
                if (!foundIPV4)
                {
                    // TODO: Indicate an error to the user, as specified in the documentation
                    clientSocket.ErrorMessage = "Can't find correct IP " + hostName;
                    clientSocket.ErrorOccurred = true;
                    toCall(clientSocket);
                    return;
                }
            }
            catch (Exception)
            {
                // see if host name is a valid ipaddress
                try
                {
                    ipAddress = IPAddress.Parse(hostName);
                }
                catch (Exception)
                {
                    // TODO: Indicate an error to the user, as specified in the documentation
                    clientSocket.ErrorMessage = "Can't find correct IP: " + hostName;
                    clientSocket.ErrorOccurred = true;
                    toCall(clientSocket);
                    return;
                }
            }

            // Create a TCP/IP socket.
            Socket socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            // This disables Nagle's algorithm (google if curious!)
            // Nagle's algorithm can cause problems for a latency-sensitive 
            // game like ours will be 
            socket.NoDelay = true;

            SocketState socketState1 = new SocketState(toCall, socket);


            // TODO: Finish the remainder of the connection process as specified.
            try
            {
                if (socket.BeginConnect(ipAddress, port, ConnectedCallback, (object)socketState1).AsyncWaitHandle.WaitOne(3000, true))
                    return;
                socket.Close();
            }
            catch (Exception ex)
            {
                clientSocket.ErrorMessage = ex.ToString();
                toCall(clientSocket);
            }
        }

        /// <summary>
        /// To be used as the callback for finalizing a connection process that was initiated by ConnectToServer.
        ///
        /// Uses EndConnect to finalize the connection.
        /// 
        /// As stated in the ConnectToServer documentation, if an error occurs during the connection process,
        /// either this method or ConnectToServer should indicate the error appropriately.
        /// 
        /// If a connection is successfully established, invokes the toCall Action that was provided to ConnectToServer (above)
        /// with a new SocketState representing the new connection.
        /// 
        /// </summary>
        /// <param name="ar">The object asynchronously passed via BeginConnect</param>
        private static void ConnectedCallback(IAsyncResult ar)
        {
            SocketState async = (SocketState)ar.AsyncState;
            
            //Finalize the connection with EndConnect
            try
            {
                async.TheSocket.EndConnect(ar);
                async.TheSocket.NoDelay = true;
            }
            catch (Exception err)
            {
                async.ErrorMessage = err.ToString();
                async.ErrorOccurred = true;
            }
            async.OnNetworkAction(async);
        }


        /////////////////////////////////////////////////////////////////////////////////////////
        // Server and Client Common Code
        /////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Begins the asynchronous process of receiving data via BeginReceive, using ReceiveCallback 
        /// as the callback to finalize the receive and store data once it has arrived.
        /// The object passed to ReceiveCallback via the AsyncResult should be the SocketState.
        /// 
        /// If anything goes wrong during the receive process, the SocketState's ErrorOccurred flag should 
        /// be set to true, and an appropriate message placed in ErrorMessage, then the SocketState's
        /// OnNetworkAction should be invoked. Depending on when the error occurs, this should happen either
        /// in this method or in ReceiveCallback.
        /// </summary>
        /// <param name="state">The SocketState to begin receiving</param>
        public static void GetData(SocketState state)
        {

            // Try to begin receiving data through ReceiveCallback
            try
            {
                state.TheSocket.BeginReceive(state.buffer, 0, 4096, SocketFlags.None, new AsyncCallback(Networking.ReceiveCallback), state);
            }

            // Indicate an error if receive process is unsuccessful 
            catch (Exception ex)
            {
                Exception exception = ex;
                state.ErrorOccurred = true;
                state.ErrorMessage = exception.ToString();
                state.OnNetworkAction(state);
            }
        }

        /// <summary>
        /// To be used as the callback for finalizing a receive operation that was initiated by GetData.
        /// 
        /// Uses EndReceive to finalize the receive.
        ///
        /// As stated in the GetData documentation, if an error occurs during the receive process,
        /// either this method or GetData should indicate the error appropriately.
        /// 
        /// If data is successfully received:
        ///  (1) Read the characters as UTF8 and put them in the SocketState's unprocessed data buffer (its string builder).
        ///      This must be done in a thread-safe manner (LOCK) with respect to the SocketState methods that access or modify its 
        ///      string builder.
        ///  (2) Call the saved delegate (OnNetworkAction) allowing the user to deal with this data.
        /// </summary>
        /// <param name="ar">   
        /// This contains the SocketState that is stored with the callback when the initial BeginReceive is called.
        /// </param>
        private static void ReceiveCallback(IAsyncResult ar)
        {

            // Asynchronous Socket
            SocketState async = (SocketState)ar.AsyncState;
            Socket theSocket = async.TheSocket;


            try
            {
                //End the receiving of bytes from the socket
                int bytesReadFromSocket = theSocket.EndReceive(ar);
                if (bytesReadFromSocket <= 0)
                {
                    async.ErrorMessage = "Socket was closed";
                    async.ErrorOccurred = true;
                }

                //Read and place characters in the data buffer
                else
                {
                    lock (async.data)
                    {
                        async.data.Append(Encoding.UTF8.GetString(async.buffer, 0, bytesReadFromSocket));
                    }

                    async.OnNetworkAction(async);
                    return;
                }
            }
            catch (Exception ex)
            {
                Exception exc = ex;
                async.ErrorOccurred = true;
                async.ErrorMessage = exc.ToString();
            }
            async.OnNetworkAction(async);
        }


        /// <summary>
        /// Calls SendBytes, which begins the asynchronous process of sending data via BeginSend, using SendCallback to finalize the send process.
        /// 
        /// If the socket is closed, does not attempt to send.
        /// 
        /// If a send fails for any reason, SendBytes ensures that the Socket is closed before returning.
        /// </summary>
        /// <param name="socket">The socket on which to send the data</param>
        /// <param name="data">The string to send</param>
        /// <returns>True if the send process was started, false if an error occurs or the socket is already closed</returns>
        public static bool Send(Socket socket, string data)
        {
            return SendBytes(socket, data, false);
        }

        /// <summary>
        /// To be used as the callback for finalizing a send operation that was initiated by Send.
        ///
        /// Uses SendCallbackHelper, which utilizes EndSend to finalize the send.
        /// 
        /// SendCallbackHelper doesn't throw, even if an error occurred during the Send operation.
        /// </summary>
        /// <param name="ar">
        /// This is the Socket (not SocketState) that is stored with the callback when
        /// the initial BeginSend is called.
        /// </param>
        private static void SendCallback(IAsyncResult ar)
        {
            SendCallbackHelper(false, ar);
        }


        /// <summary>
        /// Calls SendBytes, which begins the asynchronous process of sending data via BeginSend, using SendAndCloseCallback to finalize the send process.
        /// This variant closes the socket in the callback once complete. This is useful for HTTP servers.
        /// 
        /// If the socket is closed, does not attempt to send.
        /// 
        /// If a send fails for any reason, SendBytes ensures that the Socket is closed before returning.
        /// </summary>
        /// <param name="socket">The socket on which to send the data</param>
        /// <param name="data">The string to send</param>
        /// <returns>True if the send process was started, false if an error occurs or the socket is already closed</returns>
        public static bool SendAndClose(Socket socket, string data)
        {
            return SendBytes(socket, data, true);
        }

        /// <summary>
        /// To be used as the callback for finalizing a send operation that was initiated by SendAndClose.
        ///
        /// Uses SendCallbackHelper, which utilizes EndSend to finalize the send, then closes the socket.
        /// 
        /// SendCallbackHelper doesn't throw, even if an error occurred during the Send operation.
        /// 
        /// This method ensures that the socket is closed before returning.
        /// </summary>
        /// <param name="ar">
        /// This is the Socket (not SocketState) that is stored with the callback when
        /// the initial BeginSend is called.
        /// </param>
        private static void SendAndCloseCallback(IAsyncResult ar)
        {
            SendCallbackHelper(true, ar);
        }

        /// <summary>
        /// Private Helper Method that sends data via BeginSend and finalizes that process through either SendCallback or SendAndCloseCallback.
        /// 
        /// If the socket needs to be closed in the callback once complete, the closure will be indicated by the value of bool ifClose.
        /// 
        /// If a send fails for any reason, this method ensures that the Socket is closed before returning false.
        /// Otherwise if a send is successful, returns true.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="d"></param>
        /// <param name="ifClose"></param>
        /// <returns></returns>
        private static bool SendBytes(Socket s, string d, bool ifClose)
        {
            //Indicates success or failure of data delivery
            bool status;

            //If the socket is connected, tries to send data and finalize the send. Status set to true if successful 
            if (s.Connected)
            {
                try
                {
                    int offset = 0;
                    byte[] messageBytes = Encoding.UTF8.GetBytes(d);

                    //Closes the socket if needed
                    if (ifClose)
                        s.BeginSend(messageBytes, offset, (int)messageBytes.Length, SocketFlags.None, new AsyncCallback(Networking.SendAndCloseCallback), s);
                    
                    else
                        s.BeginSend(messageBytes, offset, (int)messageBytes.Length, SocketFlags.None, new AsyncCallback(Networking.SendCallback), s);

                    status = true;
                }

                //If send fails, the socket is closed returns a failed status. 
                catch (Exception)
                {
                    try
                    {
                        s.Shutdown(SocketShutdown.Both);
                        s.Close();
                    }
                    catch (Exception)
                    {
                    }
                    status = false;
                }
            }
            else
                status = false;

            return status;
        }

        /// <summary>
        /// Helper method used as the callback for finalizing a send operation that was initiated by either SendAndClose or Send.
        ///
        /// Uses EndSend to finalize the send, then closes the socket if the value of ifClose is true.
        /// 
        /// This method ensures that the socket is closed before returning.
        /// </summary>
        /// <param name="ifClose"></param>
        /// <param name="ar"></param>
        private static void SendCallbackHelper(bool ifClose, IAsyncResult ar)
        {
            //Finalizes the send process through EndSend
            try
            {
                Socket async = (Socket)ar.AsyncState;
                if (async.Connected)
                {
                    async.EndSend(ar);

                    //Closes if necessary
                    if (ifClose)
                        async.Close();
                }
            }
            catch (Exception)
            {
            }
        }

    }
}
