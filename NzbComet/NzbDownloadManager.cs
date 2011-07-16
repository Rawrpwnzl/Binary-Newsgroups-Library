using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using NzbComet.Advanced;
using NzbComet.NzbDecoders;
using System.IO;

namespace NzbComet
{
    public class NzbDownloadManager : IDisposable
    {
        private List<NzbServer> _servers;
        private List<NzbDownload> _downloads;
        private NzbConnectionPool _connectionPool;
        private int _maxConnections;
        private NzbDownloadQueue _downloadQueue;
        private NzbDecoderQueue _decoderQueue;
        private bool _isDisposed;
        private object _isDisposedSynchronization;

        public NzbDownloadManager()
        {
            _servers = new List<NzbServer>();
            _downloads = new List<NzbDownload>();
            _connectionPool = new NzbConnectionPool();
            _downloadQueue = new NzbDownloadQueue(_connectionPool);
            _decoderQueue = new NzbDecoderQueue(4096, 3);
            _isDisposed = false;
            _isDisposedSynchronization = new object();
        }

        public ReadOnlyCollection<NzbServer> Servers
        {
            get
            {
                return new ReadOnlyCollection<NzbServer>(_servers);
            }
        }

        public ReadOnlyCollection<NzbDownload> Downloads
        {
            get
            {
                return new ReadOnlyCollection<NzbDownload>(_downloads);
            }
        }

        public void AddDownload(NzbDownload download, string downloadDirectory, string temporaryDirectory)
        {
            ArgumentChecker.ThrowIfNull("download", download);
            ArgumentChecker.ThrowIfNullOrWhitespace("downloadDirectory", downloadDirectory);
            ArgumentChecker.ThrowIfNullOrWhitespace("temporaryDirectory", temporaryDirectory);

            lock (download)
            {
                string downloadFullpath = Path.Combine(downloadDirectory, download.DownloadDirectory);

                foreach (var currentPart in download.Parts)
                {
                    foreach (var currentSegment in currentPart.Segments)
                    {
                        var job = new NzbDownloadJob(downloadFullpath, temporaryDirectory, currentSegment, SegmentDownloadedCheckpoint);

                        _downloadQueue.EnqueueJob(job);
                    }
                }
            }

            lock (_downloads)
            {
                _downloads.Add(download);
            }
        }

        public void RemoveDownload(NzbDownload download)
        {
            lock (download)
            {
                foreach (var currentPart in download.Parts)
                {
                    lock (currentPart)
                    {
                        foreach (var currentSegment in currentPart.Segments)
                        {
                            lock (currentSegment)
                            {
                                currentSegment.Status = NzbSegmentStatus.Discarded;
                            }
                        }
                    }
                }
            }
        }

        public void AddServer(NzbServer server)
        {
            lock (server)
            {
                server.Connections.ForEach(con =>
                    {
                        _connectionPool.AddConnection(con);
                        _maxConnections++;
                    });
            }

            lock (this)
            {
                _servers.Add(server);

                _downloadQueue.SynchronizeWorkersWithConnectionPool(_maxConnections);
            }
        }

        public void RemoveServer(NzbServer server)
        {
            lock (server)
            {
                server.Connections.ForEach(con =>
                    {
                        con.Discard();
                        _maxConnections--;
                    });
            }

            lock (this)
            {
                _servers.Remove(server);

                _downloadQueue.SynchronizeWorkersWithConnectionPool(_maxConnections);
            }
        }

        public void Dispose()
        {
            lock (_downloadQueue)
            {
                lock (_decoderQueue)
                {
                    lock (_servers)
                    {
                        foreach (var download in this.Downloads)
                        {
                            this.RemoveDownload(download);
                        }

                        _downloadQueue.Dispose();
                        _downloadQueue = null;

                        _decoderQueue.Dispose();
                        _decoderQueue = null;

                        _servers.ForEach(server => server.Connections.ForEach(con => con.Discard()));

                        _servers.Clear();
                        _servers = null;

                        lock (_isDisposedSynchronization)
                        {
                            _isDisposed = true;
                        }
                    }

                }
            }
        }

        private void SegmentDownloadedCheckpoint(NzbDownloadJob downloadJob)
        {
            lock (_isDisposedSynchronization)
            {
                if (_isDisposed)
                {
                    return;
                }
            }

            var downloadedSegment = downloadJob.Segment;

            lock (downloadedSegment.Parent)
            {
                var parent = downloadedSegment.Parent;
                var numberOfSuccessfullyDownloadedSegments = parent.Segments.Where(seg => seg.Status == NzbSegmentStatus.Downloaded).Count();

                if (parent.Segments.Count == numberOfSuccessfullyDownloadedSegments)
                {
                    parent.Status = NzbPartStatus.Downloaded;

                    var job = new NzbDecoderJob(downloadJob.TemporaryDirectory, downloadJob.DownloadDirectory, downloadedSegment.Parent, Encoding.GetEncoding("iso-8859-1"), PartDecodedCheckpoint);

                    _decoderQueue.EnqueueJob(job);
                }
            }
        }

        private void PartDecodedCheckpoint(NzbPart decodedPart)
        {
            lock (_isDisposedSynchronization)
            {
                if (_isDisposed)
                {
                    return;
                }
            }

            lock (decodedPart.Parent)
            {
                var parent = decodedPart.Parent;

                var numberOfSuccessfullyDecodedParts = parent.Parts.Where(part => part.Status == NzbPartStatus.Decoded).Count();

                if (parent.Parts.Count == numberOfSuccessfullyDecodedParts)
                {
                    parent.Status = NzbDownloadStatus.Complete;

                    lock (_downloads)
                    {
                        _downloads.Remove(parent);
                    }
                }
            }
        }
    }
}
