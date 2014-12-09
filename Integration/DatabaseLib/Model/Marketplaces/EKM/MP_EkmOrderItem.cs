using System;
using EZBob.DatabaseLib.Common;
using Iesi.Collections.Generic;

namespace EZBob.DatabaseLib.Model.Database
{
	public class MP_EkmOrderItem
	{
		public virtual int Id { get; set; }

		public virtual MP_EkmOrder Order { get; set; }

        public virtual string OrderNumber { get; set; }
        public virtual int? CustomerId { get; set; }
        public virtual string CompanyName { get; set; }
        public virtual string FirstName { get; set; }
        public virtual string LastName { get; set; }
        public virtual string EmailAddress { get; set; }
        public virtual double? TotalCost { get; set; }
        public virtual DateTime OrderDate { get; set; }
        public virtual DateTime OrderDateIso { get; set; }
        public virtual string OrderStatus { get; set; }
        public virtual string OrderStatusColour { get; set; }

	}
}
