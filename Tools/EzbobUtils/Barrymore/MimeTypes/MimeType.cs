namespace Ezbob.Utils.MimeTypes {
	using System.Collections.Generic;

	public class MimeType {

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

		public MimeType() {
			m_oSecondaryMimeTypes = new SortedSet<string>();
		} // constructor

		public string FileExtension { get; set; }

		public bool IsText { get; set; }

		public string PrimaryMimeType { get; set; }

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

		public void AddSecondary(MimeType oType) {
			if ((oType == null) || ReferenceEquals(oType, this))
				return;

			foreach (var s in oType.m_oSecondaryMimeTypes)
				m_oSecondaryMimeTypes.Add(s);
		} // AddSecondary

		public MimeType CloneSecondary() {
			return new MimeType {
				PrimaryMimeType = this.PrimaryMimeType,
				IsText = this.IsText,
				SecondaryMimeTypes = this.SecondaryMimeTypes,
			};
		} // CloneSecondary

		public bool IsCommon {
			get {
				return
					(PrimaryMimeType == MimeTypeResolver.BinaryMimeTypeName) ||
					(PrimaryMimeType == MimeTypeResolver.TextMimeTypeName);
			} // get
		} // IsCommon

		public override string ToString() {
			return string.Format("{3} FileExtension {0} PrimaryMime {1} SecondoryMime {2}", FileExtension, PrimaryMimeType, SecondaryMimeTypes, IsText ? "Text" : "Binary");
		} // ToString

		private readonly SortedSet<string> m_oSecondaryMimeTypes;

	} // class MimeType
} // namespace
