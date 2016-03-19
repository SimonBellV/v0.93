//-----------------------------------------------------------------------
// <copyright file="MoBackFile.cs" company="moBack"> 
//     Copyright 2015 moBack Inc. All Rights Reserved.
// </copyright>
//-----------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using MoBackInternal;

namespace MoBack
{
    /// <summary>
    /// This class allows the user to download files from the server.
    /// </summary>
    public class MoBackFile : MoBackRow
    {
        public string url { get; private set; }
        
        /// <summary>
        /// Name of the MoBackFile. 
        /// Note: This variable is set in the constructor when upload the file. It will change when upload request success.
        /// The file name will be changed to whatever the server gives back.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; private set; }

        /// <summary>
        /// Accesor for the status of the download request.
        /// </summary>
        /// <value> A MoBackRequest state. </value>
        public MoBackRequest.RequestState Status { get; private set; }

        /// <summary>
        /// Gets the data.
        /// </summary>
        /// <value>The data.</value>
        public byte[] Data 
        {
            get 
            {
                switch (Status) 
                {
                case MoBackRequest.RequestState.COMPLETED:
                    return data;
                case MoBackRequest.RequestState.NOT_STARTED:
                    Debug.LogError("You have  to Download() a MoBackFile for its contents to become available.");
                    break;
                case MoBackRequest.RequestState.NOT_FINISHED:
                    Debug.LogError("The file is not finished downloading and cannot be accessed.");
                    break;
                case MoBackRequest.RequestState.ERROR:
                    Debug.LogError("There was a problem in downloading the file.");
                    break;
                }
                return null;
            }
        }

        private byte[] data;

