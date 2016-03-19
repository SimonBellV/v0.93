#if !UNITY_WEBPLAYER
#define USE_FileIO
#endif

/* * * * *
 * A simple JSON Parser / builder
 * ------------------------------
 * 
 * It mainly has been written as a simple JSON parser. It can build a JSON string
 * from the node-tree, or generate a node tree from any valid JSON string.
 * 
 * If you want to use compression when saving to file / stream / B64 you have to include
 * SharpZipLib ( http://www.icsharpcode.net/opensource/sharpziplib/ ) in your project and
 * define "USE_SharpZipLib" at the top of the file
 * 
 * Written by Bunny83 
 * 2012-06-09
 * 
 * Features / attributes:
 * - provides strongly typed node classes and lists / dictionaries
 * - provides easy access to class members / array items / data values
 * - the parser ignores data types. Each value is a string.
 * - only double quotes (") are used for quoting strings.
 * - values and names are not restricted to quoted strings. They simply add up and are trimmed.
 * - There are only 3 types: arrays(JSONArray), objects(JSONClass) and values(JSONData)
 * - provides "casting" properties to easily convert to / from those types:
 *   int / float / double / bool
 * - provides a common interface for each node so no explicit casting is required.
 * - the parser try to avoid errors, but if malformed JSON is parsed the result is undefined
 * 
 * 
 * 2012-12-17 Update:
 * - Added internal JSONLazyCreator class which simplifies the construction of a JSON tree
 *   Now you can simple reference any item that doesn't exist yet and it will return a JSONLazyCreator
 *   The class determines the required type by it's further use, creates the type and removes itself.
 * - Added binary serialization / deserialization.
 * - Added support for BZip2 zipped binary format. Requires the SharpZipLib ( http://www.icsharpcode.net/opensource/sharpziplib/ )
 *   The usage of the SharpZipLib library can be disabled by removing or commenting out the USE_SharpZipLib define at the top
 * - The serializer uses different types when it comes to store the values. Since my data values
 *   are all of type string, the serializer will "try" which format fits best. The order is: int, float, double, bool, string.
 *   It's not the most efficient way but for a moderate amount of data it should work on all platforms.
 * 
 * * * * */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


namespace SimpleJSON
{
	static class QuoteUtils {
		// custom method to fix garbage quotes around everything
		public static string FixQuotes(string toParse, Type valueType) {
			bool useQuotes = true;

			if (valueType != typeof(string)) {
				int i = 0;
				if (int.TryParse(toParse, out i)) {
					useQuotes = false;
				}

				float f = 0;
				if (float.TryParse(toParse, out f)) {
					useQuotes = false;
				}

				double d = 0;
				if (double.TryParse(toParse, out d)) {
					useQuotes = false;
				}

				bool b = false;
				if (bool.TryParse(toParse, out b)) {
					useQuotes = false;
				}
			}

			string output = "";
			if (useQuotes) {
				output += "\"";
			}

			output += toParse;

			if (useQuotes) {
				output += "\"";
			}

			return output;
		}
	}

	public enum SimpleJSONBinaryTag
	{
		Array            = 1,
		Class            = 2,
		Value            = 3,
		IntValue        = 4,
		DoubleValue        = 5,
		BoolValue        = 6,
		FloatValue        = 7,
	}
	
	public class SimpleJSONNode
	{
		#region common interface
		public Type valueType = typeof(string);

		public virtual void Add(string aKey, SimpleJSONNode aItem){ }
		public virtual SimpleJSONNode this[int aIndex]   { get { return null; } set { } }
		public virtual SimpleJSONNode this[string aKey]  { get { return null; } set { } }
		public virtual string Value                { get { return "";   } set { } }
		public virtual int Count                   { get { return 0;    } }
		
		public virtual void Add(SimpleJSONNode aItem)
		{
			Add("", aItem);
		}
		
		public virtual SimpleJSONNode Remove(string aKey) { return null; }
		public virtual SimpleJSONNode Remove(int aIndex) { return null; }
		public virtual SimpleJSONNode Remove(SimpleJSONNode aNode) { return aNode; }
		
		public virtual IEnumerable<SimpleJSONNode> Childs { get { yield break;} }
		public IEnumerable<SimpleJSONNode> DeepChilds
		{
			get
			{
				foreach (var C in Childs)
					foreach (var D in C.DeepChilds)
						yield return D;
			}
		}
		
