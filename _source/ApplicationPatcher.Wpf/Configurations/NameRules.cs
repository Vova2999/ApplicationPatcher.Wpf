using System.Xml.Serialization;

namespace ApplicationPatcher.Wpf.Configurations {
	public class NameRules {
		[XmlAttribute]
		public string Prefix { get; set; }

		[XmlAttribute]
		public string Suffix { get; set; }

		[XmlAttribute]
		public NameRulesType Type { get; set; }
	}
}