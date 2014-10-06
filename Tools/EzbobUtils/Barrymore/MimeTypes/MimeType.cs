namespace Ezbob.Utils.MimeTypes {
	using System.Collections.Generic;

	public class MimeType {
		#region operator *

		public static bool operator *(MimeType a, MimeType b) {
			if ((a == null) || (b == null))
				return false;

			bool aIsCommon = a.IsCommon;
			bool bIsCommon = b.IsCommon;

			if (aIsCommon && bIsCommon)
				return false;

			if (aIsCommon || bIsCommon)
				return a.IsText == b.IsText;

			return a.m_oSecondaryMimeTypes.Overlaps(b.m_oSecondaryMimeTypes);
		} // operator *

		#endregion operator *

		#region const "Text" and "Binary"

		public static readonly MimeType Text = new MimeType {
			FileExtension = string.Empty,
			IsText = true,
			PrimaryMimeType = MimeTypeResolver.TextMimeTypeName,
			SecondaryMimeTypes = string.Empty,
		};

		public static readonly MimeType Binary = new MimeType {
			FileExtension = string.Empty,
			IsText = false,
			PrimaryMimeType = MimeTypeResolver.BinaryMimeTypeName,
			SecondaryMimeTypes = string.Empty,
		};

		#endregion const "Text" and "Binary"

		#region public

		#region constructor

		public MimeType() {
			m_oSecondaryMimeTypes = new SortedSet<string>();
		} // constructor

		#endregion constructor

		#region property FileExtension

		public string FileExtension { get; set; }

		#endregion property FileExtension

		#region property IsText

		public bool IsText { get; set; }

		#endregion property IsText

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
				IsText = this.IsText,
				SecondaryMimeTypes = this.SecondaryMimeTypes,
			};
		} // CloneSecondary

		#endregion method CloneSecondary

		#region property IsCommon

		public bool IsCommon {
			get {
				return
					(PrimaryMimeType == MimeTypeResolver.BinaryMimeTypeName) ||
					(PrimaryMimeType == MimeTypeResolver.TextMimeTypeName);
			} // get
		} // IsCommon

		#endregion property IsCommon

		#region method ToString

		public override string ToString() {
			return string.Format("{3} FileExtension {0} PrimaryMime {1} SecondoryMime {2}", FileExtension, PrimaryMimeType, SecondaryMimeTypes, IsText ? "Text" : "Binary");
		} // ToString

		#endregion method ToString

		#endregion public

		#region private

		private readonly SortedSet<string> m_oSecondaryMimeTypes;

		#endregion private
	} // class MimeType
} // namespace
