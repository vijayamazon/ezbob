namespace EzBob.Tests {
	using Ezbob.Utils.Security;
	using NUnit.Framework;

	[TestFixture]
	public class EncryptorFxiture {
		[Test]
		public void can_encrypt_and_decrypt() {
			const string text = "Hello World";
			var encrypted = SecurityUtils.Encrypt(text);
			var decrypted = SecurityUtils.Decrypt(encrypted);

			Assert.That(decrypted, Is.EqualTo(text));
			Assert.That(encrypted, Is.Not.EqualTo(text));
		} // can_encrypt_and_decrypt() {
	} // class EncryptorFxiture
} // namespace
