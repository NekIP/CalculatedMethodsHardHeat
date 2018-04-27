using System;

namespace ConsoleApplication1 {
    class Program {
        static void Main(string[] args) {
            Console.Write("Номер задания = ");
            var taskNumber = int.Parse(Console.ReadLine());
            SimpleHeat(taskNumber);
        }

        private static void SimpleHeat(int taskNumber) {
            //Console.Write("A = ");
            //var a = double.Parse(Console.ReadLine().Replace(".", ","));
            var a = 0.5;
            Console.Write("H = ");
            var h = double.Parse(Console.ReadLine().Replace(".", ","));
            var heat = new SimpleHeat(a, h, taskNumber);
            Console.WriteLine("begin...");
            heat.Run(1, 1.5 * Math.Pow(h, 2));
            Console.WriteLine("end");
            Console.ReadKey();
        }

        private static void HardHeat() {
            //var a = double.Parse(Console.ReadLine());
            Console.WriteLine("start...");
            var tau = double.Parse(Console.ReadLine());
            Console.WriteLine("1...");
            var mesh = new HardHeat();
            Console.WriteLine("begin...");
            mesh.Run(1, tau);
            Console.WriteLine("end");
            Console.ReadKey();
        }
    }
}
