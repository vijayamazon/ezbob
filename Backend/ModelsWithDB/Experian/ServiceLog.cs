using System;

namespace Ezbob.Backend.ModelsWithDB.Experian
{
	public class ServiceLog
	{
		public long Id { get; set; }
		public string ServiceType { get; set; }
		public DateTime InsertDate { get; set; }
		public string ResponseData { get; set; }
		public int? CustomerId { get; set; }
		public int? DirectorId { get; set; }
	}
}
