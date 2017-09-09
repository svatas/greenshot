﻿#region Greenshot GNU General Public License

// Greenshot - a free and open source screenshot tool
// Copyright (C) 2007-2017 Thomas Braun, Jens Klingen, Robin Krom
// 
// For more information see: http://getgreenshot.org/
// The Greenshot project is hosted on GitHub https://github.com/greenshot/greenshot
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 1 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

#endregion

#region Usings

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using GreenshotOfficePlugin.OfficeExport;
using GreenshotOfficePlugin.OfficeInterop;
using GreenshotPlugin.Core;
using GreenshotPlugin.Interfaces;
using Dapplo.Log;

#endregion

namespace GreenshotOfficePlugin.Destinations
{
	public class OneNoteDestination : AbstractDestination
	{
		private const int IconApplication = 0;
		public const string DESIGNATION = "OneNote";
		private static readonly LogSource Log = new LogSource();
		private static readonly string ExePath;
		private readonly OneNotePage _page;

		static OneNoteDestination()
		{
			ExePath = PluginUtils.GetExePath("ONENOTE.EXE");
			if (ExePath != null && !File.Exists(ExePath))
			{
				ExePath = null;
			}
		}

		public OneNoteDestination()
		{
		}

		public OneNoteDestination(OneNotePage page)
		{
			_page = page;
		}

		public override string Designation
		{
			get { return DESIGNATION; }
		}

		public override string Description
		{
			get
			{
				if (_page == null)
				{
					return "Microsoft OneNote";
				}
				return _page.DisplayName;
			}
		}

		public override int Priority => 4;

		public override bool IsDynamic => true;

		public override bool IsActive => base.IsActive && ExePath != null;

		public override Bitmap GetDisplayIcon(double dpi)
		{
			return PluginUtils.GetCachedExeIcon(ExePath, IconApplication, dpi > 100);
		}

		public override IEnumerable<IDestination> DynamicDestinations()
		{
			try
			{
				return OneNoteExporter.GetPages().Where(currentPage => currentPage.IsCurrentlyViewed).Select(currentPage => new OneNoteDestination(currentPage));
			}
			catch (COMException cEx)
			{
				if (cEx.ErrorCode == unchecked((int) 0x8002801D))
				{
					Log.Warn().WriteLine("Wrong registry keys, to solve this remove the OneNote key as described here: http://microsoftmercenary.com/wp/outlook-excel-interop-calls-breaking-solved/");
				}
				Log.Warn().WriteLine(cEx, "Problem retrieving onenote destinations, ignoring: ");
			}
			catch (Exception ex)
			{
				Log.Warn().WriteLine(ex, "Problem retrieving onenote destinations, ignoring: ");
			}
			return Enumerable.Empty<IDestination>();
		}

		public override ExportInformation ExportCapture(bool manuallyInitiated, ISurface surface, ICaptureDetails captureDetails)
		{
			var exportInformation = new ExportInformation(Designation, Description);

			if (_page == null)
			{
				try
				{
					exportInformation.ExportMade = OneNoteExporter.ExportToNewPage(surface);
				}
				catch (Exception ex)
				{
					exportInformation.ErrorMessage = ex.Message;
					Log.Error().WriteLine(ex);
				}
			}
			else
			{
				try
				{
					exportInformation.ExportMade = OneNoteExporter.ExportToPage(surface, _page);
				}
				catch (Exception ex)
				{
					exportInformation.ErrorMessage = ex.Message;
					Log.Error().WriteLine(ex);
				}
			}
			return exportInformation;
		}
	}
}