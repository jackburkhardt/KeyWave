/*         INFINITY CODE         */
/*   https://infinity-code.com   */

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace InfinityCode.RealWorldTerrain.Net
{
    public class RealWorldTerrainDownloadItemUnityWebRequest : RealWorldTerrainDownloadItem
    {
        public static Func<string, UnityWebRequest> OnCreateWebRequest;
        public static Action<UnityWebRequest> OnPrepareRequest;

        private static CertificateValidator _certificateValidator;

        public UnityWebRequest uwr;
        public Dictionary<string, string> headers;

        public static CertificateValidator certificateValidator
        {
            get
            {
                if (_certificateValidator == null) _certificateValidator = new CertificateValidator();
                return _certificateValidator;
            }
        }

        public RealWorldTerrainDownloadItemUnityWebRequest(string url)
        {
            UnityWebRequest request;
            if (OnCreateWebRequest != null) request = OnCreateWebRequest(url);
            else request = UnityWebRequest.Get(url);

            request.certificateHandler = certificateValidator;

            if (OnPrepareRequest != null) OnPrepareRequest(request);

            RealWorldTerrainDownloadManager.Add(this);
            uwr = request;
        }

        public RealWorldTerrainDownloadItemUnityWebRequest(UnityWebRequest uwr)
        {
            RealWorldTerrainDownloadManager.Add(this);

            this.uwr = uwr;
        }

        public override float progress
        {
            get { return uwr.downloadProgress; }
        }

        public override void CheckComplete()
        {
            if (!uwr.isDone) return;

            if (string.IsNullOrEmpty(uwr.error))
            {
                byte[] bytes = uwr.downloadHandler.data;
                SaveWWWData(bytes);
                DispatchCompete(ref bytes);
            }
            else Debug.LogWarning("Download failed: " + uwr.url + "\n" + uwr.error);

            RealWorldTerrainDownloadManager.completeSize += averageSize;
            complete = true;

            Dispose();
        }

        public override void Dispose()
        {
            base.Dispose();

            if (uwr != null)
            {
                uwr.Dispose();
                uwr = null;
            }
        }

        public override void Start()
        {
            if (headers != null)
            {
                foreach (var header in headers) uwr.SetRequestHeader(header.Key, header.Value);
            }
            uwr.SendWebRequest();
        }

        public class CertificateValidator : CertificateHandler
        {
            protected override bool ValidateCertificate(byte[] certificateData)
            {
                return true;
            }
        }
    }
}