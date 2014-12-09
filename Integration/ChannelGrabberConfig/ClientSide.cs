using System;
using System.Collections.Generic;

namespace Integration.ChannelGrabberConfig {

	public class ClientSide : ICloneable {

		public enum SupportedErrorMessages {
			CannotValidate
		} // enum SupportedErrorMessages

		public ClientSide() {
			StoreInfoStepModelShops = "";
			LinkForm = new LinkForm();
			m_oErrorMessages = new SortedDictionary<string, string>();
		} // constructor

		public string StoreInfoStepModelShops { get; set; }
		public LinkForm LinkForm { get; set; }
		public List<ErrorMessage> ErrorMessages { get; set; }

		public void Parse() {
			m_oErrorMessages.Clear();

			foreach (ErrorMessage msg in ErrorMessages)
				m_oErrorMessages[msg.ID] = msg.Text;

			ErrorMessages.Clear();
		} // Parse

		public object Clone() {
			var x = new ClientSide {
				StoreInfoStepModelShops = (string)this.StoreInfoStepModelShops.Clone(),
				LinkForm = (LinkForm)this.LinkForm.Clone(),
			};

			foreach (KeyValuePair<string, string> pair in m_oErrorMessages)
				x.m_oErrorMessages[pair.Key] = pair.Value;

			return x;
		} // Clone

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

		private SortedDictionary<string, string> m_oErrorMessages;

	} // class ClientSide

} // namespace Integration.ChannelGrabberConfig
