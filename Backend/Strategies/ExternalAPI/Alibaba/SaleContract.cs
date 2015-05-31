namespace Ezbob.Backend.Strategies.ExternalAPI.Alibaba
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Ezbob.Backend.ModelsWithDB.Alibaba;
    using Ezbob.Database;
    using Ezbob.Backend.Models.ExternalAPI;

    public class SaleContract:AStrategy
    {
        public SaleContract(AlibabaContractDto dto)
        {
            this.contract = dto;
            this.Result = new AlibabaSaleContractResult();
        }

        public AlibabaSaleContractResult Result;
        private AlibabaContractDto contract;

        public override string Name {
            get { return "AlibabaSaleContract"; }
        }

        public override void Execute() {
            Save(Parse(contract));
        }

		private AlibabaContract Parse(AlibabaContractDto Data) {

			var Res = new AlibabaContract();

			TryRead(() => Res.RequestId = Data.requestId);
			TryRead(() => Res.LoanId = Data.loanId);
			TryRead(() => Res.OrderNumber = Data.orderNumber);
			TryRead(() => Res.ShippingMark = Data.shippingMark);
			TryRead(() => Res.TotalOrderAmount = Data.totalOrderAmount);
			TryRead(() => Res.DeviationQuantityAllowed = Data.deviationQuantityAllowed);
			TryRead(() => Res.OrderAddtlDetails = Data.orderAddtlDetails);
			TryRead(() => Res.ShippingTerms = Data.shippingTerms);
			Res.ShippingDate = TryReadDate(() => Data.shippingDate);
			TryRead(() => Res.LoadingPort = Data.loadingPort);
			TryRead(() => Res.DestinationPort = Data.destinationPort);
			TryRead(() => Res.TACoveredAmount = Data.taCoveredAmount);
			TryRead(() => Res.OrderDeposit = Data.orderDeposit);
			TryRead(() => Res.OrderBalance = Data.orderBalance);
			TryRead(() => Res.OrderCurrency = Data.orderCurrency);
			TryRead(() => Res.CommercialInvoice=Data.attachmentCommercialInvoice);
			TryRead(() => Res.BillOfLading=Data.attachmentBillOfLading);
			TryRead(() => Res.PackingList=Data.attachmentPackingList);
			TryRead(() => Res.Other=Data.attachmentOther);

			TryRead(() => Res.Seller.BusinessName = Data.sellerBusinessName);
			TryRead(() => Res.Seller.AliMemberId = Data.sellerAliMemberId);
			TryRead(() => Res.Seller.Street1 = Data.sellerStreet1);
			TryRead(() => Res.Seller.Street2 = Data.sellerStreet2);
			TryRead(() => Res.Seller.City = Data.sellerCity);
			TryRead(() => Res.Seller.State = Data.sellerState);
			TryRead(() => Res.Seller.Country = Data.sellerCountry);
			TryRead(() => Res.Seller.PostalCode = Data.sellerPostalCode);
			TryRead(() => Res.Seller.AuthRepFname = Data.sellerAuthRepFname);
			TryRead(() => Res.Seller.AuthRepLname = Data.sellerAuthRepLname);
			TryRead(() => Res.Seller.Phone = Data.sellerPhone);
			TryRead(() => Res.Seller.Fax = Data.sellerFax);
			TryRead(() => Res.Seller.Email = Data.sellerEmail);
			TryRead(() => Res.Seller.GoldSupplierFlag = Data.goldSupplierFlag);
			TryRead(() => Res.Seller.TenureWithAlibaba = Data.supplierTenureWithAlibaba);
			Res.Seller.BusinessStartDate = TryReadDate(() => Data.supplierBusinessStartDate);
			TryRead(() => Res.Seller.Size = Data.supplierSize);
			TryRead(() => Res.Seller.suspiciousReportCountCounterfeitProduct = Data.suspiciousReportCountCounterfeitProduct);
			TryRead(() => Res.Seller.suspiciousReportCountRestrictedProhibitedProduct = Data.suspiciousReportCountRestrictedProhibitedProduct);
			TryRead(() => Res.Seller.suspiciousReportCountSuspiciousMember = Data.suspiciousReportCountSuspiciousMember);
			TryRead(() => Res.Seller.ResponseRate = Data.supplierResponseRate);
			Res.Seller.GoldMemberStartDate = TryReadDate(() => Data.supplierGoldMemberStartDate);
			TryRead(() => Res.Seller.QuotationPerformance = Data.quotationPerformance);

			TryRead(() => Res.Seller.Bank.BeneficiaryBank = Data.beneficiaryBank);
			TryRead(() => Res.Seller.Bank.StreetAddr1 = Data.bankStreetAddr1);
			TryRead(() => Res.Seller.Bank.StreetAddr2 = Data.bankStreetAddr2);
			TryRead(() => Res.Seller.Bank.City = Data.bankCity);
			TryRead(() => Res.Seller.Bank.State = Data.bankState);
			TryRead(() => Res.Seller.Bank.Country = Data.bankCountry);
			TryRead(() => Res.Seller.Bank.PostalCode = Data.bankPostalCode);
			TryRead(() => Res.Seller.Bank.SwiftCode = Data.swiftcode);
			TryRead(() => Res.Seller.Bank.AccountNumber = Data.bankAccountNumber);
			TryRead(() => Res.Seller.Bank.WireInstructions = Data.otherWireInstructions);

		    if (Data.orderItems != null) {
		        for (int i = 0; i < Data.orderItems.Length; i++) {
		            var item = new AlibabaContractItem();
		            TryRead(() => item.OrderProdNumber = Data.orderItems[i].orderProdNumber);
		            TryRead(() => item.ProductName = Data.orderItems[i].productName);
		            TryRead(() => item.ProductSpecs = Data.orderItems[i].productSpecs);
		            TryRead(() => item.ProductQuantity = Data.orderItems[i].productQuantity);
		            TryRead(() => item.ProductUnit = Data.orderItems[i].productUnit);
		            TryRead(() => item.ProductUnitPrice = Data.orderItems[i].productUnitPrice);
		            TryRead(() => item.ProductTotalAmount = Data.orderItems[i].productTotalAmount);

		            Res.Items.Add(item);
		        }
		    }

		    TryRead(() => Res.Buyer.CustomerId = Data.aId);
			TryRead(() => Res.Buyer.AliId = Data.aliMemberId);
			TryRead(() => Res.Buyer.BussinessName = Data.buyerBusinessName);
			TryRead(() => Res.Buyer.street1 = Data.buyerStreet1);
			TryRead(() => Res.Buyer.street2 = Data.buyerStreet2);
			TryRead(() => Res.Buyer.City = Data.buyerCity);
			TryRead(() => Res.Buyer.State = Data.buyerState);
			TryRead(() => Res.Buyer.Zip = Data.buyerZip);
			TryRead(() => Res.Buyer.Country = Data.buyerCountry);
			TryRead(() => Res.Buyer.AuthRepFname = Data.buyerAuthRepFname);
			TryRead(() => Res.Buyer.AuthRepLname = Data.buyerAuthRepLname);
			TryRead(() => Res.Buyer.Phone = Data.buyerPhone);
			TryRead(() => Res.Buyer.Fax = Data.buyerFax);
			TryRead(() => Res.Buyer.Email = Data.buyerEmail);
			TryRead(() => Res.Buyer.OrderRequestCountLastYear = Data.buyerOrderRequestCountLastYear);
			TryRead(() => Res.Buyer.ConfirmShippingDocAndAmount = Data.buyerConfirmShippingDocAndAmount);
			TryRead(() => Res.Buyer.FinancingType = Data.financingType);
			TryRead(() => Res.Buyer.ConfirmReleaseFunds = Data.buyerConfirmReleaseFunds);

			return Res;
		}


        public void Save(AlibabaContract data)
        {
            if (data == null)
                return;
            Log.Info("Saving Alibaba consumer data into DB...");

            var con = DB.GetPersistent();
            con.BeginTransaction();

            try{
                long ContractID = DB.ExecuteScalar<long>(con,
                    "SaveAlibabaContract",
                    CommandSpecies.StoredProcedure,
                    DB.CreateTableParameter<AlibabaContract>("Tbl", new List<AlibabaContract> { data })
                    );

                if (data.Items.Any()){

                    foreach (var item in data.Items){
                        item.ContractId = ContractID;
                    }

                    DB.ExecuteNonQuery(con,
                        "SaveAlibabaContractItem",
                        CommandSpecies.StoredProcedure,
                        DB.CreateTableParameter<AlibabaContractItem>("Tbl", data.Items));
                }

                data.Seller.ContractID = ContractID;
                long SellerID = DB.ExecuteScalar<long>(con,
                    "SaveAlibabaSeller",
                    CommandSpecies.StoredProcedure,
                    DB.CreateTableParameter<AlibabaSeller>("Tbl", new List<AlibabaSeller> { data.Seller })
                    );

                data.Seller.Bank.SellerID = SellerID;
                DB.ExecuteNonQuery(con,
                    "SaveAlibabaSellerBank",
                    CommandSpecies.StoredProcedure,
                    DB.CreateTableParameter<AlibabaSellerBank>("Tbl", data.Seller.Bank)
                    );

                data.Buyer.ContractId = ContractID;
                DB.ExecuteNonQuery(con, "UpdateAlibabaBuyer", CommandSpecies.StoredProcedure,
                            new QueryParameter("CustomerId", data.Buyer.CustomerId),
                            new QueryParameter("ContractId", data.Buyer.ContractId),
                            new QueryParameter("AuthRepFname", data.Buyer.AuthRepFname),
                            new QueryParameter("AuthRepLname", data.Buyer.AuthRepLname),
                            new QueryParameter("BussinessName", data.Buyer.BussinessName),
                            new QueryParameter("City", data.Buyer.City),
                            new QueryParameter("ConfirmReleaseFunds", data.Buyer.ConfirmReleaseFunds),
                            new QueryParameter("ConfirmShippingDocAndAmount", data.Buyer.ConfirmShippingDocAndAmount),
                            new QueryParameter("Country", data.Buyer.Country),
                            new QueryParameter("Email", data.Buyer.Email),
                            new QueryParameter("Fax", data.Buyer.Fax),
                            new QueryParameter("FinancingType", data.Buyer.FinancingType),
                            new QueryParameter("OrderRequestCountLastYear", data.Buyer.OrderRequestCountLastYear),
                            new QueryParameter("Phone", data.Buyer.Phone),
                            new QueryParameter("State", data.Buyer.State),
                            new QueryParameter("Zip", data.Buyer.Zip),
                            new QueryParameter("street1", data.Buyer.street1),
                            new QueryParameter("street2", data.Buyer.street2)
                            );
            }
            catch (Exception ex)
            {
                Log.Warn(ex, "Failed to save/update Alibaba data");
                con.Rollback();
                return;
            }

            con.Commit();
            DB.FillFirst(
                this.Result,
                "LoadSaleContractResult",
                CommandSpecies.StoredProcedure,
                new QueryParameter("CustomerId", data.Buyer.CustomerId),
                new QueryParameter("AliMemberId", data.Buyer.AliId)
                );
            Log.Info("Saving/updating Alibaba data into DB completed successfully");
        }

        private void TryRead(Action a)
        {
            try{
                a();
            }
            catch {
                return;
            }//Try
        }//TryRead

        private DateTime? TryReadDate(Func<DateTime> f)
        {
            try{
                DateTime d = f();
                return (d.Year < 1900) ? (DateTime?)null : d;
            }
            catch{
                return null;
            } // try
        } // TryReadDate

        //private byte[] TryReadFile(Func<Stream> f)
        //{
        //    try {
        //        Stream st = f();
        //        using (MemoryStream ms = new MemoryStream()) {
        //            st.CopyTo(ms);
        //            return ms.ToArray();
        //        }
        //    }
        //    catch{
        //        return null;
        //    } // try
        //} // TryReadFile
    }
}
