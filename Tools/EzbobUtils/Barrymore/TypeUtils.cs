﻿namespace Ezbob.Utils {
	using System;
	using System.Collections.Generic;
	using System.Linq;

	public static class TypeUtils {
		#region static constructor

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

		#endregion static constructor

		#region method IsEnumerable

		public static bool IsEnumerable(Type oType) {
			return IsGenericInterfaceImplemented(oType, typeof (IEnumerable<>));
		} // IsEnumerable

		#endregion method IsEnumerable

		#region method CanBeNull

		public static bool CanBeNull(Type oType) {
			if (oType == null)
				return false;

			if (oType.IsClass)
				return true;

			return IsNullable(oType);
		} // CanBeNull

		#endregion method CanBeNull

		#region method IsNullable

		public static bool IsNullable(Type oType) {
			return IsGenericInterfaceImplemented(oType, typeof (Nullable<>));
		} // IsNullable

		#endregion method IsNullable

		#region method IsGenericInterfaceImplemented

		public static bool IsGenericInterfaceImplemented(Type oType, Type oInterface) {
			return
				(oType != null) && (
					IsThisTypeGenericInterfaceImplemented(oType, oInterface) ||
					oType.GetInterfaces().Any(t => IsThisTypeGenericInterfaceImplemented(t, oInterface))
				);
		} // IsGenericInterfaceImplemented

		#endregion method IsGenericInterfaceImplemented

		#region method IsThisTypeGenericInterfaceImplemented

		public static bool IsThisTypeGenericInterfaceImplemented(Type oType, Type oInterfaceType) {
			return
				(oType != null) &&
				(oType.IsGenericType && (oType.GetGenericTypeDefinition() == oInterfaceType));
		} // IsThisTypeGenericInterfaceImplemented

		#endregion method IsThisTypeGenericInterfaceImplemented

		#region method IsParametrisable

		public static bool IsParametrisable(Type oType) {
			return IsInterfaceImplemented(oType, typeof(IParametrisable));
		} // IsParametrisable

		#endregion method IsParametrisable

		#region method IsInterfaceImplemented

		public static bool HasInterface(this Type oType, Type oInterfaceType) {
			return IsInterfaceImplemented(oType, oInterfaceType);
		} // HasInterface

		public static bool IsInterfaceImplemented(Type oType, Type oInterfaceType) {
			return
				(oType != null) &&
				((oType == oInterfaceType) || oType.GetInterfaces().Contains(oInterfaceType));
		} // IsInterfaceImplemented

		#endregion method IsInterfaceImplemented

		#region method IsSimpleType

		public static bool IsSimpleType(Type oType) {
			return (oType != null) && ms_oSimpleTypes.Contains(oType.ToString());
		} // IsSimpleType

		private static readonly SortedSet<string> ms_oSimpleTypes;

		#endregion method IsSimpleType

		#region method GetConvertorToObjectArray
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
		#endregion method GetConvertorToObjectArray
	} // class TypeUtils
} // namespace
