//-----------------------------------------------------------------------
// <copyright file="DemoRequestManager.cs" company="moBack">   
// Copyright 2015 moBack Inc. All Rights Reserved.
// </copyright>
//-----------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using MoBack;
using UnityEngine;

/// <summary>
/// Handles all the requests and responses made to and from the server.
/// </summary>
public class DemoRequestManager : MonoBehaviour
{
        /// <summary>
    /// The table declaration of a MoBackTableInterface.
    /// </summary>
    private MoBackTableInterface table;

    /// <summary>
    /// The array declaration of a MoBackArray.
    /// </summary>
    private MoBackArray nArray;

    /// <summary>
    /// The stored value to store the value of the string pushed in array.
    /// </summary>
    private string storedValue;

    /// <summary>
    /// Reference to the MenuHandlerDemo script.
    /// </summary>
    private DemoMenuHandler menuHandler;
    
    /// <summary>
    /// Start this instance.
    /// </summary>
    /// <returns>A Unity coroutine IEnumerator.</returns>
    public IEnumerator Start()
    {
        nArray = new MoBackArray();
        MoBackRow newRow1 = new MoBackRow("TestMoback");

        // Create a new row for a new user with a score of 0 to start.
        newRow1.SetValue("Name", "MoBackUser");
        newRow1.SetValue("Score", 0);

        // Send the data to the table and wait for a response.
        MoBackRequest response = newRow1.Save();
        yield return response.Run();

        // If the response to the request is an error, output the error details.
        if (response.State == MoBackRequest.RequestState.ERROR)
        {
            Debug.LogError(response.errorDetails);
        }

        if (menuHandler == null) 
        {
            menuHandler = FindObjectOfType<DemoMenuHandler>();
        }
    }

    /// <summary>
    /// Gets the score and Debug.Logs it in the console as well as displays it in DisplayText.
    /// </summary>
    /// <returns>A Unity coroutine IEnumerator.</returns>
    public IEnumerator FetchScore()
    {
        // Create a variable to hold the recieved results from the server.
        string score = null;

        // Fetch data from the specified table and send the request with a specific parameter of EqualTo(), which will only return 
        // values that are equal to "MoBackUser" under the column of "Name".
        table = new MoBackTableInterface("TestMoback");
        MoBackRequest<List<MoBackRow>> tableContentsFetch = table.GetRows(new MoBackRequestParameters().EqualTo("Name", "MoBackUser"));
        yield return tableContentsFetch.Run();

        // If the fetch request is successful, display the data.
        if (tableContentsFetch.State == MoBackRequest.RequestState.COMPLETED) 
        {
            List<MoBackRow> rows = tableContentsFetch.ResponseValue;
            for (int i = 0; i < rows.Count; i++)
            {
                if (rows[i].GetString("Name") == "MoBackUser")
                { 
                    // Display the score in the debug console and in the DisplayText text object in the scene.
                    score = rows[i].GetInt("Score").ToString();
                    Debug.Log("Score: " + score);
                    menuHandler.Display("Score: " + score.ToString());
                }
            }
        }
        else
        {
            Debug.Log(tableContentsFetch.errorDetails);
        }
    }

    /// <summary>
    /// Updates an existing row with a new score and notifies the user when successful.
    /// </summary>
    /// <returns>A Unity coroutine IEnumerator.</returns>
    /// <param name="score">Score to submit.</param>
    public IEnumerator SubmitScore(int score)
    {
        // Create a new table called "TestMoBack" if it hasn't already been created; otherwise, specify which table you want to work with.
        table = new MoBackTableInterface("TestMoback");

        // Send a request for data to the server with a specific set of parameters.
        MoBackRequest<List<MoBackRow>> tableContentsFetch = table.GetRows(new MoBackRequestParameters().EqualTo("Name", "MoBackUser"));
        yield return tableContentsFetch.Run();

        // Once the request has been completed, manipulate the fetched data.
        if (tableContentsFetch.State == MoBackRequest.RequestState.COMPLETED) 
        {
            List<MoBackRow> rows = tableContentsFetch.ResponseValue;

            // Update the last item in the table to a new score and send to the server.
            MoBackRow newRow = rows[rows.Count - 1];
            newRow.SetValue("Score", score);
            MoBackRequest response = newRow.Save();

            // Wait for a response before displaying that the request was successful.
            yield return response.Run();

            if (response.State == MoBackRequest.RequestState.COMPLETED) 
            {
                menuHandler.Display("Score submitted successfully.");
            } 
            else 
            {
                menuHandler.Display("Request was not successful. Please double check your app ID and/or environment key.");

                // Display an error should one occur.
                Debug.Log(response.errorDetails);
            }
        } 
        else 
        {
            // Display an error should one occur.
            Debug.Log(tableContentsFetch.errorDetails);
        }
    }

