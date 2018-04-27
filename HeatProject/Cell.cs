namespace ConsoleApplication1 {
    public class Cell {
        public int Id { get; set; }
        public double S { get; set; }
        public double SOld { get; set; }
        public Cell(double s) {
            S = s;
            SOld = s;
        }
    }
}
