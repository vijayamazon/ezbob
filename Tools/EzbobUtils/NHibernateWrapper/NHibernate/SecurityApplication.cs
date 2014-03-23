using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;
namespace NHibernateWrapper.NHibernate
{
	[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0"), System.Diagnostics.DebuggerNonUserCode, System.Runtime.CompilerServices.CompilerGenerated]
	internal class SecurityApplication
	{
		private static System.Resources.ResourceManager resourceMan;
		private static System.Globalization.CultureInfo resourceCulture;
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static System.Resources.ResourceManager ResourceManager
		{
			get
			{
				if (object.ReferenceEquals(SecurityApplication.resourceMan, null))
				{
					System.Resources.ResourceManager resourceManager = new System.Resources.ResourceManager("NHibernate.SecurityApplication", typeof(SecurityApplication).Assembly);
					SecurityApplication.resourceMan = resourceManager;
				}
				return SecurityApplication.resourceMan;
			}
		}
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static System.Globalization.CultureInfo Culture
		{
			get
			{
				return SecurityApplication.resourceCulture;
			}
			set
			{
				SecurityApplication.resourceCulture = value;
			}
		}
		internal static string CreditInspector
		{
			get
			{
				return SecurityApplication.ResourceManager.GetString("CreditInspector", SecurityApplication.resourceCulture);
			}
		}
		internal static string HeadOfCreditDepartment
		{
			get
			{
				return SecurityApplication.ResourceManager.GetString("HeadOfCreditDepartment", SecurityApplication.resourceCulture);
			}
		}
		internal static string RiskManager
		{
			get
			{
				return SecurityApplication.ResourceManager.GetString("RiskManager", SecurityApplication.resourceCulture);
			}
		}
		internal static string ScortoCoreWeb
		{
			get
			{
				return SecurityApplication.ResourceManager.GetString("ScortoCoreWeb", SecurityApplication.resourceCulture);
			}
		}
		internal static string SecurityManager
		{
			get
			{
				return SecurityApplication.ResourceManager.GetString("SecurityManager", SecurityApplication.resourceCulture);
			}
		}
		internal SecurityApplication()
		{
		}
	}
}
