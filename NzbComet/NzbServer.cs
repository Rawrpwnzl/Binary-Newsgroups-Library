using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace NzbComet
{
    public class NzbServer : IDisposable
    {
        private List<NzbConnection> _connections;
        private ReaderWriterLockSlim _synchronizationObject;

        public NzbServer(string server, int port, string username, string password, bool useSsl, int maxConnections)
        {
            ArgumentChecker.ThrowIfNullOrWhitespace("server", server);
            ArgumentChecker.ThrowIfNullOrWhitespace("username", username);
            ArgumentChecker.ThrowIfNullOrWhitespace("password", password);
            ArgumentChecker.ThrowIfOutsideBounds("port", "Port must be between 0 and 65535", port, 1, 65535);
            ArgumentChecker.ThrowIfOutsideBounds("maxConnections", "Maximum Connections must be between 1 and 100", maxConnections, 1, 100);

            this.Server = server;
            this.Port = port;
            this.Username = username;
            this.Password = password;
            this.UseSSL = useSsl;
            this.MaxConnections = maxConnections;

            _synchronizationObject = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
            _connections = new List<NzbConnection>();

            this.InitializeConnections();
        }

        public string Server { get; private set; }
        public int Port { get; private set; }
        public string Username { get; private set; }
        public string Password { get; private set; }
        public bool UseSSL { get; private set; }
        public int MaxConnections { get; private set; }

        public List<NzbConnection> Connections
        {
            get
            {
                try
                {
                    _synchronizationObject.EnterReadLock();

                    return new List<NzbConnection>(_connections);
                }
                finally
                {
                    _synchronizationObject.ExitReadLock();
                }
            }
        }

        public void Dispose()
        {
            try
            {
                _synchronizationObject.EnterWriteLock();

                var connections = this.Connections;

                foreach (var connection in connections)
                {
                    lock (connection)
                    {
                        connection.Discard();
                        connection.Dispose();
                    }
                }

                _connections.Clear();
            }
            finally
            {
                _synchronizationObject.ExitWriteLock();
            }
        }

        private void InitializeConnections()
        {
            try
            {
                _synchronizationObject.EnterWriteLock();

                int maxConnections = this.MaxConnections;
                for (int connectionId = 0; connectionId < maxConnections; connectionId++)
                {
                    _connections.Add(new NzbConnection(this, connectionId));
                }
            }
            finally
            {
                _synchronizationObject.ExitWriteLock();
            }
        }

    }
}
