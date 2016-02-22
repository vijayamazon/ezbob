namespace Ezbob.Integration.LogicalGlue.Engine.Interface {
	public enum GetInferenceMode {
		/// <summary>
		/// Check only local DB. Return NULL if there is nothing in the local DB.
		/// In this mode remote request is never issued.
		/// </summary>
		CacheOnly,

		/// <summary>
		/// Check local DB first. If data is found and is not too old, return it.
		/// Otherwise, make a remote call and save and return retrieved data.
		/// </summary>
		DownloadIfOld,

		/// <summary>
		/// Make a remote call and save and return retrieved data.
		/// Local DB is not checked before making a remote call.
		/// </summary>
		ForceDownload,
	} // enum GetInferenceMode
} // namespace
