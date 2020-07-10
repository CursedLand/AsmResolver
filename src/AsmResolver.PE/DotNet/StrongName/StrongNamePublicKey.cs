using System;

namespace AsmResolver.PE.DotNet.StrongName
{
    // Reference
    // https://docs.microsoft.com/en-us/windows/win32/seccrypto/rsa-schannel-key-blobs
    // https://docs.microsoft.com/en-us/windows/win32/api/wincrypt/ns-wincrypt-rsapubkey
    
    /// <summary>
    /// Represents the public key in a RSA crypto system. 
    /// </summary>
    public class StrongNamePublicKey : StrongNameKeyStructure
    {
        /// <summary>
        /// Reads a private key from an input file.
        /// </summary>
        /// <param name="path">The path to the strong-name key file.</param>
        /// <returns>The private key.</returns>
        /// <exception cref="FormatException">Occurs when the input stream is not in the correct format.</exception>
        /// <exception cref="NotSupportedException">Occurs when an invalid or unsupported algorithm is specified.</exception>
        public static StrongNamePublicKey FromFile(string path) => 
            FromReader(new ByteArrayReader(System.IO.File.ReadAllBytes(path)));

        /// <summary>
        /// Reads a private key from an input stream.
        /// </summary>
        /// <param name="reader">The input stream.</param>
        /// <returns>The private key.</returns>
        /// <exception cref="FormatException">Occurs when the input stream is not in the correct format.</exception>
        /// <exception cref="NotSupportedException">Occurs when an invalid or unsupported algorithm is specified.</exception>
        public static StrongNamePublicKey FromReader(IBinaryStreamReader reader)
        {
            // Read BLOBHEADER
            ReadBlobHeader(reader, StrongNameKeyStructureType.PublicKeyBlob, 2, AlgorithmIdentifier.RsaSign);
            
            // Read RSAPUBKEY
            if ((RsaPublicKeyMagic) reader.ReadUInt32() != RsaPublicKeyMagic.Rsa2)
                throw new FormatException("Input stream does not contain a valid RSA private key header magic.");
            
            uint bitLength = reader.ReadUInt32();

            var result = new StrongNamePublicKey
            {
                PublicExponent = reader.ReadUInt32(),
                Modulus = new byte[bitLength / 8]
            };
            
            reader.ReadBytes(result.Modulus, 0, result.Modulus.Length);

            return result;
        }
        
        /// <inheritdoc />
        public override StrongNameKeyStructureType Type => StrongNameKeyStructureType.PublicKeyBlob;

        /// <inheritdoc />
        public override byte Version => 2;

        /// <inheritdoc />
        public override AlgorithmIdentifier Algorithm => AlgorithmIdentifier.RsaSign;

        /// <summary>
        /// Gets the magic header number defining the type of RSA public key structure.
        /// </summary>
        public virtual RsaPublicKeyMagic Magic => RsaPublicKeyMagic.Rsa1;

        /// <summary>
        /// Gets the number of bits used by the modulus parameter.
        /// </summary>
        public int BitLength => Modulus.Length * 8;
        
        /// <summary>
        /// Gets or sets the public exponent used in the RSA crypto system.
        /// </summary>
        public uint PublicExponent
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the modulus used in the RSA crypto system.
        /// </summary>
        public byte[] Modulus
        {
            get;
            set;
        }
        
        /// <inheritdoc />
        public override uint GetPhysicalSize()
        {
            return base.GetPhysicalSize() // _PUBLICKEYSTRUC (BLOBHEADER)
                   + sizeof(RsaPublicKeyMagic) // magic
                   + sizeof(uint) // bitlen
                   + sizeof(uint) // pubexp
                   + (uint) Modulus.Length / 8 // modulus
                ;
        }

        /// <inheritdoc />
        public override void Write(IBinaryStreamWriter writer)
        {
            base.Write(writer);
            writer.WriteUInt32((uint) Magic);
            writer.WriteUInt32((uint) BitLength);
            writer.WriteUInt32(PublicExponent);
            writer.WriteBytes(Modulus);
        }
    }
}