namespace EzServiceConfiguration {
	using System;

	public abstract class AConfigurationData {

		public virtual void Init() {
			LoadFromDB();

			if (!IsValid())
				throw new Exception(InvalidExceptionMessage);

			Adjust();

			WriteToLog();
		} // Init

		protected AConfigurationData() {
		} // constructor

		protected abstract void LoadFromDB();
		protected abstract void WriteToLog();
		protected abstract void Adjust();

		protected abstract string InvalidExceptionMessage { get; }

		protected abstract bool IsValid();

	} // class AConfigurationData

} // namespace EzServiceConfiguration
