namespace EzBob.AmazonServiceLib {
	using System;
	using EzBob.CommonLib;

	public class AmazonServiceInfo : IMarketplaceServiceInfo {
		public string Description {
			get {
				return "amazon";
			}
		}

		public string DisplayName {
			get {
				return "Amazon";
			}
		}

		public Guid InternalId {
			get {
				return new Guid("{A4920125-411F-4BB9-A52D-27E8A00D0A3B}");
			}
		}

		public bool IsPaymentAccount {
			get {
				return false;
			}
		}
	}
}
