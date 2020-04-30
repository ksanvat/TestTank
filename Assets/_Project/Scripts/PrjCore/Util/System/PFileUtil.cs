using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PrjCore.Ext.System;
using PrjCore.Util.Data;
using UnityEngine;

namespace PrjCore.Util.System {
    public static class PFileUtil {

        private const int FileBufferSize = 4096;
        private static readonly string StreamingAssetsPath = Path.Combine(Application.streamingAssetsPath, "_Game");

        public static string BuildPathFromUnix(params string[] paths) {
            if (paths.IsNullOrEmpty()) {
                return null;
            }
            
            string result = StreamingAssetsPath;
            foreach (string sp in paths) {
                if (sp.IsNullOrWhiteSpace()) {
                    continue;
                }

                var args = (sp[0] != '/' ? '/' + sp : sp).Split('/');
                args[0] = result;
            
                result = Path.Combine(args);
            }

            return result;
        }
        
        public static async Task<T> ReadJsonFileToEndAsync<T>(string path, CancellationToken ct) where T : class {
            string json = await ReadTextFileToEndAsync(path, ct);
            return PJsonUtil.FromJson<T>(json);
        }
        
        public static async Task<string> ReadTextFileToEndAsync(string path, CancellationToken ct) {
            var bytes = await ReadBinaryFileToEndAsync(path, ct);
            if (bytes == null) {
                return null;
            }

            return Encoding.UTF8.Decode(bytes, FileBufferSize, ct);
        }

        public static async Task<byte[]> ReadBinaryFileToEndAsync(string path, CancellationToken ct) {
            using (var stream = NewFileStream(path)) {
                const int chunk = FileBufferSize;

                long size = stream.Length;
                int chunksCount = (int) (size / chunk);
                int lastChunk = (int) (size % chunk);
                var bytes = new byte[size];

                int i = 0;
                for (; i < chunksCount; ++i) {
                    int n = await stream.ReadAsync(bytes, i * chunk, chunk, ct);
                    if (n != chunk) {
                        return null;
                    }
                }

                if (lastChunk > 0) {
                    int n = await stream.ReadAsync(bytes, i * chunk, lastChunk, ct);
                    if (n != lastChunk) {
                        return null;
                    }
                }

                return bytes;
            }
        }
        
        private static FileStream NewFileStream(string path) {
            return new FileStream(
                path, 
                FileMode.Open, 
                FileAccess.Read, 
                FileShare.Read, 
                FileBufferSize, 
                FileOptions.Asynchronous | FileOptions.SequentialScan
            );
        }

    }
}