namespace EzServiceConfiguration {
	using System;

	#region class AConfigurationData

	public abstract class AConfigurationData {
		#region public

		#region method Init

		public virtual void Init() {
			LoadFromDB();

			if (!IsValid())
				throw new Exception(InvalidExceptionMessage);

			Adjust();

			WriteToLog();
		} // Init

		#endregion method Init

		#endregion public

		#region protected

		#region constructor

		protected AConfigurationData() {
		} // constructor

		#endregion constructor

		protected abstract void LoadFromDB();
		protected abstract void WriteToLog();
		protected abstract void Adjust();

		protected abstract string InvalidExceptionMessage { get; }

		protected abstract bool IsValid();

		#endregion protected
	} // class AConfigurationData

	#endregion class AConfigurationData
} // namespace EzServiceConfiguration
