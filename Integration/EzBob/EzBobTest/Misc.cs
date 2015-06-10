namespace EzBobTest {
    using System.Security.Cryptography;
    using System.Text;
    using NUnit.Framework;

	[TestFixture]
	class Misc {
		[Test]
		public void TestMD5() {
            var request = "/Underwriter/PaymentAccounts/PayPointCallback?customerId=24680&cardMinExpiryDate=01%2F06%2F2015&hideSteps=True&valid=true&trans_id=ac14a8f5-c9aa-4338-bbc6-769ccf7e3c08&code=A&auth_code=001350&expiry=1117&card_no=8827&customer=Mr+Eugene+O'mahoney&amount=5.00&ip=84.95.210.15&cv2avs=SECURITY+CODE+MATCH+ONLY&ezbob2015";
		    var md5 = CalculateMD5Hash(request);
            var hash = "3388437329019fe5009ebbe2c0d8ce02";
		    Assert.AreEqual(hash, md5);
		} // TestMD5

        protected string CalculateMD5Hash(string input) {
            // step 1, calculate MD5 hash from input
            MD5 md5 = MD5.Create();
            byte[] inputBytes = Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < hash.Length; i++)
                sb.Append(hash[i].ToString("X2"));

            return sb.ToString().ToLowerInvariant();
        } // CalculateMD5Hash


		

	} // class Misc
} // namespace