        private MoBackFile() : base(null)
        {
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="MoBack.MoBackFile"/> class.
        /// </summary>
        /// <param name="fileData">File data.</param>
        /// <param name="fileName">File name.</param>
        public MoBackFile(string tableName, byte[] fileData, string fileName) : base(tableName)
        {
            data = fileData;
            Name = fileName;
        }
        
        /// <summary>
        /// Froms the JSO.
        /// </summary>
        /// <returns>The JSO.</returns>
        /// <param name="jsonObject">Json object.</param>
        public static MoBackFile FromJSON(SimpleJSONNode jsonObject)
        {
            MoBackFile fileHolder = new MoBackFile();
            fileHolder.Name = jsonObject["name"];
            fileHolder.url = jsonObject["url"];
            return fileHolder;
        }
        
        /// <summary>
        /// Converts MoBackObject data to a JSON object for storage.
        /// </summary>
        /// <returns>A JSON object.</returns>
        public override SimpleJSONClass GetJSON()
        {
            // Construct a child json object, which is the File type.
            SimpleJSONClass jsonForUploadFile = new SimpleJSONClass();
            jsonForUploadFile["__type"] = "File";
            jsonForUploadFile["name"] = Name;
            jsonForUploadFile["url"] = url;
            
            // Final json structure to send to server, to create a new row.
            SimpleJSONClass jsonStructure = new SimpleJSONClass();
            
            // Add the child json.
            jsonStructure.Add("FileURL", jsonForUploadFile);
            
            // Add other values if any.
            foreach (KeyValuePair<string, object> item in storeValues) 
            {
                if (item.Key == "objectId" || item.Key == "createdAt" || item.Key == "updatedAt")
                {
                    continue; // No need to send auto-populated fields as part of JSON message
                }
                
                SimpleJSONNode node = MoBackUtils.MoBackTypedObjectToJSON(item.Value, columnTypeData[item.Key]);
                jsonStructure.Add(item.Key, node);
            }
            
            return jsonStructure;
        }
        
        /// <summary>
        /// Deletes the file on MoBackServer
        /// </summary>
        /// <returns>The file.</returns>
        public MoBackRequest DeleteFile()
        {
            if (string.IsNullOrEmpty(MoBackAppSettings.SessionToken))
            {
                Debug.LogError("Missing Session Token. Need to use an appUser to log in first");
                return null;
            }
            
            if (string.IsNullOrEmpty(this.Name))
            {
                Debug.LogError("The file name is empty");
                return null;
            }
            
            string uri = MoBackURLS.FileDelete + this.Name;
            
            return new MoBackRequest(DeleteFileProcessor, uri, MoBackInternal.HTTPMethod.DELETE, null, null);
        }
        
        /// <summary>
        /// Uploads the file to MoBack server.
        /// </summary>
        /// <returns>The file.</returns>
        public MoBackRequest UploadFile()
        {
            if (data == null)
            {
                Debug.LogError("There is no data to upload.");
                return null;
            }
            
            // We need the file name for a valid header when construct a MultiPart/Form-Data.
            if (string.IsNullOrEmpty(Name))
            {
                Debug.LogError("missing file name.");
                return null;
            }
            
            // Get the end point to upload the file.
            string uri = MoBackInternal.MoBackURLS.FileUpload;
            
            // Construct a Content-Type for the HTTP method.
            string uploadRequestContentType;
            byte[] postData = FormatFileForMultiPartFormData(out uploadRequestContentType, this.Name, this.data);
            
            return new MoBackRequest(UploadFileProcessor, uri, MoBackInternal.HTTPMethod.PUT, null, postData, uploadRequestContentType);
        }
        
        /// <summary>
        /// Begins the request for a download from the server.
        /// </summary>
        /// <returns> Returns the data downloaded from the server if the request was successful; otherwise, notfies of an error. </returns>
        public Coroutine Download()
        {
            if (Status == MoBackRequest.RequestState.NOT_FINISHED) 
            {
                Debug.LogError("You are already downloading this file!");
                return null;
            }
            
            return CoroutineRunner.RunCoroutine(DoDownload());
        }

        /// <summary>
        /// Waits for a response from the server and completes the request.
        /// </summary>
        /// <returns> Either successfully completes the download request or will notify the user of an error. </returns>
        private IEnumerator DoDownload()
        {
            Status = MoBackRequest.RequestState.NOT_FINISHED;
            WWW fileDownloader = new WWW(this.url);
            yield return fileDownloader;

            if (string.IsNullOrEmpty(fileDownloader.error)) 
            {
                data = fileDownloader.bytes;
                Status = MoBackRequest.RequestState.COMPLETED;
            } 
            else 
            {
                Status = MoBackRequest.RequestState.ERROR;
            }
        }
        
        /// <summary>
        /// A callback when UploadFile request is completed.
        /// </summary>
        /// <param name="responseJson">Response json.</param>
        private void UploadFileProcessor(SimpleJSONNode responseJson)
        {
            this.Name = responseJson["name"];
            this.url = responseJson["url"];
        }
        
        /// <summary>
        /// Deletes the file processor.
        /// </summary>
        /// <param name="responseJson">Response json.</param>
        private void DeleteFileProcessor(SimpleJSONNode responseJson)
        {
            string responseCode = responseJson["code"];
            
            if (responseCode == "1000")
            {
                Debug.Log("Reset MoBackFile object");
                // delete request success. Reset the local version of the MoBackFile.
                this.url = null;
                this.Name = null;
                this.data = null;
            }
        }
        
        /// <summary>
        /// Add a header and footer to the byte[] to construct a MultiPart/Form-data.
        /// </summary>
        /// <returns>The file for multi part form data.</returns>
        /// <param name="requestContentType">Request content type.</param>
        /// <param name="fileName">File name.</param>
        /// <param name="uploadFile">Upload file.</param>
        private byte[] FormatFileForMultiPartFormData(out string requestContentType, string fileName, byte[] uploadFile)
        {
            // Construct boundary and content type. Use Guild to generate a random number attached to the form
            string moBackFormatDataBoundary = string.Format("--MoBackFormData{0:N}", System.Guid.NewGuid());
            
            // Construct a ContentType for the request. It's not only multipart/form-data, but also need the boundary append after it.
            requestContentType = string.Format("multipart/form-data; boundary={0}", moBackFormatDataBoundary);

            // Create a new Stream to write a new file header.
            System.IO.Stream formStream = new System.IO.MemoryStream();
            
            // Create encoding type
            System.Text.Encoding encoding = System.Text.Encoding.UTF8;
            
            /*
            // Construct a header for the upload file:
            // Result: 
            --MoBackFormData{RandomNumber}
            Content-Disposition: form-data; name="file"; filename={fileName}
            Content-Type: <-- this could be empty?
            // Note: we do need the Content-Disposition. Not sure about the Content-Type, left it blank for now.
            */
            string fileUploadheader = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"; filename=\"{2}\"\r\nContent-Type: {3}\r\n\r\n",
                                                    moBackFormatDataBoundary,
                                                    "file",
                                                    fileName,
                                                    string.Empty);
            
            // Write the header to stream.
            formStream.Write(encoding.GetBytes(fileUploadheader), 0, encoding.GetByteCount(fileUploadheader));
            
            // Write the upload file to stream
            formStream.Write(uploadFile, 0, uploadFile.Length);
            
            // Create footer for the upload file.
            string fileUploadFooter = string.Format("\r\n--{0}--\r\n", moBackFormatDataBoundary);
            
            // Write the footer to stream
            formStream.Write(encoding.GetBytes(fileUploadFooter), 0, encoding.GetByteCount(fileUploadFooter));
            
            // Construct a new byte array after finish append header and footer.
            formStream.Position = 0;
            byte[] postData = new byte[formStream.Length];
            formStream.Read(postData, 0, postData.Length);
            formStream.Close();
            
            return postData;
        }
    }
}