using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Repository;
using EzBob.Configuration;
using Scorto.Configuration;
using StructureMap;
using log4net;

namespace ExperianLib.CaisFile
{
    public class CaisFileSender
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(CaisFileSender));
        private readonly ExperianIntegrationParams _config;
        private readonly WebClient _client = new WebClient();

        public string Hostname { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        private readonly CaisReportsHistoryRepository _caisReportsHistoryRepository;

        public CaisFileSender()
        {
            _config = ConfigurationRootBob.GetConfiguration().Experian;
            Hostname = _config.SecureFtpHostName;
            UserName = _config.SecureFtpUserName;
            Password = _config.SecureFtpUserPassword;
            _caisReportsHistoryRepository = ObjectFactory.GetInstance<CaisReportsHistoryRepository>();
        }

        private static bool ValidateRemoteCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors policyErrors)
        {
            return true;
        }

        public void UploadFile(string fileName)
        {
            ServicePointManager.ServerCertificateValidationCallback += ValidateRemoteCertificate;
            _client.Credentials = new NetworkCredential(UserName, Password);
            try
            {
                _client.UploadFile(Hostname, fileName);
            }
            catch (Exception exception)
            {
                Log.Error(exception.Message);
            }
        }

        public void UploadData(HttpPostedFileBase file)
        {
            ServicePointManager.ServerCertificateValidationCallback += ValidateRemoteCertificate;
            _client.Credentials = new NetworkCredential(UserName, Password);
            try
            {
                using (var target = new MemoryStream())
                {
                    file.InputStream.CopyTo(target);
                    var data = target.ToArray();
                    _client.UploadData(Hostname + file.FileName, data);
                }
            }
            catch (Exception exception)
            {
                Log.Error(exception.Message);
                throw;
            }
        }
        public void SaveToBase(CaisReportType type, int ofItems, int goodUsers,CaisReportUploadStatus uploadStatus, string filePath)
        {
            var caisReportsHistory = new CaisReportsHistory()
                                                        {
                                                            Date = DateTime.UtcNow,
                                                            FileName = System.IO.Path.GetFileNameWithoutExtension(filePath),
                                                            OfItems = ofItems,
                                                            GoodUsers = goodUsers,
                                                            UploadStatus = uploadStatus,
                                                            FilePath = filePath
                                                        };
            _caisReportsHistoryRepository.Save(caisReportsHistory);
        }
    }
}