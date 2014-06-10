﻿namespace Integration.ChannelGrabberConfig {
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.IO;
	using System.Text;
	using log4net;
	using Newtonsoft.Json;
	using Ezbob.Logger;

	#region class Configuration

	public class Configuration {
		#region static constructor

		private const string CompanyName = "Ezbob";
		private const string EnvNameFile = "channelgrabber.json";
		private static readonly Guid ms_oHmrcGuid = new Guid("AE85D6FC-DBDB-4E01-839A-D5BD055CBAEA");

		#endregion static constructor

		#region public

		#region property Instance

		public static Configuration Instance { get { return GetInstance(new SafeILog(typeof(Configuration))); } }

		#endregion property Instance

		#region method GetInstance

		public static Configuration GetInstance(ILog oLog = null) {
			return GetInstance(new SafeILog(oLog));
		} // GetInstance

		public static Configuration GetInstance(ASafeLog oLog = null) {
			lock (typeof(Configuration)) {
				if (ms_oConfiguration != null)
					return ms_oConfiguration;

				var oPaths = new List<string>();

				oLog = oLog ?? new SafeLog();

				foreach (System.Environment.SpecialFolder nFld in new [] {
					System.Environment.SpecialFolder.ApplicationData,
					System.Environment.SpecialFolder.CommonProgramFiles,
					System.Environment.SpecialFolder.CommonProgramFilesX86,
					System.Environment.SpecialFolder.ProgramFiles,
					System.Environment.SpecialFolder.ProgramFilesX86
				}) {
// ReSharper disable EmptyGeneralCatchClause
					try {
						oPaths.Add(Path.Combine(System.Environment.GetFolderPath(nFld), CompanyName));
					}
					catch (Exception) {
						// silently ignore
					} // try
// ReSharper restore EmptyGeneralCatchClause
				}

				foreach (var sDir in oPaths) {
					string sFileContent;

					try {
						var sFilePath = Path.Combine(sDir, EnvNameFile);

						oLog.Debug("Trying to load Channel Grabber configuration from {0}", sFilePath);

						if (!File.Exists(sFilePath))
							continue;

						sFileContent = File.ReadAllText(sFilePath);
					}
					catch (Exception e) {
						oLog.Error("Failed to read Channel Grabber configuration: {0}", e.Message);
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
			return m_oInternalVendorsByName.ContainsKey(sAccountTypeName) ? m_oInternalVendorsByName[sAccountTypeName] : null;
		} // GetVendorInfo

		public VendorInfo GetVendorInfo(Guid oAccountTypeInternalID) {
			return m_oInternalVendorsByGuid.ContainsKey(oAccountTypeInternalID) ? m_oInternalVendorsByGuid[oAccountTypeInternalID] : null;
		} // GetVendorInfo

		#endregion method GetVendorInfo

		#region method ForEachVendor

		public void ForEachVendor(Action<VendorInfo> oCallback) {
			if (oCallback == null)
				return;

			foreach (KeyValuePair<string, VendorInfo> vi in m_oInternalVendorsByName)
				oCallback.Invoke(vi.Value);
		} // ForEachVendor

		#endregion method ForEachVendor

		#region method ForEachPureVendor

		public void ForEachPureVendor(Action<VendorInfo> oCallback) {
			if (oCallback == null)
				return;

			foreach (KeyValuePair<string, VendorInfo> vi in m_oInternalPureVendorsByName)
				oCallback.Invoke(vi.Value);
		} // ForEachPureVendor

		#endregion method ForEachPureVendor

		#region property Vendors

		public IDictionary<string, VendorInfo> Vendors {
			get { return m_oVendors; }
		} // Vendors

		private IDictionary<string, VendorInfo> m_oVendors; 

		#endregion property Vendors

		#region property PureVendors

		public IDictionary<string, VendorInfo> PureVendors {
			get { return m_oPureVendors; }
		} // PureVendors

		private IDictionary<string, VendorInfo> m_oPureVendors; 

		#endregion property PureVendors

		#region property Hmrc

		public VendorInfo Hmrc {
			get { return GetVendorInfo(ms_oHmrcGuid); }
		} // Hmrc

		#endregion property Hmrc

		#region GetMarketPlaceDiscriminator

		public string GetMarketplaceDiscriminator() {
			var sb = new StringBuilder();

			ForEachVendor( vi => sb.AppendFormat(" WHEN '{0}' THEN 'ChannelGrabber'", vi.Name) );

			return sb.ToString();
		} // GetMarketplaceDiscriminator

		#endregion GetMarketPlaceDiscriminator

		#endregion public

		#region private

		#region constructor

		private Configuration(string sConfigurationJson, ASafeLog oLog = null) {
			Log = oLog ?? new SafeLog();

			Log.Debug("Parsing Channel Grabber connectors configuration...");

			var lst = JsonConvert.DeserializeObject<List<VendorInfo>>(sConfigurationJson);

			m_oInternalVendorsByName = new SortedDictionary<string, VendorInfo>();
			m_oInternalVendorsByGuid = new SortedDictionary<Guid, VendorInfo>();
			m_oInternalPureVendorsByName = new SortedDictionary<string, VendorInfo>();

			lst.ForEach(v => {
				v.Parse();
				m_oInternalVendorsByName[v.Name] = v;
				m_oInternalVendorsByGuid[v.Guid()] = v;

				if (v.Guid() != ms_oHmrcGuid)
					m_oInternalPureVendorsByName[v.Name] = v;
			});

			m_oVendors = new ReadOnlyDictionary<string, VendorInfo>(m_oInternalVendorsByName);
			m_oPureVendors = new ReadOnlyDictionary<string, VendorInfo>(m_oInternalPureVendorsByName);

			// You are welcome to add your machine name here.

			if (System.Environment.MachineName.StartsWith("stasd"))
				Log.Debug("\n\n****\n\n{0} vendors found: {1}.\n\n****\n", m_oInternalVendorsByName.Count, string.Join(", ", m_oInternalVendorsByName.Keys));
			else {
				var sb = new StringBuilder();

				sb.AppendFormat("\n\n****\n\n{0} vendors found: {1}.\n\n****\n", m_oInternalVendorsByName.Count, string.Join(", ", m_oInternalVendorsByName.Keys));

				foreach (KeyValuePair<string, VendorInfo> pair in m_oInternalVendorsByName)
					sb.AppendFormat("\n{0}\n", pair.Value);

				sb.AppendFormat("\n****\n\nEnd of vendors list\n\n****\n\n");

				Log.Debug("{0}", sb.ToString());
			} // if

			Log.Debug("Parsing Channel Grabber connectors configuration complete.");
		} // constructor

		#endregion constructor

		#region property Log

		private ASafeLog Log { get; set; }

		#endregion property Log

		#region property Configurations

		private static Configuration ms_oConfiguration;

		#endregion property Configurations

		private readonly SortedDictionary<string, VendorInfo> m_oInternalVendorsByName;
		private readonly SortedDictionary<Guid, VendorInfo> m_oInternalVendorsByGuid;
		private readonly SortedDictionary<string, VendorInfo> m_oInternalPureVendorsByName;

		#endregion private
	} // class Configuration

	#endregion class Configuration
} // namespace Integration.ChannelGrabberConfig
