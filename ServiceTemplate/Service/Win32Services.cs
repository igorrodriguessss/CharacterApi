using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Security.AccessControl;
using System.ServiceProcess;
using System.ComponentModel;
using System.IO;

namespace $safeprojectname$.Service
{
    internal static class Win32Services
    {
        #region Service Methods
        private enum SERVICE_CONFIG_INFO
        {
            DESCRIPTION = 1,
            FAILURE_ACTIONS = 2,
            DELAYED_AUTO_START_INFO = 3,
            FAILURE_ACTIONS_FLAG = 4,
            SERVICE_SID_INFO = 5,
            REQUIRED_PRIVILEGES_INFO = 6,
            PRESHUTDOWN_INFO = 7
        }

        private enum SC_ACTION_TYPE : uint
        {
            SC_ACTION_NONE = 0x00000000, // No action.
            SC_ACTION_RESTART = 0x00000001, // Restart the service.
            SC_ACTION_REBOOT = 0x00000002, // Reboot the computer.
            SC_ACTION_RUN_COMMAND = 0x00000003 // Run a command.
        }

        private struct SERVICE_FAILURE_ACTIONS
        {
            public Int32 dwResetPeriod;
            public IntPtr lpRebootMsg;
            public IntPtr lpCommand;
            public Int32 cActions;
            public IntPtr lpsaActions;
        }

        private struct SC_ACTION
        {
            public SC_ACTION_TYPE Type;
            public Int32 Delay;
        }

        private static class Win32
        {
            [DllImport("advapi32.dll", SetLastError = true)]
            public static extern bool SetServiceObjectSecurity(SafeHandle serviceHandle,
                                                                SecurityInfos secInfos,
                                                                [In] byte[] lpSecDesrBuf);

            [DllImport("advapi32.dll", SetLastError = true)]
            public static extern bool QueryServiceObjectSecurity(SafeHandle serviceHandle,
                                                                  SecurityInfos secInfo,
                                                                  [Out] byte[] lpSecDesrBuf, uint bufSize,
                                                                  out uint bufSizeNeeded);

            [DllImport("advapi32.dll", EntryPoint = "ChangeServiceConfig2W", ExactSpelling = true,
                CharSet = CharSet.Unicode, SetLastError = true)]
            private static extern int ChangeServiceConfig2(SafeHandle hService, int dwInfoLevel, IntPtr lpInfo);

            [DllImport("advapi32.dll", EntryPoint = "ChangeServiceConfigW", ExactSpelling = true,
                CharSet = CharSet.Unicode, SetLastError = true)]
            public static extern int ChangeServiceConfig(SafeHandle hService, int nServiceType, int nStartType,
                                                          int nErrorControl,
                                                          String lpBinaryPathName, String lpLoadOrderGroup,
                                                          IntPtr lpdwTagId, [In] String lpDependencies,
                                                          String lpServiceStartName,
                                                          String lpPassword, String lpDisplayName);

            public static void SetServiceConfig<T>(ServiceController sc, SERVICE_CONFIG_INFO infoId, T objData)
            {
                GCHandle hdata = GCHandle.Alloc(objData, GCHandleType.Pinned);
                try
                {
                    if (0 == ChangeServiceConfig2(sc.ServiceHandle, (int)infoId, hdata.AddrOfPinnedObject()))
                        throw new Win32Exception();
                }
                finally
                {
                    hdata.Free();
                }
            }
        }

        internal static void SetDelayAutostart(ServiceController sc, bool enabled)
        {
            Win32.SetServiceConfig(sc, SERVICE_CONFIG_INFO.DELAYED_AUTO_START_INFO, enabled ? 1 : 0);
        }

        internal static void SetShutdownTimeout(ServiceController sc, TimeSpan timeoutValue)
        {
            Win32.SetServiceConfig(sc, SERVICE_CONFIG_INFO.PRESHUTDOWN_INFO, (int)timeoutValue.TotalMilliseconds);
        }

