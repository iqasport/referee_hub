using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using FluentAssertions;
using ManagementHub.Storage.Database;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection.XmlEncryption;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace ManagementHub.UnitTests.Database;

public class DataProtectionKeyXmlProtectorTests
{
	[Fact]
	public void Protect_EncryptsMarkedKeyMaterialAndPreservesKeyMetadata()
	{
		var key = new DataProtectionKey
		{
			FriendlyName = "key-id",
			Xml = """
				<key id="key-id" version="1" xmlns:p="http://schemas.asp.net/2015/03/dataProtection">
				  <creationDate>2026-01-01T00:00:00Z</creationDate>
				  <descriptor><masterKey p:requiresEncryption="true"><value>secret-key-material</value></masterKey></descriptor>
				</key>
				""",
		};
		var xmlEncryptor = new Mock<IXmlEncryptor>();
		xmlEncryptor
			.Setup(encryptor => encryptor.Encrypt(It.IsAny<XElement>()))
			.Returns<XElement>(_ => new EncryptedXmlInfo(new XElement("encryptedData"), typeof(NullXmlDecryptor)));

		var updated = DataProtectionKeyXmlProtector.Protect(key, xmlEncryptor.Object);

		updated.Should().BeTrue();
		var protectedXml = XElement.Parse(key.Xml);
		protectedXml.Attribute("id")!.Value.Should().Be("key-id");
		protectedXml.Element("creationDate").Should().NotBeNull();
		protectedXml.Descendants(XName.Get("encryptedSecret", "http://schemas.asp.net/2015/03/dataProtection"))
			.Should().ContainSingle();
		key.Xml.Should().NotContain("secret-key-material");
	}

	[Fact]
	public void Protect_SkipsKeyMaterialAlreadyEncrypted()
	{
		var key = new DataProtectionKey
		{
			FriendlyName = "key-id",
			Xml = """
				<key id="key-id" xmlns:p="http://schemas.asp.net/2015/03/dataProtection">
				  <descriptor><p:encryptedSecret decryptorType="decryptor"><encryptedData /></p:encryptedSecret></descriptor>
				</key>
				""",
		};
		var xmlEncryptor = new Mock<IXmlEncryptor>(MockBehavior.Strict);

		var updated = DataProtectionKeyXmlProtector.Protect(key, xmlEncryptor.Object);

		updated.Should().BeFalse();
		xmlEncryptor.VerifyNoOtherCalls();
	}

	[Fact]
	public void DataProtectionProvider_ProtectsPersistedKeysWithTheCertificate()
	{
		using var rsa = RSA.Create(2048);
		var request = new CertificateRequest("CN=DataProtectionTest", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
		using var certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddDays(1));
		var keyDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
		Directory.CreateDirectory(keyDirectory);

		try
		{
			var services = new ServiceCollection();
			services.AddLogging();
			services.AddDataProtection()
				.PersistKeysToFileSystem(new DirectoryInfo(keyDirectory))
				.ProtectKeysWithCertificate(certificate);

			using var serviceProvider = services.BuildServiceProvider();
			var protector = serviceProvider.GetRequiredService<IDataProtectionProvider>()
				.CreateProtector("DataProtectionKeyEncryptionTest");
			var protectedValue = protector.Protect("payload");

			protector.Unprotect(protectedValue).Should().Be("payload");
			var keyXml = File.ReadAllText(Directory.GetFiles(keyDirectory, "key-*.xml").Single());
			keyXml.Should().Contain("encryptedSecret");
		}
		finally
		{
			Directory.Delete(keyDirectory, recursive: true);
		}
	}
}
