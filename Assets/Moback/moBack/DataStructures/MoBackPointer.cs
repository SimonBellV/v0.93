//-----------------------------------------------------------------------
// <copyright file="MoBackPointer.cs" company="moBack"> 
//     Copyright 2015 moBack Inc. All Rights Reserved.
// </copyright>
//-----------------------------------------------------------------------
using System.Collections;
using UnityEngine;
using SimpleJSON;

namespace MoBack
{
    /// <summary>
    /// A class to provide users with the ability to create pointers to objects.
    /// </summary>
    public class MoBackPointer
    {
		public string objectID { get; private set; }
		public string tableID { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MoBack.MoBackPointer"/> class.
        /// </summary>
        /// <param name="objectID"> The ID associated with that object. </param>
        /// <param name="tableID"> The ID of the table the object resides in. </param>
        public MoBackPointer(string objectID, string tableID)
        {
            this.objectID = objectID;
            this.tableID = tableID;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MoBack.MoBackPointer"/> class.
        /// </summary>
        /// <param name="objectID"> Object ID. </param>
        /// <param name="table"> Table associated with the object. </param>
        public MoBackPointer(string objectID, MoBackTableInterface table) : this(objectID, table.TableName)
        {
        }

        /// <summary>
        /// Converts a MoBackPointer to a JSON object for storage.
        /// </summary>
        /// <returns> A JSON object. </returns>
        /// <param name="pointer"> A MoBackPointer object. </param>
        public static SimpleJSONClass PointerToMoBackJSON(MoBackPointer pointer)
        {
            SimpleJSONClass pointerJsonStructure = new SimpleJSONClass();
            pointerJsonStructure["__type"] = "Pointer";
            pointerJsonStructure["objectId"] = pointer.objectID;
            pointerJsonStructure["className"] = pointer.tableID;

            return pointerJsonStructure;
        }

        /// <summary>
        /// Converts a JSON object to a MoBackPointer object.
        /// </summary>
        /// <returns> A MoBackPointer object. </returns>
        /// <param name="pointerJSON"> A JSON object. </param>
        public static MoBackPointer PointerFromMoBackJSON(SimpleJSONNode pointerJSON)
        {
            string objID = pointerJSON["objectId"];
            string table = pointerJSON["className"];
            
            return new MoBackPointer(objID, table);
        }

        /// <summary>
        /// Converts a data in a MoBackPointer object to a string.
        /// </summary>
        /// <returns>A <see cref="System.String"/> that represents the current <see cref="MoBack.MoBackPointer"/>.</returns>
        public override string ToString()
        {
            return string.Format("Pointer to object {0} in {1}", objectID, tableID);
        }
    }
}
