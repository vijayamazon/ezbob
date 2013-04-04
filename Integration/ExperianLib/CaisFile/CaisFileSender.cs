using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Repository;
using EzBob.Configuration;
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
                SaveToBase(fileName, CaisUploadStatus.Uploaded);
            }
            catch (Exception exception)
            {
                SaveToBase(fileName, CaisUploadStatus.UploadError);
                Log.Error(exception.Message);
                throw;
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
                    SaveToBase(Hostname + file.FileName, CaisUploadStatus.Uploaded);
                }
            }
            catch (Exception exception)
            {
                SaveToBase(Hostname + file.FileName, CaisUploadStatus.UploadError);
                Log.Error(exception.Message);
                throw;
            }
        }
        public void UploadData(string fileData, string filePath)
        {
            ServicePointManager.ServerCertificateValidationCallback += ValidateRemoteCertificate;
            _client.Credentials = new NetworkCredential(UserName, Password);
            try
            {
                var fileName = Path.GetFileName(filePath);
                _client.UploadData(Hostname + fileName, GetBytes(fileData));
                SaveToBase(filePath, CaisUploadStatus.Uploaded);
            }
            catch (Exception exception)
            {
                SaveToBase(filePath, CaisUploadStatus.UploadError);
                Log.Error(exception.Message);
                throw;
            }
        }
        public void SaveToBase(string filePath, CaisUploadStatus status)
        {
            var caisReportsHistory = _caisReportsHistoryRepository.GetAll()
                .FirstOrDefault(x => x.FilePath == filePath) ??
                new CaisReportsHistory
                {
                    Date = DateTime.Now,
                    FilePath = filePath,
                    FileName = Path.GetFileName(filePath),
                    UploadStatus = status
                };
            caisReportsHistory.UploadStatus = status;
            _caisReportsHistoryRepository.SaveOrUpdate(caisReportsHistory);
        }

        private static byte[] GetBytes(string str)
        {
            var bytes = new byte[str.Length * sizeof(char)];
            Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }
    }
}