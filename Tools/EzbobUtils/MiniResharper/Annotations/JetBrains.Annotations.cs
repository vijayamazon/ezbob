// This file is an extract from Resharper's JetBrains.Allocations.dll (version 7.1).
// The aim of the file: to provide StringFormatMethodAttribute which is used to
// mark some methods as having a format string argument and optional list of arguments
// to be used in that format string (just like string.Format() does).

namespace JetBrains.Annotations {
	using System;

	[Flags]
	public enum ImplicitUseKindFlags {
		Default = 7,
		/// <summary>
		/// Only entity marked with attribute considered used
		/// </summary>
		Access = 1,
		/// <summary>
		/// Indicates implicit assignment to a member
		/// </summary>
		Assign = 2,
		/// <summary>
		/// Indicates implicit instantiation of a type with fixed constructor signature.
		/// That means any unused constructor parameters won't be reported as such.
		/// </summary>
		InstantiatedWithFixedConstructorSignature = 4,
		/// <summary>
		/// Indicates implicit instantiation of a type
		/// </summary>
		InstantiatedNoFixedConstructorSignature = 8
	} // enum ImplicitUseKindFlags

	/// <summary>
	/// Specify what is considered used implicitly when marked with <see cref="T:JetBrains.Annotations.MeansImplicitUseAttribute" /> or <see cref="T:JetBrains.Annotations.UsedImplicitlyAttribute" />
	/// </summary>
	[Flags]
	public enum ImplicitUseTargetFlags {
		Default = 1,
		Itself = 1,
		/// <summary>
		/// Members of entity marked with attribute are considered used
		/// </summary>
		Members = 2,
		/// <summary>
		/// Entity marked with attribute and all its members considered used
		/// </summary>
		WithMembers = 3
	} // enum ImplicitUseTargetFlags {

	/// <summary>
	/// Indicates that the marked symbol is used implicitly (e.g. via reflection, in external library),
	/// so this symbol will not be marked as unused (as well as by other usage inspections)
	/// </summary>
	[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
	public sealed class UsedImplicitlyAttribute : Attribute {
		[UsedImplicitly]
		public ImplicitUseKindFlags UseKindFlags { get; private set; }

		/// <summary>
		/// Gets value indicating what is meant to be used
		/// </summary>
		[UsedImplicitly]
		public ImplicitUseTargetFlags TargetFlags { get; private set; }

		[UsedImplicitly]
		public UsedImplicitlyAttribute() : this(ImplicitUseKindFlags.Default, ImplicitUseTargetFlags.Default) {
		} // constructor

		[UsedImplicitly]
		public UsedImplicitlyAttribute(ImplicitUseKindFlags useKindFlags, ImplicitUseTargetFlags targetFlags) {
			UseKindFlags = useKindFlags;
			TargetFlags = targetFlags;
		} // constructor

		[UsedImplicitly]
		public UsedImplicitlyAttribute(ImplicitUseKindFlags useKindFlags) : this(useKindFlags, ImplicitUseTargetFlags.Default) {
		} // constructor

		[UsedImplicitly]
		public UsedImplicitlyAttribute(ImplicitUseTargetFlags targetFlags) : this(ImplicitUseKindFlags.Default, targetFlags) {
		} // constructor
	} // class UsedImplicitlyAttribute

	/// <summary>
	/// Indicates that the marked method builds string by format pattern and (optional) arguments. 
	/// Parameter, which contains format string, should be given in constructor.
	/// The format string should be in <see cref="M:System.String.Format(System.IFormatProvider,System.String,System.Object[])" /> -like form
	/// </summary>
	/// <example>
	/// <code>
	/// [StringFormatMethod("message")]
	/// public void ShowError(string message, params object[] args)
	/// {
	///   //Do something
	/// }
	/// public void Foo()
	/// {
	///   ShowError("Failed: {0}"); // Warning: Non-existing argument in format string
	/// }
	/// </code>
	/// </example>
	[AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public sealed class StringFormatMethodAttribute : Attribute {
		/// <summary>
		/// Gets format parameter name
		/// </summary>
		[UsedImplicitly]
		public string FormatParameterName { get; private set; }

		/// <summary>
		/// Initializes new instance of StringFormatMethodAttribute
		/// </summary>
		/// <param name="formatParameterName">Specifies which parameter of an annotated method should be treated as format-string</param>
		public StringFormatMethodAttribute(string formatParameterName) {
			FormatParameterName = formatParameterName;
		} // constructor
	} // class StringFormatMethodAttribute

} // namespace
