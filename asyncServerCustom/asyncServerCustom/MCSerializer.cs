using System;
using System.Runtime.InteropServices;

namespace asyncServerCustom
{
    public static class MCSerializer
    {
        public static byte[] Serialize(object message)
        {
            int size = Marshal.SizeOf(message);
            byte[] arr = new byte[size];
            IntPtr ptr = Marshal.AllocHGlobal(size);

            Marshal.StructureToPtr(message, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
            Marshal.FreeHGlobal(ptr);

            return arr;
        }

        public static T Deserialize<T>(byte[] bytes)
        {
            T message = default(T);

            int size = Marshal.SizeOf(message);
            IntPtr ptr = Marshal.AllocHGlobal(size);

            Marshal.Copy(bytes, 0, ptr, size);

            message = (T)Marshal.PtrToStructure(ptr, message.GetType());
            Marshal.FreeHGlobal(ptr);

            return message;
        }
    }
}