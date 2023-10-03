﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    public class SendBuffer
    {
        byte[] _buffer;
        int _usedSize = 0;

        public int FreeSize { get { return _buffer.Length - _usedSize; } }

        public ArraySegment<byte> Open(int reserveData)
        {
            if(FreeSize < reserveData)
            {
                return null;
            }
            return new ArraySegment<byte>(_buffer, _usedSize, reserveData);
        }

        public ArraySegment<byte> Close(int usedSize)
        {
            ArraySegment<byte> segment = new ArraySegment<byte>(_buffer, _usedSize, usedSize);
            _usedSize += usedSize;
            return segment;
        }
    }
}