    /// <summary>
    /// Gets the distance between two MoBackGeoPoints and displays the distance in miles to the user.
    /// </summary>
    /// <param name="x1">The first latitude value.</param>
    /// <param name="y1">The first longitude value.</param>
    /// <param name="x2">The second latitude value.</param>
    /// <param name="y2">The second longitude value.</param>
    public void DisplayDistance(float x1, float y1, float x2, float y2)
    {
        // Create MoBackGeoPoint objects from the InputField data.
        MoBackGeoPoint firstPoint = new MoBackGeoPoint(x1, y1);
        MoBackGeoPoint secondPoint = new MoBackGeoPoint(x2, y2); 

        // Output the distance from the first point to the second point while specifying the measurement units to use as miles.
        Debug.Log(firstPoint.DistanceTo(secondPoint, MoBackGeoPoint.Measurement.miles)); 
        menuHandler.Display(string.Format("Distance: {0} miles.", firstPoint.DistanceTo(secondPoint, MoBackGeoPoint.Measurement.miles)));
    }

    /// <summary>
    /// Adds to the array and displays the value added to the array in addition to the current array size.
    /// </summary>
    /// <param name="val">String type value to be added to the array.</param>
    public void AddToArray(string val)
    {
        if (!string.IsNullOrEmpty(val)) 
        {
            nArray.Add(val); 
            menuHandler.Display(string.Format("Value {0} added to array sucessfully. Array size: {1}.", val, nArray.Count()));
            storedValue = val;
        }
        else 
        {
            menuHandler.Display("Please enter a valid input. Fields cannot be left empty.");    
        }
    }

    /// <summary>
    /// Replaces value at the end of the array and display what value was added to the very end of the array. Also, displays the current array size.
    /// </summary>
    /// <param name = "val">String type value to replace the last element with.</param>
    public void ReplaceAt(string val)
    {
        if (!string.IsNullOrEmpty(val)) 
        {
            if (nArray.Count() > 0)
            {
                nArray.ReplaceAt(nArray.Count() - 1, val);
                menuHandler.Display(string.Format("Value {0} replaced {1} in the array. Array size: {2}.", val, storedValue, nArray.Count()));
                storedValue = val;
            }
            else
            { 
                menuHandler.Display("Array Count is 0.");
            }
        }
        else 
        {
            menuHandler.Display("Please enter a valid input. Fields cannot be left empty.");   
        }
    }

