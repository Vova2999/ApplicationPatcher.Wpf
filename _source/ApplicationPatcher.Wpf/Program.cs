using System;
using System.Linq;
using ApplicationPatcher.Core;
using ApplicationPatcher.Core.Helpers;
using Ninject;

namespace ApplicationPatcher.Wpf {
	public static class Program {
		private static readonly Log log = Log.For(typeof(Program));

		[DoNotAddLogOffset]
		public static void Main(string[] args) {
			try {
				Run(args.FirstOrDefault());
			}
			catch (Exception exception) {
				log.Fatal(exception);
				throw;
			}
		}

		[DoNotAddLogOffset]
		private static void Run(string applicationPath) {
			var container = new StandardKernel(new ApplicationPatcherWpfNinjectModule());
			container.Get<ApplicationPatcherProcessor>().PatchApplication(applicationPath);
		}
	}
}