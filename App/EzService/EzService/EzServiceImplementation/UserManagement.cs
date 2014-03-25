namespace EzService.EzServiceImplementation 
{
	using System;
	using System.Data;
	using System.Text.RegularExpressions;
	using Ezbob.Database;
	using Ezbob.Utils;

	partial class EzServiceImplementation
	{
		public ActionMetaData CreateUnderwriter(string name, string password, string roleName)
		{
			if (!ValidatePassword(password))
			{
				Log.Warn("Illegal password:{0}", password);
				return null;
			}

			int roleId = GetRoleId(roleName);
			if (roleId == -1)
			{
				Log.Warn("Can't find role:{0}", roleName);
				return null;
			}

			if (!ValidateUsernameUniqueness(name))
			{
				Log.Warn("Username:{0} already exists!", name);
				return null;
			}

			DateTime creationDate = DateTime.UtcNow;
			string encodedPassword = PasswordEncryptor.EncodePassword(password, name, creationDate);

			DB.ExecuteNonQuery("CreateUnderwriterUser", CommandSpecies.StoredProcedure,
							   new QueryParameter("CreationTime", creationDate), 
							   new QueryParameter("Name", name),
							   new QueryParameter("EncryptedPassword", encodedPassword),
							   new QueryParameter("RoleId", roleId));
			return null;
		}

		private bool ValidateUsernameUniqueness(string name)
		{
			DataTable dt = DB.ExecuteReader("GetUserIdByName", CommandSpecies.StoredProcedure, new QueryParameter("Name", name)); 
			if (dt.Rows.Count != 0)
			{
				var sr = new SafeReader(dt.Rows[0]);
				Log.Warn("User with name:{0} already exists with id:{1}", name, sr["UserId"]);
				return false;
			}

			return true;
		}

		private bool ValidatePassword(string password)
		{
			const string configName = "PasswordValidity";
			DataTable dt = DB.ExecuteReader("GetSingleConfig", CommandSpecies.StoredProcedure, new QueryParameter("ConfigName", configName));
			if (dt.Rows.Count != 1)
			{
				Log.Warn("Can't get config value:{0}", configName);
				return false;
			}
			var sr = new SafeReader(dt.Rows[0]);
			string passwordValidity = sr["Value"];

			try
			{
				return Regex.IsMatch(password, passwordValidity);
			}
			catch
			{
				Log.Warn("Password:{0} doesn't match:{1}", password, passwordValidity);
				return false;
			}
		}

		private int GetRoleId(string roleName)
		{
			DataTable dt = DB.ExecuteReader("GetRoleId", CommandSpecies.StoredProcedure, new QueryParameter("Name", roleName));
			if (dt.Rows.Count != 1)
			{
				Log.Warn("Can't get role of:{0}", roleName);
				return -1;
			}
			var sr = new SafeReader(dt.Rows[0]);
			return sr["RoleId"];
		}
	} // class EzServiceImplementation
} // namespace EzService
