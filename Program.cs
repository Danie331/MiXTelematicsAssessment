using NetTopologySuite.Geometries;
using NetTopologySuite.Index.KdTree;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Linq;

namespace MiXTelematicsAssessment
{
    class Program
    {
        private static List<Position> _testPositions = new List<Position>
        {
            new Position { Id = 1, Name = "Position 1", Latitude = 34.544909f, Longitude = -102.100843f },
            new Position { Id = 2, Name = "Position 2", Latitude = 32.345544f, Longitude = -99.123124f },
            new Position { Id = 3, Name = "Position 3", Latitude = 33.234235f, Longitude = -100.214124f },
            new Position { Id = 4, Name = "Position 4", Latitude = 35.195739f, Longitude = -95.348899f },
            new Position { Id = 5, Name = "Position 5", Latitude = 31.895839f, Longitude = -97.789573f },
            new Position { Id = 6, Name = "Position 6", Latitude = 32.895839f, Longitude = -101.789573f },
            new Position { Id = 7, Name = "Position 7", Latitude = 34.115839f, Longitude = -100.225732f},
            new Position { Id = 8, Name = "Position 8", Latitude = 32.335839f, Longitude = -99.992232f },
            new Position { Id = 9, Name = "Position 9", Latitude = 33.535339f, Longitude = -94.792232f },
            new Position { Id = 10, Name = "Position 10", Latitude = 32.234235f, Longitude = -100.222222f }
        };
        static void Main(string[] args)
        {
            Console.WriteLine("Using benchmark:");
            Console.WriteLine();
            RunBenchmark();
            Console.WriteLine();

            Console.WriteLine("Using benchmark (with parallelism):");
            Console.WriteLine();
            RunBenchmarkParallel();
            Console.WriteLine();

            Console.WriteLine("Using NetTopologySuite/Kd-Tree:");
            Console.WriteLine();
            RunNetTopologySuite();
            Console.WriteLine();

            Console.ReadKey();
        }

        private static void RunNetTopologySuite()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var kdTree = DataProcessor.BuildKdTree();
            sw.Stop();
            var duration1 = sw.ElapsedMilliseconds;
            Console.WriteLine($"Read and process data kd-tree duration: {duration1}");
            sw.Restart();
            foreach (var item in _testPositions)
            {
                var neighbour = kdTree.NearestNeighbor(new Coordinate { Y = item.Latitude, X = item.Longitude });
                Console.WriteLine($"{item.Name}, lat: {item.Latitude}, lng: {item.Longitude} :: closest data point: lat: {neighbour.Data.Latitude}, lng: {neighbour.Data.Longitude}");
            }
            sw.Stop();
            var duration2 = sw.ElapsedMilliseconds;
            Console.WriteLine($"Nearest neighbor calculation duration: {duration2}");
            Console.WriteLine($"Total duration: {duration1 + duration2}");
        }

        private static void RunBenchmark()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var data = DataProcessor.LoadData();
            sw.Stop();
            var duration1 = sw.ElapsedMilliseconds;
            Console.WriteLine($"Read and process data file duration: {duration1}");
            sw.Restart();
            foreach (var pos in _testPositions)
            {
                var closest = DataProcessor.FindNearest(data, pos);
                Console.WriteLine($"{pos.Name}, lat: {pos.Latitude}, lng: {pos.Longitude} :: closest data point: lat: {closest.Latitude}, lng: {closest.Longitude}");
            }
            sw.Stop();
            var duration2 = sw.ElapsedMilliseconds;
            Console.WriteLine($"Benchmark closest record calculation duration: {duration2}");
            Console.WriteLine($"Total duration: {duration1 + duration2}");
        }

        private static void RunBenchmarkParallel()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var data = DataProcessor.LoadData();
            sw.Stop();
            var duration1 = sw.ElapsedMilliseconds;
            Console.WriteLine($"Read and process data file duration: {duration1}");
            sw.Restart();
            var pairs = new ConcurrentBag<KeyValuePair<Position, DataRecord>>();
            Parallel.ForEach(_testPositions, pos =>
            {
                var closest = DataProcessor.FindNearest(data, pos);
                pairs.Add(new KeyValuePair<Position, DataRecord>(pos, closest));
            });
            sw.Stop();
            foreach (var item in pairs.ToArray().OrderBy(x => x.Key.Id))
            {
                Console.WriteLine($"{item.Key.Name}, lat: {item.Key.Latitude}, lng: {item.Key.Longitude} :: closest data point: lat: {item.Value.Latitude}, lng: {item.Value.Longitude}");
            }
            var duration2 = sw.ElapsedMilliseconds;
            Console.WriteLine($"Benchmark parallel test calculation duration: {duration2}");
            Console.WriteLine($"Total duration: {duration1 + duration2}");
        }
    }
}
