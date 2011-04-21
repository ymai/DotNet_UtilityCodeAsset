using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;


namespace UtilityCodeAsset.SystemInfo
{
    public class CPU
    {
        public enum ProcessorType
        {
            X86,
            X64,
            Unknown
        }

        internal const ushort PROCESSOR_TYPE_INTEL = 0;
        internal const ushort PROCESSOR_TYPE_INTEL_IA64 = 6;
        internal const ushort PROCESSOR_TYPE_INTEL_AMD64 = 9;
        internal const ushort PROCESSOR_TYPE_UNKNOWN = 0xFFFF;

        [StructLayout(LayoutKind.Sequential)]
        internal struct SYSTEM_INFO
        {
            public ushort wProcessorArchitecture;
            public ushort wReserved;
            public uint dwPageSize;
            public IntPtr lpMinimumApplicationAddress;
            public IntPtr lpMaximumApplicationAddress;
            public UIntPtr dwActiveProcessorMask;
            public uint dwNumberOfProcessors;
            public uint dwProcessorType;
            public uint dwAllocationGranularity;
            public ushort wProcessorLevel;
            public ushort wProcessorRevision;
        };


        [DllImport("kernel32.dll")]
        internal static extern void GetNativeSystemInfo(ref SYSTEM_INFO lpSystemInfo);

        public static ProcessorType GetProcessorType()
        {
            SYSTEM_INFO sysInfo = new SYSTEM_INFO();
            GetNativeSystemInfo(ref sysInfo);

            switch (sysInfo.wProcessorArchitecture)
            {
                case PROCESSOR_TYPE_INTEL_AMD64:
                    return ProcessorType.X64;

                case PROCESSOR_TYPE_INTEL:
                    return ProcessorType.X86;

                default:
                    return ProcessorType.Unknown;
            }
        }

    }
}
