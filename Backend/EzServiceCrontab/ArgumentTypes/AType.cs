﻿namespace EzServiceCrontab.ArgumentTypes {
	using System;
	using Ezbob.Utils;

	internal abstract class AType<T> : IType {
		#region public

		public virtual string Name { get; private set; }

		#region property FullName

		public virtual string FullName {
			get { return Name + (CanBeNull ? "?" : string.Empty); }
		} // FullName

		#endregion property FullName

		#region property CanBeNull

		public virtual bool CanBeNull {
			get { return TypeUtils.CanBeNull(typeof (T)); }
		} // CanBeNull

		#endregion property CanBeNull

		#region property UnderlyingType

		public virtual Type UnderlyingType {
			get { return typeof (T); }
		} // UnderlyingType

		#endregion property UnderlyingType

		#region property Hint

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

		#endregion property Hint

		#region method ToString

		public override string ToString() {
			return string.Format("{0} as {1}", FullName, UnderlyingType);
		} // ToString

		#endregion method ToString

		#region method Clone

		public virtual IType Clone() {
			return (IType)Activator.CreateInstance(this.GetType());
		} // Clone

		#endregion method Clone

		#region method CreateInstance

		public abstract object CreateInstance(string sValue);

		#endregion method CreateInstance

		#region method Differs

		public virtual bool Differs(IType oPrevious) {
			if (oPrevious == null)
				return true;

			if (this.GetType() != oPrevious.GetType())
				return true;

			if (this.UnderlyingType != oPrevious.UnderlyingType)
				return true;

			return false;
		} // Differs

		#endregion method Differs

		#endregion public

		#region protected

		#region constructor

		protected AType(string sName) {
			Name = string.IsNullOrWhiteSpace(sName) ? typeof(T).ToString() : sName.Trim();
		} // constructor

		#endregion constructor

		#region method OnHintUpdated

		protected virtual void OnHintUpdated() {
			// Nothing here, to be overridden.
		} // OnHintUpdated

		#endregion method OnHintUpdated

		#region method GetError

		protected Exception GetError(string sValue, Exception oInner = null) {
			return new InvalidCastException(
				"Could not extract " + this + " from " +
				(sValue == null ? "a null string" : " '" + sValue + "'") +
				".",
				oInner
			);
		} // GetError

		#endregion method GetError

		#endregion protected
	} // class AType
} // namespace
