//-----------------------------------------------------------------------
// <copyright file="MoBackRequestManager.cs" company="moBack"> 
// Copyright 2015 moBack Inc. All Rights Reserved.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.IO;
using System.Net;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MoBack;
using UnityEngine;
using SimpleJSON;

namespace MoBackInternal
{
    /// <summary>
    /// Manages MoBackRequests made by the user.
    /// </summary>
    public static class MoBackRequestManager
    {
#if UNITY_EDITOR_WIN || (UNITY_STANDALONE_WIN && ! UNITY_EDITOR_OSX)
        [DllImport("SSLVerificationForWindows")]
        private static extern bool ValidateCert(IntPtr rawCert, ulong certLength, [MarshalAs(UnmanagedType.LPWStr)] string serverName);
#endif
        /// <summary> Callback for when there is an error while making a request. </summary>
        /// <param name="exception"> A WebException. </param>
        /// <param name="errorStatus"> A WebExceptionStatus. </param>
        /// <param name="status"> An HTTPStatusCode?. </param>
        /// <param name="StatusDescription"> A string value description of the status. </param>
        public delegate void RequestErrorCallback(WebException exception, WebExceptionStatus errorStatus, HttpStatusCode? status = null, string StatusDescription = null);

        /// <summary> Callback for when a request has been made succesfully. </summary>
        /// <param name="rawMessage"> A string message. </param>
        /// <param name="JsonIfAny"> A JSON object. </param>
        public delegate void RequestResultCallback(string rawMessage, SimpleJSONNode JsonIfAny);



        /// <summary>
        /// Runs the request.
        /// </summary>
        /// <param name="url"> The url to send a request to. </param>
        /// <param name="methodType"> A method type. </param>
        /// <param name="errorHandler"> A callback to handle errors. </param>
        /// <param name="callback"> A callback to handle a succesful request. Null by default. </param>
        /// <param name="query"> A set of query parameters. Null by default. </param>
        /// <param name="body"> A byte array. </param>
        /// <param name="contentType"> A content type of this request. If left the value null, the contentType will be "application/json". </param>
        public static void RunRequest(string url, HTTPMethod methodType, RequestErrorCallback errorHandler, 
                                      RequestResultCallback callback = null, MoBackRequestParameters query = null, 
                                      byte[] body = null, string contentType = null)
        {
            
            WebRequest request;
            if (query != null) 
            {
                request = WebRequest.Create(url + query);
            } 
            else 
            {
                request = WebRequest.Create(url);
            }

            if (MoBack.MoBackAppSettings.loggingLevel >= MoBackAppSettings.LoggingLevel.VERBOSE) 
            {
                Debug.Log(request.RequestUri.AbsoluteUri);
            }

            // Insert keys
            WebHeaderCollection collection = new WebHeaderCollection();
            collection.Add("X-Moback-Application-Key", MoBack.MoBackAppSettings.ApplicationID);
            collection.Add("X-Moback-Environment-Key", MoBack.MoBackAppSettings.EnvironmentKey);
            
            // Set session token if there is one.
            if (!string.IsNullOrEmpty(MoBack.MoBackAppSettings.SessionToken))
            {
				collection.Add("X-Moback-SessionToken-Key", MoBack.MoBackAppSettings.SessionToken);
            }
            
            request.Headers = collection;

            // Specify request as GET, POST, PUT, or DELETE 
            request.Method = methodType.ToString();
            
            if (string.IsNullOrEmpty(contentType))
            {
                request.ContentType = "application/json";
            }
            else
            {
                request.ContentType = contentType;
            }
            // Specify request content length
            request.ContentLength = body == null ? 0 : body.Length;
            
            string responseFromServer = String.Empty;

            #if UNITY_ANDROID && ! UNITY_EDITOR
            ServicePointManager.ServerCertificateValidationCallback = SSLValidator.Validator;
            #elif UNITY_EDITOR_WIN || (UNITY_STANDALONE_WIN && ! UNITY_EDITOR_OSX)
            ServicePointManager.ServerCertificateValidationCallback = Validator;
            #endif
            
            try 
            {
                // Open a stream and send the request to the remote server
                Stream dataStream = null;
                if (body != null) 
                {
                    dataStream = request.GetRequestStream();
                    dataStream.Write(body, 0, body.Length);
                    dataStream.Close();
                }
                
                // Complete the request, wait for and accept any response
                WebResponse response = request.GetResponse();
                // Process response
                if (MoBackAppSettings.loggingLevel >= MoBackAppSettings.LoggingLevel.VERBOSE) 
                {
                    Debug.Log(((HttpWebResponse)response).StatusDescription);
                }
                dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                responseFromServer = reader.ReadToEnd();
               // Debug.Log("the json is "+responseFromServer.ToString());
                // Close all streams
                reader.Close();
                response.Close();
            } 
            catch (WebException webException) 
            {
                HttpWebResponse errorResponse = webException.Response as HttpWebResponse;
                if (errorResponse != null) 
                {
                    if (MoBackAppSettings.loggingLevel >= MoBackAppSettings.LoggingLevel.WARNINGS) 
                    {
                        Debug.LogWarning(string.Format("Network Request Error {0}: {1}.\nFull Exception: {2}", (int)errorResponse.StatusCode, errorResponse.StatusCode.ToString(), MoBackError.FormatException(webException)));
                    }
                    errorHandler(webException, webException.Status, errorResponse.StatusCode, webException.Message);
                } 
                else 
                {
                    if (MoBackAppSettings.loggingLevel >= MoBackAppSettings.LoggingLevel.WARNINGS) 
                    {
                        Debug.LogWarning(string.Format("Network Request Failed with message {0}.\nFull exception: {1}", webException.Message, MoBackError.FormatException(webException)));
                    }
                    errorHandler(webException, webException.Status, null, webException.Message);
                }
                return;
            }

            if (MoBackAppSettings.loggingLevel >= MoBackAppSettings.LoggingLevel.VERBOSE) 
            {
                Debug.Log(responseFromServer);
            }
            if (callback != null) 
            {
                SimpleJSONNode responseAsJSON = SimpleJSONNode.Parse(responseFromServer);

                callback(responseFromServer, responseAsJSON);
            }
        }

