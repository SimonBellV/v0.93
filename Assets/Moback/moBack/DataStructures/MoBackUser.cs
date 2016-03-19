//-----------------------------------------------------------------------
// <copyright file="MoBackUser.cs" company="moBack">
// Copyright 2015 moBack Inc. All Rights Reserved.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using MoBack;
using MoBackInternal;
using SimpleJSON;
using UnityEngine;

namespace MoBack
{
    /// <summary>
    /// Manages the users. 
    /// </summary>
    public class MoBackUser : MoBackObject
    {
        /// <summary>
        /// Gets a value indicating whether this instance is logged in.
        /// </summary>
        /// <value><c>true</c> if this instance is logged in; otherwise, <c>false</c>.</value>
        public bool IsLoggedIn { get; private set; }
        
        /// <summary>
        /// Gets a value indicating whether this <see cref="MoBack.MoBackUser"/> complete signed up.
        /// </summary>
        /// <value><c>true</c> if complete signed up; otherwise, <c>false</c>.</value>
        public bool CompleteSignedUp { get; private set; }
        
        /// <summary>
        /// Gets the name of the user.
        /// </summary>
        /// <value>The name of the user.</value>
        public string UserName { get; private set; }
        
        public string EmailAddress { get; private set;}
        
        public string Password { get; private set;}
        
        public string ObjectId { get; private set;}
        
        /// <summary>
        /// Gets the created date.
        /// </summary>
        /// <value>The created date.</value>
        public DateTime CreatedDate { get; private set; }
        
