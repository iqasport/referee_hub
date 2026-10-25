using System;
using System.Linq;
using System.Xml.Linq;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection.XmlEncryption;

namespace ManagementHub.Storage.Database;

internal static class DataProtectionKeyXmlProtector
{
	private static readonly XNamespace DataProtectionNamespace = "http://schemas.asp.net/2015/03/dataProtection";
	private static readonly XName RequiresEncryptionAttribute = DataProtectionNamespace + "requiresEncryption";
	private static readonly XName EncryptedSecretElement = DataProtectionNamespace + "encryptedSecret";

	public static bool Protect(DataProtectionKey key, IXmlEncryptor xmlEncryptor)
	{
		var keyXml = XElement.Parse(key.Xml ?? throw new InvalidOperationException("Data Protection key XML is missing."));
		var elementsToEncrypt = keyXml.DescendantsAndSelf()
			.Where(element => (string?)element.Attribute(RequiresEncryptionAttribute) == "true")
			.ToArray();

		if (elementsToEncrypt.Length == 0)
		{
			if (keyXml.DescendantsAndSelf(EncryptedSecretElement).Any())
			{
				return false;
			}

			throw new InvalidOperationException("Data Protection key XML contains no encryptable key material.");
		}

		foreach (var element in elementsToEncrypt)
		{
			var encryptedXml = xmlEncryptor.Encrypt(new XElement(element));
			element.ReplaceWith(new XElement(
				EncryptedSecretElement,
				new XAttribute("decryptorType", encryptedXml.DecryptorType.AssemblyQualifiedName
					?? throw new InvalidOperationException("The XML decryptor type has no assembly-qualified name.")),
				encryptedXml.EncryptedElement));
		}

		key.Xml = keyXml.ToString(SaveOptions.DisableFormatting);
		return true;
	}
}
