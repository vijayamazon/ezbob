namespace EzService {
	using System;
	using System.Collections.Generic;
	using Ezbob.Backend.Strategies;

	public class ExecuteArguments {

		public ExecuteArguments(params object[] oStrategyArgs) {
			StrategyArguments = new List<object>(oStrategyArgs);
		} // constructor

		public virtual Type StrategyType {
			get {
				if (m_oStrategyType == null)
					m_oStrategyType = typeof (AStrategy);

				return m_oStrategyType;
			}
			set { m_oStrategyType = value ?? typeof (AStrategy); }
		} // StrategyType

		private Type m_oStrategyType;

		public int? CustomerID { get; set; }
		public int? UserID { get; set; }

		public virtual Action<AStrategy, ActionMetaData> OnInit { get; set; }

		public virtual Action<ActionMetaData> OnLaunch { get; set; }

		public Action<ActionMetaData> OnException { get; set; }

		public virtual Action<AStrategy, ActionMetaData> OnSuccess { get; set; }
		public virtual Action<ActionMetaData> OnFail { get; set; }

		public List<object> StrategyArguments { get; private set; }

		public string StrategyArgumentsStr {
			get { return string.Join("; ", StrategyArguments); }
		} // StrategyArgumentsStr

	} // class ExecuteArguments
} // namespace