		public override string ToString()
		{
			return "JSONNode";
		}
		public virtual string ToString(string aPrefix)
		{
			return "JSONNode";
		}
		
		#endregion common interface
		
		#region typecasting properties
		public virtual int AsInt
		{
			get
			{
				int v = 0;
				if (int.TryParse(Value,out v))
					return v;
				return 0;
			}
			set
			{
				valueType = value.GetType();
				Value = value.ToString();
			}
		}
		public virtual float AsFloat
		{
			get
			{
				float v = 0.0f;
				if (float.TryParse(Value,out v))
					return v;
				return 0.0f;
			}
			set
			{
				valueType = value.GetType();
				Value = value.ToString();
			}
		}
		public virtual double AsDouble
		{
			get
			{
				double v = 0.0;
				if (double.TryParse(Value,out v))
					return v;
				return 0.0;
			}
			set
			{
				valueType = value.GetType();
				Value = value.ToString();
			}
		}
		public virtual bool AsBool
		{
			get
			{
				bool v = false;
				if (bool.TryParse(Value,out v))
					return v;
				return !string.IsNullOrEmpty(Value);
			}
			set
			{
				valueType = value.GetType();
				Value = (value)?"true":"false";
			}
		}
		public virtual SimpleJSONArray AsArray
		{
			get
			{
				return this as SimpleJSONArray;
			}
		}
		public virtual SimpleJSONClass AsObject
		{
			get
			{
				return this as SimpleJSONClass;
			}
		}
		
		
		#endregion typecasting properties
		
		#region operators
		public static implicit operator SimpleJSONNode(string s)
		{
			return new SimpleJSONData(s);
		}
		public static implicit operator SimpleJSONNode(bool b)
		{
			return new SimpleJSONData(b);
		}
		public static implicit operator SimpleJSONNode(int i)
		{
			return new SimpleJSONData(i);
		}
		public static implicit operator SimpleJSONNode(float f)
		{
			return new SimpleJSONData(f);
		}
		public static implicit operator SimpleJSONNode(double d)
		{
			return new SimpleJSONData(d);
		}
		public static implicit operator string(SimpleJSONNode d)
		{
			return (d == null)?null:d.Value;
		}
		public static bool operator ==(SimpleJSONNode a, object b)
		{
			if (b == null && a is SimpleJSONLazyCreator)
				return true;
			return System.Object.ReferenceEquals(a,b);
		}
		
		public static bool operator !=(SimpleJSONNode a, object b)
		{
			return !(a == b);
		}
		public override bool Equals (object obj)
		{
			return System.Object.ReferenceEquals(this, obj);
		}
		public override int GetHashCode ()
		{
			return base.GetHashCode();
		}
		
		
		#endregion operators
		
		internal static string Escape(string aText)
		{
			string result = "";
			foreach(char c in aText)
			{
				switch(c)
				{
				case '\\' : result += "\\\\"; break;
				case '\"' : result += "\\\""; break;
				case '\n' : result += "\\n" ; break;
				case '\r' : result += "\\r" ; break;
				case '\t' : result += "\\t" ; break;
				case '\b' : result += "\\b" ; break;
				case '\f' : result += "\\f" ; break;
				default   : result += c     ; break;
				}
			}
			return result;
		}