        #if UNITY_EDITOR_WIN || (UNITY_STANDALONE_WIN && ! UNITY_EDITOR_OSX)
        public static bool Validator (object sender,
                                      System.Security.Cryptography.X509Certificates.X509Certificate certificate,
                                      System.Security.Cryptography.X509Certificates.X509Chain chain,
                                      System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            byte[] rawCert = certificate.GetRawCertData ();

            System.Security.Cryptography.X509Certificates.X509Certificate2 cert = (System.Security.Cryptography.X509Certificates.X509Certificate2)certificate;
            string serverName = cert.GetNameInfo(System.Security.Cryptography.X509Certificates.X509NameType.DnsName, false);

            IntPtr rawRawData = Marshal.AllocHGlobal (rawCert.Length);
            Marshal.Copy (rawCert, 0, rawRawData, rawCert.Length);

            bool certIsValid;
            certIsValid = ValidateCert (rawRawData, (ulong)rawCert.Length, serverName);

            Marshal.FreeHGlobal (rawRawData);

            return certIsValid;
        }

        /* C++ native windows source for SSLVerificationManagerForWindows.dll files
                Steps for getting to work in VS 2012:
                1. Make a DLL project
                2. Add any missing headers (<windows.h> <Wincrypt.h>) and libraries (should(?) only need to add Crypt32.lib in Project Properties -> Configuration -> Linker -> Input -> Additional Dependencies)
                3. Change Project Properties: Configuration Properties -> C/C++ -> Code Generation -> Runtime Library  to "Multi-threaded" instead of "Multi-threaded DLL"
                4. Change build type to 64 bits when building SSLVerificationManagerForWindows64.dll, keep at 32 bits for SSLVerificationManagerForWindows32.dll
        */
//      bool IsValidSSLCertificate( PCCERT_CONTEXT certificate, LPWSTR serverName);
//      
//      extern "C" __declspec(dllexport)
//          bool ValidateCert (BYTE* rawCert, unsigned long int rawCertLength, LPWSTR serverName)
//      {
//          CERT_BLOB certBlob;
//          certBlob.cbData = rawCertLength;
//          certBlob.pbData = rawCert;
//          
//          PCCERT_CONTEXT certContext;
//          
//          if(CryptQueryObject(CERT_QUERY_OBJECT_BLOB,
//                              &certBlob,
//                              CERT_QUERY_CONTENT_FLAG_CERT,
//                              CERT_QUERY_FORMAT_FLAG_ALL,
//                              NULL,
//                              NULL,
//                              NULL,
//                              NULL,
//                              NULL,
//                              NULL,
//                              (const void**)(&certContext)))
//          {
//              bool validCertificate = IsValidSSLCertificate(certContext, serverName);
//              CertFreeCertificateContext(certContext);
//              
//              return validCertificate;
//          }
//          return false;
//      }
//      
//      bool IsValidSSLCertificate( PCCERT_CONTEXT certificate, LPWSTR serverName)
//      {
//          LPSTR usages = szOID_PKIX_KP_SERVER_AUTH ;
//          
//          CERT_CHAIN_PARA params                           = { sizeof( params ) };
//          params.RequestedUsage.dwType                     = USAGE_MATCH_TYPE_AND;
//          params.RequestedUsage.Usage.cUsageIdentifier     = 1; // _countof( usages )
//          params.RequestedUsage.Usage.rgpszUsageIdentifier = &usages;
//          
//          PCCERT_CHAIN_CONTEXT chainContext = 0;
//          
//          if ( !CertGetCertificateChain( NULL,
//                                        certificate,
//                                        NULL,
//                                        NULL,
//                                        &params,
//                                        CERT_CHAIN_REVOCATION_CHECK_CHAIN_EXCLUDE_ROOT,
//                                        NULL,
//                                        &chainContext ) )
//          {
//              return false;
//          }
//          
//          SSL_EXTRA_CERT_CHAIN_POLICY_PARA sslPolicy = { sizeof( sslPolicy ) };
//          sslPolicy.dwAuthType                       = AUTHTYPE_SERVER;
//          sslPolicy.pwszServerName                   = serverName;
//          
//          CERT_CHAIN_POLICY_PARA policy = { sizeof( policy ) };
//          policy.pvExtraPolicyPara      = &sslPolicy;
//          
//          CERT_CHAIN_POLICY_STATUS status = { sizeof( status ) };
//          
//          BOOL verified = CertVerifyCertificateChainPolicy( CERT_CHAIN_POLICY_SSL,
//                                                           chainContext,
//                                                           &policy,
//                                                           &status );
//          
//          CertFreeCertificateChain( chainContext );
//          
//          return verified && status.dwError == 0;
//      }
        
        
        #endif
        
