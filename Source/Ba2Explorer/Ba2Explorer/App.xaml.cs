﻿using Ba2Explorer.Logging;
using Ba2Explorer.Settings;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using System.Security;
using Ba2Explorer.Utility;

namespace Ba2Explorer
{
    public partial class App : Application
    {
        internal static ILogger Logger { get; private set; }

        static readonly string associateExtension = ".ba2";
        static readonly string associateKeyName = "BA2Explorer.ba2";
        static readonly string associateExePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
        static readonly string associateFriendlyName = "Bethesda Archive 2";

        public App()
        {
            AppSettings.Load("prefs.toml");

            if (AppSettings.Instance.Logger.IsEnabled)
            {
                FileStream file = null;
                try
                {
                    file = File.Open(AppSettings.Instance.Logger.LogFilePath, FileMode.Append);

                    Logger = new FileLogger(file)
                    {
                        LogMaxSize = AppSettings.Instance.Logger.LogMaxSize
                    };
                }
                catch (Exception e)
                {
                    MessageBox.Show($"Error while setting up logger: { e.Message }", "Error", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    Logger = new NullLogger();
                }
            }
            else
            {
                Logger = new NullLogger();
            }

            Logger = Logger;

            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            this.Activated += App_Activated;
        }

        private void App_Activated(object sender, EventArgs unused)
        {
            HandleArguments();
            this.Activated -= App_Activated;
        }

        private void HandleArguments()
        {
            string[] args = Environment.GetCommandLineArgs();
            if (args.Contains("/associate-extension"))
            {
                if (UACElevationHelper.IsRunAsAdmin() && UACElevationHelper.IsProcessElevated())
                {
                    if (!AssociateBA2Extension())
                        MessageBox.Show("Associating extension was not successful.");
                    else
                        MessageBox.Show("Successfully associated extension!", "Success", MessageBoxButton.OK,
                            MessageBoxImage.Information);
                }
                else
                    MessageBox.Show("Cannot associate extension without admin rights and elevated process.");
            }
            else if (args.Contains("/unassociate-extension"))
            {
                if (UACElevationHelper.IsRunAsAdmin() && UACElevationHelper.IsProcessElevated())
                {
                    if (!UnassociateBA2Extension())
                        MessageBox.Show("Unassociating extension was not successfull.");
                    else
                        MessageBox.Show("Successfully unassociated extension!", "Success", MessageBoxButton.OK,
                            MessageBoxImage.Information);
                }
                else
                    MessageBox.Show("Cannot unassociate extension without admin rights and elevated process.");
            }
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            if (Logger != null)
            {
                try
                {
                    Logger.Log(LogPriority.Error, "!!! Unhandled exception, dispatcher: {0}", e.Dispatcher.ToString());
                    LogException(Logger, e.Exception);
                }
                catch { }
            }

            e.Handled = false;
        }

        internal static void LogException(ILogger logger, Exception e)
        {
            if (e == null)
            {
                logger.Log(LogPriority.Error, "Exception instance is null.");
            }
            else
            {
                logger.Log(LogPriority.Error, "!!! Catched {0}.", e.GetType().FullName);
                logger.Log(LogPriority.Error, "!!! Unhandled exception:{0}{4}source: {3}{4}target site: {1}{4}stack trace:{4}{2}{4}",
                    e.Message,
                    e.TargetSite,
                    e.StackTrace,
                    e.Source,
                    Environment.NewLine);

                if (e.InnerException != null)
                {
                    logger.Log(LogPriority.Error, "!!! Inner exception: ");
                    LogException(logger, e.InnerException);
                }
            }
        }

        internal static bool AssociateBA2Extension()
        {
            try
            {
                RegistryKey BaseKey;
                RegistryKey OpenMethod;
                RegistryKey Shell;

                BaseKey = Registry.ClassesRoot.CreateSubKey(associateExtension);
                BaseKey.SetValue("", associateKeyName);

                OpenMethod = Registry.ClassesRoot.CreateSubKey(associateKeyName);
                OpenMethod.SetValue("", associateFriendlyName);
                OpenMethod.CreateSubKey("DefaultIcon").SetValue("", "\"" + associateExePath + "\",0");
                Shell = OpenMethod.CreateSubKey("Shell");
                Shell.CreateSubKey("edit").CreateSubKey("command").SetValue("", "\"" + associateExePath + "\"" + " \"%1\"");
                Shell.CreateSubKey("open").CreateSubKey("command").SetValue("", "\"" + associateExePath + "\"" + " \"%1\"");
                BaseKey.Close();
                OpenMethod.Close();
                Shell.Close();

                // Tell explorer the file association has been changed
                NativeMethods.SHChangeNotify(0x08000000, 0x0000, IntPtr.Zero, IntPtr.Zero);

                return true;
            }

            catch (SecurityException)
            {
                return false;
            }
        }

        internal static bool UnassociateBA2Extension()
        {
            try
            {
                using (var baseKey = Registry.ClassesRoot.OpenSubKey(associateExtension, true))
                {
                    baseKey.DeleteValue("", false);
                }

                Registry.ClassesRoot.DeleteSubKeyTree(associateKeyName, false);

                NativeMethods.SHChangeNotify(0x08000000, 0x0000, IntPtr.Zero, IntPtr.Zero);

                return true;
            }
            catch (SecurityException)
            {
                return false;
            }
        }

        internal static bool IsAssociatedExtension()
        {
            const string keyName = "BA2Explorer.ba2";

            return Registry.ClassesRoot.OpenSubKey(keyName) != null;
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            AppSettings.Save("prefs.toml");
            if (Logger != null)
                Logger.Log(LogPriority.Info, "App closed");

            Logger?.Dispose();
            Logger = null;
            this.DispatcherUnhandledException -= App_DispatcherUnhandledException;
        }
    }
}
