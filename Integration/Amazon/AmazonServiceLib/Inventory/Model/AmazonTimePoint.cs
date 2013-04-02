using System;
using FBAInventoryServiceMWS.Service.Model;

namespace EzBob.AmazonServiceLib.Inventory.Model
{
	public class AmazonTimePoint
	{
		public static implicit operator AmazonTimePoint( Timepoint data )
		{
			var rez =  new AmazonTimePoint();
			if ( data.IsSetDateTime() )
			{
				rez.DateTime = data.DateTime;
			}

			if ( data.IsSetTimepointType() )
			{
				rez.TimepointType = data.TimepointType;
			}
			return rez;
		}

		public string TimepointType { get; set; }

		public DateTime DateTime { get; set; }
	}
}