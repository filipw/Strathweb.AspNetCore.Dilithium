using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;

namespace Strathweb.Dilithium.IdentityModel;

public class DilithiumSignatureProvider : SignatureProvider
{
    private readonly DilithiumSecurityKey _key;
    private readonly bool _canSign;
    private readonly MLDsaParameters _publicOrPrivateKey;

    public DilithiumSignatureProvider(DilithiumSecurityKey key, string algorithm, bool canSign)
        : base(key, algorithm)
    {
        
        if (key == null) throw new ArgumentNullException(nameof(key));
        if (algorithm == null) throw new ArgumentNullException(nameof(algorithm));
        _key = key;
        _canSign = canSign;
        
        var publicOrPrivateKey = _canSign ? key.PrivateKey?.Parameters : key.PublicKey.Parameters;
        
        if (publicOrPrivateKey == null)
        {
            throw new NotSupportedException("Security key cannot be used as the necessary cipher parameters are missing for the required operation");
        }

        _publicOrPrivateKey = publicOrPrivateKey;
    }

    private MLDsaSigner CreateSigner()
    {
        var signer = new MLDsaSigner(_publicOrPrivateKey, deterministic: true);
        if (_canSign)
        {
            signer.Init(true, _key.PrivateKey);
        }
        else
        {
            signer.Init(false, _key.PublicKey);
        }
        
        return signer;
    }

    public override byte[] Sign(byte[] input)
    {
        if (!_canSign)
        {
            throw new NotSupportedException("This instance is not configured for signing!");
        }

        var signer = CreateSigner();
        signer.BlockUpdate(input, 0, input.Length);
        return signer.GenerateSignature();
    }

    public override bool Verify(byte[] input, byte[] signature)
    {
        var signer = CreateSigner();
        signer.BlockUpdate(input, 0, input.Length);
        return signer.VerifySignature(signature);
    }

    public override bool Verify(byte[] input, int inputOffset, int inputLength, byte[] signature, int signatureOffset, int signatureLength)
    {
        var actualInput = new byte[inputLength];
        Array.Copy(input, inputOffset, actualInput, 0, inputLength);

        var actualSignature = new byte[signatureLength];
        Array.Copy(signature, signatureOffset, actualSignature, 0, signatureLength);

        var signer = CreateSigner();
        signer.BlockUpdate(actualInput, 0, actualInput.Length);
        return signer.VerifySignature(actualSignature);
    }

    protected override void Dispose(bool disposing)
    {
    }
}
//
// public class DilithiumSignatureProvider : SignatureProvider
// {
//     private readonly MLDsaSigner _signer;
//     private readonly bool _canSign;
//
//     public DilithiumSignatureProvider(DilithiumSecurityKey key, string algorithm, MLDsaSigner signer, bool canSign)
//         : base(key, algorithm)
//     {
//         if (key == null) throw new ArgumentNullException(nameof(key));
//         if (algorithm == null) throw new ArgumentNullException(nameof(algorithm));
//         _signer = signer ?? throw new ArgumentNullException(nameof(signer));
//         _canSign = canSign;
//     }
//
//     public override byte[] Sign(byte[] input)
//     {
//         if (!_canSign)
//         {
//             throw new NotSupportedException("This instance is not configured for signing!");
//         }
//         return _signer.GenerateSignature(input);
//     }
//
//     public override bool Verify(byte[] input, byte[] signature) => 
//         _signer.VerifySignature(input, signature);
//
//     // todo: it would be good to avoid copying here
//     public override bool Verify(byte[] input, int inputOffset, int inputLength, byte[] signature, int signatureOffset, int signatureLength)
//     {
//         var actualInput = new byte[inputLength];
//         Array.Copy(input, inputOffset, actualInput, 0, inputLength);
//
//         var actualSignature = new byte[signatureLength];
//         Array.Copy(signature, signatureOffset, actualSignature, 0, signatureLength);
//
//         return _signer.VerifySignature(actualInput, actualSignature);
//     }
//
//     protected override void Dispose(bool disposing)
//     {
//     }
//}