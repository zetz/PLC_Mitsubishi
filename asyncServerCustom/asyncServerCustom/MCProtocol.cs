using System.Runtime.InteropServices;

namespace asyncServerCustom
{
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct AccessPath_t
    {
        [FieldOffset(0)] public byte NetworkNo;
        [FieldOffset(1)] public byte PcNo;
        [FieldOffset(2)] public ushort IoNo;
        [FieldOffset(4)] public byte UnitNo;
    };


    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct MCProtocol_t
    {
        [FieldOffset(0)]
        public ushort SubHeader;
        [FieldOffset(2)]
        public AccessPath_t Accesspath;
        [FieldOffset(7)]
        public ushort DataLength;
        [FieldOffset(9)]
        public ushort MonitoringTimer;
        [FieldOffset(11)]
        public DeviceWordWrite_t DeviceWordWrite;
    };



    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct RecvMCProtocol_t
    {
        [FieldOffset(0)]
        public ushort SubHeader;
        [FieldOffset(2)]
        public AccessPath_t Accesspath;
        [FieldOffset(7)]
        public ushort DataLength;
        [FieldOffset(9)]
        public ushort EndCode;
    };


    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct RecvMCProtocol_ErrorData
    {
        [FieldOffset(0)]
        public AccessPath_t Accesspath;
        [FieldOffset(5)]
        public ushort Command;
        [FieldOffset(7)]
        public ushort SubCommand;
    };

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct AccessDevice_u
    {
        [FieldOffset(0)]
        public uint DeviceCode;
    };

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct DeviceWordWrite_t
    {
        [FieldOffset(0)]
        public ushort Command;
        [FieldOffset(2)]
        public ushort SubCommand;
        [FieldOffset(4)]
        public AccessDevice_u AccessDevice;
        [FieldOffset(8)]
        public ushort DevieceNum;
        //[FieldOffset(4)] public ushort[] buf;
        //[FieldOffset(10)]
        //public ushort WriteData;
    };


    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct DeviceWordData
    {
        [FieldOffset(0)]
        public ushort Data;
    }
}