using System.Text;

namespace BlazorWasmPortfolioGhAction.Services.Crypto;

/// <summary>
/// Pure C# MD5 for Blazor WebAssembly where System.Security.Cryptography is unavailable.
/// </summary>
public static class Md5Helper
{
    public static string ComputeHash(string input, Encoding? encoding = null)
    {
        encoding ??= Encoding.ASCII;
        return Convert.ToHexString(ComputeHashBytes(encoding.GetBytes(input)));
    }

    public static byte[] ComputeHashBytes(byte[] input)
    {
        var padded = PadInput(input);
        var hash = new uint[4];
        hash[0] = 0x67452301;
        hash[1] = 0xEFCDAB89;
        hash[2] = 0x98BADCFE;
        hash[3] = 0x10325476;

        for (var offset = 0; offset < padded.Length; offset += 64)
        {
            ProcessBlock(padded, offset, hash);
        }

        var result = new byte[16];
        Buffer.BlockCopy(BitConverter.GetBytes(hash[0]), 0, result, 0, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(hash[1]), 0, result, 4, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(hash[2]), 0, result, 8, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(hash[3]), 0, result, 12, 4);
        return result;
    }

    private static byte[] PadInput(byte[] input)
    {
        var bitLength = (ulong)input.Length * 8;
        var paddingLength = (56 - (input.Length + 1) % 64 + 64) % 64;
        var padded = new byte[input.Length + 1 + paddingLength + 8];
        Buffer.BlockCopy(input, 0, padded, 0, input.Length);
        padded[input.Length] = 0x80;

        var lengthBytes = BitConverter.GetBytes(bitLength);
        Buffer.BlockCopy(lengthBytes, 0, padded, padded.Length - 8, 8);
        return padded;
    }

