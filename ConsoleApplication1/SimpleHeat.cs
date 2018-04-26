using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace ConsoleApplication1 {
    public class SimpleHeat {
        public Cell[,] Cells { get; set; }
        public double A { get; set; }
        public double H { get; set; }
        public int CellCount => (int)(1 / H);
        public int CellCountX => (int)(Size.X / H);
        public int CellCountY => (int)(Size.Y / H);
        public int TaskNumber { get; set; }

        protected Point Size { get; set; }
        protected const string DefaultTaskName = "Задание.txt";
        protected string DefaultPath => $"Задания/{ TaskNumber }/";
        protected string DefaultPathProcess => DefaultPath + "Файлы_Процесса_Нагрева/";

        public SimpleHeat(double a, double h, int taskNumber = 1) {
            A = a;
            H = h;
            TaskNumber = taskNumber;
            InitializeFromFile();
            //Initialize();
        }

        public void Run(double tMax, double tau) {
            for (double k = 0, i = 0; k < tMax; k += tau, i += 1) {
                Heat(tau);
                SaveToFile($"{ DefaultPathProcess }heat{(int)i}.vts");
            }
        }

        public void SaveToFile(string path) {
            if (File.Exists(path)) {
                File.Delete(path);
            }
            using (var fs = new StreamWriter(File.Open(path, FileMode.OpenOrCreate))) {
                fs.WriteLine("<?xml version=\"1.0\"?>");
                fs.WriteLine("<VTKFile type=\"StructuredGrid\" version=\"0.1\" byte_order=\"LittleEndian\">");
                fs.WriteLine($"  <StructuredGrid WholeExtent=\"0 {CellCountX} 0 {CellCountY} 0 0\">");
                fs.WriteLine($"    <Piece Extent=\"0 {CellCountX} 0 {CellCountY} 0 0\">");
                fs.WriteLine("      <Points>");
                fs.WriteLine("        <DataArray type=\"Float32\" NumberOfComponents=\"3\" format=\"ascii\">");
                fs.WriteLine("          ");
                for (int j = 0; j <= CellCountX; j++) {
                    for (int i = 0; i <= CellCountY; i++) {
                        fs.Write($"{Format(i * H)} {Format(j * H)} 0.0 ");
                    }
                }
                fs.WriteLine();
                fs.WriteLine("        </DataArray>");
                fs.WriteLine("      </Points>");
                fs.WriteLine("      <CellData Scalars=\"Temperature, Proc\">");
                fs.WriteLine("        <DataArray type=\"Float32\" Name=\"Temperature\" format=\"ascii\">");
                fs.WriteLine("          ");
                for (int j = 0; j < CellCountY; j++) {
                    for (int i = 0; i < CellCountX; i++) {
                        fs.Write($"{ Format(Math.Round(Cells[i, j].S, 2)) } ");
                    }
                    fs.WriteLine();
                }
                fs.WriteLine("        </DataArray>");
                fs.WriteLine("      </CellData>");
                fs.WriteLine("    </Piece>");
                fs.WriteLine("  </StructuredGrid>");
                fs.WriteLine("</VTKFile>");
            }
        }

        public void ReadNodeFile(string path) {
            if (File.Exists(path)) {
                using (var fp = new StreamReader(File.Open(path, FileMode.Open))) {
                    /*fp.ReadLine
                    nodes = new Point*/
                }
            }
        }
        public override string ToString() {
            var result = string.Empty;
            for (var i = 0; i < CellCountX; i++) {
                for (var j = 0; j < CellCountY; j++) {
                    result += $"\t{ Math.Round(Cells[i, j].S, 4) }";
                }
                result += "\n";
            }
            return result;
        }

        private void Heat(double tau) {
            for (var i = 1; i < CellCountX - 1; i++) {
                for (var j = 1; j < CellCountY - 1; j++) {
                    Cells[i, j].S = Cells[i, j].SOld + (Math.Pow(A, 2) * (Cells[i, j - 1].S + Cells[i, j + 1].S - 2 * Cells[i, j].S) / Math.Pow(H, 2)
                        + Math.Pow(A, 2) * (Cells[i - 1, j].S + Cells[i + 1, j].S - 2 * Cells[i, j].S) / Math.Pow(H, 2)) * tau;
                    Cells[i, j].SOld = Cells[i, j].S;
                }
            }
        }

        private void Output() {
            Console.Clear();
            Console.WriteLine(this);
            Thread.Sleep(30);
        }

        private string Format(double t) =>
            t.ToString().Replace(",", ".");

        private void Initialize() {
            Size = new Point(1, 1);
            Cells = new Cell[CellCountX, CellCountY];
            for (var i = 0; i < CellCountX; i++) {
                for (var j = 0; j < CellCountY; j++) {
                    Cells[i, j] = new Cell(j == 0 ? 1 : 0);
                }
            }
        }

        private void InitializeFromFile() {
            using (var fs = new StreamReader(File.Open(DefaultPath + DefaultTaskName, FileMode.Open))) {
                var lines = fs.ReadToEnd()
                    .Split('\n')
                    .Where(x => !x.StartsWith("#"))
                    .ToList();
                var sizes = lines
                    .First()
                    .Split(' ')
                    .Select(x => double.Parse(x.Replace(".", ",")))
                    .ToList();
                var temperatures = lines
                    .Last()
                    .Split(' ')
                    .Select(x => double.Parse(x.Replace(".", ",")))
                    .ToList();
                Size = new Point(sizes[0], sizes[1]);
                Cells = new Cell[CellCountX, CellCountY];
                for (var i = 0; i < CellCountX; i++) {
                    for (var j = 0; j < CellCountY; j++) {
                        Cells[i, j] = new Cell(
                            i == 0 ? temperatures[0] :
                            j == CellCountY ? temperatures[1] :
                            i == CellCountX ? temperatures[2] :
                            j == 0 ? temperatures[3] : 0
                        );
                    }
                }
            }
        }
    }
}
