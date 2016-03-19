//-----------------------------------------------------------------------
// <copyright file="MoBackObject.cs" company="moBack"> 
//     Copyright 2015 moBack Inc. All Rights Reserved.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using MoBackInternal;

namespace MoBack
{
    /// <summary>
    /// Object that store data as key-value pairs.
    /// </summary>
    public class MoBackObject
    {
        /// <summary>
        /// Gets a list of MoBackColumns.
        /// </summary>
        /// <value> Returns a list of current MoBackColumns. </value>
        public IList<MoBackColumn> ColumnDefinitions 
        {
            get 
            {
                return columnTypeDataAsColumnDefinitions.AsReadOnly();
            }
        }

        protected Dictionary<string, object> storeValues;
        protected Dictionary<string, MoBackValueType> columnTypeData;
        protected List<MoBackColumn> columnTypeDataAsColumnDefinitions;

        /// <summary>
        /// Initializes a new instance of the MoBackObject class.
        /// </summary>
        public MoBackObject()
        {
            storeValues = new Dictionary<string, object>();
            columnTypeData = new Dictionary<string, MoBackValueType>();
            columnTypeDataAsColumnDefinitions = new List<MoBackColumn>();
        }

        /// <summary>
        /// Converts a JSON object to a MobackObject.
        /// </summary>
        /// <returns> A MoBackObject. </returns>
        /// <param name="jsonObject"> A JSON object. </param>
        public static MoBackObject FromJSON(SimpleJSONClass jsonObject)
        {
            MoBackObject mObject = new MoBackObject();
            ProcessJsonIntoMoBackObject(mObject, jsonObject);
            return mObject;
        }

        /// <summary>
        /// Determines whether this instance has value for the specified key.
        /// </summary>
        /// <returns><c>true</c> if this instance has value for the specified key; otherwise, <c>false</c>.</returns>
        /// <param name="key"> A key value as a string type. </param>
        public bool HasValueFor(string key)
        {
            return storeValues.ContainsKey(key);
        }

        /// <summary>
        /// Gets the type of the column at the specified key.
        /// </summary>
        /// <returns> The column type at the key value. </returns>
        /// <param name="key"> A key value as a string type. </param>
        public MoBackValueType GetColumnType(string key)
        {
            return columnTypeData[key];
        }

        /// <summary>
        /// Converts MoBackObject data to a JSON object for storage.
        /// </summary>
        /// <returns> A JSON object. </returns>
        public virtual SimpleJSONClass GetJSON()
        {
            SimpleJSONClass jsonStructure = new SimpleJSONClass();

            foreach (KeyValuePair<string, object> item in storeValues) 
            {
                SimpleJSONNode node = MoBackUtils.MoBackTypedObjectToJSON(item.Value, columnTypeData[item.Key]);
                jsonStructure.Add(item.Key, node);
            }
            return jsonStructure;
        }

        #region Contents getters and setters
        /// <summary>
        /// Gets the object value at the specified key.
        /// </summary>
        /// <returns> Returns the value at the specified key. </returns>
        /// <param name="key"> The key. </param>
        public object GetValue(string key)
        {
            switch (columnTypeData[key]) 
            {
            case MoBackValueType.Number:
                return ((MoBackNumber)storeValues[key]).InternalValue();
            default:
                return storeValues[key];
            }
        }

