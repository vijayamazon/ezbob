﻿namespace Ezbob.Backend.Models
{
	using System;
	using System.Runtime.Serialization;

	[Serializable]
	[DataContract]
	public class TemplateModel
	{
		[DataMember]
		public string Template { get; set; }

		public override string ToString()
		{
			return "TemplateModel";
		}
	}
}