    private static void ProcessBlock(byte[] block, int offset, uint[] hash)
    {
        var x = new uint[16];
        for (var i = 0; i < 16; i++)
        {
            x[i] = BitConverter.ToUInt32(block, offset + i * 4);
        }

        var a = hash[0];
        var b = hash[1];
        var c = hash[2];
        var d = hash[3];

        // Round 1: F = (b & c) | (~b & d)
        a = b + RotateLeft(a + ((b & c) | (~b & d)) + x[0] + 0xD76AA478, 7);
        d = a + RotateLeft(d + ((a & b) | (~a & c)) + x[1] + 0xE8C7B756, 12);
        c = d + RotateLeft(c + ((d & a) | (~d & b)) + x[2] + 0x242070DB, 17);
        b = c + RotateLeft(b + ((c & d) | (~c & a)) + x[3] + 0xC1BDCEEE, 22);
        a = b + RotateLeft(a + ((b & c) | (~b & d)) + x[4] + 0xF57C0FAF, 7);
        d = a + RotateLeft(d + ((a & b) | (~a & c)) + x[5] + 0x4787C62A, 12);
        c = d + RotateLeft(c + ((d & a) | (~d & b)) + x[6] + 0xA8304613, 17);
        b = c + RotateLeft(b + ((c & d) | (~c & a)) + x[7] + 0xFD469501, 22);
        a = b + RotateLeft(a + ((b & c) | (~b & d)) + x[8] + 0x698098D8, 7);
        d = a + RotateLeft(d + ((a & b) | (~a & c)) + x[9] + 0x8B44F7AF, 12);
        c = d + RotateLeft(c + ((d & a) | (~d & b)) + x[10] + 0xFFFF5BB1, 17);
        b = c + RotateLeft(b + ((c & d) | (~c & a)) + x[11] + 0x895CD7BE, 22);
        a = b + RotateLeft(a + ((b & c) | (~b & d)) + x[12] + 0x6B901122, 7);
        d = a + RotateLeft(d + ((a & b) | (~a & c)) + x[13] + 0xFD987193, 12);
        c = d + RotateLeft(c + ((d & a) | (~d & b)) + x[14] + 0xA679438E, 17);
        b = c + RotateLeft(b + ((c & d) | (~c & a)) + x[15] + 0x49B40821, 22);

        // Round 2: G = (b & d) | (c & ~d)
        a = b + RotateLeft(a + ((b & d) | (c & ~d)) + x[1] + 0xF61E2562, 5);
        d = a + RotateLeft(d + ((a & c) | (b & ~c)) + x[6] + 0xC040B340, 9);
        c = d + RotateLeft(c + ((d & b) | (a & ~b)) + x[11] + 0x265E5A51, 14);
        b = c + RotateLeft(b + ((c & a) | (d & ~a)) + x[0] + 0xE9B6C7AA, 20);
        a = b + RotateLeft(a + ((b & d) | (c & ~d)) + x[5] + 0xD62F105D, 5);
        d = a + RotateLeft(d + ((a & c) | (b & ~c)) + x[10] + 0x02441453, 9);
        c = d + RotateLeft(c + ((d & b) | (a & ~b)) + x[15] + 0xD8A1E681, 14);
        b = c + RotateLeft(b + ((c & a) | (d & ~a)) + x[4] + 0xE7D3FBC8, 20);
        a = b + RotateLeft(a + ((b & d) | (c & ~d)) + x[9] + 0x21E1CDE6, 5);
        d = a + RotateLeft(d + ((a & c) | (b & ~c)) + x[14] + 0xC33707D6, 9);
        c = d + RotateLeft(c + ((d & b) | (a & ~b)) + x[3] + 0xF4D50D87, 14);
        b = c + RotateLeft(b + ((c & a) | (d & ~a)) + x[8] + 0x455A14ED, 20);
        a = b + RotateLeft(a + ((b & d) | (c & ~d)) + x[13] + 0xA9E3E905, 5);
        d = a + RotateLeft(d + ((a & c) | (b & ~c)) + x[2] + 0xFCEFA3F8, 9);
        c = d + RotateLeft(c + ((d & b) | (a & ~b)) + x[7] + 0x676F02D9, 14);
        b = c + RotateLeft(b + ((c & a) | (d & ~a)) + x[12] + 0x8D2A4C8A, 20);

        // Round 3: H = b ^ c ^ d
        a = b + RotateLeft(a + (b ^ c ^ d) + x[5] + 0xFFFA3942, 4);
        d = a + RotateLeft(d + (a ^ b ^ c) + x[8] + 0x8771F681, 11);
        c = d + RotateLeft(c + (d ^ a ^ b) + x[11] + 0x6D9D6122, 16);
        b = c + RotateLeft(b + (c ^ d ^ a) + x[14] + 0xFDE5380C, 23);
        a = b + RotateLeft(a + (b ^ c ^ d) + x[1] + 0xA4BEEA44, 4);
        d = a + RotateLeft(d + (a ^ b ^ c) + x[4] + 0x4BDECFA9, 11);
        c = d + RotateLeft(c + (d ^ a ^ b) + x[7] + 0xF6BB4B60, 16);
        b = c + RotateLeft(b + (c ^ d ^ a) + x[10] + 0xBEBFBC70, 23);
        a = b + RotateLeft(a + (b ^ c ^ d) + x[13] + 0x289B7EC6, 4);
        d = a + RotateLeft(d + (a ^ b ^ c) + x[0] + 0xEAA127FA, 11);
        c = d + RotateLeft(c + (d ^ a ^ b) + x[3] + 0xD4EF3085, 16);
        b = c + RotateLeft(b + (c ^ d ^ a) + x[6] + 0x04881D05, 23);
        a = b + RotateLeft(a + (b ^ c ^ d) + x[9] + 0xD9D4D039, 4);
        d = a + RotateLeft(d + (a ^ b ^ c) + x[12] + 0xE6DB99E5, 11);
        c = d + RotateLeft(c + (d ^ a ^ b) + x[15] + 0x1FA27CF8, 16);
        b = c + RotateLeft(b + (c ^ d ^ a) + x[2] + 0xC4AC5665, 23);

        // Round 4: I = c ^ (b | ~d)
        a = b + RotateLeft(a + (c ^ (b | ~d)) + x[0] + 0xF4292244, 6);
        d = a + RotateLeft(d + (b ^ (a | ~c)) + x[7] + 0x432AFF97, 10);
        c = d + RotateLeft(c + (a ^ (d | ~b)) + x[14] + 0xAB9423A7, 15);
        b = c + RotateLeft(b + (d ^ (c | ~a)) + x[5] + 0xFC93A039, 21);
        a = b + RotateLeft(a + (c ^ (b | ~d)) + x[12] + 0x655B59C3, 6);
        d = a + RotateLeft(d + (b ^ (a | ~c)) + x[3] + 0x8F0CCC92, 10);
        c = d + RotateLeft(c + (a ^ (d | ~b)) + x[10] + 0xFFEFF47D, 15);
        b = c + RotateLeft(b + (d ^ (c | ~a)) + x[1] + 0x85845DD1, 21);
        a = b + RotateLeft(a + (c ^ (b | ~d)) + x[8] + 0x6FA87E4F, 6);
        d = a + RotateLeft(d + (b ^ (a | ~c)) + x[15] + 0xFE2CE6E0, 10);
        c = d + RotateLeft(c + (a ^ (d | ~b)) + x[6] + 0xA3014314, 15);
        b = c + RotateLeft(b + (d ^ (c | ~a)) + x[13] + 0x4E0811A1, 21);
        a = b + RotateLeft(a + (c ^ (b | ~d)) + x[4] + 0xF7537E82, 6);
        d = a + RotateLeft(d + (b ^ (a | ~c)) + x[11] + 0xBD3AF235, 10);
        c = d + RotateLeft(c + (a ^ (d | ~b)) + x[2] + 0x2AD7D2BB, 15);
        b = c + RotateLeft(b + (d ^ (c | ~a)) + x[9] + 0xEB86D391, 21);

        hash[0] += a;
        hash[1] += b;
        hash[2] += c;
        hash[3] += d;
    }

    private static uint RotateLeft(uint value, int bits) =>
        (value << bits) | (value >> (32 - bits));
}
