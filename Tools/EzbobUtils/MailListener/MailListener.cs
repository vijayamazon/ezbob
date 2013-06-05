namespace MailListener
{
    using System;
    using MailBee;
    using MailBee.ImapMail;
    using MailBee.Mime;
    using System.Threading;
    using Logger = Logger.Logger;

    public abstract class MailListener
    {
        private Imap imp;                                          // Imap object reference
        private bool exiting;                                      // Flag. If true, terminating the application is in progress and we should stop idle without attempt to download new messages
        private DateTime startTime;                                // Time when idle started. Used for restarting idle by timeout
        private readonly TimeSpan timeout = new TimeSpan(0, 5, 0); // 5 minutes timeout for idle

        protected long LastId;
        private readonly int mailboxReconnectionIntervalSeconds, port;
        private readonly string server, loginAddress, loginPassword, mailBeeLicenseKey;

        protected MailListener(int mailboxReconnectionIntervalSeconds, string server, int port, string loginAddress, string loginPassword, string mailBeeLicenseKey)
        {
            this.mailboxReconnectionIntervalSeconds = mailboxReconnectionIntervalSeconds;
            this.server = server;
            this.port = port;
            this.loginAddress = loginAddress;
            this.loginPassword = loginPassword;
            this.mailBeeLicenseKey = mailBeeLicenseKey;
            AppDomain.CurrentDomain.ProcessExit += (s, e) => exiting = true;
            LicenseImap();
            ConnectToMailboxLoop();
        }

        protected UidCollection Search(string cond)
        {
            return (UidCollection)imp.Search(true, cond, null);
        }

        private void ConnectToMailboxLoop()
        {
            while (true)
            {
                try
                {
                    if (ConnectToMailbox())
                    {
                        return;
                    }
                }
                catch (Exception e)
                {
                    Logger.ErrorFormat("Failed connecting to mailbox with error:", e);
                    Logger.InfoFormat("Waiting {0} seconds", mailboxReconnectionIntervalSeconds);
                    Thread.Sleep(TimeSpan.FromSeconds(mailboxReconnectionIntervalSeconds));
                }
            }
        }

        private bool ConnectToMailbox()
        {
            imp = new Imap();

            // Enable SSL/TLS if necessary
            imp.SslMode = MailBee.Security.SslStartupMode.OnConnect;

            // Connect to IMAP server
            imp.Connect(server, port);
            Logger.Info("Connected to the server");

            // Check for IDLE support
            if (imp.GetExtension("IDLE") == null)
            {
                Logger.Info("IDLE not supported");
                imp.Disconnect();
                return false;
            }

            // Log into IMAP account
            imp.Login(loginAddress, loginPassword);
            Logger.Info("Logged into the server");

            // Select Inbox folder
            imp.SelectFolder("Inbox");

            // Add message status event handler which will "listen to server" while idling
            imp.MessageStatus += imp_MessageStatus;

            // Add idling event handler which is used for timer
            imp.Idling += imp_Idling;

            // Start idle timer
            TimerStart();

            // Start idling
            imp.BeginIdle(IdleCallback, null);
            Logger.Info("Started idling");
            return true;
        }

        private void LicenseImap()
        {
            // Licensing IMAP component. If the license key is invalid, it'll throw MailBeeLicenseException
            try
            {
                Global.LicenseKey = mailBeeLicenseKey;
            }
            catch (MailBeeLicenseException e)
            {
                Logger.ErrorFormat("License key is invalid:{0}", e);
                Environment.Exit(-1);
            }
        }

        /// <summary>
        /// Idling event handler. Ticks every 10 milliseconds while in idle state.
        /// </summary>
        private void imp_Idling(object sender, ImapIdlingEventArgs e)
        {
            try
            {
                // If the difference between start timer time and the current time >= timeout, initiate stopping idle
                if (DateTime.Now.Subtract(startTime) >= timeout)
                {
                    imp.StopIdle();
                    Logger.Info("Initiated stopping idle by TIMER");
                }
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Error occured while in imp_Idling:{0}", ex);
                Logger.Info("Trying to reconnect to mailbox");
                SafeDispose();
                ConnectToMailboxLoop();
            }
        }

        /// <summary>
        /// MessageStatus event handler which receives notifications from IMAP server. Learns about new messages in idling state
        /// </summary>
        private void imp_MessageStatus(object sender, ImapMessageStatusEventArgs e)
        {
            try
            {
                Logger.InfoFormat("Got {0} status update", e.StatusID);

                // RECENT status means new messages have just arrived to IMAP account. Initiate stopping idle and IdleCallback will download the messages
                if ("RECENT" == e.StatusID || "EXISTS" == e.StatusID)
                {
                    imp.StopIdle();
                    Logger.Info("Initiated stopping idle");
                }
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Error occured while in imp_MessageStatus:{0}", ex);
                Logger.Info("Trying to reconnect to mailbox");
                SafeDispose();
                ConnectToMailboxLoop();
            }
        }

        /// <summary>
        /// Callback for BeginIdle. It'll be called after stopping idle and will download new messages
        /// </summary>
        private void IdleCallback(IAsyncResult result)
        {
            try
            {
                imp.EndIdle();

                // If not exiting, i.e. just stopping idle and we should try to download new messages
                // Exiting means the application is being terminated and we shouldn't try downloading new messages
                if (!exiting)
                {
                    TimerStop();
                    Logger.Info("Stopped idling, will download messages");

                    UidCollection uids = FillUids();

                    if (uids.Count != 0 && LastId != uids[uids.Count - 1])
                    {
                        LastId = uids[uids.Count - 1];
                        Logger.InfoFormat("Last received mail:{0}", LastId);
                        MailMessageCollection msgs = imp.DownloadEntireMessages(uids.ToString(), true);
                        HandleMessages(msgs);

                        if (imp.IsIdle)
                        {
                            TimerStop();
                            imp.StopIdle();
                        }
                    }

                    // Messages have been downloaded and idling starts again
                    TimerStart();
                    imp.BeginIdle(IdleCallback, null);
                    Logger.Info("Started idling again");
                }
                else
                {
                    // If exiting is in progress, disconnect after stopping idle
                    imp.Disconnect();
                }
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Error occured while in IdleCallback:{0}", ex);
                Logger.Info("Trying to reconnect to mailbox");
                SafeDispose();
                ConnectToMailboxLoop();
            }
        }

        private void SafeDispose()
        {
            try
            {
                if (imp != null)
                {
                    imp.MessageStatus -= imp_MessageStatus;
                    imp.Idling -= imp_Idling;
                    imp.Dispose();
                }
            }
            catch (Exception e)
            {
                Logger.ErrorFormat("Failed disposing imap object:{0}", e);
            }
        }

        private void TimerStart()
        {
            // Store the current time
            startTime = DateTime.Now;
            Logger.Info("TIMER started");
        }

        private void TimerStop()
        {
            Logger.Info("TIMER stopped");
        }

        protected abstract UidCollection FillUids();

        protected abstract void HandleMessages(MailMessageCollection msgs);
    }
}
