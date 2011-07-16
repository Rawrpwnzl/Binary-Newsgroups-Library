using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;

namespace NzbComet.Advanced
{
    public class NzbDownloadQueue : IDisposable
    {
        private readonly object _synchroObject;
        private readonly NzbConnectionPool _connectionPool;
        private Queue<NzbDownloadJob> _queuedJobs;
        private Thread[] _workers;

        public NzbDownloadQueue(NzbConnectionPool connectionPool)
        {
            ArgumentChecker.ThrowIfNull("connectionPool", connectionPool);

            _synchroObject = new object();
            _connectionPool = connectionPool;
            _queuedJobs = new Queue<NzbDownloadJob>();
            _workers = new Thread[0];
        }

        public void EnqueueJob(NzbDownloadJob job)
        {
            lock (_synchroObject)
            {
                if (job != null)
                {
                    lock (job.Segment)
                    {
                        job.Segment.Status = NzbSegmentStatus.QueuedDownload;
                    }
                }

                _queuedJobs.Enqueue(job);

                Monitor.Pulse(_synchroObject);
            }
        }

        public void Dispose()
        {
            foreach (var worker in _workers)
            {
                EnqueueJob(null);
            }
        }

        public void SynchronizeWorkersWithConnectionPool(int newConnectionLimit)
        {
            int currentLimit = _workers.Length;
            if (newConnectionLimit == currentLimit)
            {
                return;
            }
            else if (newConnectionLimit > currentLimit)
            {
                this.CreateWorkers(newConnectionLimit - currentLimit);
            }
            else
            {
                this.KillWorkers(currentLimit - newConnectionLimit);
            }
        }

        private void CreateWorkers(int numberOfWorkersToCreate)
        {
            int newTotalWorkers = _workers.Length + numberOfWorkersToCreate;
            int oldTotalWorkers = _workers.Length;

            Thread[] workers = new Thread[newTotalWorkers];

            for (int currentPosition = 0; currentPosition < newTotalWorkers; currentPosition++)
            {
                if (currentPosition >= oldTotalWorkers)
                {
                    workers[currentPosition] = new Thread(Consume);
                    workers[currentPosition].Name = "DownloadQueue Thread #" + currentPosition;
                    workers[currentPosition].Start();
                }
            }

            lock (_workers)
            {
                _workers = workers;
            }
        }

        private void KillWorkers(int numberOfWorkersToKill)
        {
            int newTotalWorkers = _workers.Length - numberOfWorkersToKill;
            int oldTotalWorkers = _workers.Length;

            Thread[] workers = new Thread[newTotalWorkers];

            for (int currentPosition = 0; currentPosition < newTotalWorkers; currentPosition++)
            {
                if (currentPosition >= newTotalWorkers)
                {
                    EnqueueJob(null);
                }
                else
                {
                    workers[currentPosition] = new Thread(Consume);
                    workers[currentPosition].Name = "DownloadQueue Thread #" + currentPosition;
                    workers[currentPosition].Start();
                }
            }

            lock (_workers)
            {
                _workers = workers;
            }
        }


        private void Consume()
        {
            while (true)
            {
                NzbDownloadJob job;
                NzbConnection connection;

                lock (_synchroObject)
                {
                    while (_queuedJobs.Count == 0)
                    {
                        Monitor.Wait(_synchroObject);
                    }

                    job = _queuedJobs.Dequeue();
                }

                if (job == null)
                {
                    return;
                }

                lock (job.Segment)
                {
                    if (job.Segment.Status == NzbSegmentStatus.Discarded)
                    {
                        continue;
                    }
                }

                connection = _connectionPool.GetAvailableConnection();

                if (connection == null)
                {
                    continue;
                }

                Download(job, connection);

                _connectionPool.AddConnection(connection);

                if (job.CallWhenDone != null)
                {
                    job.CallWhenDone(job);
                }
            }
        }

        private void Download(NzbDownloadJob job, NzbConnection connection)
        {
            lock (job.Segment)
            {
                job.Segment.Status = NzbSegmentStatus.Downloading;
            }

            if (connection.Status != NzbConnectionStatus.Connected)
            {
                connection.Connect();
            }

            string downloadedSegment = null;

            try
            {
                downloadedSegment = connection.DownloadSegment(job.Segment);
            }
            catch (NzbSegmentDownloadFailedException)
            {
                lock (job.Segment)
                {
                    job.Segment.Status = NzbSegmentStatus.DownloadFailed;
                    return;
                }
            }

            string tempDirectory = job.TemporaryDirectory;

            if (!Directory.Exists(tempDirectory))
            {
                Directory.CreateDirectory(tempDirectory);
            }

            byte[] rawBytes = Encoding.GetEncoding("iso-8859-1").GetBytes(downloadedSegment);

            string fullPath = Path.Combine(tempDirectory, job.Segment.Id);

            using (FileStream stream = new FileStream(fullPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None, rawBytes.Length))
            {
                stream.Write(rawBytes, 0, rawBytes.Length);
                stream.Close();
            }

            lock (job.Segment)
            {
                job.Segment.Status = NzbSegmentStatus.Downloaded;
            }
        }
    }
}