        /// <summary>
        /// Gets the updated date.
        /// </summary>
        /// <value>The updated date.</value>
        public DateTime UpdatedDate { get; private set; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="MoBack.MoBackUser"/> class.
        /// </summary>
        private MoBackUser()
        {
        }
        
        public MoBackUser(string userName, string password, string email) : base()
        {
            IsLoggedIn = false;
            CompleteSignedUp = false;
            UserName = userName;
            Password = password;
            EmailAddress = email;
        }
        
        /// <summary>
        /// Login the specified userName and password.
        /// </summary>
        /// <param name="userName">User name.</param>
        /// <param name="password">Password.</param>
        /// <returns>A MoBackRequest with a MoBackUser type.</returns>
        public MoBackRequest Login()
        {
            SimpleJSONClass jsonStructure = new SimpleJSONClass();
            jsonStructure.Add("userId", UserName);
            jsonStructure.Add("password", Password);
            byte[] postData = jsonStructure.ToString().ToByteArray();
            
            MoBackRequest.ResponseProcessor LoginProcessor = (SimpleJSONNode jsonObject) =>
            {
                string ssotoken = jsonObject["ssotoken"];
                if (string.IsNullOrEmpty(ssotoken))
                {
                    IsLoggedIn = false;
                }
                else
                {
                    IsLoggedIn = true;
                    MoBack.MoBackAppSettings.SessionToken = ssotoken;
                }
            };
            
            return new MoBackRequest(LoginProcessor, MoBackURLS.Login, HTTPMethod.POST, null, postData);
        }
        
        /// <summary>
        /// Gets the user info.
        /// </summary>
        /// <returns>A MoBackRequest with a MoBackUser type.</returns>
        public MoBackRequest GetUserInfo()
        {
            // If there is not sessiontoken means user hasn't logged in yet, just exit.
            if (!IsLoggedIn)
            {
                Debug.LogError("There is no current login user.");
                return null;
            }
        
            MoBackRequest.ResponseProcessor GetCurrentUserInfoProcessor = (SimpleJSONNode jsonObject) =>
            {
                SimpleJSONClass userInfoJson = jsonObject["user"] as SimpleJSONClass;
                
                foreach (KeyValuePair<string, SimpleJSONNode> entry in userInfoJson.dict) 
                {
                    // we stored these outside of storeValues.
                    if (entry.Key == "userId" || entry.Key == "createdAt" || entry.Key == "updatedAt" || entry.Key == "objectId")
                    {
                        continue;
                    }
                    
                    MoBackValueType moBackType;
                    object data = MoBackUtils.MoBackTypedObjectFromJSON(entry.Value, out moBackType);
                    // This should always be equivalent to calling SetValue() with the appropriate type; if SetValue() functions are refactored then so too should this, and vice-versa.
                    this.SetColumnType(entry.Key, moBackType);
                    this.storeValues[entry.Key] = data;
                }
                
                UserName = userInfoJson["userId"];
                ObjectId = userInfoJson["objectId"];
                CreatedDate = MoBackDate.DateFromString(userInfoJson["createdAt"]);
                UpdatedDate = MoBackDate.DateFromString(userInfoJson["updatedAt"]);
            };
            
            return new MoBackRequest(GetCurrentUserInfoProcessor, MoBackURLS.User, HTTPMethod.GET, null, null);
        }
        
        /// <summary>
        /// Signs up.
        /// </summary>
        /// <returns>A MoBackRequest with a MoBackUser type.</returns>
        /// <param name="userName">User name.</param>
        /// <param name="email">Email.</param>
        /// <param name="password">Password.</param>
        public MoBackRequest SignUp()
        {
            SimpleJSONClass jsonStructure;

            // Get any values in storedValue to Json.
            jsonStructure = GetJSON();
            
            // Add specific column for MoBackUser.
            jsonStructure.Add("userId", UserName);
            jsonStructure.Add("email", EmailAddress);
            jsonStructure.Add("password", Password);
            
            byte[] postData = jsonStructure.ToString().ToByteArray();
            
            MoBackRequest.ResponseProcessor SignUpProcessor = (SimpleJSONNode jsonObject) =>
            {
                // If objectId is null or empty means we're unable to create an new user.
                ObjectId = jsonObject["objectId"];
                CompleteSignedUp = !string.IsNullOrEmpty(ObjectId);
            };
            
            return new MoBackRequest(SignUpProcessor, MoBackURLS.SignUp, HTTPMethod.POST, null, postData);
        }
        
        /// <summary>
        /// Resets the password.
        /// </summary>
        /// <returns>A MoBackRequest with a MoBackUser type.</returns>
        /// <param name="userName">User name.</param>
        public MoBackRequest ResetPassword()
        {
            SimpleJSONClass jsonStructure = new SimpleJSONClass();
            jsonStructure.Add("userId", UserName);
            byte[] postData = jsonStructure.ToString().ToByteArray();
            return new MoBackRequest(MoBackURLS.ResetPassword, HTTPMethod.POST, null, postData);
        }
        
        public static MoBackRequest ResetPassword(string userName)
        {
            SimpleJSONClass jsonStructure = new SimpleJSONClass();
            jsonStructure.Add("userId", userName);
            byte[] postData = jsonStructure.ToString().ToByteArray();
            return new MoBackRequest(MoBackURLS.ResetPassword, HTTPMethod.POST, null, postData);
        }
        
        /// <summary>
        /// Deletes the current user.
        /// </summary>
        /// <returns>A MoBackRequest with a MoBackUser type.</returns>
        public MoBackRequest DeleteCurrentUser()
        {
            if (!IsLoggedIn)
            {
                Debug.LogError("This current user hasn't been logged in yet");
                return null;
            }
            
            MoBackRequest.ResponseProcessor deleteProcessor = (SimpleJSONNode jsonObject) =>
            {
                if (jsonObject["code"] == "1000")
                {
                    ObjectId = null;
                    CreatedDate = default(DateTime);
                    UpdatedDate = default(DateTime);
                    
                    IsLoggedIn = false;
                    CompleteSignedUp = false;
                    
                    // Check in storeValues and remove them if any.
                    if (storeValues.ContainsKey("objectId"))
                    {
                        storeValues.Remove("objectId");
                        Debug.Log("remove objectid");
                    }
                    
                    if (storeValues.ContainsKey("createdAt"))
                    {
                        storeValues.Remove("createdAt");
                        Debug.Log("createdAt");
                    }
                    
                    if (storeValues.ContainsKey("updatedAt"))
                    {
                        storeValues.Remove("updatedAt");
                        Debug.Log("createdAt");
                    }
                }
            };

            return new MoBackRequest(deleteProcessor, MoBackURLS.User, HTTPMethod.DELETE, null, null);
        }
        
        /// <summary>
        /// Sends the invitation.
        /// </summary>
        /// <returns>A MoBackRequest with a MoBackUser type.</returns>
        /// <param name="emailAddress">Email address.</param>
        public MoBackRequest SendInvitation(string emailAddress)
        {
            if (string.IsNullOrEmpty(emailAddress))
            {
                Debug.LogError("Invalid email address");
                return null;
            }
            
            SimpleJSONClass jsonStructure = new SimpleJSONClass();
            jsonStructure.Add("inviteeId", emailAddress);
            byte[] postData = jsonStructure.ToString().ToByteArray();
            return new MoBackRequest(MoBackURLS.Invitation, HTTPMethod.POST, null, postData);
        }
        
        /// <summary>
        /// Updates the user info.
        /// </summary>
        /// <returns>A MoBackRequest with a MoBackUser type.</returns>
        public MoBackRequest UpdateUserInfo()
        {   
            if (!IsLoggedIn)
            {
                Debug.LogError("This current user hasn't been logged in yet");
                return null;
            }
            
            SimpleJSONClass jsonStructure = GetJSON();
            
            return new MoBackRequest(MoBackURLS.User, HTTPMethod.PUT, null, jsonStructure.ToString().ToByteArray());
        }
    }
}