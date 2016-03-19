using UnityEngine;
using System;
using System.Net;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

using MoBack;
using MoBackInternal;

namespace MoBackInternal {
	public static class MoBackUtils {

		public static byte[] ToByteArray(this string toConvert) {
			return System.Text.Encoding.UTF8.GetBytes(toConvert);
		}

		public static SimpleJSONNode MoBackTypedObjectToJSON(object data, MoBackValueType dataType) {
			switch(dataType) {
			case MoBackValueType.String:
				return data == null ? "" : (string) data;
			case MoBackValueType.Boolean:
				return (bool) data;
			case MoBackValueType.Number:
				MoBackNumber number = (MoBackInternal.MoBackNumber)data;
				return number.GetJSON();
			case MoBackValueType.Date:
				return MoBackDate.DateToMobackFormatJSON((DateTime)data);
			case MoBackValueType.File:
				return ((MoBackFile)data).GetJSON();
			case MoBackValueType.Pointer:
				return MoBackPointer.PointerToMoBackJSON((MoBackPointer)data);
			case MoBackValueType.Relation:
				return MoBackRelation.MoBackJSONFromRelation((MoBackRelation)data);
            case MoBackValueType.MoBackObject:
				if(data != null) {
					return ((MoBackObject)data).GetJSON();
				} else {
					return new SimpleJSONClass();
				}
			case MoBackValueType.Array:
				return ((MoBackArray)data).GetJSON();
			default:
				throw new NotImplementedException();
			}

		}

		public static object MoBackTypedObjectFromJSON(SimpleJSONNode node, out MoBackValueType dataType) {
			if(MoBackAppSettings.doublePrecisionFloatingPoint && node.valueType == typeof(double)) 
			{
				dataType = MoBackValueType.Number;
				return new MoBackNumber(node.AsDouble);
			}
			else if(!MoBackAppSettings.doublePrecisionFloatingPoint && node.valueType == typeof(float))
			{
				dataType = MoBackValueType.Number;
				return new MoBackNumber(node.AsFloat);
			}
			else if (node.valueType == typeof(int)) 
			{
				dataType = MoBackValueType.Number;
				return new MoBackNumber(node.AsInt);
			}
			else if (node.valueType == typeof(bool)) 
			{
				dataType = MoBackValueType.Boolean;
				return node.AsBool;
			} 
			else if(node.valueType == typeof(string)) 
			{
				dataType = MoBackValueType.String;
				return node.Value;
			} 
			else if(node.valueType == typeof(SimpleJSONClass)) 
			{
				SimpleJSONClass nestedClass = (SimpleJSONClass) node;
				if (nestedClass.dict.ContainsKey("__type")) 
				{
					//is a special type, treat accordingly
					switch(nestedClass["__type"].Value) {
					case "Date":
						dataType = MoBackValueType.Date;
						return MoBackDate.DateFromMoBackJSON(node);
					case "Pointer":
						dataType = MoBackValueType.Pointer;
						return MoBackPointer.PointerFromMoBackJSON(node);
					case "Relation":
						dataType = MoBackValueType.Relation;
						return MoBackRelation.RelationFromMoBackJSON(node);
                    case "File":
						dataType = MoBackValueType.File;
						return MoBackFile.FromJSON(node);
					default:
						//not familiar with this special type; fall back to parsing as regular object
						if(MoBack.MoBackAppSettings.loggingLevel >= MoBack.MoBackAppSettings.LoggingLevel.WARNINGS) {
							Debug.LogWarning("Unrecognized MoBack reserved type '"+nestedClass["__type"]+"'.");
						}
						dataType = MoBackValueType.MoBackObject;
						return MoBackObject.FromJSON(nestedClass);
					}
				} 
				else 
				{
					dataType = MoBackValueType.MoBackObject;
					return MoBackObject.FromJSON(nestedClass);
				}
			} 
			else if(node.valueType == typeof(SimpleJSONArray)) 
			{
				dataType = MoBackValueType.Array;
				return MoBackArray.FromJSON((SimpleJSONArray)node);
			}

			throw new ArgumentException ("JSON data type not supported in some manner.", "SimpleJsonNode node");
		}


		public static MoBackValueType ExtractMobackType(ref object obj) {
			Type objType = obj.GetType();
			if(MoBackAppSettings.doublePrecisionFloatingPoint && objType == typeof(double)) 
			{
				obj = new MoBackNumber((double)obj);
				return MoBackValueType.Number;
			}
			else if(!MoBackAppSettings.doublePrecisionFloatingPoint && objType == typeof(float))
			{
				obj = new MoBackNumber((float)obj);
				return MoBackValueType.Number;
			}
			else if (objType == typeof(int)) 
			{
				
				obj = new MoBackNumber((int)obj);
				return MoBackValueType.Number;
			}
			else if (objType == typeof(MoBackNumber))
			{
				return MoBackValueType.Number;
			}
			else if (objType == typeof(bool)) 
			{
				return MoBackValueType.Boolean;
			} 
			else if(objType == typeof(string)) 
			{
				return MoBackValueType.String;
			} 
			else if(objType == typeof(DateTime))
			{
				return MoBackValueType.Date;
			}
			else if(objType == typeof(MoBackFile))
			{
				return MoBackValueType.File;
			}
			else if(objType == typeof(MoBackPointer))
			{
				return MoBackValueType.Pointer;
			}
			else if(objType == typeof(MoBackRelation))
			{
				return MoBackValueType.Relation;
            }
            else if(objType == typeof(MoBackGeoPoint))
			{
				return MoBackValueType.GeoPoint;
			}
			else if(objType == typeof(MoBackObject) || objType == typeof(MoBackRow)) 
			{
				return MoBackValueType.MoBackObject;
			} 
			else if(objType == typeof(MoBackArray)) 
			{
				return MoBackValueType.Array;
			}

			throw new ArgumentException ("Object of a data type not supported by Moback.", "object obj");
		}

	}
}