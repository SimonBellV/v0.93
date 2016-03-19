//-----------------------------------------------------------------------
// <copyright file="MoBackRow.cs" company="moBack"> 
//     Copyright 2015 moBack Inc. All Rights Reserved.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoBackInternal;
using SimpleJSON;

namespace MoBack
{
    /// <summary>
    /// This convenience class inherits from MoBackObject, and gives users the ability to easily create 
    /// rows of data without having to worry about parts of the MoBackObject that can be cumbersome.
    /// </summary>
    public class MoBackRow : MoBackObject
    {
        /// <summary>
        /// Accesor for the name of the table.
        /// </summary>
        /// <value>The name of the table.</value>
        public string TableName { get; private set; }

        /// <summary>
        /// Accesor for the object ID.
        /// </summary>
        /// <value>The object identifier.</value>
        public string ObjectId { get; private set; }

        /// <summary>
        /// Accesor for the date the row was created.
        /// </summary>
        /// <value>The created date.</value>
        public DateTime? CreatedDate { get; private set; }

        /// <summary>
        /// Accessor for the date the row was updated.
        /// </summary>
        /// <value>The updated date.</value>
        public DateTime? UpdatedDate { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MoBack.MoBackRow"/> class.
        /// </summary>
        private MoBackRow()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MoBack.MoBackRow"/> class.
        /// </summary>
        /// <param name="tableName">Table name.</param>
        public MoBackRow(string tableName) : base()
        {
            this.TableName = tableName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MoBack.MoBackRow"/> class.
        /// </summary>
        /// <param name="tableName">Table name.</param>
        /// <param name="objectId">Object identifier.</param>
        public MoBackRow(string tableName, string objectId) : this(tableName)
        {
            this.ObjectId = objectId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MoBack.MoBackRow"/> class.
        /// </summary>
        /// <param name="tableName">Table name.</param>
        /// <param name="objectId">Object identifier.</param>
        /// <param name="created">Created Date.</param>
        /// <param name="updated">Updated Date.</param>
        public MoBackRow(string tableName, string objectId, DateTime created, DateTime updated) : this(tableName, objectId)
        {
            CreatedDate = created;
            UpdatedDate = updated;
        }

        /// <summary>
        /// Creates a <see cref="MoBack.MoBackRow"/> object that encapsulates the provided MoBackObject (e.g. for saving an object to a table as a new row).
        /// </summary>
        /// <param name="tableName">Table the row should be associated with.</param>
        /// <param name="data">MoBackObject to encapsulate.</param>
        public MoBackRow(string tableName, MoBackObject data)
        {
            MoBackRow objectAsRow = new MoBackRow();
            objectAsRow.TableName = tableName;
            objectAsRow.ShallowCopy(data);
        }
        
        /// <summary>
        /// Converts a JSON object to a MoBackRow object.
        /// </summary>
        /// <returns>The JSON date converted to a MoBackRow.</returns>
        /// <param name="jsonObject">A JSON object.</param>
        /// <param name="sourceTableID">Source table ID.</param>
        public static MoBackRow FromJSON(SimpleJSONClass jsonObject, string sourceTableID = null)
        {
            MoBackRow mObject = ExtractMetadata(jsonObject, sourceTableID);
            
            ProcessJsonIntoMoBackObject(mObject, jsonObject);
            
            return mObject;
        }

        /// <summary>
        /// Converts MoBackObject data to a JSON object for storage.
        /// </summary>
        /// <returns>A JSON object.</returns>
        public override SimpleJSONClass GetJSON()
        {
            SimpleJSONClass jsonStructure = new SimpleJSONClass();
            
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
        /// Save this object to the server.
        /// </summary>
        /// <returns>A new request to save the MoBackRow to the specified table.</returns>
        public MoBackRequest Save()
        {
            return new MoBackTableInterface.RequestBuilder(new MoBackTableInterface(TableName)).SaveObject(this, UpdateAfterSave);
        }
        
        /// <summary>
        /// Deletes the MoBack object on the server.
        /// </summary>
        /// <returns>The mo back object.</returns>
        public MoBackRequest DeleteMoBackObject()
        {
            return new MoBackTableInterface.RequestBuilder(new MoBackTableInterface(TableName)).DeleteObject(this.ObjectId, ResetMetaData);
        }

        /// <summary>
        /// Request to add one or more pointers to a relations column.
        /// </summary>
        /// <returns>The relations.</returns>
        /// <param name="relationColumnID">Relation column to add pointers to.</param>
        /// <param name="pointersToAdd">Pointers to add.</param>
        public MoBackRequest AddRelations(string relationColumnID, MoBackRow[] pointersToAdd)
        {
            MoBackPointer[] targetArray = new MoBackPointer[pointersToAdd.Length];
            for (int i = 0; i < pointersToAdd.Length; i++)
            {
                targetArray[i] = GetPointer(pointersToAdd[i]);
            }

            if (string.IsNullOrEmpty(ObjectId)) 
            {
                Debug.LogError("Relations can only be added to objects that have known object IDs");
                return null;
            }
            
            return MoBackTableInterface.RequestBuilder.ModifyRelationForGivenRowColumn(TableName, ObjectId, relationColumnID, MoBackRelation.RelationAddOpAsMoBackJSON(targetArray));
        }
        
        /// <summary>
        /// Request to remove one or more pointers to a relations column.
        /// </summary>
        /// <returns>The relations.</returns>
        /// <param name="relationColumnID">Relation column to add pointers to.</param>
        /// <param name="pointerToRemove">Pointer to add.</param>
        public MoBackRequest RemoveRelations(string relationColumnID, MoBackRow[] pointerToRemove)
        {
            MoBackPointer[] targetArray = new MoBackPointer[pointerToRemove.Length];
            for (int i = 0; i < pointerToRemove.Length; i++)
            {
                targetArray[i] = GetPointer(pointerToRemove[i]);
            }
            
            if (string.IsNullOrEmpty(ObjectId)) 
            {
                return null;
            }

            return MoBackTableInterface.RequestBuilder.ModifyRelationForGivenRowColumn(TableName, ObjectId, relationColumnID, MoBackRelation.RelationRemoveOpAsMoBackJSON(targetArray));
        }
        
        public MoBackRequest<List<MoBackRow>> GetAllRelationObjects(string relationColumnName)
        {
            return MoBackTableInterface.RequestBuilder.GetAllRelationObjects(this.TableName, this.ObjectId, relationColumnName);
        }
        
        public MoBackRequest<List<MoBackPointer>> GetAllPointerObjects(string relationColumnName)
        {
            return MoBackTableInterface.RequestBuilder.GetAllRelationPointers(this.TableName, this.ObjectId, relationColumnName);
        }

        /// <summary>
        /// Check for any MoBack reserved columns, and then process them.
        /// </summary>
        /// <returns>The metadata.</returns>
        /// <param name="jsonObject">A JSON object.</param>
        /// <param name="sourceTableID">Source table ID.</param>
        private static MoBackRow ExtractMetadata(SimpleJSONClass jsonObject, string sourceTableID)
        {
            string objIO = null;
            DateTime createdOn = default(DateTime);
            DateTime updatedOn = default(DateTime);
            if (jsonObject.dict.ContainsKey("objectId")) 
            {
                objIO = jsonObject["objectId"];
            }
            if (jsonObject.dict.ContainsKey("createdAt")) 
            {
                createdOn = MoBackDate.DateFromString(jsonObject["createdAt"]);
            }
            if (jsonObject.dict.ContainsKey("updatedAt")) 
            {
                updatedOn = MoBackDate.DateFromString(jsonObject["updatedAt"]);
            }
            
            return new MoBackRow(sourceTableID, objIO, createdOn, updatedOn);
        }

        /// <summary>
        /// Updates the MoBackRow after a save.
        /// </summary>
        /// <param name="id">Identifier.</param>
        /// <param name="createdAt">Created at date.</param>
        /// <param name="updatedAt">Updated at date.</param>
        public void UpdateAfterSave(string id, DateTime? createdAt, DateTime updatedAt)
        {
            if (id != null) 
            {
                ObjectId = id;
            }
            if (createdAt != null) 
            {
                CreatedDate = createdAt.Value;
            }
            UpdatedDate = updatedAt;
        }

        /// <summary>
        /// Returns the pointer to "row".
        /// </summary>
        /// <returns>The pointer.</returns>
        /// <param name="row">Row.</param>
        private MoBackPointer GetPointer(MoBackRow row)
        {
            MoBackTableInterface targetTable = new MoBackTableInterface(row.TableName.ToString());
            MoBackPointer pToTarget = new MoBackPointer(row.ObjectId.ToString(), targetTable);
            return pToTarget;
        }
            
        public void ResetMetaData(SimpleJSONNode rawResponse)
        {
            Debug.Log ("Reset meta data");
             
            // Reset default meta values.
            ObjectId = null;
            CreatedDate = null;
            UpdatedDate = null;
        }
    }
}
