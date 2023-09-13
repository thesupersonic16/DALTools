using DALLib.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace DALLib.Scripting
{
    public class STSC2Node
    {
        public object Value { get; set; }

        public ParamFormatType paramFormatType { get; set; }
        public ParamDataType paramDataType { get; set; }
        public ParamStructType paramStructType { get; set; }

        private byte m_NodeType;
        private byte m_Operator;

        public STSC2Node LeftNode;
        public STSC2Node RightNode;

        // TODO: Get rid of this
        private short m_NodeCount;
        private short m_LeftNode;
        private short m_RightNode;

        public STSC2Node() { }

        public STSC2Node(int value)
        {
            Value = value;
            paramFormatType = ParamFormatType.Auto;
            paramDataType = ParamDataType.Int;
            paramStructType = ParamStructType.Number;

            m_NodeType = 0x01;
            m_NodeCount = 0x01;
        }

        public STSC2Node Read(ExtendedBinaryReader reader, uint baseAddress = 0, short nodeNum = 0)
        {
            byte bits;
            long valuePosition;
            long prevPosition = 0;
            if (nodeNum == 0)
            {
                uint addr = reader.ReadUInt32();

                // Ignore if null
                if (addr == 0)
                    return this;

                prevPosition = reader.GetPosition();
                reader.JumpTo(addr);

                m_NodeCount = reader.ReadInt16();
            }

            // Parse node
            bits = reader.ReadByte();
            m_NodeType = (byte)(bits >> 7);
            m_Operator = (byte)(bits & 0b0111_1111);
            valuePosition = reader.GetPosition();
            reader.JumpAhead(0x06);

            paramFormatType = (ParamFormatType)reader.ReadByte();
            bits = reader.ReadByte();
            paramDataType = (ParamDataType)(bits & 0b0001_1111);
            paramStructType = (ParamStructType)((bits & 0b1110_0000) >> 5);

            // Binary tree
            short prevNodeOffset = reader.ReadInt16();
            short nextNodeOffset = reader.ReadInt16();

            m_LeftNode = prevNodeOffset;
            m_RightNode = nextNodeOffset;

            if (nodeNum == 0)
                baseAddress = (uint)reader.GetPosition();

            if (prevNodeOffset != 0 && prevNodeOffset != 1)
            {
                reader.JumpTo(baseAddress + (prevNodeOffset - 2) * 13);
                LeftNode = new STSC2Node().Read(reader, baseAddress, prevNodeOffset);
            }
            if (nextNodeOffset != 0 && nextNodeOffset != 1)
            {
                reader.JumpTo(baseAddress + (nextNodeOffset - 2) * 13);
                RightNode = new STSC2Node().Read(reader, baseAddress, nextNodeOffset);
            }

            // Convert data
            if (paramStructType == ParamStructType.Number)
            {
                reader.JumpTo(valuePosition);
                Value = reader.ReadInt32();
            }
            else if (paramStructType == ParamStructType.Param)
            {
                // TODO: Figure out what this is?
                reader.JumpTo(valuePosition);
                Value = reader.ReadInt32();
            }
            else if (paramStructType == ParamStructType.StringAddress)
            {
                reader.JumpTo(valuePosition);
                Value = reader.ReadStringElsewhere();
            }

            if (prevPosition != 0)
                reader.JumpTo(prevPosition);
            return this;
        }

        public void Write(ExtendedBinaryWriter writer, uint offset, uint baseAddress = 0, short nodeNum = 0)
        {
            if (nodeNum == 0)
            {
                writer.Write(m_NodeCount);
                baseAddress = (uint)writer.BaseStream.Position;
            }
            writer.Write((byte)((m_NodeType << 7) | m_Operator));

            if (paramStructType == ParamStructType.Number)
                writer.Write((int)Value);
            else if (paramStructType == ParamStructType.Param)
                writer.Write((int)Value);
            else if (paramStructType == ParamStructType.StringAddress)
                writer.Write(offset);

            writer.WriteNulls(0x02);
            writer.Write((byte)paramFormatType);
            writer.Write((byte)(((byte)paramStructType << 5) | (byte)paramDataType));

            // TODO: Actually work out how binary trees work
            writer.Write(m_LeftNode);
            writer.Write(m_RightNode);

            uint leftPos = (uint)(13 * (m_LeftNode - 1) + baseAddress);
            uint rightPos = (uint)(13 * (m_RightNode - 1) + baseAddress);
            if (LeftNode != null)
            {
                writer.JumpTo(leftPos);
                LeftNode.Write(writer, offset, baseAddress, m_LeftNode);
            }
            if (RightNode != null)
            {
                writer.JumpTo(rightPos);
                RightNode.Write(writer, offset, baseAddress, m_RightNode);
            }
        }

        public override string ToString()
        {
            return Value?.ToString() ?? base.ToString();
        }

        public enum ParamFormatType
        {
            Auto,
            Local
        }

        public enum ParamDataType
        {
            Int,
            Float,
            Bool,
            String,
            StringBuffer = 0x1E,
            Void
        }

        public enum ParamStructType
        {
            Number,
            Param,
            StringAddress,
            StringBuffer = 0x07
        }

    }
}
