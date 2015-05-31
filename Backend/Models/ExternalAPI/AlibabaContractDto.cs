namespace Ezbob.Backend.Models.ExternalAPI
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class OrderItems {
        [Required]
        public int orderProdNumber { get; set; }

        [Required]
        public string productName { get; set; }

        [Required]
        public string productSpecs { get; set; }

        [Required]
        public int productQuantity { get; set; }

        [Required]
        public int productUnit { get; set; }

        [Required]
        public int productUnitPrice { get; set; }

        [Required]
        public int productTotalAmount { get; set; }
    }

    public class AlibabaContractDto
    {
        [Required]
        public string requestId { get; set; }

        [Required]
        public string responseId { get; set; }

        [Required(AllowEmptyStrings = false)]
        [Range(1, Int32.MaxValue, ErrorMessage = "Ezbob customer ID is invalid")]
        public int aId { get; set; }

        [Required]
        [Range(1, long.MaxValue, ErrorMessage = "AliMemberId is invalid")]
        public long aliMemberId { get; set; }

        [Required]
        [Range(1, Int32.MaxValue, ErrorMessage = "LoanId is invalid")]
        public int loanId { get; set; }

        [Required]
        public string orderNumber { get; set; }

        [Required]
        public string sellerBusinessName { get; set; }

        [Required]
        public string sellerAliMemberId { get; set; }

        [Required]
        public string sellerStreet1 { get; set; }

        public string sellerStreet2 { get; set; }

        [Required]
        public string sellerCity { get; set; }

        [Required]
        public string sellerState { get; set; }

        [Required]
        public string sellerCountry { get; set; }

        [Required]
        public string sellerPostalCode { get; set; }

        [Required]
        public string sellerAuthRepFname { get; set; }

        [Required]
        public string sellerAuthRepLname { get; set; }

        [Required]
        public string sellerPhone { get; set; }

        public string sellerFax { get; set; }

        [Required]
        public string sellerEmail { get; set; }

        [Required]
        public string buyerBusinessName { get; set; }

        [Required]
        public string buyerStreet1 { get; set; }

        public string buyerStreet2 { get; set; }

        [Required]
        public string buyerCity { get; set; }

        [Required]
        public string buyerState { get; set; }

        [Required]
        public string buyerZip { get; set; }

        [Required]
        public string buyerCountry { get; set; }

        [Required]
        public string buyerAuthRepFname { get; set; }

        [Required]
        public string buyerAuthRepLname { get; set; }

        [Required]
        public string buyerPhone { get; set; }

        public string buyerFax { get; set; }

        [Required]
        public string buyerEmail { get; set; }

        [Required]
        public OrderItems[] orderItems { get; set; }

        public string shippingMark { get; set; }

        [Required]
        public int totalOrderAmount { get; set; }

        public int deviationQuantityAllowed { get; set; }

        public string orderAddtlDetails { get; set; }

        [Required]
        public string shippingTerms { get; set; }

        [Required]
        public DateTime shippingDate { get; set; }

        [Required]
        public string loadingPort { get; set; }

        [Required]
        public string destinationPort { get; set; }
       
        [Required]
        public int taCoveredAmount { get; set; }

        [Required]
        public int orderDeposit { get; set; }

        [Required]
        public int orderBalance { get; set; }

        [Required]
        public string orderCurrency { get; set; }

        [Required]
        public string beneficiaryBank { get; set; }

        [Required]
        public string bankStreetAddr1 { get; set; }

        public string bankStreetAddr2 { get; set; }

        [Required]
        public string bankCity { get; set; }

        [Required]
        public string bankState { get; set; }

        [Required]
        public string bankCountry { get; set; }

        [Required]
        public string bankPostalCode { get; set; }

        [Required]
        public string swiftcode { get; set; }

        [Required]
        public int bankAccountNumber { get; set; }

        public string otherWireInstructions { get; set; }

        public string goldSupplierFlag { get; set; }

        public string supplierTenureWithAlibaba { get; set; }

        public DateTime supplierBusinessStartDate { get; set; }

        public int supplierSize { get; set; }

        public int suspiciousReportCountCounterfeitProduct { get; set; }

        public int suspiciousReportCountRestrictedProhibitedProduct { get; set; }

        public int suspiciousReportCountSuspiciousMember { get; set; }

        public int supplierResponseRate { get; set; }

        public DateTime supplierGoldMemberStartDate { get; set; }

        public int quotationPerformance { get; set; }

        public int buyerOrderRequestCountLastYear { get; set; }
        [Required]
        public bool buyerConfirmShippingDocAndAmount { get; set; }

        public string financingType { get; set; }
        [Required]
        public bool buyerConfirmReleaseFunds { get; set; }

        public byte[] attachmentCommercialInvoice { get; set; }

        public byte[] attachmentBillOfLading { get; set; }

        public byte[] attachmentPackingList { get; set; }

        public byte[] attachmentOther { get; set; }
    }
}