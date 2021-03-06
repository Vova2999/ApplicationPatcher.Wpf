﻿using System;
using System.Linq;
using ApplicationPatcher.Core;
using ApplicationPatcher.Core.Logs;
using Ninject;

namespace ApplicationPatcher.Wpf {
	public static class Program {
		private static readonly ILog Log = Core.Logs.Log.For(typeof(Program));

		public static void Main(string[] args) {
			try {
				Run(args.FirstOrDefault());
			}
			catch (Exception exception) {
				Log.Fatal(exception);
				throw;
			}
		}

		private static void Run(string applicationPath) {
			var container = new StandardKernel(new ApplicationPatcherWpfNinjectModule());
			container.Get<ApplicationPatcherCore>().PatchApplication(applicationPath);
		}
	}
}