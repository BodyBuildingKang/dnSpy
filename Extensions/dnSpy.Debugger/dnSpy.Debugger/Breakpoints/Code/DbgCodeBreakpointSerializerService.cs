﻿/*
    Copyright (C) 2014-2017 de4dot@gmail.com

    This file is part of dnSpy

    dnSpy is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    dnSpy is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with dnSpy.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.ComponentModel.Composition;
using dnSpy.Contracts.App;
using dnSpy.Contracts.Debugger.Breakpoints.Code;
using dnSpy.Contracts.MVVM;
using dnSpy.Contracts.Settings;

namespace dnSpy.Debugger.Breakpoints.Code {
	abstract class DbgCodeBreakpointSerializerService {
		public abstract void Save(DbgCodeBreakpoint[] breakpoints);
	}

	[Export(typeof(DbgCodeBreakpointSerializerService))]
	sealed class DbgCodeBreakpointSerializerServiceImpl : DbgCodeBreakpointSerializerService {
		readonly Lazy<ISettingsServiceFactory> settingsServiceFactory;
		readonly Lazy<DbgBreakpointLocationSerializerService> dbgBreakpointLocationSerializerService;
		readonly IPickSaveFilename pickSaveFilename;
		readonly IMessageBoxService messageBoxService;

		[ImportingConstructor]
		DbgCodeBreakpointSerializerServiceImpl(Lazy<ISettingsServiceFactory> settingsServiceFactory, Lazy<DbgBreakpointLocationSerializerService> dbgBreakpointLocationSerializerService, IPickSaveFilename pickSaveFilename, IMessageBoxService messageBoxService) {
			this.settingsServiceFactory = settingsServiceFactory;
			this.dbgBreakpointLocationSerializerService = dbgBreakpointLocationSerializerService;
			this.pickSaveFilename = pickSaveFilename;
			this.messageBoxService = messageBoxService;
		}

		public override void Save(DbgCodeBreakpoint[] breakpoints) {
			if (breakpoints == null)
				throw new ArgumentNullException(nameof(breakpoints));
			if (breakpoints.Length == 0)
				return;
			var filename = pickSaveFilename.GetFilename(null, "xml", PickFilenameConstants.XmlFilenameFilter);
			if (filename == null)
				return;
			var settingsService = settingsServiceFactory.Value.Create();
			new BreakpointsSerializer(settingsService, dbgBreakpointLocationSerializerService.Value).Save(breakpoints);
			try {
				settingsService.Save(filename);
			}
			catch (Exception ex) {
				messageBoxService.Show(ex);
			}
		}
	}
}