		//Modified to be more type-sensitive for use with Moback
		public static SimpleJSONNode Parse(string aJSON)
		{

			Stack<SimpleJSONNode> stack = new Stack<SimpleJSONNode>();
			SimpleJSONNode ctx = null;
			int i = 0;
			string Token = "";
			string TokenName = "";
			bool QuoteMode = false;
			bool LastTokenHadQuotes = false; //Added for context-dependent parsing of variables in MoBack
			bool LastTokenHadDecimal = false;
			while (i < aJSON.Length)
			{
				switch (aJSON[i])
				{
				case '{':
					if (QuoteMode)
					{
						Token += aJSON[i];
						break;
					}
					stack.Push(new SimpleJSONClass());
					if (ctx != null)
					{
						TokenName = TokenName.Trim();
						if (ctx is SimpleJSONArray)
							ctx.Add(stack.Peek());
						else if (TokenName != "")
							ctx.Add(TokenName,stack.Peek());

						LastTokenHadQuotes = false;
						LastTokenHadDecimal = false;
					}
					TokenName = "";
					Token = "";
					ctx = stack.Peek();
					break;
					
				case '[':
					if (QuoteMode)
					{
						Token += aJSON[i];
						break;
					}
					
					stack.Push(new SimpleJSONArray());
					if (ctx != null)
					{
						TokenName = TokenName.Trim();
						if (ctx is SimpleJSONArray)
							ctx.Add(stack.Peek());
						else if (TokenName != "")
							ctx.Add(TokenName,stack.Peek());
						
						LastTokenHadQuotes = false;
						LastTokenHadDecimal = false;
					}
					TokenName = "";
					Token = "";
					ctx = stack.Peek();
					break;
					
				case '}':
				case ']':
					if (QuoteMode)
					{
						Token += aJSON[i];
						break;
					}
					if (stack.Count == 0)
						throw new Exception("JSON Parse: Too many closing brackets");
					
					stack.Pop();
					if (Token != "")
					{
						TokenName = TokenName.Trim();
						if(LastTokenHadQuotes) {
							if (ctx is SimpleJSONArray)
								ctx.Add(Token);
							else if (TokenName != "")
								ctx.Add(TokenName, Token);
							
							LastTokenHadQuotes = false;
						} else {
							SimpleJSONNode data = null;
							//check for number, double vs. float really has to be decided globally because there's no real way to tell from a string
							if((Token[0] >= '0' && Token[0] <= '9') || Token[0] == '-') {
								if(LastTokenHadDecimal) {
									if(SimpleJSON.assumeDoublePrecisionFloatingPoint)
									{
										double d;
										if(double.TryParse(Token, out d)) {
											data = d;
										} else {
											data = Token; //give up and treat as string
										}
									}
									else
									{
										float f;
										if(float.TryParse(Token, out f)) {
											data = f;
										} else {
											data = Token; //give up and treat as string
										}
									}
									LastTokenHadDecimal = false;
								} else {
									int j;
									if(int.TryParse(Token, out j)) {
										data = j;
									} else {
										data = Token; //give up and treat as string
									}
								}
							} else {
								//assume is bool
								bool b;
								if(bool.TryParse(Token, out b)) {
									data = b;
								} else {
									data = Token; //give up and treat as string
								}
							}
							
							if (ctx is SimpleJSONArray)
								ctx.Add(data);
							else if (TokenName != "")
								ctx.Add(TokenName, data);
						}
					}
					TokenName = "";
					Token = "";
					if (stack.Count>0)
						ctx = stack.Peek();
					break;
					
				case ':':
					if (QuoteMode)
					{
						Token += aJSON[i];
						break;
					}
					TokenName = Token;
					Token = "";
					LastTokenHadQuotes = false;
					LastTokenHadDecimal = false;
					break;
					
				case '"':
					LastTokenHadQuotes = QuoteMode; //note that this is efecitvely just set to true when exiting quote mode
					QuoteMode ^= true;
					break;
					
				case ',':
					if (QuoteMode)
					{
						Token += aJSON[i];
						break;
					}
					if (Token != "")
					{
						if(LastTokenHadQuotes) {
							if (ctx is SimpleJSONArray)
								ctx.Add(Token);
							else if (TokenName != "")
								ctx.Add(TokenName, Token);
							
							LastTokenHadQuotes = false;
						} else {
							SimpleJSONNode data = null;
							//check for number, simplify to double
							if((Token[0] >= '0' && Token[0] <= '9') || Token[0] == '-') {
								if(LastTokenHadDecimal) {
									if(SimpleJSON.assumeDoublePrecisionFloatingPoint)
									{
										double d;
										if(double.TryParse(Token, out d)) {
											data = d;
										} else {
											data = Token; //give up and treat as string
										}
									}
									else
									{
										float f;
										if(float.TryParse(Token, out f)) {
											data = f;
										} else {
											data = Token; //give up and treat as string
										}
									}
									LastTokenHadDecimal = false;
								} else {
									int j;
									if(int.TryParse(Token, out j)) {
										data = j;
									} else {
										data = Token; //give up and treat as string
									}
								}
							} else {
								//assume is bool
								bool b;
								if(bool.TryParse(Token, out b)) {
									data = b;
								} else {
									data = Token; //give up and treat as string
								}
							}
							
							if (ctx is SimpleJSONArray)
								ctx.Add(data);
							else if (TokenName != "")
								ctx.Add(TokenName, data);
						}
					}
					TokenName = "";
					Token = "";
					break;

				case '.':
					if (! QuoteMode)
						LastTokenHadDecimal = true;
					Token += aJSON[i];
					break;
					
				case '\r':
				case '\n':
					break;
					
				case ' ':
				case '\t':
					if (QuoteMode)
						Token += aJSON[i];
					break;
					
				case '\\':
					++i;
					if (QuoteMode)
					{
						char C = aJSON[i];
						switch (C)
						{
						case 't' : Token += '\t'; break;
						case 'r' : Token += '\r'; break;
						case 'n' : Token += '\n'; break;
						case 'b' : Token += '\b'; break;
						case 'f' : Token += '\f'; break;
						case 'u':
						{
							string s = aJSON.Substring(i+1,4);
							Token += (char)int.Parse(s, System.Globalization.NumberStyles.AllowHexSpecifier);
							i += 4;
							break;
						}
						default  : Token += C; break;
						}
					}
					break;
					
				default:
					Token += aJSON[i];
					break;
				}
				++i;
			}
			if (QuoteMode)
			{
				throw new Exception("JSON Parse: Quotation marks seems to be messed up.");
			}
			return ctx;
		}
		
