using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

public static class GenerateCertificates
{
  public static void SigningCertificate()
  {
    // Signing
    using var algorithm = RSA.Create(keySizeInBits: 2048);

    var subject = new X500DistinguishedName("CN=WhatDidIDo Signing Certificate");
    var request = new CertificateRequest(subject, algorithm, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
    request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature, critical: true));

    var certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddYears(2));

    var encryption = Convert.ToBase64String(certificate.Export(X509ContentType.Pfx, string.Empty));
    File.WriteAllText("signing-certificate.txt", encryption);
  }

  public static void EncryptionCertificate()
  {
    // Encryption
    using var algorithm = RSA.Create(keySizeInBits: 2048);

    var subject = new X500DistinguishedName("CN=WhatDidIDo Encryption Certificate");
    var request = new CertificateRequest(subject, algorithm, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
    request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.KeyEncipherment, critical: true));

    var certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddYears(2));

    var encryption = Convert.ToBase64String(certificate.Export(X509ContentType.Pfx, string.Empty));
    File.WriteAllText("encryption-certificate.txt", encryption);
  }
}
