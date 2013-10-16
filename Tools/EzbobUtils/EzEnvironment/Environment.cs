using System.Diagnostics;
using System.IO;

using Ezbob.Logger;
using Newtonsoft.Json;

namespace Ezbob.Context {
	#region class Environment

	public class Environment : SafeLog {
		#region public

		public const string CompanyName = "Ezbob";
		public const string EnvNameFile = "environment.json";

		#region constructor

		public Environment(ASafeLog oLog = null) : base(oLog) {
			Detect();
		} // constructor

		public Environment(Name nName, string sVariant = null, ASafeLog oLog = null) : base(oLog) {
			Name = nName;
			Variant = (sVariant ?? "").Trim();
		} // constructor

		#endregion constructor

		#region property Name

		public Name Name { get; private set; }

		#endregion property Name

		#region property Variant

		public string Variant { get; private set; }

		#endregion property Variant

		#region property Context

		public string Context {
			get {
				return (Variant ?? "").Trim() == string.Empty
					? Name.ToString()
					: string.Format("{0}-{1}", Name, Variant);
			} // get
		} // Context

		#endregion property Context

		#region property MachineName

		public string MachineName { get { return System.Environment.MachineName; } }

		#endregion property MachineName

		#region property UserName

		public string UserName { get { return System.Environment.UserName; } }

		#endregion property UserName

		#region property Pid

		public int Pid { get { return Process.GetCurrentProcess().Id; } } // Pid

		#endregion property Pid

		#endregion public

		#region private

		#region method Detect

		private void Detect() {
			Name = Name.Unknown;

			string[] aryPaths = {
				Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), CompanyName),
				Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonProgramFiles), CompanyName),
				Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonProgramFilesX86), CompanyName),
				Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFiles), CompanyName),
				Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFilesX86), CompanyName),
			};

			foreach (var sDir in aryPaths) {
				var sPath = Path.Combine(sDir, EnvNameFile);

				Debug("Detect env: looking for {0}", sPath);

				if (File.Exists(sPath)) {
					Debug("Detect env: found {0}", sPath);

					var sFileContents = File.ReadAllText(sPath);

					JsonEnvironmentName jen = JsonConvert.DeserializeObject<JsonEnvironmentName>(sFileContents);

					var nName = Name.Unknown;

					if (Name.TryParse(jen.Name, true, out nName)) {
						Debug("Detect env: parsed environment name successfully.");
						Name = nName;
					} // if

					Variant = (jen.Variant ?? "").Trim();

					break;
				} // if
			} // foreach

			Debug("Detect env: environment is {0}", Name.ToString());
			Debug("Detect env: variant is {0}", Variant);
		} // Detect

		#endregion method Detect

		#endregion private
	} // class Environment

	#endregion class Environment
} // namespace Ezbob.Context
