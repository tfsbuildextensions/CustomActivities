//-----------------------------------------------------------------------
// <copyright file="HttpClientExtensions.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
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
namespace TfsBuildExtensions.TfsUtilities
{
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

    /// <summary>
    /// HttpClientExtensions
    /// </summary>
    public static class HttpClientExtensions
    {
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
                bool decompress;
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
                    throw new Exception(string.Format("Unsupported Content Type {0}", response.Content.Headers.ContentType.MediaType));
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
            private readonly Stream stream;
            private readonly bool decompress;
            private readonly byte[] expectedHashValue;
            private MD5 hashProvider;

            public DownloadStream(Stream streamVal, bool decompressVal, byte[] hashValue)
            {
                this.stream = streamVal;
                this.decompress = decompressVal;
                this.expectedHashValue = hashValue;

                if (hashValue != null && hashValue.Length == 16)
                {
                    this.expectedHashValue = hashValue;
                    this.hashProvider = MD5Util.TryCreateMD5Provider();
                }
            }

            public override bool CanRead
            {
                get
                {
                    return this.stream.CanRead;
                }
            }

            public override bool CanSeek
            {
                get
                {
                    return this.stream.CanSeek;
                }
            }

            public override bool CanWrite
            {
                get
                {
                    return this.stream.CanWrite;
                }
            }

            public override long Length
            {
                get
                {
                    return this.stream.Length;
                }
            }

            public override long Position
            {
                get
                {
                    return this.stream.Position;
                }

                set
                {
                    this.stream.Position = value;
                }
            }

            public override void Flush()
            {
                this.stream.Flush();
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                return this.stream.Read(buffer, offset, count);
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                return this.stream.Seek(offset, origin);
            }

            public override void SetLength(long value)
            {
                this.stream.SetLength(value);
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                byte[] outputBuffer;
                int outputOffset;
                int outputCount;

                this.Transform(buffer, offset, count, out outputBuffer, out outputOffset, out outputCount);
                this.stream.Write(outputBuffer, outputOffset, outputCount);
            }

            public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
            {
                byte[] outputBuffer;
                int outputOffset;
                int outputCount;

                this.Transform(buffer, offset, count, out outputBuffer, out outputOffset, out outputCount);
                return this.stream.BeginWrite(outputBuffer, outputOffset, outputCount, callback, state);
            }

            public override void EndWrite(IAsyncResult asyncResult)
            {
                this.stream.EndWrite(asyncResult);
            }

            public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                byte[] outputBuffer;
                int outputOffset;
                int outputCount;

                this.Transform(buffer, offset, count, out outputBuffer, out outputOffset, out outputCount);
                return this.stream.WriteAsync(outputBuffer, outputOffset, outputCount, cancellationToken);
            }

            public override void WriteByte(byte value)
            {
                this.Write(new[] { value }, 0, 1);
            }

            public void ValidateHash()
            {
                if (this.hashProvider != null)
                {
                    this.hashProvider.TransformFinalBlock(new byte[0], 0, 0);

                    if (!ArrayUtil.Equals(this.hashProvider.Hash, this.expectedHashValue))
                    {
                        throw new Exception("Download Corrupted");
                    }
                }
            }

            protected override void Dispose(bool disposing)
            {
                if (this.hashProvider != null)
                {
                    this.hashProvider.Dispose();
                    this.hashProvider = null;
                }

                base.Dispose(disposing);
            }

            private void Transform(byte[] buffer, int offset, int count, out byte[] outputBuffer, out int outputOffset, out int outputCount)
            {
                if (this.decompress)
                {
                    using (GZipStream gs = new GZipStream(new MemoryStream(buffer, offset, count), CompressionMode.Decompress))
                    {
                        const int DecompressedBufferSize = 4096;
                        byte[] decompressedBuffer = new byte[DecompressedBufferSize];

                        using (MemoryStream decompressedOutput = new MemoryStream())
                        {
                            int bytesRead;
                            do
                            {
                                bytesRead = gs.Read(decompressedBuffer, 0, DecompressedBufferSize);
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

                if (this.hashProvider != null && outputCount > 0)
                {
                    this.hashProvider.TransformBlock(outputBuffer, outputOffset, outputCount, null, 0);
                }
            }
        }
    }
}