		public virtual void Serialize(System.IO.BinaryWriter aWriter) {}
		
		public void SaveToStream(System.IO.Stream aData)
		{
			var W = new System.IO.BinaryWriter(aData);
			Serialize(W);
		}
		
		public void SaveToFile(string aFileName)
		{
			#if USE_FileIO
			System.IO.Directory.CreateDirectory((new System.IO.FileInfo(aFileName)).Directory.FullName);
			using(var F = System.IO.File.OpenWrite(aFileName))
			{
				SaveToStream(F);
			}
			#else
			throw new Exception("Can't use File IO stuff in webplayer");
			#endif
		}
		public string SaveToBase64()
		{
			using (var stream = new System.IO.MemoryStream())
			{
				SaveToStream(stream);
				stream.Position = 0;
				return System.Convert.ToBase64String(stream.ToArray());
			}
		}
		public static SimpleJSONNode Deserialize(System.IO.BinaryReader aReader)
		{
			SimpleJSONBinaryTag type = (SimpleJSONBinaryTag)aReader.ReadByte();
			switch(type)
			{
			case SimpleJSONBinaryTag.Array:
			{
				int count = aReader.ReadInt32();
				SimpleJSONArray tmp = new SimpleJSONArray();
				for(int i = 0; i < count; i++)
					tmp.Add(Deserialize(aReader));
				return tmp;
			}
			case SimpleJSONBinaryTag.Class:
			{
				int count = aReader.ReadInt32();                
				SimpleJSONClass tmp = new SimpleJSONClass();
				for(int i = 0; i < count; i++)
				{
					string key = aReader.ReadString();
					var val = Deserialize(aReader);
					tmp.Add(key, val);
				}
				return tmp;
			}
			case SimpleJSONBinaryTag.Value:
			{
				return new SimpleJSONData(aReader.ReadString());
			}
			case SimpleJSONBinaryTag.IntValue:
			{
				return new SimpleJSONData(aReader.ReadInt32());
			}
			case SimpleJSONBinaryTag.DoubleValue:
			{
				return new SimpleJSONData(aReader.ReadDouble());
			}
			case SimpleJSONBinaryTag.BoolValue:
			{
				return new SimpleJSONData(aReader.ReadBoolean());
			}
			case SimpleJSONBinaryTag.FloatValue:
			{
				return new SimpleJSONData(aReader.ReadSingle());
			}
				
			default:
			{
				throw new Exception("Error deserializing JSON. Unknown tag: " + type);
			}
			}
		}
		
