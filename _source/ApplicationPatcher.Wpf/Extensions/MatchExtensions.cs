using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ApplicationPatcher.Wpf.Extensions {
	public static class MatchExtensions {
		public static IEnumerable<string> GetValues(this Match match, string groupName) {
			var captureCollection = match.Groups[groupName].Captures;
			return Enumerable.Range(0, captureCollection.Count).Select(x => captureCollection[x].Value);
		}
	}
}