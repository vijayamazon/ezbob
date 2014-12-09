namespace Ezbob.Utils {
	using System.Collections.Generic;
	using System.Collections.ObjectModel;

	public class ObjList<T> {

		public ObjList(params T[] args) {
			m_sSeparator = string.Empty;
			m_oValues = new List<T>(args);
		} // constructor

		public virtual ObjList<T> Add(T obj) {
			m_oValues.Add(obj);
			return this;
		} // Add

		public virtual int Count {
			get { return m_oValues.Count; }
		} // Count

		public virtual string Separator {
			get { return m_sSeparator; }
			set { m_sSeparator = value ?? string.Empty; }
		} // Separator

		private string m_sSeparator;

		public virtual string ToString(string sSeparator) {
			return string.Join(sSeparator, m_oValues);
		} // ToString

		public override string ToString() {
			return ToString(Separator);
		} // ToString

		public virtual ReadOnlyCollection<T> Values {
			get { return m_oValues.AsReadOnly(); }
		} // Values

		private readonly List<T> m_oValues;

	} // class ObjList
} // namespace Ezbob.Utils
