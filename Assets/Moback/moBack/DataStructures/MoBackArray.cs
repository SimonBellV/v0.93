//-----------------------------------------------------------------------
// <copyright file="MoBackArray.cs" company="moBack">
//     Copyright 2015 moBack Inc. All Rights Reserved.
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
    /// Custom container that holds objects and their corresponding MoBackValueType.
    /// </summary>
    public class MoBackArray
    {
        /// <summary>
        /// Containers for the objects and their types.
        /// </summary>
        private List<object> objects;
        private List<MoBackValueType> objectTypes;

        /// <summary>
        /// Initializes a new instance of the <see cref="MoBack.MoBackArray"/> class.
        /// </summary>
        public MoBackArray()
        {
            objects = new List<object>();
            objectTypes = new List<MoBackValueType>();
        }

        /// <summary>
        /// Converts JSON objects into MoBackValueType objects.
        /// </summary>
        /// <returns> A MoBackArray containing converted JSON objects. </returns>
        /// <param name="jsonArray"> A simple JSON Array. </param>
        public static MoBackArray FromJSON(SimpleJSONArray jsonArray)
        {
            MoBackArray mArray = new MoBackArray();
            foreach (SimpleJSONNode value in jsonArray) 
            {
                MoBackValueType moBackType;
                object data = MoBackUtils.MoBackTypedObjectFromJSON(value, out moBackType);
                
                // This should always be equivalent to calling Add() with the appropriate type; if Add() functions are refactored then so too should this.
                mArray.objects.Add(data);
                mArray.objectTypes.Add(moBackType);
            }
            return mArray;
        }

        /// <summary>
        /// Converts MobackValueType objects into JSON objects.
        /// </summary>
        /// <returns> An simple JSON array of JSON objects. </returns>
        public SimpleJSONArray GetJSON()
        {
            SimpleJSONArray jsonArray = new SimpleJSONArray();
            for (int i = 0; i < objects.Count; i++) 
            {
                SimpleJSONNode node = MoBackUtils.MoBackTypedObjectToJSON(objects[i], objectTypes[i]);
                jsonArray.Add(node);
            }
            return jsonArray;
        }

        #region getters and setters
        /// <summary>
        /// Gets a string value from the container.
        /// </summary>
        /// <returns> Returns the string type element at the specified index. </returns>
        /// <param name="index"> An index value. </param>
        public string GetString(int index)
        {
            return (string)objects[index];
        }

        /// <summary>
        /// Gets a bool value from the container.
        /// </summary>
        /// <returns> Returns the bool type element at the specified index. </returns>
        /// <param name="index"> An index value. </param>
        public bool GetBool(int index)
        {
            return (bool)objects[index];
        }

        /// <summary>
        /// Gets an int value from the container.
        /// </summary>
        /// <returns> Returns the int type element at the specified index. </returns>
        /// <param name="index"> An index value. </param>
        public int GetInt(int index)
        {
            return ((MoBackNumber)objects[index]).GetInt();
        }

        /// <summary>
        /// Gets a float value from the container. 
        /// </summary>
        /// <returns> Returns the float type element at the specified index. </returns>
        /// <param name="index"> An index value. </param>
        public float GetFloat(int index)
        {
            return ((MoBackNumber)objects[index]).GetFloat();
        }

        /// <summary>
        /// Gets a double value from the container.
        /// </summary>
        /// <returns> Returns a double type element at the specified index. </returns>
        /// <param name="index"> An index value. </param>
        public double GetDouble(int index)
        {
            return ((MoBackNumber)objects[index]).GetDouble();
        }

        /// <summary>
        /// Gets a DateTime value from the container.
        /// </summary>
        /// <returns> Returns a DateTime type element at the specified index. </returns>
        /// <param name="index"> An index value. </param>
        public DateTime GetDate(int index)
        {
            return (DateTime)objects[index];
        }

        /// <summary>
        /// Gets a MoBackFile value from the container.
        /// </summary>
        /// <returns> Returns a MoBackFile type element at the specified index. </returns>
        /// <param name="index"> An index value. </param>
        public MoBackFile GetFile(int index)
        {
            return (MoBackFile)objects[index];
        }

        /// <summary>
        /// Gets a MoBackObject value from the container.
        /// </summary>
        /// <returns> Returns a MoBackObject type element at the specified index. </returns>
        /// <param name="index"> An index value. </param>
        public MoBackObject GetNestedObject(int index)
        {
            return (MoBackObject)objects[index];
        }

        /// <summary>
        /// Gets a MoBackArray value from the container.
        /// </summary>
        /// <returns> Returns the MoBackArray type element at the specified index. </returns>
        /// <param name="index"> An index value. </param>
        public MoBackArray GetNestedArray(int index)
        {
            return (MoBackArray)objects[index];
        }

        /// <summary>
        /// Gets a MoBackPointer value from the container.
        /// </summary>
        /// <returns> Returns a MoBackPointer type element at the specified index. </returns>
        /// <param name="index"> An index value. </param>
        public MoBackPointer GetPointer(int index)
        {
            return (MoBackPointer)objects[index];
        }

		/// <summary>
		/// Gets relation data from the container.
		/// </summary>
		/// <returns>The relation data.</returns>
		/// <param name="index"> An index value. </param>
		public MoBackRelation GetRelationData (int index) {
			return (MoBackRelation)objects[index];
        }
        
        /// <summary>
        /// Set the specified index and value.
        /// </summary>
        /// <param name="index"> An index value. </param>
        /// <param name="value"> A string value. </param>
        public void Set(int index, string value)
        {
            if (ValidateIndex(index)) 
            {
                objects[index] = value;
                objectTypes[index] = MoBackValueType.String;
            }
            else 
            {
                Debug.LogError("Error: Index is out of range.");
            }
        }
        
        /// <summary>
        /// Set the specified index and value.
        /// </summary>
        /// <param name="index"> An index value. </param>
        /// <param name="value"> A bool value. </param>
        public void Set(int index, bool value)
        {
            if (ValidateIndex(index)) 
            {
                objects[index] = value;
                objectTypes[index] = MoBackValueType.Boolean;
            }
            else 
            {
                Debug.LogError("Error: Index is out of range.");
            }
        }
        
        /// <summary>
        /// Set the specified index and value.
        /// </summary>
        /// <param name="index"> An index value. </param>
        /// <param name="value"> An integer value. </param>
        public void Set(int index, int value)
        {
            if (ValidateIndex(index)) 
            {
                objects[index] = new MoBackNumber(value);
                objectTypes[index] = MoBackValueType.Number;
            }
            else 
            {
                Debug.LogError("Error: Index is out of range.");
            }
        }

        /// <summary>
        /// Set the specified index and value.
        /// </summary>
        /// <param name="index"> An index value. </param>
        /// <param name="value"> A float value. </param>
        public void Set(int index, float value)
        {
            if (ValidateIndex(index)) 
            {
                objects[index] = new MoBackNumber(value);
                objectTypes[index] = MoBackValueType.Number;
            }
            else 
            {
                Debug.LogError("Error: Index is out of range.");
            }
        }

        /// <summary>
        /// Set the specified index and value.
        /// </summary>
        /// <param name="index"> An index value. </param>
        /// <param name="value"> A double value. </param>
        public void Set(int index, double value)
        {
            if (ValidateIndex(index)) 
            {
                objects[index] = new MoBackNumber(value);
                objectTypes[index] = MoBackValueType.Number;
            }
            else 
            {
                Debug.LogError("Error: Index is out of range.");
            }
        }

        /// <summary>
        /// Sets a date value. Time is auto-converted to universal time if not in it already, so be careful to use correct DateTime settings.
        /// </summary>
        /// <param name="index"> An index value. </param>
        /// <param name="value"> The value to set at the index specified. </param>
        public void Set(int index, DateTime value)
        {
            if (ValidateIndex(index)) 
            {
                objects[index] = value.ToUniversalTime();
                objectTypes[index] = MoBackValueType.Date;
            }
            else 
            {
                Debug.LogError("Error: Index is out of range.");
            }
        }
        
        /// <summary>
        /// Set the specified index and value.
        /// </summary>
        /// <param name="index"> An index value. </param>
        /// <param name="value"> A MoBackFile value. </param>
        public void Set(int index, MoBackFile value)
        {
            if (ValidateIndex(index)) 
            {
                objects[index] = value;
                objectTypes[index] = MoBackValueType.File;
            }
            else 
            {
                Debug.LogError("Error: Index is out of range.");
            }
        }

        /// <summary>
        /// Set the specified index and value.
        /// </summary>
        /// <param name="index"> An index value. </param>
        /// <param name="value"> A MoBackObject value. </param>
        public void Set(int index, MoBackObject value)
        {
            if (ValidateIndex(index)) 
            {
                objects[index] = value;
                objectTypes[index] = MoBackValueType.MoBackObject;
            }
            else 
            {
                Debug.LogError("Error: Index is out of range.");
            }
        }

        /// <summary>
        /// Set the specified index and value.
        /// </summary>
        /// <param name="index"> An index value. </param>
        /// <param name="value"> A MoBackArray value. </param>
        public void Set(int index, MoBackArray value)
        {
            if (ValidateIndex(index)) 
            {
                objects[index] = value;
                objectTypes[index] = MoBackValueType.Array;
            }
            else 
            {
                Debug.LogError("Error: Index is out of range.");
            }
        }

        /// <summary>
        /// Set the specified index and value.
        /// </summary>
        /// <param name="index"> An index value. </param>
        /// <param name="value"> A MoBackPointer value. </param>
        public void Set(int index, MoBackPointer value)
        {
            if (ValidateIndex(index)) 
            {
                objects[index] = value;
                objectTypes[index] = MoBackValueType.Pointer;
            }
            else 
            {
                Debug.LogError("Error: Index is out of range.");
            }
        }

        /// <summary>
        /// Add the specified value.
        /// </summary>
        /// <param name="value"> A string value. </param>
        public void Add(string value)
        {
            objects.Add(value);
            objectTypes.Add(MoBackValueType.String);
        }
        
        /// <summary>
        /// Add the specified value.
        /// </summary>
        /// <param name="value"> A bool value. </param>
        public void Add(bool value)
        {
            objects.Add(value);
            objectTypes.Add(MoBackValueType.Boolean);
        }

        /// <summary>
        /// Add the specified value.
        /// </summary>
        /// <param name="value"> An integer value. </param>
        public void Add(int value)
        {
            objects.Add(new MoBackNumber(value));
            objectTypes.Add(MoBackValueType.Number);
        }

        /// <summary>
        /// Add the specified value.
        /// </summary>
        /// <param name="value"> A float value. </param>
        public void Add(float value)
        {
            objects.Add(new MoBackNumber(value));
            objectTypes.Add(MoBackValueType.Number);
        }

        /// <summary>
        /// Add the specified value.
        /// </summary>
        /// <param name="value"> A double value. </param>
        public void Add(double value)
        {
            objects.Add(new MoBackNumber(value));
            objectTypes.Add(MoBackValueType.Number);
        }

        /// <summary>
        /// Sets a date value. Time is auto-converted to universal time if not in it already, so be careful to use correct DateTime settings.
        /// </summary>
        /// <param name="value"> The DateTime value. </param>
        public void Add(DateTime value)
        {
            objects.Add(value.ToUniversalTime());
            objectTypes.Add(MoBackValueType.Date);
        }

        /// <summary>
        /// Add a MoBackFile type object to the container.
        /// </summary>
        /// <param name="value"> A MoBackFile value. </param>
        public void Add(MoBackFile value)
        {
            objects.Add(value);
            objectTypes.Add(MoBackValueType.File);
        }

        /// <summary>
        /// Adds a MoBackObject type object to the container.
        /// </summary>
        /// <param name="value"> A MoBackObject value. </param>
        public void Add(MoBackObject value)
        {
            objects.Add(value);
            objectTypes.Add(MoBackValueType.MoBackObject);
        }

        /// <summary>
        /// Adds a MoBackArray type value to the container.
        /// </summary>
        /// <param name="value"> A MoBackArray value. </param>
        public void Add(MoBackArray value)
        {
            objects.Add(value);
            objectTypes.Add(MoBackValueType.Array);
        }

        /// <summary>
        /// Adds a MoBackPointer type value to the container.
        /// </summary>
        /// <param name="value"> A MoBackPointer value. </param>
        public void Add(MoBackPointer value)
        {
            objects.Add(value);
            objectTypes.Add(MoBackValueType.Pointer);
        }

        #endregion

        /// <summary>
        /// Check a container for a matching value. Return true if found. Return false if not found.
        /// </summary>
        /// <param name="value"> The object value to search the container for. </param>
        /// <returns> A bool value. </returns>
        public bool Contains(object value)
        {
            if (GetIndex(value) >= 0) 
            { 
                return true; 
            } 

            return false;
        }

        /// <summary>
        /// Removes a specified value based on index from the container.
        /// </summary>
        /// <param name="index"> The index of the value that needs to be removed. </param>
        public void RemoveAt(int index)
        {
            if (GetIndex(objects[index]) >= 0 && (index >= 0 && index <= objects.Count)) 
            {
                objects.Remove(objects[index]);
                objectTypes.Remove(objectTypes[index]);
            }
        }

        /// <summary>
        /// Removes a specified value from the container.
        /// </summary>
        /// <param name="value"> The specified value that needs to be removed. </param>
        public void Remove(object value)
        {
            int index = GetIndex(value);

            if (index >= 0) 
            {
                objects.Remove(objects[index]);
                objectTypes.Remove(objectTypes[index]);
            } 
            else 
            {
                if (MoBackAppSettings.loggingLevel >= MoBackAppSettings.LoggingLevel.ERRORS) 
                {
                    Debug.LogError(String.Format("Error: No matching value for {0}.", value));
                }
                return;
            }
        }

        /// <summary>
        /// Replaces the value at the specified index with a new value.
        /// </summary>
        /// <param name="index"> The index of the value that needs to be replaced. </param>
        /// <param name="value"> The value to replace the old value with at the specified index. </param>
        public void ReplaceAt(int index, object value)
        {
            if (GetIndex(objects[index]) >= 0 && (index >= 0 && index <= objects.Count)) 
            {
                if (value.GetType() == typeof(DateTime)) 
                {
                    objects[index] = ((DateTime)value).ToUniversalTime();
                } 
                else 
                {
                    objects[index] = value;
                }
                objectTypes[index] = Convert(value);
            } 
            else 
            {
                if (MoBackAppSettings.loggingLevel >= MoBackAppSettings.LoggingLevel.ERRORS) 
                {
                    Debug.LogError("Error: Index out of range.");
                }
                return;
            }
        }

        /// <summary>
        /// Replaces the specified value (value1) with a new value (value2).
        /// </summary>
        /// <param name="value1"> The value to be replaced. </param>
        /// <param name="value2"> The value to replace the first value with. </param>
        public void Replace(object value1, object value2)
        {
            int index = GetIndex(value1);

            if (index >= 0) 
            {
                if (value2.GetType() == typeof(DateTime))
                {
                    objects[index] = ((DateTime)value2).ToUniversalTime();
                }
                else
                {
                    objects[index] = value2;
                }
                objectTypes[index] = Convert(value2);
            } 
            else 
            {
                if (MoBackAppSettings.loggingLevel >= MoBackAppSettings.LoggingLevel.ERRORS) 
                {
                    Debug.LogError(String.Format("Error: No matching value for {0}.", value1));
                }
                return;
            }
        }

        /// <summary>
        /// Returns the number of elements contained in objects list.
        /// </summary>
        /// <returns> An integer value. </returns>
        public int Count()
        {
            return objects.Count;
        }

        /// <summary>
        /// Returns whether the index is valid.
        /// </summary>
        /// <returns><c>true</c>, if index is valid, <c>false</c> otherwise.</returns>
        /// <param name="index"> An index value. </param>
        private bool ValidateIndex(int index)
        {
            if (index >= 0 && index <= objects.Count)
            {
                return true;
            }
            else 
            {
                return false;
            }
        }

        /// <summary>
        /// Finds the matching value. To be used with the Remove, Replace, and Contains functions.
        /// </summary>
        /// <param name="value"> Any user specified object that may or may not be contained in _objects. </param>
        /// <returns> Returns the index of an object that matches the user specified value. </returns>
        private int GetIndex(object value)
        {
            for (int i = 0; i < objects.Count; i++) 
            {
                if (objects[i].GetType() == typeof(MoBackNumber)) 
                {
                    if (objects[i].Equals(value) || ((MoBackNumber)objects[i]).InternalValue().Equals(value))
                    {
                        return i;
                    }
                } 
                else if (objects[i].GetType() == typeof(DateTime) && value.GetType() == typeof(DateTime)) 
                {
                    if (objects[i].Equals(((DateTime)value).ToUniversalTime()))
                    {
                        return i;
                    }
                } 
                else 
                {
                    if (objects[i].Equals((object)value)) 
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        /// <summary>
        /// Convert the type of the specified value to a storable MobackValueType.
        /// </summary>
        /// <param name="value"> A value that needs to be converted into a MoBack friendly value type. </param>
        /// <returns> Returns the MoBackValueType equivalent to the actual type. </returns>
        private MoBackValueType Convert(object value)
        {
            switch (value.GetType().ToString()) 
            {
            case "System.String":
                return MoBackValueType.String;
            case "System.Boolean":
                return MoBackValueType.Boolean;
            case "System.DateTime": 
                return MoBackValueType.Date;
            case "MoBack.MoBackFile":
                return MoBackValueType.File;
            case "MoBack.MoBackPointer":
                return MoBackValueType.Pointer;
            case "MoBack.MoBackObject":
                return MoBackValueType.MoBackObject;
            case "MoBack.MoBackArray":
                return MoBackValueType.Array;
            default:
                return MoBackValueType.Number;
            }
        }
    }
}