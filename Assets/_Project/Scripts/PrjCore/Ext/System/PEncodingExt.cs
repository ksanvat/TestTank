using System.Text;
using System.Threading;

namespace PrjCore.Ext.System {
    public static class PEncodingExt {
        public static string Decode(this Encoding encoding, byte[] bytes, int chunk, CancellationToken ct) {
            if (encoding == null) {
                return null;
            }
            
            var decoder = encoding.GetDecoder();
            
            int chunksCount = bytes.Length / chunk;
            int lastChunk = bytes.Length % chunk;

            var chars = new char[encoding.GetMaxCharCount(chunk)];
            var sb = new StringBuilder(chars.Length);            
            
            int i = 0;
            for (; i < chunksCount; ++i) {
                ct.ThrowIfCancellationRequested();
                
                int n = decoder.GetChars(bytes, i * chunk, chunk, chars, 0, false);
                if (n > 0) {
                    sb.Append(chars, 0, n);
                }
            }

            if (lastChunk > 0) {
                ct.ThrowIfCancellationRequested();
                
                int n = decoder.GetChars(bytes, i * chunk, lastChunk, chars, 0, false);
                if (n > 0) {
                    sb.Append(chars, 0, n);
                }
            }

            return sb.ToString();
        }
    }
}