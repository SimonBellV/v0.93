//-----------------------------------------------------------------------
// <copyright file="MoBackRelation.cs" company="moBack"> 
//     Copyright 2015 moBack Inc. All Rights Reserved.
// </copyright>
//-----------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using MoBackInternal;

namespace MoBack
{
	public class MoBackRelation {
		/// <summary>
		/// Read-only list of pointers in this relation.
		/// </summary>
		/// <value>The pointers.</value>
		public IList<MoBackPointer> pointers {
			get {
				return _pointers.AsReadOnly();
			}
		}

		/// <summary>
		/// Internal list of pointers in this relation.
		/// </summary>
		private List<MoBackPointer> _pointers;

		/// <summary>
		/// Initializes a new instance of the <see cref="MoBack.MoBackRelation"/> class.
		/// Private, because users don't make their own relations.
		/// </summary>
		private MoBackRelation() { 
			_pointers = new List<MoBackPointer> ();
		}

		/// <summary>
		/// Gets JSON the server expects for an add relation operation, that adds the supplied list of pointers to some relation.
		/// </summary>
		/// <returns>The add operation as mo back JSO.</returns>
		/// <param name="pointersToAdd">Pointers to add.</param>
		public static SimpleJSONClass RelationAddOpAsMoBackJSON (MoBackPointer[] pointersToAdd) {
			SimpleJSONClass relationOpJsonStructure = new SimpleJSONClass ();
			relationOpJsonStructure ["__op"] = "addRelation";
			
			SimpleJSONArray pointers = new SimpleJSONArray ();
			for (int i = 0; i < pointersToAdd.Length; i++) {
				pointers.Add(MoBackPointer.PointerToMoBackJSON(pointersToAdd[i]));
			}
			relationOpJsonStructure ["objects"] = pointers;
			
			return relationOpJsonStructure;
		}

		/// <summary>
		/// Gets JSON the server expects for an remove relation operation, that removes the supplied list of pointers to some relation.
		/// </summary>
		/// <returns>The remove operation as mo back JSO.</returns>
		/// <param name="pointersToRemove">Pointers to add.</param>
        public static SimpleJSONClass RelationRemoveOpAsMoBackJSON (MoBackPointer[] pointersToRemove) {
			SimpleJSONClass relationOpJsonStructure = new SimpleJSONClass ();
			relationOpJsonStructure ["__op"] = "removeRelation";
			
			SimpleJSONArray pointers = new SimpleJSONArray ();
			for (int i = 0; i < pointersToRemove.Length; i++) {
				pointers.Add(MoBackPointer.PointerToMoBackJSON(pointersToRemove[i]));
			}
			relationOpJsonStructure ["objects"] = pointers;
			
			return relationOpJsonStructure;
		}
        
		/// <summary>
		/// MoBack JSON from relation. Of dubious usefulness because uploading and updating relations directly is presently forbidden.
		/// </summary>
		/// <returns>The moback JSON representing the relation.</returns>
		/// <param name="relation">Relation.</param>
		public static SimpleJSONNode MoBackJSONFromRelation(MoBackRelation relation) {
			SimpleJSONClass relationJSON = new SimpleJSONClass();
			
			relationJSON ["__type"] = "Pointer";
			
			SimpleJSONArray pointersJSON = new SimpleJSONArray();
			for (int i = 0; i < relation._pointers.Count; i++) {
				pointersJSON.Add(MoBackPointer.PointerToMoBackJSON(relation._pointers[i]));
			}
			relationJSON ["value"] = pointersJSON;
            
            return relationJSON;
        }
        
		/// <summary>
		/// Creates a MoBackRelation object from JSON returned from server.
		/// </summary>
		/// <returns>The relation.</returns>
		/// <param name="relationJSON">Relation in JSON form, from server.</param>
        public static MoBackRelation RelationFromMoBackJSON(SimpleJSONNode relationJSON) {
            MoBackRelation relation = new MoBackRelation();
            SimpleJSONArray pointersJSON = relationJSON ["value"].AsArray;
            
            for (int i = 0; i < pointersJSON.Count; i++) {
                
                // If the nested json doesn't have a pointer type, just return null.
                // When AchieveRelation with ?include={ColumnName}, it will return an object type.
                if (pointersJSON[i]["__type"].Value != "Pointer")
                {
                    if (pointersJSON[i]["__type"].Value == "object")
                    {
                        Debug.LogWarning("The response JSON contains Object type, not Pointer type, can't parse this relationJson. Try MoBackRelation.MoBackRowFromRelationJSON() instead");
                        return null;
                    }
                    else
                    {
                        Debug.LogError("Unknown type: " + pointersJSON[i]["__type"].Value);
                        return null;
                    }
                }

                relation._pointers.Add(MoBackPointer.PointerFromMoBackJSON(pointersJSON[i]));
            }
            
            return relation;
		}
        
        /// <summary>
        /// Return a list of MoBackRow object from the relation JSON
        /// </summary>
        /// <returns>The back row from relation JSO.</returns>
        /// <param name="relationJSON">Relation JSO.</param>
        public static List<MoBackRow> MoBackRowFromRelationJSON(SimpleJSONNode relationJSON)
        {
            SimpleJSONArray nestedArray = relationJSON["value"] as SimpleJSONArray;
            
            List<MoBackRow> moBackRows = new List<MoBackRow>();
            
            // If the nested json doesn't have an object type, just return null.
            // When AchieveRelation without ?include={ColumnName}, it will return a pointer type.
            for (int i = 0; i < nestedArray.Count; i++) 
            {
                if (nestedArray[i]["__type"].Value != "object")
                {
                    if (nestedArray[i]["__type"].Value == "Pointer")
                    {
                        Debug.LogWarning("The response JSON contains Pointer type, not Object type, can't parse this relationJson. Try MoBackRelation.RelationFromMoBackJSON() instead");
                        return null;
                    }
                    else
                    {
                        Debug.LogError("Unknown type: " + nestedArray[i]["__type"].Value);
                        return null;
                    }
                }
                
                SimpleJSONClass jsonObject = nestedArray[i] as SimpleJSONClass;
                
                // This will get the MoBackObject exlucde the 2 keys: "__type" and "className".
                moBackRows.Add(MoBackRow.FromJSON(jsonObject, nestedArray[i]["className"]));
            }
            
            return moBackRows;
        }
	}
}