		public static SimpleJSONNode LoadFromStream(System.IO.Stream aData)
		{
			using(var R = new System.IO.BinaryReader(aData))
			{
				return Deserialize(R);
			}
		}
		public static SimpleJSONNode LoadFromFile(string aFileName)
		{
			#if USE_FileIO
			using(var F = System.IO.File.OpenRead(aFileName))
			{
				return LoadFromStream(F);
			}
			#else
			throw new Exception("Can't use File IO stuff in webplayer");
			#endif
		}
		public static SimpleJSONNode LoadFromBase64(string aBase64)
		{
			var tmp = System.Convert.FromBase64String(aBase64);
			var stream = new System.IO.MemoryStream(tmp);
			stream.Position = 0;
			return LoadFromStream(stream);
		}
	} // End of JSONNode
	
	public class SimpleJSONArray : SimpleJSONNode, IEnumerable
	{
		private List<SimpleJSONNode> m_List = new List<SimpleJSONNode>();

		public SimpleJSONArray() {
			this.valueType = typeof(SimpleJSONArray);
		}

		public override SimpleJSONNode this[int aIndex]
		{
			get
			{
				if (aIndex<0 || aIndex >= m_List.Count)
					return new SimpleJSONLazyCreator(this);
				return m_List[aIndex];
			}
			set
			{
				if (aIndex<0 || aIndex >= m_List.Count)
					m_List.Add(value);
				else
					m_List[aIndex] = value;
			}
		}
		public override SimpleJSONNode this[string aKey]
		{
			get{ return new SimpleJSONLazyCreator(this);}
			set{ m_List.Add(value); }
		}
		public override int Count
		{
			get { return m_List.Count; }
		}
		public override void Add(string aKey, SimpleJSONNode aItem)
		{
			m_List.Add(aItem);
		}
		public override SimpleJSONNode Remove(int aIndex)
		{
			if (aIndex < 0 || aIndex >= m_List.Count)
				return null;
			SimpleJSONNode tmp = m_List[aIndex];
			m_List.RemoveAt(aIndex);
			return tmp;
		}
		public override SimpleJSONNode Remove(SimpleJSONNode aNode)
		{
			m_List.Remove(aNode);
			return aNode;
		}
		public override IEnumerable<SimpleJSONNode> Childs
		{
			get
			{
				foreach(SimpleJSONNode N in m_List)
					yield return N;
			}
		}
		public IEnumerator GetEnumerator()
		{
			foreach(SimpleJSONNode N in m_List)
				yield return N;
		}
		public override string ToString()
		{
			string result = "[";
			foreach (SimpleJSONNode N in m_List)
			{
				if (result.Length > 2)
					result += ",";
				result += N.ToString();
			}
			result += "]";
			return result;
		}
		public override string ToString(string aPrefix)
		{
			string result = "[";
			foreach (SimpleJSONNode N in m_List)
			{
				if (result.Length > 3)
					result += ",";
				result += "\n" + aPrefix + "   ";                
				result += N.ToString(aPrefix+"   ");
			}
			result += "\n" + aPrefix + "]";
			return result;
		}
		public override void Serialize (System.IO.BinaryWriter aWriter)
		{
			aWriter.Write((byte)SimpleJSONBinaryTag.Array);
			aWriter.Write(m_List.Count);
			for(int i = 0; i < m_List.Count; i++)
			{
				m_List[i].Serialize(aWriter);
			}
		}
	} // End of JSONArray
	
	public class SimpleJSONClass : SimpleJSONNode, IEnumerable
	{
		public Dictionary<string,SimpleJSONNode> dict = new Dictionary<string, SimpleJSONNode>();

		public SimpleJSONClass() {
			this.valueType = typeof(SimpleJSONClass);
		}

		public override SimpleJSONNode this[string aKey]
		{
			get
			{
				if (dict.ContainsKey(aKey))
					return dict[aKey];
				else
					return new SimpleJSONLazyCreator(this, aKey);
			}
			set
			{
				if (dict.ContainsKey(aKey))
					dict[aKey] = value;
				else
					dict.Add(aKey,value);
			}
		}
		public override SimpleJSONNode this[int aIndex]
		{
			get
			{
				if (aIndex < 0 || aIndex >= dict.Count)
					return null;
				return dict.ElementAt(aIndex).Value;
			}
			set
			{
				if (aIndex < 0 || aIndex >= dict.Count)
					return;
				string key = dict.ElementAt(aIndex).Key;
				dict[key] = value;
			}
		}
		public override int Count
		{
			get { return dict.Count; }
		}
		
		
		public override void Add(string aKey, SimpleJSONNode aItem)
		{
			if (!string.IsNullOrEmpty(aKey))
			{
				if (dict.ContainsKey(aKey))
					dict[aKey] = aItem;
				else
					dict.Add(aKey, aItem);
			}
			else
				dict.Add(Guid.NewGuid().ToString(), aItem);
		}
		
