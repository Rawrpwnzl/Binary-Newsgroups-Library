using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.IO;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using NzbComet.ServerCommands;

namespace NzbComet
{
    public class NzbConnection : IDisposable
    {
        private readonly int _id;
        private NzbConnectionStatus _status;
        private NzbServer _server;
        private TcpClient _nntpClient;
        private Stream _netStream;
        private NntpParser _nntpParser;

        private ReaderWriterLockSlim _synchronizationObject;

        private const int _tcpClientSendTimeoutInMilliseconds = 500;
        private const int _tcpClientReceiveTimeoutInMilliseconds = 500;

        public NzbConnection(NzbServer server, int id)
        {
            ArgumentChecker.ThrowIfNull("server", server);

            _id = id;
            _server = server;
            _synchronizationObject = new ReaderWriterLockSlim();
            _status = NzbConnectionStatus.Disconnected;
            _nntpParser = new NntpParser(this);
            this.Server = _server;
        }

        public int Id
        {
            get
            {
                try
                {
                    _synchronizationObject.EnterReadLock();

                    return _id;
                }
                finally
                {
                    _synchronizationObject.ExitReadLock();
                }
            }
        }
        public NzbConnectionStatus Status
        {
            get
            {
                try
                {
                    _synchronizationObject.EnterWriteLock();

                    if (_nntpClient == null || !_nntpClient.Connected || !_nntpClient.Client.Connected)
                    {
                        _status = NzbConnectionStatus.Disconnected;
                    }

                    return _status;
                }
                finally
                {
                    _synchronizationObject.ExitWriteLock();
                }
            }

            private set
            {
                try
                {
                    _synchronizationObject.EnterWriteLock();

                    _status = value;
                }
                finally
                {
                    _synchronizationObject.ExitWriteLock();
                }
            }
        }
        public Stream BaseStream
        {
            get
            {
                try
                {
                    _synchronizationObject.EnterReadLock();

                    return _netStream;
                }
                finally
                {
                    _synchronizationObject.ExitReadLock();
                }
            }
        }
        public NzbServer Server { get; private set; }

        public void Connect()
        {
            this.CreateNewTcpClient();
            this.SetStream();

            this.Status = NzbConnectionStatus.Connecting;

            try
            {
                _nntpParser.Authenticate(_server.Username, _server.Password);

                this.Status = NzbConnectionStatus.Connected;
            }
            catch (NzbAuthenticationFailedException)
            {
                this.Status = NzbConnectionStatus.AuthenticationFailure;
            }
        }

        public void Disconnect()
        {
            this.Status = NzbConnectionStatus.Disconnecting;

            _nntpParser.Disconnect();
            this.CleanupTcpClient();            

            this.Status = NzbConnectionStatus.Disconnected;
        }

        public string DownloadSegment(NzbSegment segment)
        {
            string downloadedArticle = null;

            foreach (var currentGroup in segment.Groups)
            {
                _nntpParser.JoinGroup(currentGroup);

                var getBodyCommand = new NzbServerGetBodyCommand(segment.Id);

                _nntpParser.TrySendCommand(getBodyCommand);

                try
                {
                    downloadedArticle = _nntpParser.DownloadArticle(segment.Id);

                    return downloadedArticle;
                }
                catch (NzbArticleNotFoundException)
                {
                }
                catch (NzbArticleDownloadFailedException)
                {
                    throw new NzbSegmentDownloadFailedException(segment);
                }
            }

            throw new NzbSegmentDownloadFailedException(segment);
        }

        public void Dispose()
        {
            this.Discard();
        }

        public void Discard()
        {
            this.Disconnect();

            this.Status = NzbConnectionStatus.Discarded;
        }

        private void CreateNewTcpClient()
        {
            this.CleanupTcpClient();

            try
            {
                _synchronizationObject.EnterWriteLock();

                _nntpClient = new TcpClient();
                _nntpClient.SendTimeout = _tcpClientSendTimeoutInMilliseconds;
                _nntpClient.ReceiveTimeout = _tcpClientReceiveTimeoutInMilliseconds;
                _nntpClient.Connect(_server.Server, _server.Port);
            }
            finally
            {
                _synchronizationObject.ExitWriteLock();
            }
        }

        private void CleanupTcpClient()
        {
            try
            {
                _synchronizationObject.EnterWriteLock();

                if (_nntpClient != null)
                {
                    _nntpClient.Close();
                    _nntpClient = null;
                }
            }
            finally
            {
                _synchronizationObject.ExitWriteLock();
            }
        }

        private void SetStream()
        {
            try
            {
                _synchronizationObject.EnterWriteLock();

                if (_server.UseSSL)
                {
                    SslStream stream = new SslStream(this._nntpClient.GetStream(), true, new RemoteCertificateValidationCallback(NzbConnection.ValidateServerCertificate));

                    stream.AuthenticateAsClient(_server.Server);

                    _netStream = stream;
                }
                else
                {
                    _netStream = this._nntpClient.GetStream();
                }
            }
            finally
            {
                _synchronizationObject.ExitWriteLock();
            }
        }

        private static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            sslPolicyErrors = SslPolicyErrors.None;
            return true;
        }
    }
}
