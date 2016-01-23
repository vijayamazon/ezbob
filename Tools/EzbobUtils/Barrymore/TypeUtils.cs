namespace Ezbob.Utils {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using Ezbob.Logger;

	public static class TypeUtils {
		static TypeUtils() {
			ms_oSimpleTypes = new SortedSet<string> {
				typeof (bool).ToString(),
				typeof (char).ToString(),
				typeof (sbyte).ToString(),
				typeof (byte).ToString(),
				typeof (short).ToString(),
				typeof (ushort).ToString(),
				typeof (int).ToString(),
				typeof (uint).ToString(),
				typeof (long).ToString(),
				typeof (ulong).ToString(),
				typeof (float).ToString(),
				typeof (double).ToString(),
				typeof (decimal).ToString(),
				typeof (DateTime).ToString(),
				typeof (string).ToString(),
				typeof (Guid).ToString(),

				typeof (bool?).ToString(),
				typeof (char?).ToString(),
				typeof (sbyte?).ToString(),
				typeof (byte?).ToString(),
				typeof (short?).ToString(),
				typeof (ushort?).ToString(),
				typeof (int?).ToString(),
				typeof (uint?).ToString(),
				typeof (long?).ToString(),
				typeof (ulong?).ToString(),
				typeof (float?).ToString(),
				typeof (double?).ToString(),
				typeof (decimal?).ToString(),
				typeof (DateTime?).ToString(),
				typeof (Guid?).ToString(),
			};
		} // static constructor

		public static bool IsSubclassOf(Type typeToTest, Type baseTypeCandidate, ASafeLog log = null) {
			if ((typeToTest == null) || (baseTypeCandidate == null)) {
				if (log != null) {
					log.Warn(
						"IsSubclassOf('{0}', '{1}') returns false because at least one of the operands is null.",
						typeToTest,
						baseTypeCandidate
					);
				} // if

				return false;
			} // if

			bool isSubclass = (typeToTest == baseTypeCandidate) || typeToTest.IsSubclassOf(baseTypeCandidate);

			if (log != null)
			log.Warn("IsSubclassOf('{0}', '{1}') = {2}.", typeToTest, baseTypeCandidate, isSubclass);

			return isSubclass;
		} // IsSubclassOf

		public static Type FindType(string sName, ASafeLog oLog = null) {
			if (string.IsNullOrWhiteSpace(sName))
				return null;

			sName = sName.Trim();

			bool bHasDot = sName.IndexOf('.') > 0;

			foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies()) {
				Type t = PureFindType(sName, a, bHasDot, oLog);

				if (t != null)
					return t;
			} // for each Assembly

			return null;
		} // FindType

		public static Type FindType(string sName, Assembly asm, ASafeLog oLog = null) {
			if (string.IsNullOrWhiteSpace(sName))
				return null;

			sName = sName.Trim();

			bool bHasDot = sName.IndexOf('.') > 0;

			if (asm != null)
				return PureFindType(sName, asm, bHasDot, oLog);

			return
				PureFindType(sName, Assembly.GetCallingAssembly(), bHasDot, oLog) ??
				PureFindType(sName, Assembly.GetExecutingAssembly(), bHasDot, oLog);
		} // FindType

		private static Type PureFindType(string sName, Assembly asm, bool bHasDot, ASafeLog oLog) {
			Type[] aryTypes;

			oLog = oLog.Safe();

			try {
				aryTypes = asm.GetTypes();
			}
			catch (ReflectionTypeLoadException rtle) {
				oLog.Alert(rtle, "Failed to retrieve types list of assembly '{0}' while looking for '{1}'.", asm.FullName, sName);

				if ((rtle.LoaderExceptions != null) && (rtle.LoaderExceptions.Length > 0)) {
					oLog.Debug("ReflectionTypeLoadException.LoaderExceptions - begin");

					for (int i = 0; i < rtle.LoaderExceptions.Length; i++)
						oLog.Debug(rtle.LoaderExceptions[i], "Inner exception #{0}.", i);

					oLog.Debug("ReflectionTypeLoadException.LoaderExceptions - end.");
				} // if

				return null;
			}
			catch (Exception e) {
				oLog.Alert(e, "Failed to retrieve types list of assembly '{0}' while looking for '{1}'.", asm.FullName, sName);
				return null;
			} // try

			foreach (Type t in aryTypes) {
				if (bHasDot) {
					if ((t.AssemblyQualifiedName ?? string.Empty).StartsWith(sName))
						return t;
				}
				else {
					if (t.Name.EndsWith(sName))
						return t;
				} // if
			} // for each type

			return null;
		} // PureFindType

		public static bool IsEnumerable(Type oType) {
			return IsGenericInterfaceImplemented(oType, typeof (IEnumerable<>));
		} // IsEnumerable

		public static bool CanBeNull(Type oType) {
			if (oType == null)
				return false;

			if (oType.IsClass)
				return true;

			return IsNullable(oType);
		} // CanBeNull

		public static bool IsNullable(Type oType) {
			return IsGenericInterfaceImplemented(oType, typeof (Nullable<>));
		} // IsNullable

		public static bool IsGenericInterfaceImplemented(Type oType, Type oInterface) {
			return
				(oType != null) && (
					IsThisTypeGenericInterfaceImplemented(oType, oInterface) ||
					oType.GetInterfaces().Any(t => IsThisTypeGenericInterfaceImplemented(t, oInterface))
				);
		} // IsGenericInterfaceImplemented

		public static bool IsThisTypeGenericInterfaceImplemented(Type oType, Type oInterfaceType) {
			return
				(oType != null) &&
				(oType.IsGenericType && (oType.GetGenericTypeDefinition() == oInterfaceType));
		} // IsThisTypeGenericInterfaceImplemented

		public static bool IsParametrisable(Type oType) {
			return IsInterfaceImplemented(oType, typeof(IParametrisable));
		} // IsParametrisable

		public static bool HasInterface(this Type oType, Type oInterfaceType) {
			return IsInterfaceImplemented(oType, oInterfaceType);
		} // HasInterface

		public static bool IsInterfaceImplemented(Type oType, Type oInterfaceType) {
			return
				(oType != null) &&
				((oType == oInterfaceType) || oType.GetInterfaces().Contains(oInterfaceType));
		} // IsInterfaceImplemented

		public static bool IsSimpleType(Type oType) {
			return (oType != null) && ms_oSimpleTypes.Contains(oType.ToString());
		} // IsSimpleType

		public static bool IsPlainType(Type oType) {
			if (oType == null)
				return false;

			if (oType.IsEnum)
				return true;

			if (IsNullable(oType))
				if (Nullable.GetUnderlyingType(oType).IsEnum)
					return true;

			return IsSimpleType(oType);
		} // IsPlainType

		private static readonly SortedSet<string> ms_oSimpleTypes;

		// ReSharper disable PossibleNullReferenceException

		public static Func<object, object[]> GetConvertorToObjectArray(Type oType) {
			Func<object, object[]> func;

			if (TypeUtils.IsSimpleType(oType))
				func = o => new object[] { o };
			else if (TypeUtils.IsParametrisable(oType))
				func = o => (o as IParametrisable).ToParameter();
			else
				func = o => o.ToObjectArray();

			return func;
		} // GetConvertorToObjectArray

		// ReSharper restore PossibleNullReferenceException

	} // class TypeUtils
} // namespace
