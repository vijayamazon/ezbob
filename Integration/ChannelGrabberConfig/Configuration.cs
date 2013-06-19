using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using log4net;
using Newtonsoft.Json;

namespace Integration.ChannelGrabberConfig {
	#region class Configuration

	public class Configuration {
		#region static constructor

		private const string CompanyName = "Ezbob";
		private const string EnvNameFile = "channelgrabber.json";

		#endregion static constructor

		#region public

		#region property Instance

		public static Configuration Instance { get { return GetInstance(); } }

		#endregion property Instance

		#region method GetInstance

		public static Configuration GetInstance(ILog oLog = null) {
			lock (typeof(Configuration)) {
				if (ms_oConfiguration != null)
					return ms_oConfiguration;

				string[] aryPaths = {
					Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), CompanyName),
					Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonProgramFiles), CompanyName),
					Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonProgramFilesX86), CompanyName),
					Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFiles), CompanyName),
					Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFilesX86), CompanyName),
				};

				foreach (var sDir in aryPaths) {
					var sFilePath = Path.Combine(sDir, EnvNameFile);

					if (oLog != null)
						oLog.DebugFormat("Trying to load Channel Grabber configuration from {0}", sFilePath);

					string sFileContent = null;

					try {
                        if (!File.Exists(sFilePath)) continue;
						sFileContent = File.ReadAllText(sFilePath);
					}
					catch (Exception e) {
						if (oLog != null)
							oLog.ErrorFormat("Failed to read Channel Grabber configuration from {0} because {1}", sFilePath, e.Message);

						continue;
					} // try

					ms_oConfiguration = new Configuration(sFileContent, oLog);
					return ms_oConfiguration;
				} // for each
			} // lock

			throw new ConfigException("Failed to load Channel Grabber configuration.");
		} // GetInstance

		#endregion method GetInstance

		#region method GetVendorInfo

		public VendorInfo GetVendorInfo(string sAccountTypeName) {
			return Vendors.ContainsKey(sAccountTypeName) ? Vendors[sAccountTypeName] : null;
		} // GetVendorInfo

		#endregion method GetVendorInfo

		#region method ForEachVendor

		public void ForEachVendor(Action<VendorInfo> oCallback) {
			if (oCallback == null)
				return;

			foreach (KeyValuePair<string, VendorInfo> vi in Vendors)
				oCallback.Invoke(vi.Value);
		} // ForEachVendor

		#endregion method ForEachVendor

		#region property Vendors

		public Dictionary<string, VendorInfo> Vendors { get; private set; }

		#endregion property Vendors

		#region method GetSourceLabelCSS

		public string GetSourceLabelCSS() {
			var sb = new StringBuilder();

			ForEachVendor(vi => {
				sb.AppendFormat(".source_labels.{0}, .source_labels.{1} {{", vi.Name, vi.Name.ToLower());
				vi.ClientSide.LinkForm.SourceLabel.ForEach( css => sb.Append(css.ToString()) );
				sb.Append("}");

				sb.AppendFormat(".source_labels_on.{0}, .source_labels_on.{1} {{", vi.Name, vi.Name.ToLower());
				vi.ClientSide.LinkForm.SourceLabelOn.ForEach( css => sb.Append(css.ToString()) );
				sb.Append("}");
			});

			return sb.ToString();
		} // GetSourceLabelCSS

		#endregion method GetSourceLabelCSS

		#endregion public

		#region private

		#region constructor

		private Configuration(string sConfigurationJson, ILog oLog = null) {
			Log = oLog;

			Debug("Parsing Channel Grabber connectors configuration...");

			var lst = JsonConvert.DeserializeObject<List<VendorInfo>>(sConfigurationJson);

			Vendors = new Dictionary<string, VendorInfo>();
			lst.ForEach(v => {
				v.Parse();
				Vendors[v.Name] = v;
			});

			Debug(Vendors.ToString());

			Debug("Parsing Channel Grabber connectors configuration complete.");
		} // constructor

		#endregion constructor

		#region method Debug

		private void Debug(string sFormat, params object[] args) {
			if (Log == null)
				return;

			Log.DebugFormat(sFormat, args);
		} // Debug

		#endregion method Error

		#region method Error

		private void Error(string sFormat, params object[] args) {
			if (Log == null)
				return;

			Log.ErrorFormat(sFormat, args);
		} // Error

		#endregion method Error

		#region property Log

		private ILog Log { get; set; }

		#endregion property Log

		#region property Configurations

		private static Configuration ms_oConfiguration;

		#endregion property Configurations

		#endregion private
	} // class Configuration

	#endregion class Configuration
} // namespace Integration.ChannelGrabberConfig
