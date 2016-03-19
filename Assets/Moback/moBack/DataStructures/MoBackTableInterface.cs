//-----------------------------------------------------------------------
// <copyright file="MoBackTableInterface.cs" company="moBack">
// Copyright 2015 moBack Inc. All Rights Reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace MoBack
{
  using System;
  using System.Linq;
  using System.Collections;
  using System.Collections.Generic;
  using MoBack;
  using MoBackInternal;
  using SimpleJSON;
  using UnityEngine;

    /// <summary>
    /// The MoBackTableInterface class provides convenient access to tables already stored on the server.
    /// </summary>
    public class MoBackTableInterface
    {
        /// <summary>
        /// Read-only list of this table's known column names.
        /// Use GetColumnType() to learn the type of a particular column.
        /// Use FetchColumnDescriptions() to get column info (names, types) from server.
        /// </summary>
        public IList<string> ColumnNames;

        /// <summary>
        /// Accessor for the name of the table.
        /// </summary>
        /// <value>The name of the table.</value>
        public string TableName { get; private set; }

        /// <summary>
        /// Accessor for the RequestBuilder.
        /// </summary>
        /// <value>The request builder.</value>
        public RequestBuilder RequestB { get; private set; }

        /// <summary>
        /// The column names.
        /// </summary>
        public List<string> columnNames;

        /// <summary>
        /// The columns.
        /// </summary>
        public Dictionary<string, MoBackValueType> columns;

        /// <summary>
        /// Initializes a new instance of the <see cref="MoBack.MoBackTableInterface"/> class.
        /// </summary>
        /// <param name="tableName">Table name.</param>
        public MoBackTableInterface(string tableName) : this()
        {
            TableName = tableName;
        }

        /// <summary>
        /// Gets the type of the column.
        /// </summary>
        /// <returns>The column type.</returns>
        /// <param name="columnName">Column name.</param>
        public MoBackValueType GetColumnType(string columnName)
        {
            return columns[columnName];
        }

        /// <summary>
        /// Fetches the column descriptions.
        /// </summary>
        /// <returns>The column descriptions.</returns>
        public MoBackRequest FetchColumnDescriptions()
        {
            return RequestB.FetchColumnDescriptions();
        }

        /// <summary>
        /// Creates the column.
        /// </summary>
        /// <returns>The column.</returns>
        /// <param name="columnName">Column name.</param>
        /// <param name="columnType">Column type.</param>
        public MoBackRequest CreateColumn(string columnName, MoBackValueType columnType)
        {
            return RequestB.CreateColumn(columnName, columnType);
        }

        /// <summary>
        /// Gets the row.
        /// </summary>
        /// <returns>The row.</returns>
        /// <param name="id">Identifier .</param>
        public MoBackRequest<MoBackRow> GetRow(string id)
        {
            return RequestB.GetRow(id);
        }

        /// <summary>
        /// Gets all rows.
        /// </summary>
        /// <returns>A list of rows.</returns>
        /// <param name="query">A optional query.</param>
        public MoBackRequest<List<MoBackRow>> GetRows(MoBackRequestParameters query = null)
        {
            return RequestB.GetRows(query);
        }

        /// <summary>
        /// Deletes this table.
        /// </summary>
        /// <returns>Returns the status of the request.</returns>
        public MoBackRequest DeleteThisTable()
        {
            return RequestB.DeleteThisTable();
        }

        /// <summary>
        /// Deletes the column.
        /// </summary>
        /// <returns>Returns the status of the request.</returns>
        /// <param name="columnName">Column name.</param>
        public MoBackRequest DeleteColumn(string columnName)
        {
            return RequestB.DeleteColumn(columnName);
        }

        /// <summary>
        /// Deletes the object.
        /// </summary>
        /// <returns>Returns the status of the request.</returns>
        /// <param name="objectID">Object identifier.</param>
        public MoBackRequest DeleteObject(string objectID, MoBackRequest.ResponseProcessor deleteProcessor = null)
        {
            return RequestB.DeleteObject(objectID, deleteProcessor);
        }

        /// <summary>
        /// Clears the table.
        /// </summary>
        /// <returns>Returns the status of the request.</returns>
        public MoBackRequest ClearTable()
        {
            return RequestB.ClearTable();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MoBack.MoBackTableInterface"/> class.
        /// </summary>
        private MoBackTableInterface()
        {
            this.columns = new Dictionary<string, MoBackValueType>();
            this.ColumnNames = (columnNames = new List<string>()).AsReadOnly();
            this.RequestB = new RequestBuilder(this);
        }

        /// <summary>
        /// Builds MoBackRequest and MoBackRequest objects for the various requests that are possible.
        /// If you want to fire a request yourself (perhaps you want to create your own threaded system that runs on callbacks instead of coroutines?):
        /// 1. Grab a MoBackRequest from the table's RequestBuilder.
        /// 2. Call MoBackRequest.SetHandlers() to set the callbacks for request success or failure.
        /// 3. Run MoBackRequest.Execute(); Note that this will block the current thread.
        /// </summary>
        public class RequestBuilder
        {
            /// <summary>
            /// The MoBack table.
            /// </summary>
            private MoBackTableInterface table;

            /// <summary>
            /// Initializes a new instance of the <see cref="MoBack.MoBackTableInterface+RequestBuilder"/> class.
            /// </summary>
            /// <param name="table">A table.</param>
            public RequestBuilder(MoBackTableInterface table)
            {
                this.table = table;
            }

            /// <summary>
            /// Fetches the column descriptions.
            /// </summary>
            /// <returns>The column descriptions.</returns>
            public MoBackRequest FetchColumnDescriptions()
            {
                return null;
            }

            /// <summary>
            /// Creates the column.
            /// </summary>
            /// <returns>Returns the status of the request.</returns>
            /// <param name="columnName">Column name.</param>
            /// <param name="columnType">Column type.</param>
            public MoBackRequest CreateColumn(string columnName, MoBackValueType columnType)
            {
                SimpleJSONClass columnTypeSpecifier = new SimpleJSONClass();

                // TODO: check if any of these type-names don't fit the moback spec. According to REST api reference they should be...
                columnTypeSpecifier.Add("__type", columnTypeSpecifier.ToString());
                SimpleJSONClass columnDataHolder = new SimpleJSONClass();
                columnDataHolder.Add(columnName, columnTypeSpecifier);

                /*
                Sample uri: https://api.moback.com/objectmgr/api/schema/{tableName}/
                Sample Json Request Body:
                {
                    "FavoriteFood" : {"__type" : "string"}
                }
                */
                return new MoBackRequest(MoBackURLS.TablesSpecial + table.TableName, HTTPMethod.PUT, null, columnDataHolder.ToString().ToByteArray());
            }

            /// <summary>
            /// Gets the row.
            /// </summary>
            /// <returns>Returns the status of the request.</returns>
            /// <param name="id">Identifier .</param>
            public MoBackRequest<MoBackRow> GetRow(string id)
            {
                /*
                Sample uri: https://api.moback.com/objectmgr/api/collections/{tableName}/{objectID}
                */
                return new MoBackRequest<MoBackRow>(GetObject_processor, MoBackURLS.TablesDefault + table.TableName + "/" + id, HTTPMethod.GET, null, null);
            }

            /// <summary>
            /// Gets the rows.
            /// </summary>
            /// <returns>Returns the status of the request.</returns>
            /// <param name="query">An optional query.</param>
            public MoBackRequest<List<MoBackRow>> GetRows(MoBackRequestParameters query = null)
            {
				/*
                Sample uri: https://api.moback.com/objectmgr/api/collections/{tableName}
                */
                return new MoBackRequest<List<MoBackRow>>(GetObjects_processor, MoBackURLS.TablesDefault + table.TableName, HTTPMethod.GET, query);
            }


            /// <summary>
            /// Saves the object.
            /// </summary>
            /// <returns>Returns the status of the request.</returns>
            /// <param name="toSave">To save.</param>
            /// <param name="objectDataUpdater">Object data updater.</param>
            public MoBackRequest SaveObject(MoBackRow toSave, Action<string, DateTime?, DateTime> objectDataUpdater)
            {
                string uri = MoBackURLS.TablesDefault + table.TableName;
                byte[] bytes = toSave.GetJSON().ToString().ToByteArray();

                // Make a new object
                if (string.IsNullOrEmpty(toSave.ObjectId))
                {
                    // Declare a callback inline so we can store the MoBackRow in it without creating a bunch of extra infrastructure:
                    MoBackRequest.ResponseProcessor objUpdater = (SimpleJSONNode json) =>
                    {
                        /*
                        Sample Json response:
                        {
                            "createdAt": "2016-01-18T18:41:39.475Z",
                            "objectId": "Vp0x4-Swp23tC3Ap",
                            "success": true,
                            "__acl":
                            {
                                "globalRead": true,
                                "globalWrite": true
                            }
                        }
                        */

                        string objId = json["objectId"];
                        DateTime createdDate = MoBackDate.DateFromString(json["createdAt"]);
                        objectDataUpdater(objId, createdDate, createdDate);
                    };

                    /*
                    Sample uri: https://api.moback.com/objectmgr/api/collections/{tableName}
                    Sample Json Request Body:
                    {
                       "Score" : 10,
                       "Name" : "Locke",
                       "Over21" : yes
                    }
                    */
                    return new MoBackRequest(objUpdater, uri, HTTPMethod.POST, null, bytes);
                }
                else
                {
                    // Update the object and declare a callback inline so we can store the MoBackRow in it without creating a bunch of extra infrastructure:
                    MoBackRequest.ResponseProcessor objUpdater = (SimpleJSONNode json) =>
                    {
                        /*
                        Sample Json response:
                        {
                            {
                                "updatedAt": "2016-01-21T23:27:18.209Z",
                                "__acl":
                                {
                                    "globalRead": true,
                                    "globalWrite": true
                                }
                            }
                        }
                        */
                        DateTime updatedDate = MoBackDate.DateFromString(json["updatedAt"]);
                        objectDataUpdater(null, null, updatedDate);
                    };

                    uri += "/" + toSave.ObjectId;

					/*
                    Sample uri: https://api.moback.com/objectmgr/api/collections/{tableName}/{objectID}
                    Sample Json Request Body:
                    {
                        "Name" : "George"
                    }
                    */
                    return new MoBackRequest(objUpdater, uri, HTTPMethod.PUT, null, bytes);
                }
            }

            /// <summary>
            /// Deletes this table.
            /// </summary>
            /// <returns>Returns the status of the request.</returns>
            public MoBackRequest DeleteThisTable()
            {
				/*
                Sample uri: https://api.moback.com/objectmgr/api/collections/{tableName}/{objectID}
                */
                return new MoBackRequest(MoBackURLS.TablesSpecial + table.TableName, HTTPMethod.DELETE);
            }

            /// <summary>
            /// Deletes the column.
            /// </summary>
            /// <returns>Returns the status of the request.</returns>
            /// <param name="columnName">Column name.</param>
            public MoBackRequest DeleteColumn(string columnName)
            {
                SimpleJSONClass columnOp = new SimpleJSONClass();
                columnOp.Add("__op", "Delete");
                SimpleJSONClass columnDataHolder = new SimpleJSONClass();
                columnDataHolder.Add(columnName, columnOp);

				/*
                Sample uri: https://api.moback.com/objectmgr/api/schema/{tableName}/
                Sample Json Request Body:
                {
                    "Over21" : {"__op" : "Delete"}
                }
                */
                return new MoBackRequest(MoBackURLS.TablesSpecial + table.TableName, HTTPMethod.PUT, null, columnDataHolder.ToString().ToByteArray());
            }

            /// <summary>
            /// Deletes the object.
            /// </summary>
            /// <returns>Returns the status of the request.</returns>
            /// <param name="objectID">Object identifier.</param>
            public MoBackRequest DeleteObject(string objectID, MoBackRequest.ResponseProcessor deleteProcessor = null)
            {
                if (deleteProcessor == null)
                {
					/*
                    Sample uri: https://api.moback.com/objectmgr/api/collections/{tableName}/{objectID}
                    */
                    return new MoBackRequest(MoBackURLS.TablesDefault + table.TableName + "/" + objectID, HTTPMethod.DELETE);
                }

                /*
                Sample uri: https://api.moback.com/objectmgr/api/collections/{tableName}/{objectID}
                */
                return new MoBackRequest(deleteProcessor, MoBackURLS.TablesDefault + table.TableName + "/" + objectID, HTTPMethod.DELETE);
            }

            /// <summary>
            /// Clears the table.
            /// </summary>
            /// <returns>Returns the status of the request.</returns>
            public MoBackRequest ClearTable()
            {
                // TODO: cleanup seem like not working anymore. Only works on the website which uses developer session token.
                // Need to check with Mona or Mike for further info.

                /*
                Sample uri: https://api.moback.com/objectmgr/api/collections/{tableName}/cleanup
                */
                return new MoBackRequest(MoBackURLS.TablesDefault + table.TableName + "/cleanup", HTTPMethod.DELETE);
            }

            /// <summary>
            /// Saves multiple objects with one single call to server.
            /// </summary>
            /// <returns>The MobackRequest.</returns>
            /// <param name="rowsToSave">MoBack Rows need to save.</param>
            public MoBackRequest SaveMultipleObjects(List<MoBackRow> rowsToSave)
            {
                string uri = MoBackURLS.Batch + table.TableName;
                SimpleJSONArray jsonArray = new SimpleJSONArray();
                SimpleJSONClass jsonStructure = new SimpleJSONClass();
                for (int i = 0; i < rowsToSave.Count; i++)
                {
                    SimpleJSONClass jsonToSave = rowsToSave[i].GetJSON();

                    if (rowsToSave[i].ObjectId != null)
                    {
                        jsonToSave.Add("objectId", rowsToSave[i].ObjectId);
                    }

                    jsonArray.Add(jsonToSave);
                }

                jsonStructure.Add("objects", jsonArray);
                Debug.Log(jsonStructure.ToString());
                byte[] bytes = jsonStructure.ToString().ToByteArray();

                // Construct a callback to update all rowsToSave.
                MoBackRequest.ResponseProcessor objUpdater = (SimpleJSONNode responseJson) =>
                {
                    SimpleJSONClass allObjectsInJsonFormat = responseJson["batchResponse"].AsObject;

                    // The response should have the same amount as of the request body.
                    if (allObjectsInJsonFormat.Count != rowsToSave.Count)
                    {
                        Debug.LogError(string.Format("Response objects count: {0}. Request objects count: {1}", allObjectsInJsonFormat.Count, rowsToSave.Count));
                        Debug.LogError("Response doesn't have the same object as Request Body. Something wrong!");
                        return;
                    }

                    for (int i = 0; i < rowsToSave.Count; i++)
                    {
                        SimpleJSONClass jsonAtCurrentIndex = allObjectsInJsonFormat[i] as SimpleJSONClass;

                        if (!jsonAtCurrentIndex["success"].AsBool)
                        {
                            Debug.LogError("Unable to save: " + jsonAtCurrentIndex["objectId"]);
                            continue;
                        }

                        if (!string.IsNullOrEmpty(jsonAtCurrentIndex["createdAt"].ToString()))
                        {
                            string objectId = jsonAtCurrentIndex["objectId"];
                            DateTime createdTime = MoBackDate.DateFromString(jsonAtCurrentIndex["createdAt"]);

                            rowsToSave[i].UpdateAfterSave(objectId, createdTime, createdTime);
                        }
                        else if (!string.IsNullOrEmpty(jsonAtCurrentIndex["updatedAt"].ToString()))
                        {
                            string objectId = jsonAtCurrentIndex["objectId"];
                            DateTime updateTime = MoBackDate.DateFromString(jsonAtCurrentIndex["updatedAt"]);
                            rowsToSave[i].UpdateAfterSave(objectId, null, updateTime);
                        }
                    }
                };

                /*
                Sample uri: https://api.moback.com/objectmgr/api/collections/batch/{tableName}
                Sample Json Request Body:
                {
                    "objects" : [{ "Name" : "Joe", "FavoriteFood" : ["pizza", "fried eggs", "ham sandwich"]}]
                }
                */
                return new MoBackRequest(objUpdater, uri, HTTPMethod.POST, null, bytes);
            }

            /// <summary>
            /// Deletes the multiple ojbects.
            /// </summary>
            /// <returns>A MoBackRequest.</returns>
            /// <param name="objectsToDelete">Objects to delete.</param>
            public MoBackRequest DeleteMultipleOjbects(List<MoBackRow> objectsToDelete)
            {
                string uri = MoBackURLS.BatchDelete + table.TableName;

                SimpleJSONArray jsonArray = new SimpleJSONArray();
                SimpleJSONClass jsonBody = new SimpleJSONClass();

                foreach (MoBackRow item in objectsToDelete)
                {
                    jsonArray.Add(MoBackUtils.MoBackTypedObjectToJSON(item.ObjectId, MoBackValueType.String));
                }

                jsonBody.Add("objectIds", jsonArray);
                byte[] bytes = jsonBody.ToString().ToByteArray();

                MoBackRequest.ResponseProcessor deleteCallBack = (SimpleJSONNode responseJson) =>
                {
                    SimpleJSONArray responseArray = responseJson["deletedObjectIds"].AsArray;

                    Debug.Log(responseArray.ToString());

                    foreach (SimpleJSONNode item in responseArray)
                    {
                        MoBackRow deletedRow = objectsToDelete.Find(row => row.ObjectId == item.Value);
                        if (deletedRow != null)
                        {
                            deletedRow.ResetMetaData(null);
                        }
                        else
                        {
                            Debug.Log("Can't find row");
                        }
                    }
                };

                /*
                Sample uri: https://api.moback.com/objectmgr/api/collections/batch/delete/{tableName}
                Sample Json Request Body:
                {
                    "objectIds" : ["Vp6pfOSwp23tC3IN", "Vp6ZnOSwp23tC3Hv"]
                }
                */
                return new MoBackRequest(deleteCallBack, uri, HTTPMethod.POST, null, bytes);
            }

            // Basically the callbacks for processing results, with particular regards to the current table
            #region JSONResponseProcessors
            /// <summary>
            /// Fetch the column descriptions_processor.
            /// </summary>
            /// <param name="jsonObject">JSON object.</param>
            private void FetchColumnDescriptions_processor(SimpleJSONNode jsonObject)
            {
                // Stub
            }

            /// <summary>
            /// Gets the object processor.
            /// </summary>
            /// <returns>The a MoBackRow converted from a JSON object.</returns>
            /// <param name="jsonObject">A JSON object.</param>
            private MoBackRow GetObject_processor(SimpleJSONNode jsonObject)
            {
                return MoBackRow.FromJSON(jsonObject as SimpleJSONClass, table.TableName);
            }

            /// <summary>
            /// Gets the objects processor.
            /// </summary>
            /// <returns>A list of objects of the MoBackRow type converted from a JSON object.</returns>
            /// <param name="jsonObject">JSON object.</param>
            private List<MoBackRow> GetObjects_processor(SimpleJSONNode jsonObject)
            {
                List<MoBackRow> objects = new List<MoBackRow>();

                // TODO: this doesn't work. Need to double check later.
                SimpleJSONArray results = jsonObject["results"].AsArray;
                for (int i = 0; i < results.Count; i++)
                {
                    objects.Add(MoBackRow.FromJSON(results[i] as SimpleJSONClass, table.TableName));
                }

                return objects;
            }

            /// <summary>
            /// Modifies the relation for a given row/column.
            /// </summary>
            /// <returns>The request object.</returns>
            /// <param name="tableName">Table name.</param>
            /// <param name="objectID">Row ID.</param>
            /// <param name="columnName">Column ID.</param>
            /// <param name="relationOp">JSON that signifies details of relation operation to MoBack servers.</param>
            public static MoBackRequest ModifyRelationForGivenRowColumn(string tableName, string objectID, string columnName, SimpleJSONNode relationOp) {
                SimpleJSONClass opOnColumn = new SimpleJSONClass ();
                opOnColumn [columnName] = relationOp;

                /*
                Sample uri: https://api.moback.com/objectmgr/api/collections/{tableName}/{objectID}
                Sample Json Request Body:
                {
                    "Car": {
                        "objects": [
                        {
                            "__type": "Pointer",
                            "objectId": "VqFsguSwp23tC3gF",
                            "className": "Relations"
                            },{
                            "__type": "Pointer",
                            "objectId": "VqFsnuSwp23tC3gH",
                            "className": "Relations"
                            }
                        ],
                        "__op": "addRelation"
                    }
                }
                */
                return new MoBackRequest (MoBackURLS.TablesDefault + tableName + "/" + objectID, HTTPMethod.PUT, null, opOnColumn.ToString().ToByteArray());
            }

            /// <summary>
            /// Gets all relation objects.
            /// </summary>
            /// <returns>The all relation objects.</returns>
            /// <param name="tableName">Table name.</param>
            /// <param name="objectID">Object I.</param>
            /// <param name="columnName">Column name.</param>
            public static MoBackRequest<List<MoBackRow>> GetAllRelationObjects(string tableName, string objectID, string columnName)
            {
                MoBackRequest<List<MoBackRow>>.ResponseProcessor getAllRelationObjectsProcessor = (SimpleJSONNode responseJson) =>
                {
                    SimpleJSONNode relationJson = responseJson[columnName];
                    List<MoBackRow> moBackObjects = MoBackRelation.MoBackRowFromRelationJSON(relationJson);
                    return moBackObjects;
                };

                /*
                Sample uri: https://api.moback.com/objectmgr/api/collections/{tableName}/{objectID}?include={columnName}
                */
                return new MoBackRequest<List<MoBackRow>>(getAllRelationObjectsProcessor, MoBackURLS.TablesDefault + tableName + "/" + objectID + string.Format("?include={0}",columnName), HTTPMethod.GET);
            }

            /// <summary>
            /// Gets all relation pointers.
            /// </summary>
            /// <returns>The all relation pointers.</returns>
            /// <param name="tableName">Table name.</param>
            /// <param name="objectID">Object I.</param>
            /// <param name="columnName">Column name.</param>
            public static MoBackRequest<List<MoBackPointer>> GetAllRelationPointers(string tableName, string objectID, string columnName)
            {
                MoBackRequest<List<MoBackPointer>>.ResponseProcessor getAllRelationPointersProcessor = (SimpleJSONNode responseJson) =>
                {
                    SimpleJSONNode relationJson = responseJson[columnName];
                    MoBackRelation moBackRelation = MoBackRelation.RelationFromMoBackJSON(relationJson);

                    if (moBackRelation != null)
                    {
                        return moBackRelation.pointers.ToList();
                    }

                    return null;
                };

                /*
                Sample uri: https://api.moback.com/objectmgr/api/collections/{tableName}/{objectID}
                */
                return new MoBackRequest<List<MoBackPointer>>(getAllRelationPointersProcessor, MoBackURLS.TablesDefault + tableName + "/" + objectID, HTTPMethod.GET);
            }

            #endregion
        }
    }
}
