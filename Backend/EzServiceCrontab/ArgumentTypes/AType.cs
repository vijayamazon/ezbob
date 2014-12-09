namespace EzServiceCrontab.ArgumentTypes {
	using System;
	using Ezbob.Utils;

	internal abstract class AType<T> : IType {

		public virtual string Name { get; private set; }

		public virtual string FullName {
			get { return Name + (CanBeNull ? "?" : string.Empty); }
		} // FullName

		public virtual bool CanBeNull {
			get { return TypeUtils.CanBeNull(typeof (T)); }
		} // CanBeNull

		public virtual Type UnderlyingType {
			get { return typeof (T); }
		} // UnderlyingType

		public virtual string Hint {
			get {
				if (m_sHint == null)
					m_sHint = string.Empty;

				return m_sHint;
			} // get
			set {
				m_sHint = (value ?? string.Empty).Trim();
				OnHintUpdated();
			} // set
		} // Hint

		private string m_sHint;

		public override string ToString() {
			return string.Format("{0} as {1}", FullName, UnderlyingType);
		} // ToString

		public virtual IType Clone() {
			return (IType)Activator.CreateInstance(this.GetType());
		} // Clone

		public abstract object CreateInstance(string sValue);

		public virtual bool Differs(IType oPrevious) {
			if (oPrevious == null)
				return true;

			if (this.GetType() != oPrevious.GetType())
				return true;

			if (this.UnderlyingType != oPrevious.UnderlyingType)
				return true;

			return false;
		} // Differs

		protected AType(string sName) {
			Name = string.IsNullOrWhiteSpace(sName) ? typeof(T).ToString() : sName.Trim();
		} // constructor

		protected virtual void OnHintUpdated() {
			// Nothing here, to be overridden.
		} // OnHintUpdated

		protected Exception GetError(string sValue, Exception oInner = null) {
			return new InvalidCastException(
				"Could not extract " + this + " from " +
				(sValue == null ? "a null string" : " '" + sValue + "'") +
				".",
				oInner
			);
		} // GetError

	} // class AType
} // namespace
