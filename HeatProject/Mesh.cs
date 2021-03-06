﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ConsoleApplication1 {
    public class Mesh {
        public List<Node> Nodes;
        public List<TriangleCell> Cells;
        public List<Edge> Edges;
        public Dictionary<Node, List<TriangleCell>> DictCells { get; set; }
        public Mesh() {
            DictCells = new Dictionary<Node, List<TriangleCell>>();
            InitFromFile("carman.1 - копия");
        }

        public void InitFromFile(string fileName) {
            Nodes = ReadNodeFromFile(fileName + ".node");
            Cells = ReadEleFromFile(fileName + ".ele", Nodes);
            Cells.ForEach(x => x.Param = new Param { T = 0 });
            Cells = InitNeighFromFile(fileName + ".neigh", Cells);
            Cells = InitEdges(fileName + ".edge", Cells, Nodes, out Edges);
            //InitPoly(fileName + ".poly", Edges);
            Console.WriteLine("Init Complete...");
        }

        private List<Node> ReadNodeFromFile(string path) {
            var result = new List<Node>();
            using (var fs = new StreamReader(File.Open(path, FileMode.Open))) {
                result = fs.ReadToEnd()
                    .Split('\n')
                    .Skip(1)
                    .Where(x => !(string.IsNullOrWhiteSpace(x) || x[0] == '#'))
                    .Select(x => {
                        var str = x;
                        if (x[0] == ' ') {
                            str = str.Remove(0, 1);
                        }
                        var parsed = str.Replace('.', ',').Split(' ');
                        return new Node {
                            Id = int.Parse(parsed[0]),
                            X = double.Parse(parsed[1]),
                            Y = double.Parse(parsed[2])
                        };
                    })
                    .ToList();
            }
            return result;
        }

        private List<TriangleCell> ReadEleFromFile(string path, List<Node> nodes) {
            if (!File.Exists(path)) {
                throw new FileNotFoundException("Error_ele_path");
            }
            var result = new List<TriangleCell>();
            Func<Point, Point> abs = (Point p) => new Point(Math.Abs(p.X), Math.Abs(p.Y));
            using (var fs = new StreamReader(File.Open(path, FileMode.Open))) {
                result = fs.ReadToEnd()
                    .Split('\n')
                    .Skip(1)
                    .Where(x => !(string.IsNullOrWhiteSpace(x) || x[0] == '#'))
                    .Select(x => {
                        var nums = x
                            .Split(' ')
                            .Where(y => !string.IsNullOrWhiteSpace(y))
                            .Select(s => int.Parse(s))
                            .ToArray();
                        var cell = new TriangleCell {
                            Id = nums[0],
                            NCount = 3,
                            ECount = 3,
                            Node = new List<Node> {
                                nodes[nums[1] - 1],
                                nodes[nums[2] - 1],
                                nodes[nums[3] - 1]
                            },
                            Type = nums[4]
                        };
                        AddToDict(cell.Node[0], cell);
                        AddToDict(cell.Node[1], cell);
                        AddToDict(cell.Node[2], cell);
                        cell.C = (cell.Node[0] + cell.Node[1] + cell.Node[2]) / 3;
                        cell.H = new Vector {
                            X = new[] {
                                Math.Abs(cell.Node[0].X - cell.Node[1].X),
                                Math.Abs(cell.Node[1].X - cell.Node[2].X),
                                Math.Abs(cell.Node[0].X - cell.Node[2].X)
                            }.Max(),
                            Y = new[] {
                                Math.Abs(cell.Node[0].Y - cell.Node[1].Y),
                                Math.Abs(cell.Node[1].Y - cell.Node[2].Y),
                                Math.Abs(cell.Node[0].Y - cell.Node[2].Y)
                            }.Max()
                        };
                        return cell;
                    })
                    .ToList();
            }
            return result;
        }

        private void AddToDict(Node node, TriangleCell cell) {
            if (DictCells.ContainsKey(node)) {
                DictCells[node].Add(cell);
            }
            else {
                DictCells.Add(node, new List<TriangleCell> { cell });
            }
        }

        private List<TriangleCell> InitNeighFromFile(string path, List<TriangleCell> cells) {
            var result = new List<TriangleCell>();
            using (var fs = new StreamReader(File.Open(path, FileMode.Open))) {
                result = fs.ReadToEnd()
                    .Split('\n')
                    .Skip(1)
                    .Where(x => !(string.IsNullOrWhiteSpace(x) || x[0] == '#'))
                    .Select(x => {
                        var nums = x
                            .Split(' ')
                            .Where(y => !string.IsNullOrWhiteSpace(y))
                            .Select(s => int.Parse(s))
                            .ToArray();
                        cells[nums[0] - 1].Neighbours = new List<TriangleCell> {
                            nums[1] > 0
                                ?
                                    cells[nums[1] - 1].Neighbours != null && cells[nums[1] - 1].Neighbours.Contains(cells[nums[0] - 1])
                                        ? null
                                        : cells[nums[1] - 1]
                                : null,
                            nums[2] > 0
                                ?
                                    cells[nums[2] - 1].Neighbours != null && cells[nums[2] - 1].Neighbours.Contains(cells[nums[0] - 1])
                                        ? null
                                        : cells[nums[2] - 1]
                                : null,
                            nums[3] > 0
                                ?
                                    cells[nums[3] - 1].Neighbours != null && cells[nums[3] - 1].Neighbours.Contains(cells[nums[0] - 1])
                                        ? null
                                        : cells[nums[3] - 1]
                                : null
                        };
                        return cells[nums[0] - 1];
                    })
                    .ToList();
            }
            return result;
        }

        private List<TriangleCell> InitEdges(string path, List<TriangleCell> cells, List<Node> nodes, out List<Edge> initializedEdges) {
            var result = new List<TriangleCell>();
            var edges = new List<Edge>();
            using (var fs = new StreamReader(File.Open(path, FileMode.Open))) {
                result = fs.ReadToEnd()
                    .Split('\n')
                    .Skip(1)
                    .Where(x => !(string.IsNullOrWhiteSpace(x) || x[0] == '#'))
                    .SelectMany(x => {
                        var nums = x
                            .Split(' ')
                            .Where(y => !string.IsNullOrWhiteSpace(y))
                            .Select(s => int.Parse(s))
                            .ToArray();
                        var edge = new Edge() {
                            Id = nums[0],
                            Node1 = nodes[nums[1] - 1],
                            Node2 = nodes[nums[2] - 1],
                            //Type = nums[3]
                        };
                        var newCells = DictCells[edge.Node1].Union(DictCells[edge.Node2])
                            .Where(r => r.Node.Contains(edge.Node1) && r.Node.Contains(edge.Node2))
                            .ToList();
                        edge.Cell1 = newCells[0];
                        edge.Cell2 = newCells.Count > 1 ? newCells[1] : null;
                        var sqrt = 1 / Math.Sqrt(3);
                        edge.C = new List<Point> {
                            (edge.Node1 + edge.Node2) * 0.5,
                            (edge.Node1 + edge.Node2) * 0.5 - 0.5 * sqrt * (edge.Node2 - edge.Node1),
                            (edge.Node1 + edge.Node2) * 0.5 + 0.5 * sqrt * (edge.Node2 - edge.Node1)
                        };
                        edge.Normal = new Vector { X = edge.Node2.Y - edge.Node1.Y, Y = edge.Node1.X - edge.Node2.X };
                        edge.L = Math.Sqrt(Math.Pow(edge.Normal.X, 2) + Math.Pow(edge.Normal.Y, 2));
                        edge.Normal = (edge.Normal / edge.L) as Vector;
                        edges.Add(edge);
                        newCells.ForEach(r => {
                            if (r.Edge == null) {
                                r.Edge = new  List<Edge>();
                            }
                            r.Edge.Add(edge);
                        });
                        return newCells;
                    })
                    .Distinct()
                    .ToList();
            }
            result.ForEach(x => {
                var a = x.Edge[0].L;
                var b = x.Edge[1].L;
                var c = x.Edge[2].L;
                var p = (a + b + c) / 2.0;
                x.S = Math.Sqrt(p * (p - a) * (p - b) * (p - c));
            });
            initializedEdges = edges;
            return result;
        }


        private void InitPoly(string path,  List<Edge> edges)
        {
            using (var fs = new StreamReader(File.Open(path, FileMode.Open)))
            {
                fs.ReadToEnd()
                    .Split('\n')
                    .Skip(2)
                    .Where(x => !(string.IsNullOrWhiteSpace(x) || x[0] == '#' ))
                    .ToList()
                    .ForEach(x =>
                    {
                        var nums = x
                            .Split(' ')
                            .Where(y => !string.IsNullOrWhiteSpace(y))
                            .Select(s => int.Parse(s))
                            .ToArray();
                        var n1 = nums[1] - 1;
                        var n2 = nums[2] - 1;
                        var edge = edges[0];
                        for (int i = 0; i < edges.Count; i++)
                        {
                            if ((edges[i].Node1.Id == n1 && edges[i].Node2.Id == n2) || (edges[i].Node1.Id == n2 && edges[i].Node2.Id == n1))
                            {
                                edge = edges[i];
                                break;
                            }
                        }
                        if (edge.Id >= 0) {
                            edge.Type = nums[3];
                        }
                    });
            }
        }

    }
}
