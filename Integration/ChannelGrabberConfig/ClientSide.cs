using System;
using System.Collections.Generic;

namespace Integration.ChannelGrabberConfig {
	#region class ClientSide

	public class ClientSide : ICloneable {
		#region public

		#region enum SupportedErrorMessages

		public enum SupportedErrorMessages {
			CannotValidate
		} // enum SupportedErrorMessages

		#endregion enum SupportedErrorMessages

		#region constructor

		public ClientSide() {
			StoreInfoStepModelShops = "";
			LinkForm = new LinkForm();
			m_oErrorMessages = new SortedDictionary<string, string>();
		} // constructor

		#endregion constructor

		#region properties

		public string StoreInfoStepModelShops { get; set; }
		public LinkForm LinkForm { get; set; }
		public List<ErrorMessage> ErrorMessages { get; set; }

		#endregion properties

		#region method Parse

		public void Parse() {
			m_oErrorMessages.Clear();

			foreach (ErrorMessage msg in ErrorMessages)
				m_oErrorMessages[msg.ID] = msg.Text;

			ErrorMessages.Clear();
		} // Parse

		#endregion method Parse

		#region method Clone

		public object Clone() {
			var x = new ClientSide {
				StoreInfoStepModelShops = (string)this.StoreInfoStepModelShops.Clone(),
				LinkForm = (LinkForm)this.LinkForm.Clone(),
			};

			foreach (KeyValuePair<string, string> pair in m_oErrorMessages)
				x.m_oErrorMessages[pair.Key] = pair.Value;

			return x;
		} // Clone

		#endregion method Clone

		#region method ErrorMessage

		public string ErrorMessage(SupportedErrorMessages nMsgID) {
			string sMsgID = nMsgID.ToString();

			if (m_oErrorMessages.ContainsKey(sMsgID))
				return m_oErrorMessages[sMsgID];

			switch (nMsgID) {
			case SupportedErrorMessages.CannotValidate:
				return "Cannot validate: invalid credentials.";

			default:
				throw new ConfigException(string.Format("Unsupported error message requested: {0}", sMsgID));
			} // switch
		} // ErrorMessage

		#endregion method ErrorMessage

		#endregion public

		#region private

		private SortedDictionary<string, string> m_oErrorMessages;

		#endregion private
	} // class ClientSide

	#endregion class ClientSide
} // namespace Integration.ChannelGrabberConfig
