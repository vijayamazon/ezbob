namespace Ezbob.Utils.MimeTypes {
	using System.Collections.Generic;

	public class MimeType {
		#region operator *

		public static bool operator *(MimeType a, MimeType b) {
			if ((a == null) || (b == null))
				return false;

			SortedSet<string> oLookIn;
			SortedSet<string> oTraverse;

			if (a.m_oSecondaryMimeTypes.Count < b.m_oSecondaryMimeTypes.Count) {
				oLookIn = a.m_oSecondaryMimeTypes;
				oTraverse = b.m_oSecondaryMimeTypes;
			}
			else {
				oLookIn = b.m_oSecondaryMimeTypes;
				oTraverse = a.m_oSecondaryMimeTypes;
			} // if

			foreach (var s in oTraverse)
				if (oLookIn.Contains(s))
					return true;

			return false;
		} // operator *

		#endregion operator *

		#region public

		#region constructor

		public MimeType() {
			m_oSecondaryMimeTypes = new SortedSet<string>();
		} // constructor

		#endregion constructor

		#region property FileExtension

		public string FileExtension { get; set; }

		#endregion property FileExtension

		#region property PrimaryMimeType

		public string PrimaryMimeType { get; set; }

		#endregion property PrimaryMimeType

		#region property SecondaryMimeTypes

		public string SecondaryMimeTypes {
			get { return string.Join(",", m_oSecondaryMimeTypes); }
			set {
				string sSecondaryMimeTypes = (value ?? string.Empty).Trim();

				m_oSecondaryMimeTypes.Clear();

				foreach (var s in sSecondaryMimeTypes.Split(','))
					if (!string.IsNullOrWhiteSpace(s))
						m_oSecondaryMimeTypes.Add(s.Trim());
			} // set
		} // SecondaryMimeTypes

		#endregion property SecondaryMimeTypes

		#region method AddSecondary

		public void AddSecondary(MimeType oType) {
			if ((oType == null) || ReferenceEquals(oType, this))
				return;

			foreach (var s in oType.m_oSecondaryMimeTypes)
				m_oSecondaryMimeTypes.Add(s);
		} // AddSecondary

		#endregion method AddSecondary

		#region method CloneSecondary

		public MimeType CloneSecondary() {
			return new MimeType {
				PrimaryMimeType = this.PrimaryMimeType,
				SecondaryMimeTypes = this.SecondaryMimeTypes,
			};
		} // CloneSecondary

		#endregion method CloneSecondary

		#endregion public

		#region private

		private readonly SortedSet<string> m_oSecondaryMimeTypes;

		#endregion private

		public override string ToString() {
			return string.Format("FileExtrension {0} PrimaryMime {1} SecondoryMime {2}", FileExtension, PrimaryMimeType, SecondaryMimeTypes);
		}
	} // class MimeType
} // namespace