        internal static void SetServiceExeArgs(ServiceController sc, string exePath, string arguments)
        {
            exePath = Path.GetFullPath(exePath.Trim(' ', '\'', '"'));
            string fqExec = String.Format("\"{0}\" {1}", exePath, arguments).TrimEnd();

            const int notChanged = -1;
            if (0 == Win32.ChangeServiceConfig(sc.ServiceHandle, notChanged, notChanged, notChanged, fqExec,
                                               null, IntPtr.Zero, null, null, null, null))
                throw new Win32Exception();
        }

        internal static void SetRestartOnFailure(ServiceController sc, int restartAttempts, int restartDelay, int resetFailuresDelay)
        {
            SC_ACTION[] actions =
                new SC_ACTION[3]
                    {
                        new SC_ACTION
                            {
                                Delay = Math.Max(0, Math.Min(int.MaxValue, restartDelay)),
                                Type = SC_ACTION_TYPE.SC_ACTION_RESTART
                            },
                        new SC_ACTION
                            {
                                Delay = Math.Max(0, Math.Min(int.MaxValue, restartDelay)),
                                Type = SC_ACTION_TYPE.SC_ACTION_RESTART
                            },
                        new SC_ACTION
                            {
                                Delay = Math.Max(0, Math.Min(int.MaxValue, restartDelay)),
                                Type = SC_ACTION_TYPE.SC_ACTION_RESTART
                            },
                    };

            for (int i = Math.Max(0, restartAttempts); i < actions.Length; i++)
                actions[i] = new SC_ACTION { Delay = 0, Type = SC_ACTION_TYPE.SC_ACTION_NONE };

            GCHandle hdata = GCHandle.Alloc(actions, GCHandleType.Pinned);
            try
            {
                SERVICE_FAILURE_ACTIONS cfg = new SERVICE_FAILURE_ACTIONS();
                cfg.dwResetPeriod = Math.Max(-1, Math.Min(int.MaxValue, resetFailuresDelay));
                cfg.lpRebootMsg = cfg.lpCommand = IntPtr.Zero;
                cfg.cActions = actions.Length;
                cfg.lpsaActions = hdata.AddrOfPinnedObject();

                Win32.SetServiceConfig(sc, SERVICE_CONFIG_INFO.FAILURE_ACTIONS, cfg);
            }
            finally
            {
                hdata.Free();
            }
        }

        internal static void SetAccess(ServiceController sc, IEnumerable<ServiceAccessAttribute> aces)
        {
            uint bufSizeNeeded;
            byte[] psd = new byte[0];

            Win32.QueryServiceObjectSecurity(sc.ServiceHandle, SecurityInfos.DiscretionaryAcl, psd, 0, out bufSizeNeeded);
            if (bufSizeNeeded < 0 || bufSizeNeeded > short.MaxValue)
                throw new Win32Exception();

            if (!Win32.QueryServiceObjectSecurity(sc.ServiceHandle, SecurityInfos.DiscretionaryAcl, psd = new byte[bufSizeNeeded], bufSizeNeeded, out bufSizeNeeded))
                throw new Win32Exception();

            RawSecurityDescriptor rsd = new RawSecurityDescriptor(psd, 0);

            while (rsd.DiscretionaryAcl.Count > 0)
                rsd.DiscretionaryAcl.RemoveAce(0);

            rsd.DiscretionaryAcl.InsertAce(rsd.DiscretionaryAcl.Count,
                new CommonAce(AceFlags.None, AceQualifier.AccessAllowed, (int)ServiceAccessRights.SERVICE_ALL_ACCESS,
                    new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null), false, null));

            foreach (ServiceAccessAttribute ace in aces)
            {
                SecurityIdentifier sid = new SecurityIdentifier(ace.Sid, null);
                rsd.DiscretionaryAcl.InsertAce(rsd.DiscretionaryAcl.Count,
                    new CommonAce(AceFlags.None, ace.Qualifier, (int)ace.AccessMask, sid, false, null));
            }

            byte[] rawsd = new byte[rsd.BinaryLength];
            rsd.GetBinaryForm(rawsd, 0);

            if (!Win32.SetServiceObjectSecurity(sc.ServiceHandle, SecurityInfos.DiscretionaryAcl, rawsd))
                throw new Win32Exception();
        }
        #endregion
    }
}
