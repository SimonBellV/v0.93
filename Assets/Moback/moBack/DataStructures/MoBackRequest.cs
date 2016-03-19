//-----------------------------------------------------------------------
// <copyright file="MoBackRequest.cs" company="moBack"> 
//     Copyright 2015 moBack Inc. All Rights Reserved.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Net;
using System.Threading;
using System.Collections;
using UnityEngine;
using SimpleJSON;
using MoBackInternal;
using MoBack;

namespace MoBack
{
    /// <summary>
    /// This class is repsonbile for contacting the server and sending / receiving data.
    /// </summary>
    public class MoBackRequest
    {
        /// <summary> MoBack response fields. </summary>
        public enum RequestState
        {
            NOT_STARTED = 0,
            NOT_FINISHED,
            ERROR,
            COMPLETED
        }

        /// <summary>
        /// Accessor for the request state.
        /// </summary>
        /// <value> The state. </value>
        public RequestState State { get; protected set; }
        
        public string message = null;
        public MoBackError errorDetails = null;

        /// <summary> 
        /// Optional response processor (such as for grabbing the ID from a recently-saved object).
        /// </summary>
        /// <param name="rawResponse"> A JSON object. </param>
        public delegate void ResponseProcessor(SimpleJSONNode rawResponse);

        private ResponseProcessor responseProcessor;

        // MoBackRequest parameters
        private string url;
        private HTTPMethod methodType;
        private MoBackRequestParameters query;
        private byte[] body;
        private MoBackRequestManager.RequestErrorCallback errorHandler;
        private MoBackRequestManager.RequestResultCallback resultHandler;
        private string contentType;
        protected Coroutine currentRun;
        

        /// <summary>
        /// Initializes a new instance of the <see cref="MoBack.MoBackRequest"/> class.
        /// </summary>
        /// <param name="url"> A url. </param>
        /// <param name="methodType"> A method type. </param>
        /// <param name="query"> A query. </param>
        /// <param name="body"> A byte array. </param>
        public MoBackRequest(string url, HTTPMethod methodType, MoBackRequestParameters query = null, byte[] body = null, string contentType = null)
        {
            this.url = url;
            this.methodType = methodType;
            this.query = query;
            this.body = body;
            this.contentType = contentType;
        }

        /// <summary>
        /// Allows for a response processor (such as for grabbing the ID from a recently-saved object), but REMEMBER that this callback comes from another thread.
        /// </summary>
        /// <param name="responseProcessor"> Response processor. </param>
        /// <param name="url"> A url. </param>
        /// <param name="methodType"> Method type. </param>
        /// <param name="query"> Query. </param>
        /// <param name="body"> A byte array. </param>
        public MoBackRequest(ResponseProcessor responseProcessor, string url, HTTPMethod methodType, MoBackRequestParameters query = null, byte[] body = null, string contentType = null) : this(url, methodType, query, body, contentType)
        {
            this.responseProcessor = responseProcessor;
        }

        
        /// <summary>
        /// Coroutine to run a request.
        /// </summary>
        /// <returns> The current status of the request. </returns>
        public Coroutine Run()
        {
            if (State == RequestState.NOT_FINISHED) 
            {
                if (MoBackAppSettings.loggingLevel >= MoBackAppSettings.LoggingLevel.WARNINGS) 
                {
                    Debug.LogWarning("Trying to run a MoBackRequest when it's already running.");
                }
                return currentRun;
            }
            State = RequestState.NOT_FINISHED;
            
            #if UNITY_ANDROID
            if(!MoBackRequestManager.SSLValidator.initialized) MoBackRequestManager.SSLValidator.init();
            #endif
            new Thread(Execute).Start();

            currentRun = CoroutineRunner.RunCoroutine(PollForAndProcessResponse());
            return currentRun;
        }

        /// <summary>
        /// Requests the error callback.
        /// </summary>
        /// <param name="exception"> A web exception. </param>
        /// <param name="errorStatus"> The error status. </param>
        /// <param name="status"> Status. Default to null. </param>
        /// <param name="statusDescription"> Status description. Default to null. </param>
        protected void RequestErrorCallback(WebException exception, WebExceptionStatus errorStatus, HttpStatusCode? status = null, string statusDescription = null)
        {
            errorDetails = new MoBackError(exception, errorStatus, status, statusDescription);
            currentRun = null;
            State = RequestState.ERROR;
        }

        /// <summary>
        /// Requests the result callback.
        /// </summary>
        /// <param name="rawMessage"> The raw message. </param>
        /// <param name="jsonIfAny"> A JSON object, if any. </param>
        protected virtual void RequestResultCallback(string rawMessage, SimpleJSONNode jsonIfAny)
        {
            if (responseProcessor != null)
            {
                responseProcessor(jsonIfAny);
            }
            
            message = rawMessage;
            currentRun = null;
            State = RequestState.COMPLETED;
        }

        /// <summary>
        /// Execute this instance.
        /// </summary>
        private void Execute()
        {
            MoBackRequestManager.RunRequest(url, methodType, RequestErrorCallback, RequestResultCallback, query, body, contentType);
        }

        /// <summary>
        /// Polls for and process response.
        /// </summary>
        /// <returns> Waits for a response while the request is still processing. </returns>
        private IEnumerator PollForAndProcessResponse()
        {
            while (State == RequestState.NOT_FINISHED) 
            {
                yield return null;
            }
        }
    }

