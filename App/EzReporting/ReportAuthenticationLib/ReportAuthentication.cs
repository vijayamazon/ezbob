using System.Data;
using System.Linq;
using System.Security.Cryptography;
using Ezbob.Database;
using Ezbob.Logger;

namespace ReportAuthenticationLib {
	public class ReportAuthentication : SafeLog {
		#region public

		#region constructor

		public ReportAuthentication(AConnection oDB, ASafeLog log = null) : base(log) {
			m_oDB = oDB;
		} // constructor

		#endregion constructor

		#region method IsValidPassword

		public bool IsValidPassword(string userName, string inputPassword) {
			SafeReader sr = m_oDB.GetFirst(string.Format("select * from ReportUsers WHERE UserName = '{0}'", userName));

			if (sr["Id"] == string.Empty)
				return false;

			byte[] key = sr["Password"];
			byte[] salt = sr["Salt"];

			using (var deriveBytes = new Rfc2898DeriveBytes(inputPassword, salt)) {
				byte[] newKey = deriveBytes.GetBytes(20);  // derive a 20-byte key
				return newKey.SequenceEqual(key);
			} // using
		} // IsValidPassword

		#endregion method IsValidPassword

		#region method UpdatePassword

		public bool UpdatePassword(string userName, string oldPassword, string newPassword) {
			if (IsValidPassword(userName, oldPassword)) {
				using (var deriveBytes = new Rfc2898DeriveBytes(newPassword, 20)) {
					byte[] salt = deriveBytes.Salt;
					byte[] key = deriveBytes.GetBytes(20);  // derive a 20-byte key

					m_oDB.ExecuteNonQuery("RptChangePassword",
						new QueryParameter("@UserName", userName) { Type = DbType.String, Size = 50 },
						new QueryParameter("@Password", key) { Type = DbType.Binary, Size = 20 * sizeof(byte) },
						new QueryParameter("@Salt", salt) { Type = DbType.Binary, Size = 20 * sizeof(byte) }
					);
				} // using

				return true;
			} // if valid password

			return false;
		} // UpdatePassword

		#endregion method UpdatePassword

		#region method ResetPassword

		public void ResetPassword(string userName, string newPassword) {
			using (var deriveBytes = new Rfc2898DeriveBytes(newPassword, 20)) {
				byte[] salt = deriveBytes.Salt;
				byte[] key = deriveBytes.GetBytes(20);  // derive a 20-byte key

				m_oDB.ExecuteNonQuery("RptChangePassword",
					new QueryParameter("@UserName", userName) { Type = DbType.String, Size = 50 },
					new QueryParameter("@Password", key) { Type = DbType.Binary, Size = 20 * sizeof (byte) },
					new QueryParameter("@Salt", salt) { Type = DbType.Binary, Size = 20 * sizeof (byte) }
				);
			} // using
		} // ResetPassword

		#endregion method ResetPassword

		#region method AddUserToDb

		public void AddUserToDb(string userName, string name) {
			using (var deriveBytes = new Rfc2898DeriveBytes(userName, 20)) {
				byte[] salt = deriveBytes.Salt;
				byte[] key = deriveBytes.GetBytes(20);  // derive a 20-byte key

				m_oDB.ExecuteNonQuery("RptAddReportUser",
					new QueryParameter("@UserName", userName) { Type = DbType.String, Size = 50 },
					new QueryParameter("@Name", name) { Type = DbType.String, Size = 50 },
					new QueryParameter("@Password", key) { Type = DbType.Binary, Size = 20 * sizeof(byte) },
					new QueryParameter("@Salt", salt) { Type = DbType.Binary, Size = 20 * sizeof(byte) }
				);
			} // using
		} // AddUserToDb

		#endregion method AddUserToDb

		#region method DropUser

		public void DropUser(int nUserID) {
			m_oDB.ExecuteNonQuery("RptDropReportUser", new QueryParameter("@UserID", nUserID));
		} // DropUser
		#endregion method DropUser

		#endregion public

		#region private

		private AConnection m_oDB;

		#endregion private
	} // class ReportAuthentication
} // namespace ReportAuthenticationLib
