using System.Xml.Linq;

namespace HatenaBlogToHatenaBlog;

public static class AtomPub {
	public static class Namespaces {
		public static readonly XNamespace Atom = (XNamespace)"http://www.w3.org/2005/Atom";
		public static readonly XNamespace App = (XNamespace)"http://www.w3.org/2007/app";
		public static readonly XNamespace Hatena = (XNamespace)"http://www.hatena.ne.jp/info/xmlns#";

		public static readonly XNamespace Root = "http://purl.org/atom/ns#";
	}
}