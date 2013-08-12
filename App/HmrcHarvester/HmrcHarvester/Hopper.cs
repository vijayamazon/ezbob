using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Ezbob.HmrcHarvester {
	#region class Hopper

	/// <summary>
	/// Harvest result (retrieved files and errors).
	/// </summary>
	public class Hopper {
		#region public

		#region constructor

		/// <summary>
		/// Creates a Hopper object.
		/// </summary>
		public Hopper() {
			Errors = new SortedDictionary<DataType, SortedDictionary<FileType, SortedDictionary<string, HarvesterError>>>();
			Files = new SortedDictionary<DataType, SortedDictionary<FileType, SortedDictionary<string, byte[]>>>();
			Seeds = new SortedDictionary<DataType, SortedDictionary<string, ISeeds>>();

			ErrorCount = 0;

			foreach (DataType dt in Enum.GetValues(typeof (DataType))) {
				Errors[dt] = new SortedDictionary<FileType, SortedDictionary<string, HarvesterError>>();
				Files[dt] = new SortedDictionary<FileType, SortedDictionary<string, byte[]>>();
				Seeds[dt] = new SortedDictionary<string, ISeeds>();

				foreach (FileType ft in Enum.GetValues(typeof (FileType))) {
					Errors[dt][ft] = new SortedDictionary<string, HarvesterError>();
					Files[dt][ft] = new SortedDictionary<string, byte[]>();
				} // for each file type
			} // for each data type
		} // constructor

		#endregion constructor

		#region method Add

		/// <summary>
		/// Adds an error message. Thread safe.
		/// </summary>
		/// <param name="fi">File identifier where the error occured.</param>
		/// <param name="response">HTTP response with an error.</param>
		public void Add(SheafMetaData fi, HttpResponseMessage response) {
			lock (this) {
				Errors[fi.DataType][fi.FileType][fi.BaseFileName] = new HarvesterError {
					Code = response.StatusCode,
					Message = response.ReasonPhrase
				};

				ErrorCount++;
			} // lock

		} // Add

		/// <summary>
		/// Adds a file. Thread safe.
		/// </summary>
		/// <param name="fi">File identifier.</param>
		/// <param name="oFileData">File to add.</param>
		public void Add(SheafMetaData fi, byte[] oFileData) {
			lock (this) {
				Files[fi.DataType][fi.FileType][fi.BaseFileName] = oFileData;
			} // lock
		} // Add

		/// <summary>
		/// Adds parsed data. Thread safe.
		/// </summary>
		/// <param name="fi">File identifier.</param>
		/// <param name="oData">Data to add.</param>
		public void Add(SheafMetaData fi, ISeeds oData) {
			if (oData == null)
				return;

			lock (this) {
				Seeds[fi.DataType][fi.BaseFileName] = oData;
			} // lock
		} // Add

		#endregion method Add

		#region property Errors

		/// <summary>
		/// Errors storage.
		/// </summary>
		public SortedDictionary<DataType, SortedDictionary<FileType, SortedDictionary<string, HarvesterError>>> Errors { get; private set; }

		#endregion property Errors 

		#region property ErrorCount

		/// <summary>
		/// Number of errors (e.g. to check that there are no errors).
		/// </summary>
		public int ErrorCount { get; private set; }

		#endregion property ErrorCount

		#region property Files

		/// <summary>
		/// Files storage.
		/// </summary>
		public SortedDictionary<DataType, SortedDictionary<FileType, SortedDictionary<string, byte[]>>> Files { get; private set; } 

		#endregion property Files

		#region property Seeds

		/// <summary>
		/// Parsed data storage.
		/// </summary>
		public SortedDictionary<DataType, SortedDictionary<string, ISeeds>> Seeds { get; private set; }

		#endregion property Seeds

		#region property ForEachFile

		/// <summary>
		/// Performs specified action on every file of specific data type and file type.
		/// </summary>
		/// <param name="dt">Data type to process.</param>
		/// <param name="ft">File type to process.</param>
		/// <param name="action">Action to take. Does nothing if the action is null. Action arguments:
		/// data type, file type, file name, file data.
		/// </param>
		public void ForEachFile(DataType dt, FileType ft, Action<DataType, FileType, string, byte[]> action) {
			if (action == null)
				return;

			foreach (KeyValuePair<string, byte[]> f in Files[dt][ft])
				action(dt, ft, f.Key, f.Value);
		} // ForEachFile

		#endregion property ForEachFile

		#endregion public
	} // class Hopper

	#endregion class Hopper
} // namespace Ezbob.HmrcHarvester
