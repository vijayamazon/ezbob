namespace EchoSignLib {
	using System.Collections.Generic;
	using System.Linq;

	public class EchoSignSendResult {
		public EchoSignSendResult() {
			this.errorList = new List<string>();
		} // constructor

		public EchoSignSendResultCode Code { get; set; }
		public IReadOnlyCollection<string> ErrorList { get { return this.errorList.AsReadOnly(); } }

		public EchoSignSendResult AddErrorMessage(string errorMessage, params object[] args) {
			if (string.IsNullOrWhiteSpace(errorMessage))
				return this;

			if (args.Length < 1)
				this.errorList.Add(errorMessage);
			else
				this.errorList.Add(string.Format(errorMessage, args));

			return this;
		} // AddErrorMessage

		private readonly List<string> errorList;
	} // class EchoSignSendResult
} // namespace
