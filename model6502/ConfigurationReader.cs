namespace Model
{
	using System.Globalization;
	using System.IO;
	using System.Runtime.Serialization.Json;
	using System.Xml.Linq;
	using System.Xml.XPath;

	public class ConfigurationReader
	{
		private XElement root;

		public ConfigurationReader(string path)
		{
			using (var input = File.Open(path, FileMode.Open))
			{
				using (var reader = JsonReaderWriterFactory.CreateJsonReader(input, new System.Xml.XmlDictionaryReaderQuotas()))
				{
					this.root = XElement.Load(reader);
				}
			}
		}

		#region Public member accessors

		public bool GetBooleanValue(string path, bool defaultValue)
		{
			return GetBooleanValue(this.root, path, defaultValue);
		}

		public bool GetBooleanValue(string path)
		{
			return this.GetBooleanValue(path, false);
		}

		public byte GetByteValue(string path, byte defaultValue)
		{
			return GetByteValue(this.root, path, defaultValue);
		}

		public byte GetByteValue(string path)
		{
			return this.GetByteValue(path, 0);
		}

		public ushort GetUShortValue(string path, ushort defaultValue)
		{
			return GetUShortValue(this.root, path, defaultValue);
		}

		public ushort GetUShortValue(string path)
		{
			return this.GetUShortValue(path, 0);
		}

		public int GetIntValue(string path, int defaultValue)
		{
			return GetIntValue(this.root, path, defaultValue);
		}

		public int GetIntValue(string path)
		{
			return this.GetIntValue(path, 0);
		}

		public double GetDoubleValue(string path, double defaultValue)
		{
			return GetDoubleValue(this.root, path, defaultValue);
		}

		public double GetDoubleValue(string path)
		{
			return this.GetDoubleValue(path, 0.0);
		}

		public string GetStringValue(string path, string defaultValue)
		{
			return GetStringValue(this.root, path, defaultValue);
		}

		public string GetStringValue(string path)
		{
			return this.GetStringValue(path, string.Empty);
		}

		#endregion

		#region Private xelement accessors

		private static bool GetBooleanValue(XElement root, string path, bool defaultValue)
		{
			var value = GetStringValue(root, path);
			return string.IsNullOrEmpty(value) ? defaultValue : bool.Parse(value);
		}

		private static bool GetBooleanValue(XElement root, string path)
		{
			return GetBooleanValue(root, path, false);
		}

		private static byte GetByteValue(XElement root, string path, byte defaultValue)
		{
			var value = GetStringValue(root, path);
			return string.IsNullOrEmpty(value) ? defaultValue : byte.Parse(value, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
		}

		private static byte GetByteValue(XElement root, string path)
		{
			return GetByteValue(root, path, 0);
		}

		private static ushort GetUShortValue(XElement root, string path, ushort defaultValue)
		{
			var value = GetStringValue(root, path);
			return string.IsNullOrEmpty(value) ? defaultValue : ushort.Parse(value, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
		}

		private static ushort GetUShortValue(XElement root, string path)
		{
			return GetUShortValue(root, path, 0);
		}

		private static int GetIntValue(XElement root, string path, int defaultValue)
		{
			var value = GetStringValue(root, path);
			return string.IsNullOrEmpty(value) ? defaultValue : int.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);
		}

		private static int GetIntValue(XElement root, string path)
		{
			return GetIntValue(root, path, 0);
		}

		private static double GetDoubleValue(XElement root, string path, double defaultValue)
		{
			var value = GetStringValue(root, path);
			return string.IsNullOrEmpty(value) ? defaultValue : double.Parse(value, NumberStyles.Float, CultureInfo.InvariantCulture);
		}

		private static double GetDoubleValue(XElement root, string path)
		{
			return GetDoubleValue(root, path, 0.0);
		}

		private static string GetStringValue(XElement root, string path, string defaultValue)
		{
			var element = root.XPathSelectElement(path);
			return element == null ? defaultValue : element.Value;
		}

		private static string GetStringValue(XElement root, string path)
		{
			return GetStringValue(root, path, string.Empty);
		}

		#endregion
	}
}