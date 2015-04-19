namespace CallCreditLib {
	using System;
	using System.Collections.Generic;
	using Callcredit.CRBSB;
	using Ezbob.Backend.ModelsWithDB.CallCredit.CallCreditData;

	public partial class CallCreditModelBuilder {
		public List<CallCreditDataAccs> GetAccs(CT_outputapplicant applicant, int oiaid) {

			var Accounts = new List<CallCreditDataAccs>();
			TryRead(() => {
				foreach (var Ac in applicant.accs) {
					var account = new CallCreditDataAccs {
						AccNocs = new List<CallCreditDataAccsNocs>(),
						AccHistory = new List<CallCreditDataAccsHistory>(),
						OiaID = oiaid
					};

					var acc = Ac;
					//account holder details
					TryRead(() => account.AccHolderName = acc.accholderdetails.name, "Account holder’s name details");
					TryRead(() => account.Dob = acc.accholderdetails.dob, "Account holder’s date of birth");
					TryRead(() => account.StatusCode = acc.accholderdetails.statuscode, "Account holder status code");
					TryRead(() => account.StartDate = acc.accholderdetails.startdate, "account holder start day");
					TryRead(() => account.EndDate = acc.accholderdetails.enddate, "account holder end day");
					//account holder address
					TryRead(() => account.CurrentAddress = Convert.ToBoolean(acc.accholderdetails.address.current), "account holder current address check");
					TryRead(() => account.UnDeclaredAddressType = (int)acc.accholderdetails.address.undeclaredaddresstype, "type of undeclared address for account holder");
					TryRead(() => account.AddressValue = acc.accholderdetails.address.Value, "account holder address value");
					//account default related details
					TryRead(() => account.DefDate = acc.@default.defdate, "Default date");
					TryRead(() => account.OrigDefBal = acc.@default.origdefbal, "Original default balance");
					TryRead(() => account.TermBal = acc.@default.termbal, "Termination balance");
					TryRead(() => account.DefSatDate = acc.@default.defsatdate, "Default satisfaction date");
					TryRead(() => account.RepoDate = acc.@default.repodate, "Repossession date");
					//account delinquency related details
					TryRead(() => account.DelinqDate = acc.delinquent.delinqdate, "Delinquency date");
					TryRead(() => account.DelinqBal = acc.delinquent.delinqbal, "Delinquency balance");
					//general account details
					TryRead(() => account.AccNo = acc.accdetails.accno, "Account number (own data only)");
					TryRead(() => account.AccSuffix = (int)acc.accdetails.accsuffix, "Account suffix code (own data only)");
					TryRead(() => account.Joint = acc.accdetails.joint, "Joint account indicator");
					TryRead(() => account.Status = acc.accdetails.status, "Status of account");
					TryRead(() => account.DateUpdated = acc.accdetails.dateupdated, "Date that account was last updated");
					TryRead(() => account.AccTypeCode = acc.accdetails.acctypecode, "Account type code");
					TryRead(() => account.AccGroupId = (int)acc.accdetails.accgroupid, "Account Type Group Identifier");
					TryRead(() => account.CurrencyCode = acc.accdetails.currencycode, "Currency code");
					TryRead(() => account.Balance = acc.accdetails.balance, "Current balance on account");
					TryRead(() => account.CurCreditLimit = acc.accdetails.limit, "Current credit limit on account");
					TryRead(() => account.OpenBalance = acc.accdetails.openbalance, "Account opening balance");
					TryRead(() => account.ArrStartDate = acc.accdetails.arrstartdate, "Arrangement start date");
					TryRead(() => account.ArrEndDate = acc.accdetails.arrenddate, "Arrangement end date");
					TryRead(() => account.PayStartDate = acc.accdetails.paystartdate, "Payment start date");
					TryRead(() => account.accStartDate = acc.accdetails.accstartdate, "Account start date");
					TryRead(() => account.AccEndDate = acc.accdetails.accenddate, "Account end date");
					TryRead(() => account.RegPayment = acc.accdetails.regpayment, "Regular payment value");
					TryRead(() => account.ExpectedPayment = acc.accdetails.expectedpayment, "Expected payment value");
					TryRead(() => account.ActualPayment = acc.accdetails.actualpayment, "Actual payment value");
					TryRead(() => account.RepayPeriod = (int)acc.accdetails.repayperiod, "Repayment period");
					TryRead(() => account.RepayFreqCode = acc.accdetails.repayfreqcode, "Repayment frequency code");
					TryRead(() => account.LumpPayment = acc.accdetails.lumppayment, "Lump or balloon payment");
					TryRead(() => account.PenIntAmt = acc.accdetails.penintamt, "Penalty interest amount");
					TryRead(() => account.PromotionalRate = Convert.ToBoolean(acc.accdetails.promotionalrate), "Promotional rate check");
					TryRead(() => account.MinimumPayment = Convert.ToBoolean(acc.accdetails.minimumpayment), "Check for minimum payment has been made within accepted tolerances");
					TryRead(() => account.StatementBalance = acc.accdetails.statementbalance, "The balance at the statement date");
					//account supplier details
					TryRead(() => account.SupplierName = acc.supplierdetails.suppliername, "Supplier name");
					TryRead(() => account.SupplierTypeCode = acc.supplierdetails.suppliertypecode, "Supplier type code");
					TryRead(() => account.Apacs = Convert.ToBoolean(acc.supplierdetails.apacs), " Check for data supplier has supplied Behavioral Data");

					TryRead(() => {
						foreach (var AcHis in acc.acchistory) {
							var AccHist = new CallCreditDataAccsHistory();

							var acchistory = AcHis;
							TryRead(() => AccHist.M = new DateTime(Convert.ToInt32(acchistory.m.Substring(0, 4)), Convert.ToInt32(acchistory.m.Substring(5)), 1, 0, 0, 0, DateTimeKind.Utc), "Year and month that history record applies to");
							TryRead(() => AccHist.Bal = acchistory.bal, "Balance amount for account history record");
							TryRead(() => AccHist.CreditLimit = acchistory.limit, "Credit limit for account history record");
							TryRead(() => AccHist.Acc = acchistory.acc, "Account status code for account history record");
							TryRead(() => AccHist.Pay = acchistory.pay, "Payment status code for account history record");
							TryRead(() => AccHist.StmtBal = acchistory.stmtbal, "Statement balance for account history record ");
							TryRead(() => AccHist.PayAmt = acchistory.payamt, "Payment amount for account history record");
							TryRead(() => AccHist.CashAdvCount = acchistory.cashadvcount, "Number of cash advances for account history record");
							TryRead(() => AccHist.CashAdvTotal = (int)acchistory.cashadvtotal, "Total value of cash advances for account history record ");
							account.AccHistory.Add(AccHist);
						}
					},"Account history");

					TryRead(() => {
						foreach (var AcN in acc.notice) {
							var AccNoc = new CallCreditDataAccsNocs();
							var accnotice = AcN;
							TryRead(() => AccNoc.NoticeType = accnotice.type, "Notice Type (Correction or Dispute)");
							TryRead(() => AccNoc.RefNum = accnotice.refnum, "Notice Type (Notice Reference Number)");
							TryRead(() => AccNoc.DateRaised = accnotice.dateraised, "Date that the Notice was raised)");
							TryRead(() => AccNoc.Text = accnotice.text, "Text for Notice of Correction");
							TryRead(() => AccNoc.NameDetails = accnotice.name, "Name details as provided on the Notice of Correction)");
							TryRead(() => AccNoc.CurrentAddress = Convert.ToBoolean(accnotice.address.current), "current address check");
							TryRead(() => AccNoc.UnDeclaredAddressType = (int)accnotice.address.undeclaredaddresstype, "type of undeclared address");
							TryRead(() => AccNoc.AddressValue = accnotice.address.Value, "Address value related to notice against account");
							account.AccNocs.Add(AccNoc);
						}
					},"Account notices");
					Accounts.Add(account);
				}
			}, "Accs");

			return Accounts;
		}

		public List<CallCreditDataAddresses> GetSummaryAddresses(CT_outputsummaryblock summary) {
			var Addresses = new List<CallCreditDataAddresses>();

			TryRead(() => {
				foreach (var Ad in summary.address) {

					var address = new CallCreditDataAddresses();

					var add = Ad;

					TryRead(() => address.CurrentAddress = Convert.ToBoolean(add.current), "applicant’s/associate's current address check");
					TryRead(() => address.AddressId = (int)add.id, "Address Identifier");
					TryRead(() => address.Messagecode = (int)add.messagecode, "Message code indicating the level of confirmation");
					TryRead(() => address.UnDeclaredAddressType = (int)add.undeclaredaddresstype, "type of undeclared address");
					TryRead(() => address.AddressValue = add.Value, "Address value related to summary addresses item");
					Addresses.Add(address);
				}
			},"Summary Addresses");
			return Addresses;
		}

		public List<CallCreditDataAddressConfs> GetAddressConfs(CT_outputapplicant applicant) {

			var AddressConfs = new List<CallCreditDataAddressConfs>();
			TryRead(() => {
				foreach (var AdCnf in applicant.addressconfs) {
					var conf = new CallCreditDataAddressConfs {
						Residents = new List<CallCreditDataAddressConfsResidents>()
					};

					var addconfs = AdCnf;
					TryRead(() => conf.PafValid = Convert.ToBoolean(addconfs.pafvalid), "Postcode Address File validation check");
					TryRead(() => conf.OtherResidents = Convert.ToBoolean(addconfs.otherresidents), "Other residents flag (provided on LSAR only)");
					TryRead(() => conf.CurrentAddress = Convert.ToBoolean(addconfs.address.current), "applicant’s/associate's current address check");
					TryRead(() => conf.UnDeclaredAddressType = (int)addconfs.address.undeclaredaddresstype, "type of undeclared address");
					TryRead(() => conf.AddressValue = addconfs.address.Value, "Address value to be confirmed");
					TryRead(() => {
						foreach (var Rsd in addconfs.resident) {
							var resident = new CallCreditDataAddressConfsResidents {
								ErHistory = new List<CallCreditDataAddressConfsResidentsErHistory>(),
								ResidentNocs = new List<CallCreditDataAddressConfsResidentsNocs>()
							};
							var resids = Rsd;

							TryRead(() => resident.MatchType = resids.matchtype, "resident is individual match indicator");
							TryRead(() => resident.CurrentName = Convert.ToBoolean(resids.currentname), "resident's current name check");
							TryRead(() => resident.DeclaredAlias = Convert.ToBoolean(resids.declaredalias), "resident's declared alias check");
							TryRead(() => resident.NameDetails = resids.name, "Confirmation of individual's details");
							TryRead(() => resident.Duration = resids.duration, "Effective duration of residency");
							TryRead(() => resident.StartDate = resids.startdate, "Effective start date of residency");
							TryRead(() => resident.EndDate = resids.enddate, "Effective end date of residency");
							TryRead(() => resident.ErValid = (int)resids.ervalid, "Electoral Roll validation identifier");
							TryRead(() => {
								foreach (var ErHis in resids.erhistory) {
									var erhistory = new CallCreditDataAddressConfsResidentsErHistory {
										ErHistoryNocs = new List<CallCreditDataAddressConfsResidentsErHistoryNocs>()
									};

									var erhist = ErHis;
									TryRead(() => erhistory.StartDate = erhist.startdate, "");
									TryRead(() => erhistory.EndDate = erhist.enddate, "");
									TryRead(() => erhistory.Optout = Convert.ToBoolean(erhist.optout), "");
									TryRead(() => erhistory.RollingRoll = Convert.ToBoolean(erhist.rollingroll), "");
									TryRead(() => {
										foreach (var ErHisNoc in erhist.notice) {
											var erhnoc = new CallCreditDataAddressConfsResidentsErHistoryNocs();

											var erhisnotice = ErHisNoc;
											TryRead(() => erhnoc.NoticeType = erhisnotice.type, "Notice Type (Correction or Dispute)");
											TryRead(() => erhnoc.RefNum = erhisnotice.refnum, "Notice Type (Notice Reference Number)");
											TryRead(() => erhnoc.DateRaised = erhisnotice.dateraised, "Date that the Notice was raised)");
											TryRead(() => erhnoc.Text = erhisnotice.text, "Text for Notice of Correction");
											TryRead(() => erhnoc.NameDetails = erhisnotice.name, "Name details as provided on the Notice of Correction)");
											TryRead(() => erhnoc.CurrentAddress = Convert.ToBoolean(erhisnotice.address.current), "current address check");
											TryRead(() => erhnoc.UnDeclaredAddressType = (int)erhisnotice.address.undeclaredaddresstype, "type of undeclared address");
											TryRead(() => erhnoc.AddressValue = erhisnotice.address.Value, "Address value related to notice against a period of Electoral Roll history");
											erhistory.ErHistoryNocs.Add(erhnoc);
										}
									}, "Erhistory Notices");

									resident.ErHistory.Add(erhistory);
								}
							}, "Erhistory");

							TryRead(() => {
								foreach (var ResNoc in resids.notice) {
									var resnoc = new CallCreditDataAddressConfsResidentsNocs();

									var resnotice = ResNoc;
									TryRead(() => resnoc.NoticeType = resnotice.type, "Notice Type (Correction or Dispute)");
									TryRead(() => resnoc.RefNum = resnotice.refnum, "Notice Type (Notice Reference Number)");
									TryRead(() => resnoc.DateRaised = resnotice.dateraised, "Date that the Notice was raised)");
									TryRead(() => resnoc.Text = resnotice.text, "Text for Notice of Correction");
									TryRead(() => resnoc.NameDetails = resnotice.name, "Name details as provided on the Notice of Correction)");
									TryRead(() => resnoc.CurrentAddress = Convert.ToBoolean(resnotice.address.current), "current address check");
									TryRead(() => resnoc.UnDeclaredAddressType = (int)resnotice.address.undeclaredaddresstype, "type of undeclared address");
									TryRead(() => resnoc.AddressValue = resnotice.address.Value, "Address value related to notice against a redident");
									resident.ResidentNocs.Add(resnoc);
								}
							}, "Resident Notices");

							conf.Residents.Add(resident);
						}
					}, "Residents");

					AddressConfs.Add(conf);
				}
			}, "AddressConfs");

			return AddressConfs;
		}

		public List<CallCreditDataAddressLinks> GetAddressLinks(CT_outputapplicantAddresslinks addresslink) {
			var AddressLinks = new List<CallCreditDataAddressLinks>();
			TryRead(() => {
				foreach (var AdL in addresslink.links) {
					var addlink = new CallCreditDataAddressLinks {
						AddressLinkNocs = new List<CallCreditDataAddressLinksNocs>()
					};

					var addlinks = AdL;
					TryRead(() => addlink.CreationDate = addlinks.creationdate, "Address Link creation date");
					TryRead(() => addlink.LastConfDate = addlinks.lastconfdate, "Address Link most recent confirmation date");
					TryRead(() => addlink.From = (int)addlinks.from, "Address ID for the from address");
					TryRead(() => addlink.To = (int)addlinks.to, "Address ID for the to address");
					TryRead(() => addlink.SupplierName = addlinks.supplierdetails.suppliername, "Address Link Supplier name");
					TryRead(() => addlink.SupplierTypeCode = addlinks.supplierdetails.suppliertypecode, "Address Link Supplier type code");
					TryRead(() => {
						foreach (var AddLinkNoc in addlinks.notice) {
							var addlinknoc = new CallCreditDataAddressLinksNocs();

							var addlinknotice = AddLinkNoc;
							TryRead(() => addlinknoc.NoticeType = addlinknotice.type, "Notice Type (Correction or Dispute)");
							TryRead(() => addlinknoc.RefNum = addlinknotice.refnum, "Notice Type (Notice Reference Number)");
							TryRead(() => addlinknoc.DateRaised = addlinknotice.dateraised, "Date that the Notice was raised)");
							TryRead(() => addlinknoc.Text = addlinknotice.text, "Text for Notice of Correction");
							TryRead(() => addlinknoc.NameDetails = addlinknotice.name, "Name details as provided on the Notice of Correction)");
							TryRead(() => addlinknoc.CurrentAddress = Convert.ToBoolean(addlinknotice.address.current), "current address check");
							TryRead(() => addlinknoc.UnDeclaredAddressType = (int)addlinknotice.address.undeclaredaddresstype, "type of undeclared address");
							TryRead(() => addlinknoc.AddressValue = addlinknotice.address.Value, "Address value related to notice against address link");
							addlink.AddressLinkNocs.Add(addlinknoc);
						}
					}, "Address link Notices");

					AddressLinks.Add(addlink);
				}
			}, "Address links");

			return AddressLinks;
		}

		public List<CallCreditDataLinkAddresses> GetLinkAddresses(CT_outputapplicantAddresslinks addresslink) {
			var LinkAddresses = new List<CallCreditDataLinkAddresses>();
			TryRead(() => {
				foreach (var LinkAd in addresslink.addresses) {
					var linkaddresses = new CallCreditDataLinkAddresses ();

					var linkadd = LinkAd;
					TryRead(() => linkaddresses.AddressID = (int)linkadd.addressid, "Address id within an Address Link");
					TryRead(() => linkaddresses.Declared = Convert.ToBoolean(linkadd.declared), "Address declared on the application check");
					TryRead(() => linkaddresses.NavLinkID = linkadd.navlinkid, "Identifier for Address Link navigation");
					TryRead(() => linkaddresses.CurrentAddress = Convert.ToBoolean(linkadd.current), "Applicant's current address check");
					TryRead(() => linkaddresses.UnDeclaredAddressType = (int)linkadd.undeclaredaddresstype, "Type of undeclared address");
					TryRead(() => linkaddresses.AddressValue = linkadd.Value, "Address value held against an address link");
					LinkAddresses.Add(linkaddresses);
				}
			}, "Link Addresses");

			return LinkAddresses;
		}


		public List<CallCreditDataAliasLinks> GetAliasLinks(CT_outputapplicant applicant) {
			var AliasLinks = new List<CallCreditDataAliasLinks>();
			TryRead(() => {
				foreach (var AlL in applicant.aliaslinks) {
					var aliaslink = new CallCreditDataAliasLinks {
						AliasLinkNocs = new List<CallCreditDataAliasLinksNocs>()
					};

					var alslinks = AlL;
					TryRead(() => aliaslink.Declared = Convert.ToBoolean(alslinks.declared), "alias was declared on input check");
					TryRead(() => aliaslink.NameBefore = alslinks.name, "Name before Alias Link");
					TryRead(() => aliaslink.Alias = alslinks.alias, "Name after Alias Link");
					TryRead(() => aliaslink.CreationDate = alslinks.creationdate, "Date the Alias Link was created");
					TryRead(() => aliaslink.LastConfDate = alslinks.lastconfdate, "Most recent date that the Alias Link was confirmed");
					TryRead(() => aliaslink.SupplierName = alslinks.supplierdetails.suppliername, "Alias Link Supplier name");
					TryRead(() => aliaslink.SupplierTypeCode = alslinks.supplierdetails.suppliertypecode, "Alias Link Supplier type code");
					TryRead(() => {
						foreach (var AlsLinkNoc in alslinks.notice) {
							var aliaslinknoc = new CallCreditDataAliasLinksNocs();

							var alslinknotice = AlsLinkNoc;
							TryRead(() => aliaslinknoc.NoticeType = alslinknotice.type, "Notice Type (Correction or Dispute)");
							TryRead(() => aliaslinknoc.RefNum = alslinknotice.refnum, "Notice Type (Notice Reference Number)");
							TryRead(() => aliaslinknoc.DateRaised = alslinknotice.dateraised, "Date that the Notice was raised)");
							TryRead(() => aliaslinknoc.Text = alslinknotice.text, "Text for Notice of Correction");
							TryRead(() => aliaslinknoc.NameDetails = alslinknotice.name, "Name details as provided on the Notice of Correction)");
							TryRead(() => aliaslinknoc.CurrentAddress = Convert.ToBoolean(alslinknotice.address.current), "Current address check");
							TryRead(() => aliaslinknoc.UnDeclaredAddressType = (int)alslinknotice.address.undeclaredaddresstype, "Type of undeclared address");
							TryRead(() => aliaslinknoc.AddressValue = alslinknotice.address.Value, "Address value related to notice against alias link");
							aliaslink.AliasLinkNocs.Add(aliaslinknoc);
						}
					}, "Alias link Notices");

					AliasLinks.Add(aliaslink);

				}
			}, "Alias links");

			return AliasLinks;
		}

		public List<CallCreditDataAssociateLinks> GetAssociateLinks(CT_outputapplicant applicant) {
			var AssociateLinks = new List<CallCreditDataAssociateLinks>();
			TryRead(() => {
				foreach (var AsL in applicant.associatelinks) {
					var associatelink = new CallCreditDataAssociateLinks {
						AssociateLinkNocs = new List<CallCreditDataAssociateLinksNocs>()
					};

					var asslinks = AsL;
					TryRead(() => associatelink.DeclaredAddress = Convert.ToBoolean(asslinks.declaredaddress), "Declared address associate check");
					TryRead(() => associatelink.OiaID = (int)asslinks.oiaid, "pointer between an applicant's Associate Link and the corresponding opt-in associate report");
					TryRead(() => associatelink.NavLinkID = asslinks.navlinkid, "Identifier for Associate Link Navigation");
					TryRead(() => associatelink.AssociateName = asslinks.name, "Name of the associate");
					TryRead(() => associatelink.CreationDate = asslinks.creationdate, "Date the Associate Link was created");
					TryRead(() => associatelink.LastConfDate = asslinks.lastconfdate, "Most recent date the Associate Link was confirmed");
					TryRead(() => associatelink.SupplierName = asslinks.supplierdetails.suppliername, "Associate Link Supplier name");
					TryRead(() => associatelink.SupplierTypeCode = asslinks.supplierdetails.suppliertypecode, "Associate Link Supplier type code");
					TryRead(() => {
						foreach (var AssLinkNoc in asslinks.notice) {
							var associatelinknoc = new CallCreditDataAssociateLinksNocs();

							var asslinknotice = AssLinkNoc;
							TryRead(() => associatelinknoc.NoticeType = asslinknotice.type, "Notice Type (Correction or Dispute)");
							TryRead(() => associatelinknoc.RefNum = asslinknotice.refnum, "Notice Type (Notice Reference Number)");
							TryRead(() => associatelinknoc.DateRaised = asslinknotice.dateraised, "Date that the Notice was raised)");
							TryRead(() => associatelinknoc.Text = asslinknotice.text, "Text for Notice of Correction");
							TryRead(() => associatelinknoc.NameDetails = asslinknotice.name, "Name details as provided on the Notice of Correction)");
							TryRead(() => associatelinknoc.CurrentAddress = Convert.ToBoolean(asslinknotice.address.current), "Current address check");
							TryRead(() => associatelinknoc.UnDeclaredAddressType = (int)asslinknotice.address.undeclaredaddresstype, "Type of undeclared address");
							TryRead(() => associatelinknoc.AddressValue = asslinknotice.address.Value, "Address value related to notice against associate link");
							associatelink.AssociateLinkNocs.Add(associatelinknoc);
						}
					}, "Associate link Notices");

					AssociateLinks.Add(associatelink);

				}
			}, "Associate links");

			return AssociateLinks;
		}

		public List<CallCreditDataCifasFiling> GetCifasFiling(CT_outputapplicantCifas applicantcifas) {
			var CifasFiling = new List<CallCreditDataCifasFiling>();
			TryRead(() => {
				foreach (var CfsFl in applicantcifas.filing) {
					var cifasfiling = new CallCreditDataCifasFiling {
						CifasFilingNocs = new List<CallCreditDataCifasFilingNocs>()
					};

					var cifasfil = CfsFl;
					TryRead(() => cifasfiling.PersonName = cifasfil.person.name, "Person`s name details");
					TryRead(() => cifasfiling.Dob = cifasfil.person.dob, "Person`s date of birth");
					TryRead(() => cifasfiling.CurrentAddressP = Convert.ToBoolean(cifasfil.person.address.current), "Current address check");
					TryRead(() => cifasfiling.UnDeclaredAddressTypeP = (int)cifasfil.person.address.undeclaredaddresstype, "Type of undeclared address");
					TryRead(() => cifasfiling.AddressValueP = cifasfil.person.address.Value, "Address value related to person against CIFAS filing");
					TryRead(() => cifasfiling.CompanyNumber = cifasfil.company.number, "Registered Company number");
					TryRead(() => cifasfiling.CompanyName = cifasfil.company.name, "Company name");
					TryRead(() => cifasfiling.CurrentAddressC = Convert.ToBoolean(cifasfil.person.address.current), "Current address check");
					TryRead(() => cifasfiling.UnDeclaredAddressTypeC = (int)cifasfil.person.address.undeclaredaddresstype, "Type of undeclared address");
					TryRead(() => cifasfiling.AddressValueC = cifasfil.person.address.Value, "Address value related to company against CIFAS filing");
					TryRead(() => cifasfiling.MemberNumber = (int)cifasfil.details.membernumber, "Owning member reference number");
					TryRead(() => cifasfiling.CaseReferenceNo = cifasfil.details.casereferenceno, "Case reference number");
					TryRead(() => cifasfiling.MemberName = cifasfil.details.name, "Member name");
					TryRead(() => cifasfiling.ProductCode = cifasfil.details.productcode, "Product code");
					TryRead(() => cifasfiling.FraudCategory = cifasfil.details.fraudcategory, "Fraud category");
					TryRead(() => cifasfiling.ProductDesc = cifasfil.details.productdesc, "Description of product code");
					TryRead(() => cifasfiling.FraudDesc = cifasfil.details.frauddesc, "Description of fraud category");
					TryRead(() => cifasfiling.InputDate = cifasfil.details.inputdate, "The exact date upon which a confirmed fraud originally became live on the CIFAS database");
					TryRead(() => cifasfiling.ExpiryDate = cifasfil.details.expirydate, "The date upon which a confirmed fraud becomes expired on the CIFAS database");
					TryRead(() => cifasfiling.TransactionType = cifasfil.details.transactiontype, "Transaction type");
					TryRead(() => {
						foreach (var CifasFilkNoc in cifasfil.notice) {
							var cifasfilingnoc = new CallCreditDataCifasFilingNocs();

							var cifasfilnotice = CifasFilkNoc;
							TryRead(() => cifasfilingnoc.NoticeType = cifasfilnotice.type, "Notice Type (Correction or Dispute)");
							TryRead(() => cifasfilingnoc.Refnum = cifasfilnotice.refnum, "Notice Type (Notice Reference Number)");
							TryRead(() => cifasfilingnoc.DateRaised = cifasfilnotice.dateraised, "Date that the Notice was raised)");
							TryRead(() => cifasfilingnoc.Text = cifasfilnotice.text, "Text for Notice of Correction");
							TryRead(() => cifasfilingnoc.NameDetails = cifasfilnotice.name, "Name details as provided on the Notice of Correction)");
							TryRead(() => cifasfilingnoc.CurrentAddress = Convert.ToBoolean(cifasfilnotice.address.current), "Current address check");
							TryRead(() => cifasfilingnoc.UnDeclaredAddressType = (int)cifasfilnotice.address.undeclaredaddresstype, "Type of undeclared address");
							TryRead(() => cifasfilingnoc.AddressValue = cifasfilnotice.address.Value, "Address value related to Cifas filing notice");
							cifasfiling.CifasFilingNocs.Add(cifasfilingnoc);
						}
					}, "Associate link Notices");

					CifasFiling.Add(cifasfiling);

				}
			}, "Associate links");

			return CifasFiling;
		}


		public List<CallCreditDataCifasPlusCases> GetCifasPlusCases(CT_outputapplicantCifas applicantcifas) {
			var CifasPlusCase = new List<CallCreditDataCifasPlusCases>();
			TryRead(() => {
				foreach (var CfsCase in applicantcifas.@case) {
					var cifascases = new CallCreditDataCifasPlusCases {
						Dmrs = new List<CallCreditDataCifasPlusCasesDmrs>(),
						FilingReasons = new List<CallCreditDataCifasPlusCasesFilingReasons>(),
						CifasPlusCaseNocs = new List<CallCreditDataCifasPlusCasesNocs>(),
						Subjects = new List<CallCreditDataCifasPlusCasesSubjects>()
					};

					var cifascase = CfsCase;
					TryRead(() => cifascases.CaseId = Convert.ToInt32(cifascase.caseid), "Case unique reference number within FIND");
					TryRead(() => cifascases.OwningMember = (int)cifascase.details.owningmember, "Owning member reference number");
					TryRead(() => cifascases.ManagingMember = (int)cifascase.details.managingmember, "Managing member reference number");
					TryRead(() => cifascases.CaseType = cifascase.details.casetype, "CIFAS Case Type");
					TryRead(() => cifascases.ProductCode = cifascase.details.productcode, "CIFAS Product code");
					TryRead(() => cifascases.Facility = cifascase.details.facility, "Financial facility was granted indicator");
					TryRead(() => cifascases.SupplyDate = cifascase.details.supplydate, "The exact date upon which a confirmed fraud originally became live on the CIFAS database");
					TryRead(() => cifascases.ExpiryDate = cifascase.details.expirydate, "The date upon which a confirmed fraud becomes expired on the CIFAS database");
					TryRead(() => cifascases.ApplicationDate = cifascase.details.applicationdate, "The date upon which an application for credit was made");

					for (int i = 0; i < cifascase.details.filingreasons.Length; ++i) {
						var cifascasefr = new CallCreditDataCifasPlusCasesFilingReasons();

						TryRead(() => cifascasefr.FilingReason = cifascase.details.filingreasons[i], "CIFAS Case Filing Reason");
						cifascases.FilingReasons.Add(cifascasefr);
					}

					for (int k = 0; k < cifascase.dmrs.Length; ++k) {
						var cifascasedmr = new CallCreditDataCifasPlusCasesDmrs();

						TryRead(() => cifascasedmr.dmr = (int)cifascase.dmrs[k], "The data matching routine used to match to the CIFAS case");
						cifascases.Dmrs.Add(cifascasedmr);
					}

					TryRead(() => {
						foreach (var CifasCaseSubj in cifascase.details.subjects) {
							var cifascasesubject = new CallCreditDataCifasPlusCasesSubjects();

							var cifascasesubj = CifasCaseSubj;
							TryRead(() => cifascasesubject.PersonName = cifascasesubj.person.name, "Notice Type (Person name for a CIFAS Case)");
							TryRead(() => cifascasesubject.PersonDob = cifascasesubj.person.dob, "Person date of birth for a CIFAS Case");
							TryRead(() => cifascasesubject.CompanyName = cifascasesubj.company.name, "Company name for a CIFAS Case");
							TryRead(() => cifascasesubject.CompanyNumber = cifascasesubj.company.number, "Company number for a CIFAS Case");
							TryRead(() => cifascasesubject.HomeTelephone = cifascasesubj.hometelephone, "Home telephone number of a Subject");
							TryRead(() => cifascasesubject.MobileTelephone = cifascasesubj.mobiletelephone, "Mobile telephone number of a Subject");
							TryRead(() => cifascasesubject.Email = cifascasesubj.email, "Email address of a Subject");
							TryRead(() => cifascasesubject.SubjectRole = cifascasesubj.subjectrole, "Subject's indication of the type of its involvement with the SIFAS Case ");
							TryRead(() => cifascasesubject.SubjectRoleQualifier = cifascasesubj.subjectrolequalifier, "Subject's role qualifier within the SIFAS Case");
							TryRead(() => cifascasesubject.AddressType = cifascasesubj.addressdata[0].type, "CIFAS Address Type");
							TryRead(() => cifascasesubject.CurrentAddress = Convert.ToBoolean(cifascasesubj.addressdata[0].address.current), "Current address check");
							TryRead(() => cifascasesubject.UndeclaredAddressType = (int)cifascasesubj.addressdata[0].address.undeclaredaddresstype, "Type of undeclared address");
							TryRead(() => cifascasesubject.AddressValue = cifascasesubj.addressdata[0].address.Value, "Address value held against CIFAS Plus Case subject");
							cifascases.Subjects.Add(cifascasesubject);
						}
					}, "CIFAS Plus Case Subjects");

					TryRead(() => {
						foreach (var CifasCaseNoc in cifascase.notice) {
							var cifascasenoc = new CallCreditDataCifasPlusCasesNocs();

							var cifascasenotice = CifasCaseNoc;
							TryRead(() => cifascasenoc.NoticeType = cifascasenotice.type, "Notice Type (Correction or Dispute)");
							TryRead(() => cifascasenoc.Refnum = cifascasenotice.refnum, "Notice Type (Notice Reference Number)");
							TryRead(() => cifascasenoc.DateRaised = cifascasenotice.dateraised, "Date that the Notice was raised)");
							TryRead(() => cifascasenoc.Text = cifascasenotice.text, "Text for Notice of Correction");
							TryRead(() => cifascasenoc.NameDetails = cifascasenotice.name, "Name details as provided on the Notice of Correction)");
							TryRead(() => cifascasenoc.CurrentAddress = Convert.ToBoolean(cifascasenotice.address.current), "Current address check");
							TryRead(() => cifascasenoc.UnDeclaredAddressType = (int)cifascasenotice.address.undeclaredaddresstype, "Type of undeclared address");
							TryRead(() => cifascasenoc.AddressValue = cifascasenotice.address.Value, "Address value related to Cifas Plus Case notice");
							cifascases.CifasPlusCaseNocs.Add(cifascasenoc);
						}
					}, "CIFAS Plus Case Notices");

					CifasPlusCase.Add(cifascases);

				}
			}, "Associate links");

			return CifasPlusCase;
		}
	}
}