        #if UNITY_ANDROID
        class SSLVerificationJob {
            public byte[][] certs;
            public System.Threading.AutoResetEvent waitHandle;
            public bool certIsValid;
            
            public SSLVerificationJob (byte[][] certs, System.Threading.AutoResetEvent waitHandle) {
                this.certs = certs;
                this.waitHandle = waitHandle;
            }
        }

        public static class SSLValidator
        {
            static Queue<SSLVerificationJob> validatorQueue = new Queue<SSLVerificationJob>();

            public static bool initialized;
            
            public static void init() {
                CoroutineRunner.RunCoroutine(pollForVerificationJobs());
                initialized = true;
            }
            
            public static bool doSecurityCheckOnMainThread(byte[][] rawCerts)
            {
                System.Threading.AutoResetEvent waitHandle = new System.Threading.AutoResetEvent (false);
                SSLVerificationJob toVerify = new SSLVerificationJob (rawCerts, waitHandle);

                lock (validatorQueue) {
                    validatorQueue.Enqueue (toVerify);
                }

                waitHandle.WaitOne ();
                return toVerify.certIsValid;
            }
            
            static IEnumerator pollForVerificationJobs () {
                for (;;) {
                    yield return null;

                    while (validatorQueue.Count > 0) {
                        SSLVerificationJob toVerify = null;
                        lock (validatorQueue) {
                            toVerify = validatorQueue.Dequeue();
                        }

                        AndroidJavaObject fallbackCommunicator = new AndroidJavaObject ("FallbackAndroidHTTPService");
                        for (int i = 0; i < toVerify.certs.Length; i++) {
                            fallbackCommunicator.Call ("AddCert", toVerify.certs [i]);
                        }
                        toVerify.certIsValid = fallbackCommunicator.Call<bool> ("CheckCertificates");
                        toVerify.waitHandle.Set(); //signal processing is done
                    }
                }
            }
            
            public static bool Validator (object sender,
                                          System.Security.Cryptography.X509Certificates.X509Certificate certificate,
                                          System.Security.Cryptography.X509Certificates.X509Chain chain,
                                          System.Net.Security.SslPolicyErrors sslPolicyErrors)
            {
                //list of raw certs
                List<byte[]> rawCerts = new List<byte[]> ();
                rawCerts.Add (certificate.GetRawCertData ());

                //get rid of any duplicates in list
                foreach (System.Security.Cryptography.X509Certificates.X509ChainElement nextCertInChain in chain.ChainElements) 
                {
                    if (! certificate.Equals (nextCertInChain.Certificate)) {
                        certificate = nextCertInChain.Certificate;
                        rawCerts.Add(nextCertInChain.Certificate.GetRawCertData());
                    }
                }
                
                return doSecurityCheckOnMainThread (rawCerts.ToArray());
            }
        }

