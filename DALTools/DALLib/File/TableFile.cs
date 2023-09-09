using DALLib.IO;
using DALLib.Misc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
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

        /// <summary>
        /// Convert table to an array of T
        /// Table must be loaded prior to conversion
        /// </summary>
        /// <typeparam name="T">Representative type to fill</typeparam>
        /// <returns>Array of rows as T</returns>
        public T[] ToObject<T>()
        {
            var objs = new T[Rows.Count];
            for (int i = 0; i < Rows.Count; i++)
            {
                var obj = Activator.CreateInstance<T>();

                foreach (var property in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.CreateInstance))
                {
                    int columnIndex = -1;

                    // Search for PropertyNameAttribute
                    var attr = property.GetCustomAttribute<PropertyNameAttribute>();
                    if (attr != null)
                        columnIndex = Columns.FindIndex(x => x.Name == attr.PropertyName);
                    
                    // Guess property name
                    if (columnIndex == -1)
                    {
                        // Convert to snack_case
                        string name = string.Concat(property.Name.Select((x, index) =>
                        index > 0 && char.IsUpper(x) ? $"_{x}" : x.ToString())).ToLower();
                        columnIndex = Columns.FindIndex(x => x.Name == name);
                    }

                    // Ignore if nothing is found
                    if (columnIndex == -1)
                        continue;

                    property.SetValue(obj, 
                        Convert.ChangeType(Rows[i][columnIndex], property.PropertyType));
                }

                objs[i] = obj;
            }

            return objs;
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
