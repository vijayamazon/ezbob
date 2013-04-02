using System;
using System.Collections.Generic;
using EZBob.DatabaseLib.Model.Database;

namespace EzBob.Web.Areas.Underwriter.Models
{
    public class CollectionStatusItem
    {
        public int LoanId { get; set; }
        public bool IsAddCollectionFee { get; set; }
        public decimal CollectionFee { get; set; }
        public string LoanRefNumber { get; set; }
    }

    public class CollectionStatusModel
    {
        public CollectionStatusType CurrentStatus { get; set; }
        
        public string CollectionDateOfDeclaration { get; set; }
        public string CollectionDescription { get; set; }
        public List<CollectionStatusItem> Items { get; set; }
    }
}