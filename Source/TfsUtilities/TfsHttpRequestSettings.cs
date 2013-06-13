//-----------------------------------------------------------------------
// <copyright file="TfsHttpRequestSettings.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
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
namespace TfsBuildExtensions.TfsUtilities
{
    using System;
    using System.ComponentModel;
    using System.Text;
    using System.Threading;

    /// <summary>
    /// Provides common settings for a <c>TfsHttpMessageHandler</c> instance.
    /// </summary>
    public class TfsHttpRequestSettings
    {
        private static readonly Lazy<Encoding> LEncoding = new Lazy<Encoding>(() => new UTF8Encoding(false), LazyThreadSafetyMode.PublicationOnly);

        /// <summary>
        /// Initializes a new <c>TfsHttpRequestSettings</c> instance with compression enabled.
        /// </summary>
        public TfsHttpRequestSettings()
        {
            this.CompressionEnabled = true;
            this.ExpectContinue = true;
        }

        /// <summary>
        /// Gets the encoding used for outgoing requests.
        /// </summary>
        public static Encoding Encoding
        {
            get
            {
                return LEncoding.Value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not compression should be used on outgoing requests. The
        /// default value is true.
        /// </summary>
        [DefaultValue(true)]
        public bool CompressionEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not the Expect: 100-continue header should be sent on
        /// outgoing request. The default value is true.
        /// </summary>
        [DefaultValue(true)]
        public bool ExpectContinue
        {
            get;
            set;
        }
    }
}

// MICROSOFT LIMITED PUBLIC LICENSE
// 1. Definitions
// The terms “reproduce,” “reproduction,” “derivative works,” and “distribution” have the same meaning here as under U.S. copyright law.
// A “contribution” is the original software, or any additions or changes to the software.
// A “contributor” is any person that distributes its contribution under this license.
// “Licensed patents” are a contributor’s patent claims that read directly on its contribution.
// 2. Grant of Rights
// (A) Copyright Grant - Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
// (B) Patent Grant - Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.
// 3. Conditions and Limitations
// (A) No Trademark License- This license does not grant you rights to use any contributors’ name, logo, or trademarks.
// (B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, your patent license from such contributor to the software ends automatically.
// (C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution notices that are present in the software.
// (D) If you distribute any portion of the software in source code form, you may do so only under this license by including a complete copy of this license with your distribution.  If you distribute any portion of the software in compiled or object code form, you may only do so under a license that complies with this license.
// (E) The software is licensed “as-is.” You bear the risk of using it. The contributors give no express warranties, guarantees or conditions.  You may have additional consumer rights under your local laws which this license cannot change. To the extent permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular purpose and non-infringement.
// (F) Platform Limitation - The licenses granted in sections 2(A) and 2(B) extend only to the software or derivative works that you create that run on a Microsoft Windows operating system product.