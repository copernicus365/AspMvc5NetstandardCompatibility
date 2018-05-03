// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;

namespace Microsoft.CodeDom.Providers.DotNetCompilerPlatform {
    static class AppSettings {
        private static volatile bool _settingsInitialized;
        private static object _lock = new object();

        private static void LoadSettings(NameValueCollection appSettings) {
            string disableProfilingDuringCompilation = appSettings["aspnet:DisableProfilingDuringCompilation"];

            if (!bool.TryParse(disableProfilingDuringCompilation, out _disableProfilingDuringCompilation)) {
                _disableProfilingDuringCompilation = true;
            }

            _roslynCompilerLocation =  appSettings["aspnet:RoslynCompilerLocation"];
        }

        private static void EnsureSettingsLoaded() {
            if (_settingsInitialized) {
                return;
            }

            lock (_lock) {
                if (!_settingsInitialized) {
                    try {
                        LoadSettings(WebConfigurationManager.AppSettings);
                    }
                    finally {
                        _settingsInitialized = true;
                    }
                }
            }
        }

        private static bool _disableProfilingDuringCompilation = true;
        public static bool DisableProfilingDuringCompilation {
            get {
                EnsureSettingsLoaded();
                return _disableProfilingDuringCompilation;
            }
        }

        private static string _roslynCompilerLocation = string.Empty;
        public static string RoslynCompilerLocation {
            get {
                EnsureSettingsLoaded();
                return _roslynCompilerLocation;
            }
        }
    }
}
