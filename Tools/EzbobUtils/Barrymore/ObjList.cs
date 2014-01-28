namespace Ezbob.Utils {
	using System.Collections.Generic;
	using System.Collections.ObjectModel;

	#region class ObjList

	public class ObjList<T> {
		#region public

		#region constructor

		public ObjList(params T[] args) {
			m_sSeparator = string.Empty;
			m_oValues = new List<T>(args);
		} // constructor

		#endregion constructor

		#region method Add

		public virtual ObjList<T> Add(T obj) {
			m_oValues.Add(obj);
			return this;
		} // Add

		#endregion method Add

		#region property Count

		public virtual int Count {
			get { return m_oValues.Count; }
		} // Count

		#endregion property Count

		#region property Separator

		public virtual string Separator {
			get { return m_sSeparator; }
			set { m_sSeparator = value ?? string.Empty; }
		} // Separator

		private string m_sSeparator;

		#endregion property Separator

		#region method ToString

		public virtual string ToString(string sSeparator) {
			return string.Join(sSeparator, m_oValues);
		} // ToString

		public override string ToString() {
			return ToString(Separator);
		} // ToString

		#endregion method ToString

		#region property Values

		public virtual ReadOnlyCollection<T> Values {
			get { return m_oValues.AsReadOnly(); }
		} // Values

		#endregion property Values

		#endregion public

		#region private

		private readonly List<T> m_oValues;

		#endregion private
	} // class ObjList

	#endregion class ObjList
} // namespace Ezbob.Utils