        /* Java language source for FallbackAndroidHTTPService (keeping it elsewhere might be cleaner, but I'd rather just be sure it doesn't get lost) 
         Compilation notes for people new to unity plugins: 
         Either compile in android studio as a library, or compile using javac like so:
            javac -source 1.6 -target 1.6 FallbackAndroidHTTPService.java
            jar cvf AndroidHTTPService.jar FallbackAndroidHTTPService.class */
        //
        //      import java.io.ByteArrayInputStream;
        //      
        //      import javax.net.ssl.TrustManager;
        //      import javax.net.ssl.X509TrustManager;
        //      import javax.net.ssl.TrustManagerFactory;
        //      
        //      import java.security.cert.CertificateException;
        //      import java.security.cert.X509Certificate;
        //      import java.security.cert.CertificateFactory;
        //      
        //      
        //      import java.security.KeyStore;
        //      import java.security.NoSuchAlgorithmException;
        //      import java.util.ArrayList;
        //      
        //      public class FallbackAndroidHTTPService {
        //          
        //          ArrayList<byte[]> certsAsArrayList = null;
        //          
        //          public void AddCert(byte[] rawCert) {
        //              if(certsAsArrayList == null) certsAsArrayList = new ArrayList<byte[]>();
        //              certsAsArrayList.add(rawCert);
        //          }
        //          
        //          public boolean CheckCertificates() {
        //              
        //              //get X509TrustManager from somewhere
        //              X509TrustManager trustManager = GetTrustManager();
        //              
        //              //find java certificate factory
        //              String x509Algorithm = "X.509";//TrustManagerFactory.getDefaultAlgorithm();
        //              
        //              CertificateFactory cFactory = null; //possibly should be explicit "X.509
        //              try {
        //                  cFactory = CertificateFactory.getInstance(x509Algorithm);
        //              } catch (CertificateException e) {
        //                  e.printStackTrace();
        //              }
        //              
        //              X509Certificate[] certs = new X509Certificate[certsAsArrayList.size()];
        //              for(int i = 0; i < certsAsArrayList.size(); i++) {
        //                  certs[i] = CertificateFromRawCertData(certsAsArrayList.get(i), cFactory);
        //              }
        //              
        //              try {
        //                  trustManager.checkClientTrusted(certs, x509Algorithm);
        //                  return  true;
        //              } catch (CertificateException e) {
        //                  e.printStackTrace();
        //                  return false;
        //              }
        //          }
        //          
        //          
        //          public X509TrustManager GetTrustManager() {
        //              try {
        //                  String defaultAlgorithm = TrustManagerFactory.getDefaultAlgorithm();
        //                  TrustManagerFactory trustManagerFactory = TrustManagerFactory.getInstance(defaultAlgorithm);
        //                  trustManagerFactory.init((KeyStore) null);
        //                  
        //                  return (X509TrustManager) (trustManagerFactory.getTrustManagers()[0]);
        //              } catch (Exception e) {
        //                  e.printStackTrace();
        //                  return  null;
        //              }
        //          }
        //          
        //          public X509Certificate CertificateFromRawCertData(byte[] rawCertData, CertificateFactory certFactory) {
        //              try {
        //                  return (X509Certificate) certFactory.generateCertificate(new ByteArrayInputStream(rawCertData));
        //              } catch (CertificateException e) {
        //                  e.printStackTrace();
        //                  e.printStackTrace();
        //                  return null;
        //              }
        //          }
        
        #endif
    }

    /// <summary
    /// The MoBack server URL.
    /// </summary>
    public static class MoBackURLS
    {
        public const string MoBackServer = "https://api.moback.com/";
        public const string TablesDefault = MoBackServer + "objectmgr/api/collections/";
        public const string Batch = MoBackServer + "objectmgr/api/collections/batch/";
        public const string BatchDelete = Batch + "delete/";
        public const string TablesSpecial = MoBackServer + "objectmgr/api/schema/";
        public const string Login = MoBackServer + "usermanager/api/users/login";
        public const string SignUp = MoBackServer + "usermanager/api/users/signup";
        public const string ResetPassword = MoBackServer + "usermanager/api/users/password/reset";
        public const string Invitation = MoBackServer + "usermanager/api/users/invitation";
        public const string User = MoBackServer + "usermanager/api/users/user";
		public const string FileUpload = MoBackServer + "filemanager/api/files/upload";
		public const string FileDelete = MoBackServer + "filemanager/api/files/file/";
		public const string AppStorage = MoBackServer  + "filemanager/api/files/appstorage";
 			
    }
}