		public override SimpleJSONNode Remove(string aKey)
		{
			if (!dict.ContainsKey(aKey))
				return null;
			SimpleJSONNode tmp = dict[aKey];
			dict.Remove(aKey);
			return tmp;        
		}
		public override SimpleJSONNode Remove(int aIndex)
		{
			if (aIndex < 0 || aIndex >= dict.Count)
				return null;
			var item = dict.ElementAt(aIndex);
			dict.Remove(item.Key);
			return item.Value;
		}
		public override SimpleJSONNode Remove(SimpleJSONNode aNode)
		{
			try
			{
				var item = dict.Where(k => k.Value == aNode).First();
				dict.Remove(item.Key);
				return aNode;
			}
			catch
			{
				return null;
			}
		}
		
		public override IEnumerable<SimpleJSONNode> Childs
		{
			get
			{
				foreach(KeyValuePair<string,SimpleJSONNode> N in dict)
					yield return N.Value;
			}
		}
		
		public IEnumerator GetEnumerator()
		{
			foreach(KeyValuePair<string, SimpleJSONNode> N in dict)
				yield return N;
		}
		public override string ToString()
		{
			string result = "{";
			foreach (KeyValuePair<string, SimpleJSONNode> N in dict)
			{
				if (result.Length > 2)
					result += ",";
				result += "\"" + Escape(N.Key) + "\":" + N.Value.ToString();
			}
			result += "}";
			return result;
		}
		public override string ToString(string aPrefix)
		{
			string result = "{";
			foreach (KeyValuePair<string, SimpleJSONNode> N in dict)
			{
				if (result.Length > 3)
					result += ",";
				result += "\n" + aPrefix + "   ";
				result += "\"" + Escape(N.Key) + "\" : " + N.Value.ToString(aPrefix+"   ");
			}
			result += "\n" + aPrefix + "}";
			return result;
		}
		public override void Serialize (System.IO.BinaryWriter aWriter)
		{
			aWriter.Write((byte)SimpleJSONBinaryTag.Class);
			aWriter.Write(dict.Count);
			foreach(string K in dict.Keys)
			{
				aWriter.Write(K);
				dict[K].Serialize(aWriter);
			}
		}
	} // End of JSONClass
	
	public class SimpleJSONData : SimpleJSONNode
	{
		private string m_Data;
		public override string Value
		{
			get { return m_Data; }
			set { m_Data = value; }
		}
		public SimpleJSONData(string aData)
		{
			m_Data = aData;
		}
		public SimpleJSONData(float aData)
		{
			AsFloat = aData;
		}
		public SimpleJSONData(double aData)
		{
			AsDouble = aData;
		}
		public SimpleJSONData(bool aData)
		{
			AsBool = aData;
		}
		public SimpleJSONData(int aData)
		{
			AsInt = aData;
		}
		
		public override string ToString()
		{
			return QuoteUtils.FixQuotes(Escape (m_Data), valueType);
		}
		public override string ToString(string aPrefix)
		{
			return QuoteUtils.FixQuotes(Escape (m_Data), valueType);
		}
		public override void Serialize (System.IO.BinaryWriter aWriter)
		{
			var tmp = new SimpleJSONData("");
			
			tmp.AsInt = AsInt;
			if (tmp.m_Data == this.m_Data)
			{
				aWriter.Write((byte)SimpleJSONBinaryTag.IntValue);
				aWriter.Write(AsInt);
				return;
			}
			tmp.AsFloat = AsFloat;
			if (tmp.m_Data == this.m_Data)
			{
				aWriter.Write((byte)SimpleJSONBinaryTag.FloatValue);
				aWriter.Write(AsFloat);
				return;
			}
			tmp.AsDouble = AsDouble;
			if (tmp.m_Data == this.m_Data)
			{
				aWriter.Write((byte)SimpleJSONBinaryTag.DoubleValue);
				aWriter.Write(AsDouble);
				return;
			}
			
			tmp.AsBool = AsBool;
			if (tmp.m_Data == this.m_Data)
			{
				aWriter.Write((byte)SimpleJSONBinaryTag.BoolValue);
				aWriter.Write(AsBool);
				return;
			}
			aWriter.Write((byte)SimpleJSONBinaryTag.Value);
			aWriter.Write(m_Data);
		}
	} // End of JSONData
	
