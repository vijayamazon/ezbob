namespace EchoSignLib {
	using System;
	using System.Globalization;
	using System.Text;
	using Ezbob.Utils;
	using Ezbob.Utils.MimeTypes;

	#region enum TemplateType

	internal enum TemplateType {
		Other = 0,
		BoardResolution = 1,
		PersonalGuarantee = 2,
		Max,
	} // enum TemplateType

	#endregion enum TemplateType

	#region class Template

	internal class Template {
		#region public

		#region constructor

		public Template() {
			m_oMime = new MimeTypeResolver();
			MimeType = m_oMime[null];
			TemplateType = TemplateType.Other;
		} // Template

		#endregion constructor

		public int ID { get; set; }

		#region property TypeID

		public int TypeID {
			get { return m_nTypeID; }

			set {
				m_nTypeID = value;

				if (((int)TemplateType.Other < m_nTypeID) && (m_nTypeID < (int)TemplateType.Max))
					TemplateType = (TemplateType)m_nTypeID;
			} // set
		} // TypeID

		private int m_nTypeID;

		#endregion property TypeID

		public TemplateType TemplateType { get; private set; } // TemplateType

		public bool IsOfKnownType { get { return (TemplateType != TemplateType.Other) && (TemplateType != TemplateType.Max); } }

		public string DocumentName { get; set; }

		#region property FileNameBase

		public string FileNameBase {
			get { return m_sFileNameBase ?? string.Empty; }

			set {
				m_sFileNameBase = (value ?? string.Empty).Trim();

				while (m_sFileNameBase.EndsWith("."))
					m_sFileNameBase = m_sFileNameBase.Substring(0, m_sFileNameBase.Length - 2);
			} // set
		} // FileNameBase

		private string m_sFileNameBase;

		#endregion property FileNameBase

		#region property FileExtension

		public string FileExtension {
			get { return m_sFileExtension ?? string.Empty; }

			set {
				m_sFileExtension = (value ?? string.Empty).Trim();

				while (m_sFileExtension.StartsWith("."))
					m_sFileExtension = m_sFileExtension.Substring(1);

				if (m_sFileExtension != string.Empty)
					m_sFileExtension = "." + m_sFileExtension;

				MimeType = m_oMime[m_sFileExtension];
			} // set
		} // FileExtension

		private string m_sFileExtension;

		#endregion property FileExtension

		public byte[] FileContent { get; set; }

		#region property FileName

		public string FileName {
			get {
				if (string.IsNullOrWhiteSpace(m_sFileName))
					m_sFileName = FileNameBase + FileExtension;

				return m_sFileName;
			} // get
		} // FileName

		private string m_sFileName;

		#endregion property FileName

		public string MimeType { get; private set; }

		#region method FillCommonDetails

		public void FillCommonDetails(Person oCustomer, Company oCompany) {
			string sTemplate = System.Text.Encoding.UTF8.GetString(FileContent);

			sTemplate = sTemplate.Replace("__COMPANY_NAME__", oCompany.Name);
			sTemplate = sTemplate.Replace("__CUSTOMER_NAME__", oCustomer.FullName);
			sTemplate = sTemplate.Replace("__CURRENT_DATE__", DateTime.Now.ToString("MMMM d yyyy", CultureInfo.InvariantCulture));

			FileContent = Encoding.UTF8.GetBytes(sTemplate);
		} // FillCommonDetails

		#endregion method FillCommonDetails

		#region method PersonalGuarantee

		public byte[] PersonalGuarantee(Person oSigner, int nApprovedSum) {
			if ((nApprovedSum <= 0) || (nApprovedSum > 50000))
				nApprovedSum = 50000;

			string sTemplate = System.Text.Encoding.UTF8.GetString(FileContent);

			sTemplate = sTemplate.Replace("__GUARANTOR_DETAILS__", oSigner.FullDetails);
			sTemplate = sTemplate.Replace("__LOAN_AMOUNT__", nApprovedSum.ToString("C2", ms_oCulture));

			return Encoding.UTF8.GetBytes(sTemplate);
		} // PersonalGuarantee

		#endregion method PersonalGuarantee

		#endregion public

		#region private

		private readonly MimeTypeResolver m_oMime;
		private static readonly CultureInfo ms_oCulture = new CultureInfo("en-GB", false);

		#endregion private
	} // class Template

	#endregion class Template
} // namespace
