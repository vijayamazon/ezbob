namespace Ezbob.Database.ExtractedValue {
	using System;
	using System.Reflection;

	#region class DbValue

	public class DbValue<T> : ADbValue {
		#region public

		#region constructor

		public DbValue(object oVal) : this(oVal, default (T)) {} // constructor

		public DbValue(object oVal, T oDefault) {
			HasValue = false;

			try {
				m_oValue = (T)typeof (T).GetMethod("parse", BindingFlags.Public | BindingFlags.Static).Invoke(null, new object[] { oVal });
				HasValue = true;
			}
			catch (Exception) {
				m_oValue = oDefault;
			} // try
		} // constructor

		#endregion constructor

		public static implicit operator T(DbValue<T> oValue) { return oValue.m_oValue; } // operator T

		public virtual Type ValueType {
			get { return typeof (T); }
		} // ValueType

		public override bool HasValue { get; protected set; } // HasValue

		#endregion public

		#region private

		private readonly T m_oValue;

		#endregion private
	} // class DbValue

	#endregion class DbValue
} // namespace Ezbob.Database.ExtractedValue
