using System;
using System.IO;
using System.Collections.Generic;

using static dotNSASM.Util;

namespace NSASM_GUI
{
    class InlineUtil
    {
        private static void PutToList(List<byte> list, ushort value)
        {
            list.Add((byte)(value & 0xFF));
            list.Add((byte)(value >> 8));
        }

        private static void PutToList(List<byte> list, string value)
        {
            for (int i = 0; i < value.Length; i++)
                list.Add((byte)value[i]);
        }

        private static void PutBytes(List<byte> bytes, ushort size,
            string[][] code, ushort heap, ushort stack, ushort regs,
            ushort segCnt
        ) {
            PutToList(bytes, "NS");
            PutToList(bytes, size);
            PutToList(bytes, heap);
            PutToList(bytes, stack);
            PutToList(bytes, regs);
            PutToList(bytes, segCnt);

            foreach (string[] seg in code)
            {
                PutToList(bytes, 0xA5A5);
                PutToList(bytes, seg[0]);
                PutToList(bytes, 0xAAAA);
                PutToList(bytes, seg[1]);
            }

            PutToList(bytes, 0xFFFF);
            ushort sum = 0;
            for (int i = 0; i < bytes.Count; i++)
                sum += bytes[i];
            PutToList(bytes, sum);
        }

        public static byte[] Compile(string str)
        {
            ushort heap = 64, stack = 32, regs = 16;

            string conf = GetSegment(str, ".<conf>");
            if (conf == null)
            {
                Print("Conf load error.\n");
                return null;
            }
            if (conf.Length > 0)
            {
                StringReader confReader = new StringReader(conf);
                try
                {
                    string buf;
                    while (confReader.Peek() != -1)
                    {
                        buf = confReader.ReadLine();
                        switch (buf.Split(' ')[0])
                        {
                            case "heap":
                                heap = ushort.Parse(buf.Split(' ')[1]);
                                break;
                            case "stack":
                                stack = ushort.Parse(buf.Split(' ')[1]);
                                break;
                            case "reg":
                                regs = ushort.Parse(buf.Split(' ')[1]);
                                break;
                        }
                    }
                }
                catch (Exception e)
                {
                    Print("Conf load error.\n");
                    return null;
                }
            }

            string[][] code = GetSegments(str);
            ushort segCnt = (ushort)code.GetLength(0);

            List<byte> bytes = new List<byte>();
            PutBytes(bytes, 0xFFFF, code, heap, stack, regs, segCnt);
            if (bytes.Count > 0x10000)
            {
                Print("Code is out of file.\n");
                return null;
            }
            ushort size = (ushort)(bytes.Count & 0xFFFF);
            bytes.Clear();
            PutBytes(bytes, size, code, heap, stack, regs, segCnt);

            return bytes.ToArray();
        }
    }
}
