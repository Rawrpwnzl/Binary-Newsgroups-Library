using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using NzbComet.ServerCommands;

namespace NzbComet
{

    internal class NntpParser
    {
        private NzbConnection _connection;
        private ReaderWriterLockSlim _synchronizationObject;

        public NntpParser(NzbConnection connection)
        {
            ArgumentChecker.ThrowIfNull("connection", connection);

            _connection = connection;
            _synchronizationObject = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        }

        public void Authenticate(string username, string password)
        {
            if (_connection.Status == NzbConnectionStatus.Discarded)
            {
                return;
            }

            try
            {
                string receivedServerMessage = string.Empty;

                receivedServerMessage = this.ReadLine();

                if (!string.IsNullOrWhiteSpace(receivedServerMessage))
                {
                    var authenticateCommand = new NzbServerAuthenticateUserCommand(username);

                    this.TrySendCommand(authenticateCommand);

                    if (authenticateCommand.Response == NzbServerCommandResponse.Success)
                    {
                        var authenticatePasswordCommand = new NzbServerAuthenticatePasswordCommand(password);

                        this.TrySendCommand(authenticatePasswordCommand);

                        if (authenticatePasswordCommand.Response == NzbServerCommandResponse.Success)
                        {
                            this.TrySendCommand(new NzbServerSwitchToReaderModeCommand());
                            receivedServerMessage = this.ReadLine();
                        }
                        else
                        {
                            throw new NzbAuthenticationFailedException();
                        }
                    }
                }
                else if (receivedServerMessage.StartsWith("2"))
                {
                    this.TrySendCommand(new NzbServerSwitchToReaderModeCommand());

                    receivedServerMessage = this.ReadLine();
                }
                else
                {
                    throw new NzbAuthenticationFailedException();
                }
            }
            catch (Exception)
            {
                throw new NzbAuthenticationFailedException();
            }
        }

        public void Disconnect()
        {
            try
            {
                var quitCommand = new NzbServerQuitCommand();

                this.TrySendCommand(quitCommand);
            }
            catch (Exception)
            {
            }
        }

        public void JoinGroup(string newGroup)
        {
            TrySendCommand(new NzbServerJoinGroupCommand(newGroup));
        }

        public string DownloadArticle(string articleId)
        {
            if (_connection.Status == NzbConnectionStatus.Discarded)
            {
                return null;
            }

            try
            {
                StringBuilder downloadedArticle = new StringBuilder(0x40000);
                StreamReader reader = null;

                reader = new StreamReader(_connection.BaseStream, Encoding.GetEncoding("iso-8859-1"));

                bool isArticleRetrievalCompleted = false;

                reader.ReadLine();

                if (reader.ReadLine().StartsWith("430"))
                {
                    throw new NzbArticleNotFoundException(articleId);
                }

                do
                {
                    if (_connection.Status == NzbConnectionStatus.Discarded)
                    {
                        return null;
                    }

                    string downloadedArticleLine = reader.ReadLine();

                    if (downloadedArticleLine == ".")
                    {
                        isArticleRetrievalCompleted = true;
                    }
                    else
                    {
                        if (downloadedArticleLine.StartsWith(".."))
                        {
                            downloadedArticleLine = downloadedArticleLine.Remove(0, 1);
                        }

                        downloadedArticle.Append(downloadedArticleLine);
                        downloadedArticle.Append("\r\n");
                    }

                }
                while (!isArticleRetrievalCompleted && _connection.Status == NzbConnectionStatus.Connected);


                return downloadedArticle.ToString();
            }
            catch (Exception)
            {
                throw new NzbArticleDownloadFailedException(articleId);
            }

        }

        private string ReadLine()
        {
            if (_connection.Status == NzbConnectionStatus.Discarded)
            {
                return null;
            }

            string readLine;

            try
            {
                _synchronizationObject.EnterWriteLock();

                readLine = new StreamReader(_connection.BaseStream).ReadLine();
            }
            finally
            {
                _synchronizationObject.ExitWriteLock();
            }

            return readLine;
        }

        internal bool TrySendCommand(NzbServerCommand command)
        {
            if (_connection.Status == NzbConnectionStatus.Discarded)
            {
                return false;
            }

            try
            {
                _synchronizationObject.EnterWriteLock();

                try
                {
                    new StreamWriter(_connection.BaseStream) { AutoFlush = true }.WriteLine(command.Command);

                    if (command.InvokesResponse)
                    {
                        command.CheckResponse(ReadLine());
                    }


                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
            finally
            {
                _synchronizationObject.ExitWriteLock();
            }
        }
    }
}