    /// <summary>
    /// A class that inherits from MoBackRequest, but handles generic responses and methods.
    /// </summary>
    public class MoBackRequest<T> : MoBackRequest
    {
        /// <summary>
        /// A generic delegate to process a response.
        /// </summary>
        /// <param name="rawResponse">A JSON object.</param>
        /// <returns>A generic value.</returns>
        new public delegate T ResponseProcessor(SimpleJSONNode rawResponse);

        private ResponseProcessor responseProcessor;
        private T response;

        /// <summary>
        /// Gets the response value.
        /// </summary>
        /// <value> The response value. Notifies the user of the status of a request. </value>
        public T ResponseValue 
        {
            get 
            {
                switch (State) 
                {
                case RequestState.COMPLETED:
                    return response;
                case RequestState.NOT_STARTED:
                    Debug.LogError("You have to start a MoBackRequest with MoBackRequest.Run() and wait for it to complete before you can access it's outcome.");
                    break;
                case RequestState.NOT_FINISHED:
                    Debug.LogError("You have to wait for a MoBackRequest to complete before you can access it's outcome.");
                    break;
                case RequestState.ERROR:
                    Debug.LogError("There was a problem in processing the MoBackRequest and the outcome cannot be accessed. See additonal fields for more info.");
                    break;
                }
                return default(T);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MoBack.MoBackRequest`1"/> class.
        /// </summary>
        /// <param name="responseProcessor">Response processor.</param>
        /// <param name="url"> A url. </param>
        /// <param name="methodType"> Method type. </param>
        /// <param name="query"> A query. </param>
        /// <param name="body"> A byte array of data. </param>
		public MoBackRequest(ResponseProcessor responseProcessor, string url, HTTPMethod methodType, MoBackRequestParameters query = null, byte[] body = null, string contentType = null) : base(url, methodType, query, body,contentType)
        {
            this.responseProcessor = responseProcessor;
        }

//		public MoBackRequest(ResponseProcessor responseProcessor, string url, HTTPMethod methodType, MoBackRequestParameters query = null, byte[] body = null, string contentType = null ) : base(url, methodType, query, body)
//		{
//			this.responseProcessor = responseProcessor;
//        }
        
        /// <summary>
        /// Requests the result callback.
        /// </summary>
        /// <param name="rawMessage"> The raw message. </param>
        /// <param name="jsonIfAny"> A JSON object, if any. </param>
        protected override void RequestResultCallback(string rawMessage, SimpleJSONNode jsonIfAny)
        {
            if (responseProcessor != null)
            {
                response = responseProcessor(jsonIfAny);
            }
            
            message = rawMessage;
            currentRun = null;
            State = RequestState.COMPLETED;
        }
    }

    /// <summary>
    /// A class defined a MoBackError.
    /// </summary>
    public class MoBackError
    {
        /// <summary>
        /// Accessor for exceptions.
        /// </summary>
        /// <value> The specific exception. </value>
        public WebException Exception { get; private set; }

        /// <summary>
        /// Accessor for the connection status.
        /// </summary>
        /// <value> The connection status. </value>
        public WebExceptionStatus ConnectionStatus { get; private set; }

        /// <summary>
        /// Accessor for the HTTP status.
        /// </summary>
        /// <value> The http status. </value>
        public HttpStatusCode? HttpStatus { get; private set; }

        /// <summary>
        /// Accessor for the message.
        /// </summary>
        /// <value> The message as a string. </value>
        public string Message { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MoBack.MoBackError"/> class.
        /// </summary>
        /// <param name="exception"> An exception. </param>
        /// <param name="exceptionType"> The exception type. </param>
        /// <param name="httpStatus"> Http status. </param>
        /// <param name="statusDescription"> Status description. </param>
        public MoBackError(WebException exception, WebExceptionStatus exceptionType, HttpStatusCode? httpStatus, string statusDescription)
        {
            this.Exception = exception;
            this.ConnectionStatus = exceptionType;
            this.HttpStatus = httpStatus;
            this.Message = statusDescription;
        }

        /// <summary>
        /// Formats the exception.
        /// </summary>
        /// <returns> The exception. </returns>
        /// <param name="e"> The exception to be formatted. </param>
        public static string FormatException(Exception e)
        {
            string s = string.Empty;
            do 
            {
                s += e.Message;
                e = e.InnerException;
            } 
            while (e != null);
            
            return s;
        }
        
        /// <summary>
        /// Converts the status of a connection to a string.
        /// </summary>
        /// <returns> The readable connection status. </returns>
        /// <param name="status"> The current web exception status. </param>
        public static string HumanReadableConnectionStatus(WebExceptionStatus status)
        {
            switch (status) 
            {
            case WebExceptionStatus.NameResolutionFailure:
                return "Server Not Found";
            default:
                return status.ToString();
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents the current <see cref="MoBack.MoBackError"/>.
        /// </summary>
        /// <returns> Returns the error as a string. </returns>
        public override string ToString()
        {
            if (HttpStatus != null) 
            {
                return string.Format("Error {0}: {1}", (int)HttpStatus, HttpStatus.ToString());
            } 
            else 
            {
                return "Connection Failed: " + HumanReadableConnectionStatus(ConnectionStatus);
            }
        }
    }
}
