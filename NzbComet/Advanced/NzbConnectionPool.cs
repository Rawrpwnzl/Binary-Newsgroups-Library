using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace NzbComet.Advanced
{
    public class NzbConnectionPool
    {
        private readonly object _synchroObject;
        private readonly Queue<NzbConnection> _connectionPool;

        public NzbConnectionPool()
        {
            _synchroObject = new object();
            _connectionPool = new Queue<NzbConnection>();
        }

        public void AddConnection(NzbConnection connection)
        {
            lock (_synchroObject)
            {
                _connectionPool.Enqueue(connection);

                Monitor.Pulse(_synchroObject);
            }
        }

        public NzbConnection GetAvailableConnection()
        {
            NzbConnection availableConnection = null;

            lock (_synchroObject)
            {
                while (_connectionPool.Count == 0)
                {
                    Monitor.Wait(_synchroObject);
                }

                availableConnection = _connectionPool.Dequeue();
            }

            // This connection shouldn't be used anymore. Let's skip it and try another one.
            if (availableConnection != null && availableConnection.Status == NzbConnectionStatus.Discarded)
            {
                return GetAvailableConnection();
            }

            return availableConnection;
        }
    }
}
