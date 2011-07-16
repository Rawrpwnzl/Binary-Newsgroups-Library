using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;

namespace NzbComet.NzbDecoders
{
    public class NzbDecoderQueue : IDisposable
    {
        private readonly object _synchroObject;
        private readonly int _maxWorkers;
        private readonly int _hddCacheSize;
        private readonly Queue<NzbDecoderJob> _queuedJobs;
        private readonly Thread[] _workers;

        public NzbDecoderQueue(int hddCacheSize, int maxWorkers)
        {
            ArgumentChecker.ThrowIfBelow("hddCacheSize", "HDD cache size must be above 0.", hddCacheSize, 0);
            ArgumentChecker.ThrowIfOutsideBounds("maxWorkers", "Max Workers must be between 0 and 100.", maxWorkers, 0, 100);

            _hddCacheSize = hddCacheSize;
            _synchroObject = new object();
            _maxWorkers = maxWorkers;
            _queuedJobs = new Queue<NzbDecoderJob>();
            _workers = new Thread[maxWorkers];

            for (int currentPosition = 0; currentPosition < maxWorkers; currentPosition++)
            {
                _workers[currentPosition] = new Thread(Consume);
                _workers[currentPosition].Name = "DecoderQueue Thread #" + currentPosition;
                _workers[currentPosition].Start();
            }
        }

        public void EnqueueJob(NzbDecoderJob job)
        {
            lock (_synchroObject)
            {
                if (job != null)
                {
                    lock (job.Part)
                    {
                        job.Part.Status = NzbPartStatus.QueuedDecoding;
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

        private void Consume()
        {
            while (true)
            {
                NzbDecoderJob job;

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

                lock (job.Part)
                {
                    job.Part.Status = NzbPartStatus.Decoding;
                }

                Decode(job);

                lock (job.Part)
                {
                    job.Part.Status = NzbPartStatus.Decoded;
                }

                if (job.CallWhenDone != null)
                {
                    job.CallWhenDone(job.Part);
                }
            }
        }

        private void Decode(NzbDecoderJob job)
        {
            string originalFilename = null;
            var decoderFactory = new NzbDecoderFactory();

            var decoder = decoderFactory.CreateDecoder(job.Part, job.RawSegmentsDirectory, job.FileEncoding);

            byte[] decodedBytes = decoder.Decode(out originalFilename);

            if (!Directory.Exists(job.DestinationDirectory))
            {
                Directory.CreateDirectory(job.DestinationDirectory);
            }

            string fullPath = Path.Combine(job.DestinationDirectory, originalFilename);

            using (var fileStream = new FileStream(fullPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None, _hddCacheSize))
            {
                fileStream.Flush(true);

                fileStream.Write(decodedBytes, 0, decodedBytes.Length);
            }

            lock (job.Part.Segments)
            {
                var segments = job.Part.Segments;
                foreach (var currentSegment in job.Part.Segments)
                {
                    string rawSegmentsPath = null;
                    lock (currentSegment)
                    {
                        rawSegmentsPath = Path.Combine(job.RawSegmentsDirectory, currentSegment.Id);
                    }

                    if (File.Exists(rawSegmentsPath))
                    {
                        File.Delete(rawSegmentsPath);
                    }
                }
            }
        }
    }
}
