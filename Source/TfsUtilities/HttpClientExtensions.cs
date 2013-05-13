// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//
// Licensed under the Microsoft Limited Public License (the "License");
// you may not use this file except in compliance with the License.
// A full copy of the license is provided in the root folder of the 
// project directory.
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// -----------------------------------------------------------------------

using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Common;
using Microsoft.TeamFoundation.Common.Internal;

namespace TfsBuildExtensions.TfsUtilities
{
    public static class HttpClientExtensions
    {
        private const int DefaultChunkSize = 16 * 1024 * 1024;
       
        /// <summary>
        /// Downloads the content of a file and copies it to the specified stream if the request succeeds. 
        /// </summary>
        /// <param name="client">Http client.</param>
        /// <param name="requestUri">Download uri.</param>
        /// <param name="stream">Stream to write file content to.</param>
        /// <returns>Http response message.</returns>
        public static async Task<HttpResponseMessage> DownloadFileFromTfsAsync(this HttpClient client, Uri requestUri, Stream stream)
        {
            TFCommonUtil.CheckForNull(client, "client");
            TFCommonUtil.CheckForNull(requestUri, "requestUri");
            TFCommonUtil.CheckForNull(stream, "stream");

            HttpResponseMessage response = await client.GetAsync(requestUri.ToString());

            if (response.IsSuccessStatusCode && response.StatusCode != HttpStatusCode.NoContent)
            {
                Boolean decompress;
                if (StringComparer.OrdinalIgnoreCase.Equals(response.Content.Headers.ContentType.MediaType, "application/octet-stream"))
                {
                    decompress = false;
                }
                else if (StringComparer.OrdinalIgnoreCase.Equals(response.Content.Headers.ContentType.MediaType, "application/gzip"))
                {
                    decompress = true;
                }
                else
                {
                    throw new Exception(String.Format("Unsupported Content Type {0}", response.Content.Headers.ContentType.MediaType));
                }

                using (DownloadStream downloadStream = new DownloadStream(stream, decompress, response.Content.Headers.ContentMD5))
                {
                    await response.Content.CopyToAsync(downloadStream);
                    downloadStream.ValidateHash();
                }
            }

            return response;
        }

        /// <summary>
        /// Wraps the download stream to provide hash calculation and content decompression.
        /// </summary>
        private class DownloadStream : Stream
        {
            private Stream _stream;
            private Boolean _decompress;
            private MD5 _hashProvider;
            private Byte[] _expectedHashValue;

            public DownloadStream(Stream stream, Boolean decompress, Byte[] hashValue)
            {
                _stream = stream;
                _decompress = decompress;
                _expectedHashValue = hashValue;

                if (hashValue != null && hashValue.Length == 16)
                {
                    _expectedHashValue = hashValue;
                    _hashProvider = MD5Util.TryCreateMD5Provider();
                }
            }

            public override Boolean CanRead
            {
                get
                {
                    return _stream.CanRead;
                }
            }

            public override Boolean CanSeek
            {
                get
                {
                    return _stream.CanSeek;
                }
            }

            public override Boolean CanWrite
            {
                get
                {
                    return _stream.CanWrite;
                }
            }

            public override Int64 Length
            {
                get
                {
                    return _stream.Length;
                }
            }

            public override Int64 Position
            {
                get
                {
                    return _stream.Position;
                }
                set
                {
                    _stream.Position = value;
                }
            }

            public override void Flush()
            {
                _stream.Flush();
            }

            public override Int32 Read(Byte[] buffer, Int32 offset, Int32 count)
            {
                return _stream.Read(buffer, offset, count);
            }

            public override Int64 Seek(Int64 offset, SeekOrigin origin)
            {
                return _stream.Seek(offset, origin);
            }

            public override void SetLength(Int64 value)
            {
                _stream.SetLength(value);
            }

            public override void Write(Byte[] buffer, Int32 offset, Int32 count)
            {
                Byte[] outputBuffer;
                Int32 outputOffset;
                Int32 outputCount;

                Transform(buffer, offset, count, out outputBuffer, out outputOffset, out outputCount);
                _stream.Write(outputBuffer, outputOffset, outputCount);
            }

            public override IAsyncResult BeginWrite(Byte[] buffer, Int32 offset, Int32 count, AsyncCallback callback, Object state)
            {
                Byte[] outputBuffer;
                Int32 outputOffset;
                Int32 outputCount;

                Transform(buffer, offset, count, out outputBuffer, out outputOffset, out outputCount);
                return _stream.BeginWrite(outputBuffer, outputOffset, outputCount, callback, state);
            }

            public override void EndWrite(IAsyncResult asyncResult)
            {
                _stream.EndWrite(asyncResult);
            }

            public override Task WriteAsync(Byte[] buffer, Int32 offset, Int32 count, CancellationToken cancellationToken)
            {
                Byte[] outputBuffer;
                Int32 outputOffset;
                Int32 outputCount;

                Transform(buffer, offset, count, out outputBuffer, out outputOffset, out outputCount);
                return _stream.WriteAsync(outputBuffer, outputOffset, outputCount, cancellationToken);
            }

            public override void WriteByte(Byte value)
            {
                Write(new Byte[] { value }, 0, 1);
            }

            public void ValidateHash()
            {
                if (_hashProvider != null)
                {
                    _hashProvider.TransformFinalBlock(new Byte[0], 0, 0);

                    if (!ArrayUtil.Equals(_hashProvider.Hash, _expectedHashValue))
                    {
                        throw new Exception("Download Corrupted");
                    }
                }
            }

            protected override void Dispose(Boolean disposing)
            {
                if (_hashProvider != null)
                {
                    _hashProvider.Dispose();
                    _hashProvider = null;
                }

                base.Dispose(disposing);
            }

            private void Transform(
                Byte[] buffer,
                Int32 offset,
                Int32 count,
                out Byte[] outputBuffer,
                out Int32 outputOffset,
                out Int32 outputCount)
            {
                if (_decompress)
                {
                    using (GZipStream gs = new GZipStream(new MemoryStream(buffer, offset, count), CompressionMode.Decompress))
                    {
                        int decompressedBufferSize = 4096;
                        Byte[] decompressedBuffer = new Byte[decompressedBufferSize];

                        Int32 bytesRead = 0;

                        using (MemoryStream decompressedOutput = new MemoryStream())
                        {
                            do
                            {
                                bytesRead = gs.Read(decompressedBuffer, 0, decompressedBufferSize);
                                if (bytesRead > 0)
                                {
                                    decompressedOutput.Write(decompressedBuffer, 0, bytesRead);
                                }
                            }
                            while (bytesRead > 0);

                            outputBuffer = decompressedOutput.ToArray();
                            outputOffset = 0;
                            outputCount = outputBuffer.Length;
                        }
                    }
                }
                else
                {
                    outputBuffer = buffer;
                    outputOffset = offset;
                    outputCount = count;
                }

                if (_hashProvider != null && outputCount > 0)
                {
                    _hashProvider.TransformBlock(outputBuffer, outputOffset, outputCount, null, 0);
                }
            }
        }
    }
}
