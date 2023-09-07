using DALLib.IO;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace DALLib.File
{
    public class TableFile : FileBase
    {
        public List<TableColumn> Columns = new List<TableColumn>();
        public List<string[]> Rows = new List<string[]>();

        public override void Load(ExtendedBinaryReader reader, bool keepOpen = false)
        {
            int columnCount = reader.ReadInt32();
            int rowCount = reader.ReadInt32();
            int rowPosition = reader.ReadInt32();

            Columns.Clear();
            Rows.Clear();

            for (int i = 0; i < columnCount; i++)
            {
                Columns.Add(new TableColumn(
                    (TableType)reader.ReadUInt32(), 
                    reader.ReadStringElsewhere()));
            }

            // TODO: Is this needed?
            reader.JumpTo(rowPosition);

            for (int r = 0; r < rowCount; r++)
            {
                var row = new string[columnCount];
                for (int c = 0; c < columnCount; c++)
                {
                    switch (Columns[c].Type)
                    {
                        case TableType.Int:
                            row[c] = reader.ReadInt32().ToString(CultureInfo.CreateSpecificCulture("en-GB"));
                            break;
                        case TableType.String:
                            row[c] = reader.ReadStringElsewhere();
                            break;
                        default:
                            throw new NotImplementedException(Columns[c].Type.ToString());
                    }
                }
                Rows.Add(row);
            }
        }

        public override void Save(ExtendedBinaryWriter writer)
        {
            writer.Write(Columns.Count);
            writer.Write(Rows.Count);
            writer.AddOffset("rowPosition");

            for (int col = 0; col < Columns.Count; col++)
            {
                writer.Write((uint)Columns[col].Type);
                writer.AddOffset($"column{col}");
            }

            writer.FillInOffset("rowPosition");

            for (int row = 0; row < Rows.Count; row++)
            {
                for (int col = 0; col < Columns.Count; col++)
                {
                    switch (Columns[col].Type)
                    {
                        case TableType.Int:
                            writer.Write(Convert.ToInt32(Rows[row][col], CultureInfo.CreateSpecificCulture("en-GB")));
                            break;
                        case TableType.String:
                            writer.AddOffset($"str{row}_{col}");
                            break;
                        default:
                            throw new NotImplementedException(Columns[col].Type.ToString());
                    }
                }
            }

            // Write strings
            for (int col = 0; col < Columns.Count; col++)
            {
                writer.FillInOffset($"column{col}");
                writer.WriteNullTerminatedString(Columns[col].Name);
            }

            for (int row = 0; row < Rows.Count; row++)
            {
                for (int col = 0; col < Columns.Count; col++)
                {
                    if (Columns[col].Type == TableType.String)
                    {
                        writer.FillInOffset($"str{row}_{col}");
                        writer.WriteNullTerminatedString(Rows[row][col]);
                    }
                }
            }
        }

        public enum TableType
        {
            Unknown0,
            Int,
            Unknown2,
            Unknown3,
            Unknown4,
            String,
        }

        public class TableColumn
        {
            public TableType Type;
            public string Name;

            public TableColumn(TableType type, string name)
            {
                Type = type;
                Name = name;
            }
        }
    }
}