        /// <summary>
        /// Gets the string value at the specified key.
        /// </summary>
        /// <returns> Returns the value at the specified key. </returns>
        /// <param name="key"> The key. </param>
        public string GetString(string key)
        {
            if (storeValues.ContainsKey(key)) 
            {
                return (string)storeValues[key];
            } 
            else 
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the string value at the specified key.
        /// </summary>
        /// <returns> Returns the value at the specified key. </returns>
        /// <param name="key"> The key. </param>
        public bool GetBool(string key)
        {
            if (storeValues.ContainsKey(key)) 
            {
                return (bool)storeValues[key];
            } 
            else 
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the string value at the specified key.
        /// </summary>
        /// <returns> Returns the value at the specified key. </returns>
        /// <param name="key"> The key. </param>
        public int GetInt(string key)
        {
            if (storeValues.ContainsKey(key)) 
            {
                return ((MoBackNumber)storeValues[key]).GetInt();
            } 
            else 
            {
                return 0;
            }
        }

        /// <summary>
        /// Gets the string value at the specified key.
        /// </summary>
        /// <returns> Returns the value at the specified key. </returns>
        /// <param name="key"> The key. </param>
        public float GetFloat(string key)
        {
            if (storeValues.ContainsKey(key))
            {
                return ((MoBackNumber)storeValues[key]).GetFloat();
            } 
            else 
            {
                return 0f;
            }
        }

        /// <summary>
        /// Gets the string value at the specified key.
        /// </summary>
        /// <returns> Returns the value at the specified key. </returns>
        /// <param name="key"> The key. </param>
        public double GetDouble(string key)
        {
            if (storeValues.ContainsKey(key))
            {
                return ((MoBackNumber)storeValues[key]).GetDouble();
            } 
            else 
            {
                return 0.0;
            }
        }

        /// <summary>
        /// Gets the string value at the specified key.
        /// </summary>
        /// <returns> Returns the value at the specified key. </returns>
        /// <param name="key"> The key. </param>
        public DateTime GetDate(string key)
        {
            if (storeValues.ContainsKey(key)) 
            {
                return (DateTime)storeValues[key];
            } 
            else 
            {
                return new DateTime(0);
            }
        }

        /// <summary>
        /// Gets the string value at the specified key.
        /// </summary>
        /// <returns> Returns the value at the specified key. </returns>
        /// <param name="key"> The key. </param>
        public MoBackFile GetFile(string key)
        {
            if (storeValues.ContainsKey(key)) 
            {
                return (MoBackFile)storeValues[key];
            } 
            else 
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the string value at the specified key.
        /// </summary>
        /// <returns> Returns the value at the specified key. </returns>
        /// <param name="key"> The key. </param>
        public MoBackObject GetNestedObject(string key)
        {
            if (storeValues.ContainsKey(key)) 
            {
                return (MoBackObject)storeValues[key];
            } 
            else 
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the string value at the specified key.
        /// </summary>
        /// <returns> Returns the value at the specified key. </returns>
        /// <param name="key"> The key. </param>
        public MoBackArray GetNestedArray(string key)
        {
            if (storeValues.ContainsKey(key)) 
            {
                return (MoBackArray)storeValues[key];
            } 
            else 
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the string value at the specified key.
        /// </summary>
        /// <returns> Returns the value at the specified key. </returns>
        /// <param name="key"> The key. </param>
        public MoBackPointer GetPointer(string key)
        {
            if (storeValues.ContainsKey(key)) 
            {
                return (MoBackPointer)storeValues[key];
            } 
            else 
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the relation data at the specified key.
        /// Knwon Issue: the method will return null if you don't call GetRow({objectID that has a relation}) first.
        /// Reason: The response JSON, which is receiveed after adding a relation, doesn't show the added pointers.
        /// We can create an inline method in MoBackRow.AddRelation to store the pass-in pointers array to storesvalue, but i don't think it's a good idea.
        /// Currently, if users want to get the relationdata without calling GetRow, they can create a MoBackRequest by using: GetAllPointerObjects(string key);
        /// </summary>
        /// <returns> The relation data. </returns>
        /// <param name="key"> The key. </param>
        public MoBackRelation GetRelationData(string key) 
        {
            if (storeValues.ContainsKey(key))
            {
                return (MoBackRelation)storeValues[key];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Sets the value with a string element at the specified key.
        /// </summary>
        /// <param name="key"> The key to be set. </param>
        /// <param name="value"> The value to set. </param>
        public void SetValue(string key, string value)
        {
            storeValues[key] = value;
            SetColumnType(key, MoBackValueType.String);
        }

        /// <summary>
        /// Sets the value with a bool element at the specified key.
        /// </summary>
        /// <param name="key"> The key to be set. </param>
        /// <param name="value"> The value to set. </param>
        public void SetValue(string key, bool value)
        {
            storeValues[key] = value;
            SetColumnType(key, MoBackValueType.Boolean);
        }

        /// <summary>
        /// Sets the value with an int element at the specified key.
        /// </summary>
        /// <param name="key"> The key to be set. </param>
        /// <param name="value"> The value to set. </param>
        public void SetValue(string key, int value)
        {
            storeValues[key] = new MoBackNumber(value);
            SetColumnType(key, MoBackValueType.Number);
        }

        /// <summary>
        /// Sets the value with a float element at the specified key.
        /// </summary>
        /// <param name="key"> The key to be set. </param>
        /// <param name="value"> The value to set. </param>
        public void SetValue(string key, float value)
        {
            storeValues[key] = new MoBackNumber(value);
            SetColumnType(key, MoBackValueType.Number);
        }

        /// <summary>
        /// Sets the value with a double element at the specified key.
        /// </summary>
        /// <param name="key"> The key to be set. </param>
        /// <param name="value"> The value to set. </param>
        public void SetValue(string key, double value)
        {
            storeValues[key] = new MoBackNumber(value);
            SetColumnType(key, MoBackValueType.Number);
        }

        /// <summary>
        /// Sets a date value. Time is auto-converted to universal time if not in it already, so be careful to use correct DateTime settings.
        /// </summary>
        /// <param name="key"> The key that you want to set the value for. </param>
        /// <param name="value"> The value that you want to set. </param>
        public void SetValue(string key, DateTime value)
        {
            storeValues[key] = value.ToUniversalTime();
            SetColumnType(key, MoBackValueType.Date);
        }

        /// <summary>
        /// Sets the value with a MoBackFile element at the specified key.
        /// </summary>
        /// <param name="key"> The key to be set. </param>
        /// <param name="value"> The value to set. </param>
        public void SetValue(string key, MoBackFile value)
        {
            storeValues[key] = value;
            SetColumnType(key, MoBackValueType.File);
        }

        /// <summary>
        /// Sets the value with a MoBackObject element at the specified key.
        /// </summary>
        /// <param name="key"> The key to be set. </param>
        /// <param name="value"> The value to set. </param>
        public void SetValue(string key, MoBackObject value)
        {
            storeValues[key] = value;
            SetColumnType(key, MoBackValueType.MoBackObject);
        }

        /// <summary>
        /// Sets the value with a MoBackArray element at the specified key.
        /// </summary>
        /// <param name="key"> The key to be set. </param>
        /// <param name="value"> The value to set. </param>
        public void SetValue(string key, MoBackArray value)
        {
            storeValues[key] = value;
            SetColumnType(key, MoBackValueType.Array);
        }

        /// <summary>
        /// Sets the value with a MoBackPointer element at the specified key.
        /// </summary>
        /// <param name="key"> The key to be set. </param>
        /// <param name="value"> The value to set. </param>
        public void SetValue(string key, MoBackPointer value)
        {
            storeValues[key] = value;
            SetColumnType(key, MoBackValueType.Pointer);
        }
        #endregion

        /// <summary>
        /// Processes a JSON object into a MoBackObject.
        /// </summary>
        /// <param name="mObject"> A MoBackObject to process. </param>
        /// <param name="jsonObject"> A JSON object. </param>
        protected static void ProcessJsonIntoMoBackObject(MoBackObject mObject, SimpleJSONClass jsonObject)
        {
            foreach (KeyValuePair<string, SimpleJSONNode> entry in jsonObject.dict) 
            {
                // These 2 values are in Relation array values when RetrieveRelation with ?include={ColumnName}
                // key == success is for BatchProcessing, after saving/updating, success is auto generated and return in json.
                if (entry.Key == "__type" || entry.Key == "className" || entry.Key == "success")
                {
                    continue;
                }
                
                MoBackValueType moBackType;
                object data = MoBackUtils.MoBackTypedObjectFromJSON(entry.Value, out moBackType);
                
                // This should always be equivalent to calling SetValue() with the appropriate type; if SetValue() functions are refactored then so too should this, and vice-versa.
                mObject.SetColumnType(entry.Key, moBackType);
                mObject.storeValues[entry.Key] = data;
            }
        }
        
        /// <summary>
        /// Completes a shallow copy of elements in a MoBackObject.
        /// </summary>
        /// <param name="copyFrom"> The MoBackObject to copy from. </param>
        protected void ShallowCopy(MoBackObject copyFrom)
        {
            storeValues = copyFrom.storeValues;
            columnTypeData = copyFrom.columnTypeData;
            columnTypeDataAsColumnDefinitions = copyFrom.columnTypeDataAsColumnDefinitions;
        }

        /// <summary>
        /// Sets the type of the column.
        /// </summary>
        /// <param name="columnName"> The column to manipulate. </param>
        /// <param name="type"> The type of the specified column. </param>
        protected void SetColumnType(string columnName, MoBackValueType type)
        {
            if (!columnTypeData.ContainsKey(columnName)) 
            {
                columnTypeDataAsColumnDefinitions.Add(new MoBackColumn(columnName, type));
                columnTypeData[columnName] = type;
            }
        }
    }
}
