using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace CSHlibrary.Models;

public class ArtikelModel
{
    public int id { get; set; }
    public string? Name { get; set; }
    public string? Beschreibung { get; set; }
    public string? Groesse { get; set; }
    public string? Farbe { get; set; }
    public int Menge { get; set; }
    public double? Preis { get; set; }
}
