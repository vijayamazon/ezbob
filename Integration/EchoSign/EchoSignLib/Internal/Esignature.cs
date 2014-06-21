namespace EchoSignLib {
	using System.Collections.Generic;
	using Ezbob.Utils;

	internal class Esignature : ITraversable {
		public Esignature(int nSignatureID) {
			ID = nSignatureID;
			Signers = new SortedDictionary<int, Esigner>();
		} // constructor

		public int ID { get; private set; }
		public int CustomerID { get; set; }
		public string DocumentKey { get; set; }

		public SortedDictionary<int, Esigner> Signers { get; private set; }
	} // class Esignature
} // namespace
