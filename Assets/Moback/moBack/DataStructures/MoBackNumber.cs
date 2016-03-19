//-----------------------------------------------------------------------
// <copyright file="MoBackNumber.cs" company="moBack"> 
//     Copyright 2015 moBack Inc. All Rights Reserved.
// </copyright>
//-----------------------------------------------------------------------
using System.Collections;
using UnityEngine;
using SimpleJSON;

namespace MoBackInternal
{
    /// <summary>
    /// Class that converts types to MoBack's abstraction of all numeric types to a "Number".
    /// </summary>
    public class MoBackNumber
    {
        /// <summary> Types of NumberFormats. </summary>
        public enum NumberFormat
        {
            INT,
            FLOAT,
            DOUBLE
        }

        private NumberFormat leastPreciseType;

        private int intVal;
        private float floatVal;
        private double doubleVal;

        /// <summary>
        /// Initializes a new instance of the MoBackNumber class.
        /// </summary>
        /// <param name="number"> A int value. </param>
        public MoBackNumber(int number)
        {
            intVal = number;
            leastPreciseType = NumberFormat.INT;
        }

        /// <summary>
        /// Initializes a new instance of the MoBackNumber class.
        /// </summary>
        /// <param name="number"> A float value. </param>
        public MoBackNumber(float number)
        {
            floatVal = number;
            leastPreciseType = NumberFormat.FLOAT;
        }

        /// <summary>
        /// Initializes a new instance of the MoBackNumber class.
        /// </summary>
        /// <param name="number"> A double value. </param>
        public MoBackNumber(double number)
        {
            doubleVal = number;
            leastPreciseType = NumberFormat.DOUBLE;
        }

        /// <summary> Converts the specified value into a MoBackNumber. </summary>
        /// <param name="value"> An int type value. </param>
        /// <returns> The value as a MoBackNumber. </returns>
        static public implicit operator MoBackNumber(int value)
        {
            return new MoBackNumber(value);
        }

        /// <summary>
        /// Converts the MoBackNumber into JSON objects.
        /// </summary>
        /// <returns> A JSON object. </returns>
        public SimpleJSONData GetJSON()
        {
            switch (leastPreciseType) 
            {
            case NumberFormat.INT:
                return (SimpleJSONData)intVal;
            case NumberFormat.FLOAT:
                return (SimpleJSONData)floatVal;
            case NumberFormat.DOUBLE:
                return (SimpleJSONData)doubleVal;
            default:
                Debug.LogError("Somehow fallen through case statements in a manner that shouldn't be possible");
                return (SimpleJSONData)0;
            }
        }

        /// <summary>
        /// Provides access to the actual number type of the MoBackNumber.
        /// </summary>
        /// <returns> The int, float, or double value of the MoBackNumber. </returns>
        public object InternalValue()
        {
            switch (leastPreciseType) 
            {
            case NumberFormat.INT:
                return (object)intVal;
            case NumberFormat.FLOAT:
                return (object)floatVal;
            case NumberFormat.DOUBLE:
                return (object)doubleVal;
            default:
                Debug.LogError("Somehow fallen through case statements in a manner that shouldn't be possible");
                return (object)0;
            }
        }

        /// <summary>
        /// Gets int value.
        /// </summary>
        /// <returns> An int type value. </returns>
        public int GetInt()
        {
            switch (leastPreciseType) 
            {
            case NumberFormat.INT:
                return intVal;
            case NumberFormat.FLOAT:
                if (MoBack.MoBackAppSettings.loggingLevel >= MoBack.MoBackAppSettings.LoggingLevel.WARNINGS) 
                {
                    if (floatVal != Mathf.Floor(floatVal)) 
                    {
                        Debug.LogWarning("Loss of data in casting numeric value from float to integer when extracting from MoBackObject or MoBackRow");
                    }
                }
                return (int)floatVal;
            case NumberFormat.DOUBLE:
                if (MoBack.MoBackAppSettings.loggingLevel >= MoBack.MoBackAppSettings.LoggingLevel.WARNINGS) 
                {
                    if (doubleVal != System.Math.Floor(doubleVal)) 
                    {
                        Debug.LogWarning("Loss of data in casting numeric value from double to integer when extracting from MoBackObject or MoBackRow");
                    }
                }
                return (int)doubleVal;
            }

            Debug.LogError("Somehow fallen through case statements in a manner that shouldn't be possible");
            return 0; 
        }

        /// <summary>
        /// Gets a float value.
        /// </summary>
        /// <returns> A float type value. </returns>
        public float GetFloat()
        {
            switch (leastPreciseType) 
            {
            case NumberFormat.INT:
                if (MoBack.MoBackAppSettings.loggingLevel >= MoBack.MoBackAppSettings.LoggingLevel.WARNINGS) 
                {
                    if (((float)intVal) != intVal) 
                    {
                        Debug.LogWarning("Loss of data in casting numeric value from integer to float when extracting from MoBackObjec or MoBackRowt");
                    }
                }
                return (float)intVal;
            case NumberFormat.FLOAT:
                return floatVal;
            case NumberFormat.DOUBLE:
                // Note that warning against precision loss in the case of a dobule is basically impossible.
                return (float)doubleVal;
            }

            Debug.LogError("Somehow fallen through case statements in a manner that shouldn't be possible");
            return 0; 
        }

        /// <summary>
        /// Gets the double value.
        /// </summary>
        /// <returns> A double type value. </returns>
        public double GetDouble()
        {
            switch (leastPreciseType) 
            {
            case NumberFormat.INT:
                return (double)intVal;
            case NumberFormat.FLOAT:
                return (double)floatVal;
            case NumberFormat.DOUBLE:
                return doubleVal;
            }

            Debug.LogError("Somehow fallen through case statements in a manner that shouldn't be possible");
            return 0; 
        }
    }
}