    /// <summary>
    /// Replaces the last value entered with a new value. Displays the most recently entered value replaced with a new value.
    /// </summary>
    /// <param name="val">String type value to replace the most recently entered value with.</param>
    public void Replace(string val)
    {
        if (!string.IsNullOrEmpty(val)) 
        {
            if (nArray.Count() > 0) 
            {
                menuHandler.Display(string.Format("{0} replaced by {1}.", storedValue, val));
                nArray.Replace(storedValue, val);
                storedValue = val;
            } 
            else 
            { 
                menuHandler.Display("Array Count is 0.");
            }
        }
        else 
        {
            menuHandler.Display("Please enter a valid input. Fields cannot be left empty.");
        }
    }
        
//    // This method is called in DemoMenuHandler, when tap on the button "InviteFriend" (sorry for my laziness :)).
//    // Make sure to Log in first.
//    public IEnumerator UploadFile(string fileName)
//    {
//        // Full request body:
//        /*
//        PUT /filemanager/api/files/upload HTTP/1.1
//        Host: api.moback.com
//        X-Moback-Application-Key: NTcyYWI3YzktMThhNS00ZDkxLWExNDItODYwYzc4ZTk2NGY3
//        X-Moback-SessionToken-Key: Vm8zaVdlU3dwMjN0QzJPbQ==
//        Content-Type: multipart/form-data; boundary=--MoBackFormData{RandomNumber}
//        Cache-Control: no-cache
//        Postman-Token: 1ca86fca-4cae-a585-923e-f82d1991e695
//        
//        --MoBackFormData{RandomNumber}
//        Content-Disposition: form-data; name="file"; filename="image.png"
//        Content-Type:
//        
//        --MoBackFormData{RandomNumber}
//        */
//    
//        // Get Path from StreamingAssets. Make sure to place an Image in StreamingAssets folder.
//        // StreamingAssets folder is a special folder, all files in here will be port to the final app without compression. You can read more on Unity.
//        string path = System.IO.Path.Combine(Application.streamingAssetsPath, fileName);
//        
//        // Get the end point to upload the file.
//        string url = MoBackInternal.MoBackURLS.FileUpload;
//        
//        // Construct boundary and content type. Use Guild to generate a random number attached to the form. Not sure if we need it.
//        string moBackFormatDataBoundary = string.Format("--MoBackFormData{0:N}", System.Guid.NewGuid());
//        
//        // Construct a ContentType for the request. It's not only multipart/form-data, but also need the boundary append after it.
//        string uploadRequestContentType = string.Format("multipart/form-data; boundary={0}", moBackFormatDataBoundary);
//        
//        // Convert the file to byte array by passing the file path to it.
//        byte[] uploadFile = System.IO.File.ReadAllBytes(path);
//        
//        // Create a new Stream to write a new file header.
//        System.IO.Stream formStream = new System.IO.MemoryStream();
//        
//        // Create encoding type, not sure what this for, but we need it
//        System.Text.Encoding encoding = System.Text.Encoding.UTF8;
//        
//        // Construct a header for the upload file:
//        // Result: 
//        /*
//            --MoBackFormData{RandomNumber}
//            Content-Disposition: form-data; name="file"; filename="image.png"
//            Content-Type:
//        */
//        // Note: we do need the Content-Disposition. I'm not sure about the Content-Type, I left it blank and it still works.
//        string fileUploadheader = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"; filename=\"{2}\"\r\nContent-Type: {3}\r\n\r\n",
//                                                moBackFormatDataBoundary,
//                                                "file",
//                                                fileName,
//                                                "");
//        
//        // Write the header to stream.
//        formStream.Write(encoding.GetBytes(fileUploadheader), 0, encoding.GetByteCount(fileUploadheader));
//        
//        // Write the upload file to stream
//        formStream.Write (uploadFile, 0, uploadFile.Length);
//        
//        // Create footer for the upload file.
//        string fileUploadFooter = string.Format("\r\n--{0}--\r\n", moBackFormatDataBoundary);
//        
//        // Write the footer to stream
//        formStream.Write(encoding.GetBytes(fileUploadFooter), 0, encoding.GetByteCount(fileUploadFooter));
//        
//        // Construct a new byte array after finish append header and footer.
//        formStream.Position = 0;
//        byte[] postData = new byte[formStream.Length];
//        formStream.Read(postData, 0 , postData.Length);
//        formStream.Close();
//        
//        MoBackRequest response = new MoBackRequest(url, MoBackInternal.HTTPMethod.PUT, null, postData, uploadRequestContentType);
//        yield return response.Run();
//        if (response.State == MoBackRequest.RequestState.ERROR) 
//        {
//            Debug.Log("Response error  " + response.errorDetails);
//        }
//       
//    }
}
