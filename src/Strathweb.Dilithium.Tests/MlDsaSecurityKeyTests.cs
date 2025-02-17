using Microsoft.IdentityModel.Tokens;
using Strathweb.Dilithium.IdentityModel;

namespace Strathweb.Dilithium.Tests;

public class MlDsaSecurityKeyTests
{
    [Theory]
    [InlineData("ML-DSA-44")]
    [InlineData("ML-DSA-65")]
    [InlineData("ML-DSA-87")]
    public void CanInit(string algorithm)
    {
        var securityKey = new MlDsaSecurityKey(algorithm);
        
        Assert.NotNull(securityKey.KeyId);
        Assert.NotNull(securityKey.PublicKey);
        Assert.NotNull(securityKey.PrivateKey);
        Assert.True(securityKey.IsSupportedAlgorithm(algorithm));
        Assert.NotNull(securityKey.CryptoProviderFactory);
        Assert.Equal(typeof(MlDsaCryptoProviderFactory), securityKey.CryptoProviderFactory.GetType());
        Assert.Equal(PrivateKeyStatus.Exists, securityKey.PrivateKeyStatus);
    }
    
    [Theory]
    [InlineData("ML-DSA-44")]
    [InlineData("ML-DSA-65")]
    [InlineData("ML-DSA-87")]
    public void CanExportToJWK(string algorithm)
    {
        var securityKey = new MlDsaSecurityKey(algorithm);
        var jwk = securityKey.ToJsonWebKey(includePrivateKey: true);
        
        Assert.Equal("AKP", jwk.Kty);
        Assert.Equal(securityKey.KeyId, jwk.KeyId);
        Assert.Equal(algorithm, jwk.Alg);
        Assert.Equal(securityKey.PublicKey.GetEncoded(), Base64UrlEncoder.DecodeBytes(jwk.X));
        Assert.Equal(securityKey.PrivateKey.GetEncoded(), Base64UrlEncoder.DecodeBytes(jwk.D));
        Assert.True(securityKey.HasPrivateKey);
        Assert.Equal(PrivateKeyStatus.Exists, securityKey.PrivateKeyStatus);
    }
    
    [Fact]
    public void CanExportToJWK_WithoutPrivateKey()
    {
        var securityKey = new MlDsaSecurityKey("ML-DSA-44");
        var jwk = securityKey.ToJsonWebKey(includePrivateKey: false);
        
        Assert.Equal("AKP", jwk.Kty);
        Assert.Equal(securityKey.KeyId, jwk.KeyId);
        Assert.Equal("ML-DSA-44", jwk.Alg);
        Assert.Equal(securityKey.PublicKey.GetEncoded(), Base64UrlEncoder.DecodeBytes(jwk.X));
        Assert.Null(jwk.D);
    }
    
    [Theory]
    [InlineData("ML-DSA-44")]
    [InlineData("ML-DSA-65")]
    [InlineData("ML-DSA-87")]
    public void CanImportFromJWK(string algorithm)
    {
        var securityKey = new MlDsaSecurityKey(algorithm);
        var jwk = securityKey.ToJsonWebKey(includePrivateKey: true);

        var importedKey = new MlDsaSecurityKey(jwk);
        
        Assert.Equal(securityKey.KeyId, importedKey.KeyId);
        Assert.Equal(securityKey.PublicKey.GetEncoded(), importedKey.PublicKey.GetEncoded());
        Assert.NotNull(importedKey.PrivateKey);
        Assert.Equal(securityKey.PrivateKey.GetEncoded(), importedKey.PrivateKey.GetEncoded());
        Assert.Equal(PrivateKeyStatus.Exists, importedKey.PrivateKeyStatus);
        Assert.True(importedKey.IsSupportedAlgorithm(algorithm));
        Assert.NotNull(importedKey.CryptoProviderFactory);
        Assert.Equal(typeof(MlDsaCryptoProviderFactory), importedKey.CryptoProviderFactory.GetType());
    }
    
    [Fact]
    public void CanImportFromJWK_WithoutPrivateKey()
    {
        var securityKey = new MlDsaSecurityKey("ML-DSA-44");
        var jwk = securityKey.ToJsonWebKey(includePrivateKey: false);

        var importedKey = new MlDsaSecurityKey(jwk);
        
        Assert.Equal(securityKey.KeyId, importedKey.KeyId);
        Assert.Equal(securityKey.PublicKey.GetEncoded(), importedKey.PublicKey.GetEncoded());
        Assert.True(importedKey.IsSupportedAlgorithm("ML-DSA-44"));
        Assert.NotNull(importedKey.CryptoProviderFactory);
        Assert.Equal(typeof(MlDsaCryptoProviderFactory), importedKey.CryptoProviderFactory.GetType());
        Assert.Null(importedKey.PrivateKey);
        Assert.False(importedKey.HasPrivateKey);
        Assert.Equal(PrivateKeyStatus.DoesNotExist, importedKey.PrivateKeyStatus);
    }
    
    [Theory]
    [InlineData("ML-DSA-44")]
    [InlineData("ML-DSA-65")]
    [InlineData("ML-DSA-87")]
    public void CanImportFromByteArrayEncodedKeys(string algorithm)
    {
        var securityKey = new MlDsaSecurityKey(algorithm);
        var importedKey = new MlDsaSecurityKey(algorithm, securityKey.KeyId, securityKey.PublicKey.GetEncoded(), securityKey.PrivateKey.GetEncoded());
        
        Assert.Equal(securityKey.KeyId, importedKey.KeyId);
        Assert.Equal(securityKey.PublicKey.GetEncoded(), importedKey.PublicKey.GetEncoded());
        Assert.NotNull(importedKey.PrivateKey);
        Assert.Equal(securityKey.PrivateKey.GetEncoded(), importedKey.PrivateKey.GetEncoded());
        Assert.Equal(PrivateKeyStatus.Exists, importedKey.PrivateKeyStatus);
        Assert.True(importedKey.IsSupportedAlgorithm(algorithm));
        Assert.NotNull(importedKey.CryptoProviderFactory);
        Assert.Equal(typeof(MlDsaCryptoProviderFactory), importedKey.CryptoProviderFactory.GetType());
    }
}
