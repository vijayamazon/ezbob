﻿namespace EzServiceCrontab.ArgumentTypes {
	using System;
	using System.Collections.Generic;
	using Ezbob.Logger;
	using Ezbob.Utils;
	using Ezbob.Utils.Lingvo;

	internal class TypeRepository {
		#region public

		#region constructor

		public TypeRepository(ASafeLog oLog) {
			m_oLog = oLog ?? new SafeLog();

			m_oTypes = new List<IType>();

			Type oBase = typeof (AType<>);

			foreach (Type t in oBase.Assembly.GetTypes()) {
				if ((t != oBase) && t.IsClass && t.HasInterface(typeof(IType)))
					m_oTypes.Add(Activator.CreateInstance(t) as IType);
			} // for each type

			if (m_oTypes.Count < 1)
				m_oLog.Alert("No types were found for argument type repository!");
			else {
				m_oLog.Debug("Creating a type repository completed with {0} in it.", Grammar.Number(m_oTypes.Count, "type"));

				foreach (var t in m_oTypes)
					m_oLog.Debug("Type in repository: {0}", t);

				m_oLog.Debug("End of repository type list.");
			} // if
		} // constructor

		#endregion constructor

		#region method CreateValue

		public bool CreateValue(string sName, bool bIsNullable, string sValue, string sHint, out object oResult) {
			try {
				foreach (IType t in m_oTypes) {
					if ((t.CanBeNull == bIsNullable) && (t.Name == sName)) {
						if (t is Enumeration)
							t.Hint = sHint;

						oResult = t.CreateInstance(sValue);
						return true;
					} // if
				} // for each
			}
			catch (Exception e) {
				m_oLog.Alert(e, "Failed to create a value from name '{0}', is nullable {1}, hint '{2}', data '{3}'.", sName, bIsNullable, sHint, sValue);
			} // try

			oResult = null;
			return false;
		} // CreateValue

		#endregion method CreateValue

		#endregion public

		#region private

		private readonly List<IType> m_oTypes;
		private readonly ASafeLog m_oLog;

		#endregion private
	} // class TypeRepository
} // namespace
