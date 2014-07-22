namespace Ezbob.Backend.ModelsWithDB.Experian {
	public class ERR1Attribute : ASrcAttribute {
		public ERR1Attribute(string sNodeName = null) : base("ERR1", sNodeName) {}

		public override bool IsDisplayed { get { return false; } }
	} // class ERR1Attribute

	public class DL12Attribute : ASrcAttribute {
		public DL12Attribute(string sNodeName = null) : base("DL12", sNodeName) {}

		public override DisplayMetaData MetaData {
			get {
				return new DisplayMetaData {
					ID = GroupName,
					UnlimitedWidth = true,
				};
			}
		} // MetaData
	} // class DL12Attribute

	public class DL13Attribute : ASrcAttribute {
		public DL13Attribute(string sNodeName = null) : base("DL13", sNodeName) {}

		public override DisplayMetaData MetaData {
			get {
				return new DisplayMetaData {
					UnlimitedWidth = true,
				};
			}
		} // MetaData
	} // class 

	public class PrevCompNamesAttribute : ASrcAttribute {
		public PrevCompNamesAttribute(string sNodeName = null) : base("DL15/PREVCOMPNAMES", sNodeName) {}

		public override DisplayMetaData MetaData {
			get {
				return new DisplayMetaData {
					UnlimitedWidth = true,
					DisplayDirection = DisplayMetaData.DisplayDirections.Horizontal,
				};
			}
		} // MetaData
	} // class PrevCompNamesAttribute

	public class DL17Attribute : ASrcAttribute {
		public DL17Attribute(string sNodeName = null) : base("DL17", sNodeName) {}

		public override DisplayMetaData MetaData {
			get {
				return new DisplayMetaData {
					UnlimitedWidth = true,
				};
			}
		} // MetaData
	} // class DL17Attribute

	public class DL23Attribute : ASrcAttribute {
		public DL23Attribute(string sNodeName = null) : base("DL23", sNodeName) {}

		public override DisplayMetaData MetaData {
			get {
				return new DisplayMetaData {
					UnlimitedWidth = true,
				};
			}
		} // MetaData
	} // class DL23Attribute

	public class SharehldsAttribute : ASrcAttribute {
		public SharehldsAttribute(string sNodeName = null) : base("DL23/SHAREHLDS", sNodeName) {}

		public override DisplayMetaData MetaData {
			get {
				return new DisplayMetaData {
					ID = "DL23SHAREHOLDING",
					DisplayDirection = DisplayMetaData.DisplayDirections.Horizontal,
					UnlimitedWidth = true,
					Sorting = "Description of Shareholder,Description of Shareholding,% of Shareholding",
				};
			}
		} // MetaData
	} // class SharehldsAttribute

	public class DL26Attribute : ASrcAttribute {
		public DL26Attribute(string sNodeName = null) : base("DL26", sNodeName) {}

		public override DisplayMetaData MetaData {
			get {
				return new DisplayMetaData {
					UnlimitedWidth = true,
				};
			}
		} // MetaData
	} // class DL26Attribute

	public class SummaryLineAttribute : ASrcAttribute {
		public SummaryLineAttribute(string sNodeName = null) : base("DL27/SUMMARYLINE", sNodeName) {}

		public override DisplayMetaData MetaData {
			get {
				return new DisplayMetaData {
					DisplayDirection = DisplayMetaData.DisplayDirections.Horizontal,
				};
			}
		} // MetaData
	} // class SummaryLineAttribute

	public class DL41Attribute : ASrcAttribute {
		public DL41Attribute(string sNodeName = null) : base("DL41", sNodeName) {}

		public override DisplayMetaData MetaData {
			get {
				return new DisplayMetaData {
					ID = GroupName,
				};
			}
		} // MetaData
	} // class DL41Attribute

	public class DL42Attribute : ASrcAttribute {
		public DL42Attribute(string sNodeName = null) : base("DL42", sNodeName) {}

		public override DisplayMetaData MetaData {
			get {
				return new DisplayMetaData {
					UnlimitedWidth = true,
				};
			}
		} // MetaData
	} // class DL42Attribute

	public class DL48Attribute : ASrcAttribute {
		public DL48Attribute(string sNodeName = null) : base("DL48", sNodeName) {}

		public override DisplayMetaData MetaData {
			get {
				return new DisplayMetaData {
					DisplayDirection = DisplayMetaData.DisplayDirections.Horizontal,
				};
			}
		} // MetaData
	} // class DL48Attribute

	public class DL52Attribute : ASrcAttribute {
		public DL52Attribute(string sNodeName = null) : base("DL52", sNodeName) {}

		public override DisplayMetaData MetaData {
			get {
				return new DisplayMetaData {
					DisplayDirection = DisplayMetaData.DisplayDirections.Horizontal,
				};
			}
		} // MetaData
	} // class DL52Attribute

	public class DL65Attribute : ASrcAttribute {
		public DL65Attribute(string sNodeName = null) : base("DL65", sNodeName) {}

		public override DisplayMetaData MetaData {
			get {
				return new DisplayMetaData {
					ID = GroupName,
					DisplayDirection = DisplayMetaData.DisplayDirections.Horizontal,
					UnlimitedWidth = true,
					Sorting = "Alterations to the order,Total Amount of Debenture Secured,*",
				};
			}
		} // MetaData
	} // class DL65Attribute

	public class DL68Attribute : ASrcAttribute {
		public DL68Attribute(string sNodeName = null) : base("DL68", sNodeName) {}

		public override DisplayMetaData MetaData {
			get {
				return new DisplayMetaData {
					DisplayDirection = DisplayMetaData.DisplayDirections.Horizontal,
				};
			}
		} // MetaData
	} // class DL68Attribute

	public class DL72Attribute : ASrcAttribute {
		public DL72Attribute(string sNodeName = null) : base("DL72", sNodeName) {}

		public override DisplayMetaData MetaData {
			get {
				return new DisplayMetaData {
					DisplayDirection = DisplayMetaData.DisplayDirections.Horizontal,
				};
			}
		} // MetaData
	} // class DL72Attribute

	public class DL76Attribute : ASrcAttribute {
		public DL76Attribute(string sNodeName = null) : base("DL76", sNodeName) {}

		public override DisplayMetaData MetaData {
			get {
				return new DisplayMetaData {
					UnlimitedWidth = true,
				};
			}
		} // MetaData
	} // class DL76Attribute

	public class DL78Attribute : ASrcAttribute {
		public DL78Attribute(string sNodeName = null) : base("DL78", sNodeName) {}

		public override DisplayMetaData MetaData {
			get {
				return new DisplayMetaData {
					UnlimitedWidth = true,
				};
			}
		} // MetaData
	} // class DL78Attribute

	public class CaisMonthlyAttribute : ASrcAttribute {
		public CaisMonthlyAttribute(string sNodeName = null) : base("DL95/MONTHLYDATA", sNodeName) {}
	} // class CaisMonthlyAttribute

	public class DL97Attribute : ASrcAttribute {
		public DL97Attribute(string sNodeName = null) : base("DL97", sNodeName) {}

		public override DisplayMetaData MetaData {
			get {
				return new DisplayMetaData {
					DisplayDirection = DisplayMetaData.DisplayDirections.Horizontal,
					ID = GroupName,
				};
			}
		} // MetaData
	} // class DL97Attribute

	public class DL99Attribute : ASrcAttribute {
		public DL99Attribute(string sNodeName = null) : base("DL99", sNodeName) {}

		public override DisplayMetaData MetaData {
			get {
				return new DisplayMetaData {
					ID = GroupName,
				};
			}
		} // MetaData
	} // class DL99Attribute

	public class DLA2Attribute : ASrcAttribute {
		public DLA2Attribute(string sNodeName = null) : base("DLA2", sNodeName) {}

		public override DisplayMetaData MetaData {
			get {
				return new DisplayMetaData {
					UnlimitedWidth = true,
					DisplayDirection = DisplayMetaData.DisplayDirections.Horizontal,
				};
			}
		} // MetaData
	} // class DLA2Attribute

	public class DLB5Attribute : ASrcAttribute {
		public DLB5Attribute(string sNodeName = null) : base("DLB5", sNodeName) {}

		public override DisplayMetaData MetaData {
			get {
				return new DisplayMetaData {
					ID = GroupName,
					UnlimitedWidth = true,
					DisplayDirection = DisplayMetaData.DisplayDirections.Horizontal,
				};
			}
		} // MetaData
	} // class DLB5Attribute

	public class LenderDetailsAttribute : ASrcAttribute {
		public LenderDetailsAttribute(string sNodeName = null) : base("LENDERDETAILS", sNodeName) {}

		public override bool IsTopLevel { get { return false; } }
	} // class LenderDetailsAttribute
} // namespace
