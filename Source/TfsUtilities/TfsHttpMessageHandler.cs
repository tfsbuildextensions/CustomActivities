// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//
// Licensed under the Microsoft Limited Public License (the "License");
// you may not use this file except in compliance with the License.
// A full copy of the license is provided at the bottom of this file
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TfsBuildExtensions.TfsUtilities
{
    /// <summary>
    /// Provides authentication for Visual Studio Services.
    /// </summary>
    public class TfsHttpMessageHandler : HttpMessageHandler
    {
        private TfsHttpClientHandler _innerHandler;

        /// <summary>
        /// Initializes a new <c>TfsHttpMessageHandler</c> instance with the specified credentials and request 
        /// settings.
        /// </summary>
        /// <param name="credentials">The credentials which should be used</param>
        /// <param name="settings">The request settings which should be used</param>
        public TfsHttpMessageHandler(
            NetworkCredential credentials,
            TfsHttpRequestSettings settings)
        {
            this.Credentials = credentials;
            this.Settings = settings;

            _innerHandler = new TfsHttpClientHandler(settings);
        }

        /// <summary>
        /// Gets the credentials associated with this handler.
        /// </summary>
        public NetworkCredential Credentials
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the settings associated with this handler.
        /// </summary>
        public TfsHttpRequestSettings Settings
        {
            get;
            private set;
        }

        internal Boolean ExpectContinue
        {
            get
            {
                return _innerHandler.ExpectContinue;
            }
            set
            {
                _innerHandler.ExpectContinue = value;
            }
        }

        /// <summary>
        /// Handles the authentication hand-shake for Tfs
        /// </summary>
        /// <param name="request">The HTTP request message</param>
        /// <param name="cancellationToken">The cancellation token used for cooperative cancellation</param>
        /// <returns>A new <c>Task&lt;HttpResponseMessage&gt;</c> which wraps the response from the remote service</returns>
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            HttpResponseMessage response = null;
            for (Int32 i = 3; i > 0; i--)
            {
                response = await _innerHandler.SendAsync(request, cancellationToken);
                if (!(response.StatusCode == HttpStatusCode.Found))
                {
                    // Make sure that once we can authenticate with the service that we turn off the 
                    // Expect100Continue behavior to increase performance.
                    this.ExpectContinue = false;
                    break;
                }
                else
                {
                    // If we have to change properties about the handler we need to instantiate a new
                    // one or else the underlying APIs barf
                    _innerHandler = new TfsHttpClientHandler(this.Settings);
                    request.Headers.Authorization = new AuthenticationHeaderValue("Basic", FormatBasicAuthHeader(Credentials));
                }
            }

            return response;
        }

        private static String FormatBasicAuthHeader(NetworkCredential credential)
        {
            String authHeader = String.Empty;
            if (!String.IsNullOrEmpty(credential.Domain))
            {
                authHeader = String.Format(CultureInfo.InvariantCulture,
                                           "{0}\\{1}:{2}",
                                           credential.Domain, credential.UserName, credential.Password);
            }
            else
            {
                authHeader = String.Format(CultureInfo.InvariantCulture,
                                           "{0}:{1}",
                                           credential.UserName, credential.Password);
            }

            return Convert.ToBase64String(TfsHttpRequestSettings.Encoding.GetBytes(authHeader));
        }

        /// <summary>
        /// This class is here simply to allow us to utilize the WebAPI HttpWebRequest handling without
        /// exposing the properties on the handler which do not make sense.
        /// </summary>
        private sealed class TfsHttpClientHandler : HttpClientHandler
        {
            public TfsHttpClientHandler(TfsHttpRequestSettings settings)
            {
                this.AllowAutoRedirect = false;
                this.ClientCertificateOptions = ClientCertificateOption.Automatic;
                this.ExpectContinue = settings.ExpectContinue;
                this.PreAuthenticate = false;
                this.UseCookies = false;
                this.UseDefaultCredentials = false;
                this.UseProxy = true;

                if (settings.CompressionEnabled)
                {
                    this.AutomaticDecompression = DecompressionMethods.GZip;
                }
            }

            public Boolean ExpectContinue
            {
                get;
                set;
            }

            public new Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                request.Headers.ExpectContinue = this.ExpectContinue;
                return base.SendAsync(request, cancellationToken);
            }
        }
    }
}


//MICROSOFT LIMITED PUBLIC LICENSE
//1. Definitions

//The terms “reproduce,” “reproduction,” “derivative works,” and “distribution” have the same meaning here as under U.S. copyright law.

//A “contribution” is the original software, or any additions or changes to the software.

//A “contributor” is any person that distributes its contribution under this license.

//“Licensed patents” are a contributor’s patent claims that read directly on its contribution.

//2. Grant of Rights

//(A) Copyright Grant - Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.

//(B) Patent Grant - Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.

//3. Conditions and Limitations

//(A) No Trademark License- This license does not grant you rights to use any contributors’ name, logo, or trademarks.

//(B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, your patent license from such contributor to the software ends automatically.

//(C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution notices that are present in the software.

//(D) If you distribute any portion of the software in source code form, you may do so only under this license by including a complete copy of this license with your distribution.  If you distribute any portion of the software in compiled or object code form, you may only do so under a license that complies with this license.

//(E) The software is licensed “as-is.” You bear the risk of using it. The contributors give no express warranties, guarantees or conditions.  You may have additional consumer rights under your local laws which this license cannot change. To the extent permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular purpose and non-infringement.

//(F) Platform Limitation - The licenses granted in sections 2(A) and 2(B) extend only to the software or derivative works that you create that run on a Microsoft Windows operating system product.
