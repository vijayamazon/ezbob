using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EzBob.CommonLib;
using Ezbob.Logger;

namespace Integration.ChannelGrabberSecurityInfoConverter {

	internal class FileSaver : SafeLog {

		public FileSaver(ASafeLog oLog) : base(oLog) {
			m_sCurPath = Path.Combine(Directory.GetCurrentDirectory(), "utc." + DateTime.UtcNow.ToString("yyyy-MM-dd.HH-mm-ss"));

			Info("Saving files to {0}", m_sCurPath);

			try {
				if (!Directory.Exists(m_sCurPath))
					Directory.CreateDirectory(m_sCurPath);
			}
			catch (Exception e) {
				Fatal("Failed to create target directory: {0}", e.Message);
				throw new Exception("Failed to create target directory", e);
			} // try
		} // constructor

		public bool Save(int nItemID, byte[] oData) {
			string sFileName = Path.Combine(m_sCurPath, nItemID.ToString() + ".old.xml");

			Info("Saving old data to {0}", sFileName);

			try {
				File.WriteAllBytes(sFileName, oData);
				return true;
			}
			catch (Exception e) {
				Error("Failed to save data: {0}", e.Message);
				return false;
			} // try
		} // Save

		public void Save(int nItemID, AccountModel oData) {
			string sFileName = Path.Combine(m_sCurPath, nItemID.ToString() + ".new.xml");

			Info("Saving new data to {0}", sFileName);

			try {
				File.WriteAllText(sFileName, SerializeDataHelper.SerializeToString(oData));
			}
			catch (Exception e) {
				Error("Failed to save data: {0}", e.Message);
			} // try
		} // Save

		private string m_sCurPath;

	}

	// class FileSaver

} // namespace Integration.ChannelGrabberSecurityInfoConverter
