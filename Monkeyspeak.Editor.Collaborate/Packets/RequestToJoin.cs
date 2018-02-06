using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Monkeyspeak.Editor.Collaborate.Packets
{
    public sealed class RequestToJoin : IPacket
    {
        public RequestToJoin()
        {
        }

        public RequestToJoin(IEditor editor)
        {
            EditorId = editor.GetHashCode();
        }

        public int EditorId { get; private set; }

        public PacketType Type => PacketType.Join;

        public void Read(BinaryReader reader)
        {
            EditorId = reader.ReadInt32();
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(EditorId);
        }
    }
}