	internal class SimpleJSONLazyCreator : SimpleJSONNode
	{
		private SimpleJSONNode m_Node = null;
		private string m_Key = null;
		
		public SimpleJSONLazyCreator(SimpleJSONNode aNode)
		{
			m_Node = aNode;
			m_Key  = null;
		}
		public SimpleJSONLazyCreator(SimpleJSONNode aNode, string aKey)
		{
			m_Node = aNode;
			m_Key = aKey;
		}
		
		private void Set(SimpleJSONNode aVal)
		{
			if (m_Key == null)
			{
				m_Node.Add(aVal);
			}
			else
			{
				m_Node.Add(m_Key, aVal);
			}
			m_Node = null; // Be GC friendly.
		}
		
		public override SimpleJSONNode this[int aIndex]
		{
			get
			{
				return new SimpleJSONLazyCreator(this);
			}
			set
			{
				var tmp = new SimpleJSONArray();
				tmp.Add(value);
				Set(tmp);
			}
		}
		
		public override SimpleJSONNode this[string aKey]
		{
			get
			{
				return new SimpleJSONLazyCreator(this, aKey);
			}
			set
			{
				var tmp = new SimpleJSONClass();
				tmp.Add(aKey, value);
				Set(tmp);
			}
		}
		public override void Add (SimpleJSONNode aItem)
		{
			var tmp = new SimpleJSONArray();
			tmp.Add(aItem);
			Set(tmp);
		}
		public override void Add (string aKey, SimpleJSONNode aItem)
		{
			var tmp = new SimpleJSONClass();
			tmp.Add(aKey, aItem);
			Set(tmp);
		}
		public static bool operator ==(SimpleJSONLazyCreator a, object b)
		{
			if (b == null)
				return true;
			return System.Object.ReferenceEquals(a,b);
		}
		
		public static bool operator !=(SimpleJSONLazyCreator a, object b)
		{
			return !(a == b);
		}
		public override bool Equals (object obj)
		{
			if (obj == null)
				return true;
			return System.Object.ReferenceEquals(this, obj);
		}
		public override int GetHashCode ()
		{
			return base.GetHashCode();
		}
		
		public override string ToString()
		{
			return "";
		}
		public override string ToString(string aPrefix)
		{
			return "";
		}
		
		public override int AsInt
		{
			get
			{
				SimpleJSONData tmp = new SimpleJSONData(0);
				Set(tmp);
				return 0;
			}
			set
			{
				SimpleJSONData tmp = new SimpleJSONData(value);
				Set(tmp);
			}
		}
		public override float AsFloat
		{
			get
			{
				SimpleJSONData tmp = new SimpleJSONData(0.0f);
				Set(tmp);
				return 0.0f;
			}
			set
			{
				SimpleJSONData tmp = new SimpleJSONData(value);
				Set(tmp);
			}
		}
		public override double AsDouble
		{
			get
			{
				SimpleJSONData tmp = new SimpleJSONData(0.0);
				Set(tmp);
				return 0.0;
			}
			set
			{
				SimpleJSONData tmp = new SimpleJSONData(value);
				Set(tmp);
			}
		}
		public override bool AsBool
		{
			get
			{
				SimpleJSONData tmp = new SimpleJSONData(false);
				Set(tmp);
				return false;
			}
			set
			{
				SimpleJSONData tmp = new SimpleJSONData(value);
				Set(tmp);
			}
		}
		public override SimpleJSONArray AsArray
		{
			get
			{
				SimpleJSONArray tmp = new SimpleJSONArray();
				Set(tmp);
				return tmp;
			}
		}
		public override SimpleJSONClass AsObject
		{
			get
			{
				SimpleJSONClass tmp = new SimpleJSONClass();
				Set(tmp);
				return tmp;
			}
		}
	} // End of JSONLazyCreator
	
	public static class SimpleJSON
	{
		public static bool assumeDoublePrecisionFloatingPoint;

		public static SimpleJSONNode Parse(string aJSON)
		{
			return SimpleJSONNode.Parse(aJSON);
		}
	}
}