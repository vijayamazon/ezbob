namespace EzBob.Models
{
	using System;
	using EZBob.DatabaseLib.Model;

    public class PayPointCardModel
    {
        public int Id { get; set; }
        public DateTime DateAdded { get; set; }
        public string TransactionId { get; set; }
        public string CardNo { get; set; }
        public DateTime? ExpireDate { get; set; }
        public bool IsDefault { get; set; }

        public static PayPointCardModel FromCard(PayPointCard card)
        {
	        return new PayPointCardModel
		        {
			        Id = card.Id,
			        CardNo = card.CardNo,
			        DateAdded = card.DateAdded,
			        ExpireDate = card.ExpireDate,
			        TransactionId = card.TransactionId,
			        IsDefault = card.IsDefaultCard
		        };
        }
    }
}