using EzBob.CommonLib.Security;
using NUnit.Framework;
using EzBob.CommonLib.Security;
namespace EzBob.Tests
{
    [TestFixture]
    public class EncryptorFxiture
    {
        [Test]
        public void can_encrypt_and_decrypt()
        {
            var text = "Hello World";
            var encrypted = Encryptor.Encrypt(text);
            var decrypted = Encryptor.Decrypt(encrypted);

            Assert.That(decrypted, Is.EqualTo(text));
            Assert.That(encrypted, Is.Not.EqualTo(text));
        }
    }
}