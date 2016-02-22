namespace EzBob.Web.Areas.Underwriter.Controllers {
	using System;
	using System.Collections.Generic;
	using ExperianLib.Web_References.IDHubService;
	using Ezbob.Database;
	using Ezbob.Utils;
	using EzBob.Web.Areas.Underwriter.Models;

	internal abstract class AGridRow {
		public abstract string RowIDFieldName();

		public abstract void Init(long nRowID, SafeReader oRow);

		public virtual bool IsValid() {
			return true;
		} // IsValid

	} // AGridRow

	internal abstract class AGridRowBase : AGridRow {

		public override void Init(long nCustomerID, SafeReader oRow) {
			Id = nCustomerID;
			Email = oRow["Email"];
			m_sMP_List = oRow["MpTypeName"];
		} // Init

		public virtual long Id { get; private set; }
		public virtual string Email { get; private set; }
		
		public override string RowIDFieldName() {
			return "CustomerID";
		} // RowIDFieldName

		protected AGridRowBase() {
			m_sMP_List = "";
		} // constructor

		protected string m_sMP_List;
	} // class AGridBase

	internal abstract class AGridRowCommon : AGridRowBase {
		public override void Init(long nCustomerID, SafeReader oRow) {
			base.Init(nCustomerID, oRow);

			CustomerStatus = oRow["CustomerStatus"];
			Name = oRow["FullName"];
			SegmentType = oRow["SegmentType"];
			IsWasLate = oRow["IsWasLate"];
		} // Init

		public virtual string CustomerStatus { get; protected set; }
		public virtual string Name { get; private set; }
		public virtual string SegmentType { get; private set; }
		public virtual string IsWasLate { get; private set; }
	} // class AGridRowCommon

	internal abstract class AGridRowCommonCart : AGridRowCommon {
		public override void Init(long nCustomerID, SafeReader oRow) {
			base.Init(nCustomerID, oRow);

			Cart = oRow["Medal"];
			ApplyDate = oRow["ApplyDate"];
			RegDate = oRow["RegDate"];
		} // Init

		public virtual string Cart { get; private set; }
		public virtual string MP_List { get { return m_sMP_List; } }
		public virtual DateTime ApplyDate { get; private set; }
		public virtual DateTime RegDate { get; private set; }
	} // class AGridRowCommonCart

	internal abstract class AGridRowCommonFull : AGridRowCommonCart {
		public override void Init(long nCustomerID, SafeReader oRow) {
			base.Init(nCustomerID, oRow);

			CalcAmount = oRow["CalcAmount"];
		} // Init

		public virtual decimal CalcAmount { get; private set; }
	} // class AGridRowCommonFull

	internal abstract class AGridRowSalesCollection : AGridRowCommon {

		public override void Init(long nCustomerID, SafeReader oRow) {
			base.Init(nCustomerID, oRow);

			MobilePhone = oRow["MobilePhone"];
			DaytimePhone = oRow["DaytimePhone"];
			AmountTaken = oRow["AmountTaken"];
			OSBalance = oRow["OSBalance"];
			CRMstatus = oRow["CRMstatus"];
			CRMcomment = oRow["CRMcomment"];
		} // Init

		public virtual string MobilePhone { get; private set; }
		public virtual string DaytimePhone { get; private set; }
		public virtual decimal AmountTaken { get; private set; }
		public virtual decimal OSBalance { get; private set; }
		public virtual string CRMstatus { get; private set; }
		public virtual string CRMcomment { get; private set; }
	} // class AGridRowSalesCollection

	internal abstract class AGridApprovedLate : AGridRowCommonFull {

		public override void Init(long nCustomerID, SafeReader oRow) {
			base.Init(nCustomerID, oRow);

			ApproveDate = oRow["ApproveDate"];
			ApprovedSum = oRow["ApprovedSum"];
			AmountTaken = oRow["AmountTaken"];
			ApprovesNum = oRow["ApprovesNum"];
			RejectsNum = oRow["RejectsNum"];
		} // Init

		public virtual DateTime ApproveDate { get; private set; }
		public virtual decimal ApprovedSum { get; private set; }
		public virtual decimal AmountTaken { get; private set; }
		public virtual int ApprovesNum { get; private set; }
		public virtual int RejectsNum { get; private set; }
	} // class AGridApprovedLate

	internal class GridAllRow : AGridRowCommonFull {
		public override void Init(long nCustomerID, SafeReader oRow) {
			base.Init(nCustomerID, oRow);

			ApprovedSum = oRow["ApprovedSum"];
			OSBalance = oRow["OSBalance"];
		} // Init

		public virtual decimal ApprovedSum { get; private set; }
		public virtual decimal OSBalance { get; private set; }
	} // class GridAllRow

	internal class GridRegisteredRow : AGridRowBase {

		public override void Init(long nCustomerID, SafeReader oRow) {
			base.Init(nCustomerID, oRow);

			UserStatus = oRow["UserStatus"];
			RegDate = oRow["RegDate"];
			WizardStep = oRow["WizardStep"];
			SegmentType = oRow["SegmentType"];
			IsWasLate = oRow["IsWasLate"];
			MP_Statuses = oRow["MpStatus"];
		} // Init

		public virtual long UserId { get { return Id; } }

		public virtual string UserStatus { get; private set; }

		public virtual DateTime RegDate { get; private set; }

		public virtual string MP_Statuses { get; private set; } 

		public virtual string WizardStep { get; private set; }

		public virtual string SegmentType { get; private set; }
		public virtual string IsWasLate { get; private set; }

	} // class GridRegisteredRow

	internal class GridRejectedRow : AGridRowCommonCart {

		public override void Init(long nCustomerID, SafeReader oRow) {
			base.Init(nCustomerID, oRow);

			DateRejected = oRow["DateRejected"];
			Reason = oRow["Reason"];
			ApprovesNum = oRow["ApprovesNum"];
			RejectsNum = oRow["RejectsNum"];
			OSBalance = oRow["OSBalance"];
			LastStatus = oRow["LastStatus"];
			CRMcomment = oRow["CRMcomment"];
			Broker = oRow["Broker"];
			FirstSale = oRow["FirstSale"];


		} // Init

		public virtual DateTime DateRejected { get; private set; }
		public virtual string Reason { get; private set; }
		public virtual int ApprovesNum { get; private set; }
		public virtual int RejectsNum { get; private set; }
		public virtual decimal OSBalance { get; private set; }
		public virtual string LastStatus { get; private set; }
		public virtual string CRMcomment { get; private set; }
		public virtual string Broker { get; private set; }
		public virtual string FirstSale { get; private set; }
	} // class GridRejectedRow

	internal class GridCollectionRow : AGridRowSalesCollection {

		public override void Init(long nCustomerID, SafeReader oRow) {
			base.Init(nCustomerID, oRow);

			CollectionStatus = oRow["CollectionStatus"];
		} // Init

		public virtual string CollectionStatus { get; private set; }
	} // class GridCollectionRow

	internal class GridSalesRow : AGridRowSalesCollection {

		public override void Init(long nCustomerID, SafeReader oRow) {
			base.Init(nCustomerID, oRow);

			ApprovedSum = oRow["ApprovedSum"];
			OfferDate = oRow["OfferDate"];
			Interactions = oRow["Interactions"];
		} // Init

		public override bool IsValid() {
			if (string.IsNullOrWhiteSpace(CRMstatus))
				return true;

			if (CRMstatus != "NoSale")
				return true;

			return false;
		} // IsValid

		public virtual decimal ApprovedSum { get; private set; }
		public virtual DateTime OfferDate { get; private set; }
		public virtual int Interactions { get; private set; }
	} // class GridSalesRow

	internal class GridLoansRow : AGridRowCommonCart {

		public override void Init(long nCustomerID, SafeReader oRow) {
			LastLoanAmount = oRow["LastLoanAmount"];

			if (!IsValid())
				return;

			base.Init(nCustomerID, oRow);

			FirstLoanDate = oRow["FirstLoanDate"];
			LastLoanDate = oRow["LastLoanDate"];
			AmountTaken = oRow["AmountTaken"];
			TotalPrincipalRepaid = oRow["TotalPrincipalRepaid"];
			OSBalance = oRow["OSBalance"];
			NextRepaymentDate = oRow["NextRepaymentDate"];
			CustomerStatus = oRow["CustomerStatus"];
			LastStatus = oRow["LastStatus"];
			CRMcomment = oRow["CRMcomment"];
			Broker = oRow["Broker"];
			FirstSale = oRow["FirstSale"];
		} // Init

		public override bool IsValid() {
			return LastLoanAmount > 0;
		} // IsValid

		public virtual DateTime FirstLoanDate { get; private set; }
		public virtual DateTime LastLoanDate { get; private set; }
		public virtual decimal LastLoanAmount { get; private set; }
		public virtual decimal AmountTaken { get; private set; }
		public virtual decimal TotalPrincipalRepaid { get; private set; }
		public virtual decimal OSBalance { get; private set; }
		public virtual DateTime NextRepaymentDate { get; private set; }
		public virtual string LastStatus { get; private set; }
		public virtual string CRMcomment { get; private set; }
		public virtual string Broker { get; private set; }
		public virtual string FirstSale { get; private set; }
	} // class GridLoansRow

	internal class GridApprovedRow : AGridApprovedLate {

		public override void Init(long nCustomerID, SafeReader oRow) {
			base.Init(nCustomerID, oRow);

			LastStatus = oRow["LastStatus"];
			CRMcomment = oRow["CRMcomment"];
			Broker = oRow["Broker"];
			FirstSale = oRow["FirstSale"];
		} // Init

		public virtual string LastStatus { get; private set; }
		public virtual string CRMcomment { get; private set; }
		public virtual string Broker { get; private set; }
		public virtual string FirstSale { get; private set; }
	} // GridApprovedRow

	internal class GridLateRow : AGridApprovedLate {

		public override void Init(long nCustomerID, SafeReader oRow) {
			base.Init(nCustomerID, oRow);

			OSBalance = oRow["OSBalance"];
			LatePaymentDate = oRow["LatePaymentDate"];
			LatePaymentAmount = oRow["LatePaymentAmount"];
			Delinquency = oRow["Delinquency"];
			CRMstatus = oRow["CRMstatus"];
			CRMcomment = oRow["CRMcomment"];
		} // Init

		public virtual decimal OSBalance { get; private set; }
		public virtual DateTime LatePaymentDate { get; private set; }
		public virtual decimal LatePaymentAmount { get; private set; }
		public virtual int Delinquency { get; private set; }
		public virtual string CRMstatus { get; private set; }
		public virtual string CRMcomment { get; private set; }
	} // GridLateRow

	internal class GridWaitingRow : AGridRowCommonFull {

		public override void Init(long nCustomerID, SafeReader oRow) {
			base.Init(nCustomerID, oRow);

			CurrentStatus = oRow["CurrentStatus"];
			OSBalance = oRow["OSBalance"];
			LastStatus = oRow["LastStatus"];
			CRMcomment = oRow["CRMcomment"];
			Broker = oRow["Broker"];
			FirstSale = oRow["FirstSale"];
		} // Init

		public virtual string CurrentStatus { get; private set; }
		public virtual decimal OSBalance { get; private set; }
		public virtual string LastStatus { get; protected set; }
		public virtual string CRMcomment { get; protected set; }
		public virtual string Broker { get; protected set; }
		public virtual string FirstSale { get; protected set; }
	} // class GridWaitingRow

	internal class GridEscalatedRow : GridWaitingRow {

		public override void Init(long nCustomerID, SafeReader oRow) {
			base.Init(nCustomerID, oRow);

			EscalationDate = oRow["EscalationDate"];
			Underwriter = oRow["Underwriter"];
			Reason = oRow["Reason"];
		} // Init

		public virtual DateTime EscalationDate { get; private set; }
		public virtual string Underwriter { get; private set; }
		public virtual string Reason { get; private set; }
	} // class GridEscalatedRow

	internal class GridPendingRow : GridWaitingRow {

		public override void Init(long nCustomerID, SafeReader oRow) {
			base.Init(nCustomerID, oRow);

			Pending = oRow["Pending"];
			LastStatus = oRow["LastStatus"];
			CRMcomment = oRow["CRMcomment"];
			Broker = oRow["Broker"];
			FirstSale = oRow["FirstSale"];
		} // Init

		public virtual string Pending { get; private set; }
	} // class GridPendingRow

	internal class GridLogbookRow : AGridRow {

		public override string RowIDFieldName() {
			return "EntryID";
		} // RowIDFieldName

		public virtual long EntryID { get; private set; }
		public virtual string LogbookEntryTypeDescription { get; private set; }
		public virtual string FullName { get; private set; }
		public virtual DateTime EntryTime { get; private set; }
		public virtual string EntryContent { get; private set; }

		public override void Init(long nRowID, SafeReader oRow) {
			EntryID = nRowID;
			LogbookEntryTypeDescription = oRow["LogbookEntryTypeDescription"];	
			FullName = oRow["FullName"];	
			EntryTime = oRow["EntryTime"];	
			EntryContent = oRow["EntryContent"];
		} // Init
	} // GridLogbookRow

	internal class GridBroker : AGridRow {
		public override string RowIDFieldName() {
			return "BrokerID";
		} // RowIDFieldName

		public virtual long BrokerID { get; set; }
		public virtual string FirmName { get; set; }
		public virtual string ContactName { get; set; }
		public virtual string ContactEmail { get; set; }
		public virtual string ContactMobile { get; set; }
		public virtual string ContactOtherPhone { get; set; }
		public virtual string FirmWebSiteUrl { get; set; }
		public virtual string IsTest { get; set; }
		public virtual int OriginID { get; set; }
		public virtual string Origin { get; set; }

		public override void Init(long nRowID, SafeReader oRow) {
			oRow.Fill(this);
			BrokerID = nRowID;
		} // Init
	} // GridBroker

	internal class GridInvestor : AGridRow {
	
		public override string RowIDFieldName() {
			return "InvestorID";
		} // RowIDFieldName

		public virtual long InvestorID { get; set; }
		public virtual string CompanyName { get; set; }
		public virtual string InvestorType { get; set; }
		public virtual DateTime Timestamp { get; set; }

		public override void Init(long nRowID, SafeReader oRow) {
			oRow.Fill(this);
			InvestorID = nRowID;
		} // Init
	} // GridInvestor
	
	internal class GridPendingInvestorRow : AGridRow {

		public GridPendingInvestorRow(IEnumerable<PendingInvestorModel> investors = null) {
			if (investors != null) {
				allinvestors = new List<PendingInvestorModel>(investors);
			}
		}
		
		public override string RowIDFieldName() {
			return "CustomerID";
		} // RowIDFieldName

		public virtual long Id { get; set; }
		public virtual string Name { get; set; }
		public virtual string Grade { get; set; }
		public virtual string ApplicantScore { get; set; }
		public virtual decimal ApprovedAmount { get; set; }
		public virtual string Term { get; set; }
		public virtual DateTime RequestApprovedAt { get; set; }
		public virtual string TimeLimitUntilAutoreject { get; set; }
		public virtual string FindInvestor { get; set; }
		public virtual string EditOffer { get; set; }
		public virtual string SubmitChosenInvestor { get; set; }
		public virtual string ManageChosenInvestor { get; set; }
		public virtual long CashRequestID { get; set; }
		private readonly List<PendingInvestorModel> allinvestors;
		[NonTraversable]
		public virtual List<PendingInvestorModel> ChooseInvestor { get; set; }

		public override void Init(long nRowID, SafeReader oRow) {
			oRow.Fill(this);
			Id = nRowID;
			ChooseInvestor = new List<PendingInvestorModel>();

			foreach (var investor in allinvestors) {
				if (investor.InvestorFunds >= ApprovedAmount)
					ChooseInvestor.Add(investor);
			}
		} // Init
		
	} // GridPendingInvestor
} // namespace
