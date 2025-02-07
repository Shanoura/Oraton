using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oraton.Classes
{
    public class TextStyle
    {
        public int Id { get; set; }
        public int NoteId { get; set; }
        public int RowId { get; set; }
        public int StartIndex { get; set; }
        public int Length { get; set; }
        public string? FontFamily { get; set; }
        public float? FontSize { get; set; }
    }
}
