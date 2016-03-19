//-----------------------------------------------------------------------
// <copyright file="MoBackGeoPoint.cs" company="moBack">
//     Copyright 2015 moBack Inc. All Rights Reserved.
// </copyright>
//-----------------------------------------------------------------------
using System.Collections;
using UnityEngine;
using SimpleJSON;

namespace MoBack
{
        /// <summary>
        /// Allows the user access to latitude and longitude data. 
        /// Also, gives the user the ability to find the distance between two MoBackGeoPoints.
        /// </summary>
        public class MoBackGeoPoint
        {
                /// <summary>
                /// Gets or sets the longitude.
                /// </summary>
                /// <value>The longitude.</value>
                public double Longitude { set; get; }

                /// <summary>
                /// Gets or sets the latitude.
                /// </summary>
                /// <value>The latitude.</value>
                public double Latitude { set; get; }

                /// <summary>
                /// Measurement units to be used when dealing using the DistanceTo() function.
                /// </summary>
                public enum Measurement
                {
                        kilometers,
                        miles
                }

                /// <summary>
                /// Converts latitude and longitude data from a JSON object to a double value type.
                /// </summary>
                /// <returns>The JSO.</returns>
                /// <param name="node">Node.</param>
                public static MoBackGeoPoint FromJSON(SimpleJSONNode node)
                {
                        double lat = node["lat"].AsDouble;
                        double lon = node["lon"].AsDouble;
                        return new MoBackGeoPoint(lat, lon);
                }

                /// <summary>
                /// Constructor for MoBackGeoPoint.
                /// </summary>
                /// <param name="latitude">Latitude.</param>
                /// <param name="longitude">Longitude.</param>
                public MoBackGeoPoint(double latitude, double longitude)
                {
                        this.Longitude = longitude;
                        this.Latitude = latitude;
                }

                /// <summary>
                /// Returns the JSON object.
                /// </summary>
                /// <returns>The JSO.</returns>
                public SimpleJSONClass GetJSON()
                {
                        SimpleJSONClass jsonObject = new SimpleJSONClass();
                        jsonObject.Add("__type", "GeoPoint");
                        jsonObject.Add("lat", Latitude);
                        jsonObject.Add("lon", Longitude);
                        return jsonObject;
                }

                /// <summary>
                /// Calculates the distance between two MoBackGeoPoints. Will return a value in kilometers or miles as the result.  
                /// </summary>
                /// <returns>The <see cref="System.Double"/>.</returns>
                /// <param name="point2"> A second MoBackGeoPoint latitude and longitude pair. </param>
                /// <param name="option"> Either Measurement.kilometers or Measurement.miles.</param>
                public double DistanceTo(MoBackGeoPoint point2, Measurement option = Measurement.kilometers)
                {
                        double rEarth = 6371; // Radius of Earth in kilometers

                        double rLatitude1 = (Latitude * Mathf.PI) / 180;
                        double rLatitude2 = (point2.Latitude * Mathf.PI) / 180;
                        double aLatitude = (((point2.Latitude - Latitude) * Mathf.PI) / 180) / 2;
                        double bLongitude = (((point2.Longitude - Longitude) * Mathf.PI) / 180) / 2;

                        float c = Mathf.Pow(Mathf.Sin((float)aLatitude), 2) + ((Mathf.Cos((float)rLatitude1) * Mathf.Cos((float)rLatitude2)) * Mathf.Pow(Mathf.Sin((float)bLongitude), 2));

                        double angle = 2 * Mathf.Atan2(Mathf.Sqrt(c), Mathf.Sqrt(1 - c));
                       
                        switch (option) 
                        {
                        case Measurement.kilometers:
                                return Mathf.Round((float)(angle * rEarth) * 10) / 10;
                        case Measurement.miles:
                                return Mathf.Round((float)((angle * rEarth) * 0.621371192) * 10) / 10;
                        default:
                                if (MoBackAppSettings.loggingLevel >= MoBackAppSettings.LoggingLevel.ERRORS) 
                                {
                                        Debug.LogError("Error: Not a valid measurement. Defaulting to kilometers.");
                                } 
                                return Mathf.Round((float)(angle * rEarth) * 10) / 10;
                        }
                }
        }
}