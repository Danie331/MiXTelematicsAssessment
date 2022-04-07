using NetTopologySuite.Geometries;
using NetTopologySuite.Index.KdTree;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace MiXTelematicsAssessment
{
    public sealed class DataProcessor
    {
        private const string _dataFilePath = @"VehiclePositions_DataFile\VehiclePositions.dat";
        private const double _radians = Math.PI / 180.0;
        private const double _earthRadius = 6376500.0;

        /// <summary>
        /// NetTopologySuite "nearest neighbour method"
        /// </summary>
        /// <returns>Indexed spatial data as a KdTree</returns>
        public static KdTree<DataRecord> BuildKdTree()
        {
            var tree = new KdTree<DataRecord>();
            foreach (var rec in LoadData())
            {
                tree.Insert(new Coordinate { Y = rec.Latitude, X = rec.Longitude }, rec);
            }

            return tree;
        }

        public static DataRecord FindNearest(IEnumerable<DataRecord> data, Position pos)
        {
            var seedRecord = data.First();
            var seedDist = GetDistance(seedRecord.Longitude, seedRecord.Latitude, pos.Longitude, pos.Latitude);
            DataRecord nearest = seedRecord;
            foreach (var rec in data.Skip(1))
            {
                var dist = GetDistance(rec.Longitude, rec.Latitude, pos.Longitude, pos.Latitude);
                if (dist < seedDist)
                {
                    seedDist = dist;
                    nearest = rec;
                }
            }

            return nearest;
        }

        public static IEnumerable<DataRecord> LoadData()
        {
            var fileBytes = File.ReadAllBytes(_dataFilePath);
            var filePosition = 0;
            var data = new List<DataRecord>();
            while (filePosition < fileBytes.Length)
            {
                var epochDatetime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

                var positionId = BitConverter.ToInt32(fileBytes, filePosition);
                filePosition += sizeof(int);
                var vehicleRegistration = ReadNullTerminatedString(fileBytes, ref filePosition);
                filePosition += 1;
                var latitude = BitConverter.ToSingle(fileBytes, filePosition);
                filePosition += sizeof(float);
                var longitude = BitConverter.ToSingle(fileBytes, filePosition);
                filePosition += sizeof(float);
                var recordedTimeUTCBytes = BitConverter.ToUInt64(fileBytes, filePosition);
                var recordedTimeUTC = epochDatetime.AddSeconds(recordedTimeUTCBytes);
                filePosition += sizeof(ulong);
                data.Add(new DataRecord
                {
                    PositionId = positionId,
                    VehicleRegistration = vehicleRegistration,
                    Latitude = latitude,
                    Longitude = longitude,
                    RecordedTimeUTC = recordedTimeUTC
                });
            }

            return data;
        }

        private static string ReadNullTerminatedString(byte[] data, ref int position)
        {
            StringBuilder sb = new StringBuilder();
            while (data[position] != 0)
            {
                sb.Append((char)data[position]);
                position += 1;
            }
            return sb.ToString();
        }

        private static double GetDistance(double lng1, double lat1, double lng2, double lat2)
        {
            var ratioY1 = lat1 * _radians;
            var ratioX1 = lng1 * _radians;
            var ratioY2 = lat2 * _radians;
            var ratioX2 = lng2 * _radians - ratioX1;
            var delta = Math.Pow(Math.Sin((ratioY2 - ratioY1) / 2.0), 2.0) + Math.Cos(ratioY1) * Math.Cos(ratioY2) * Math.Pow(Math.Sin(ratioX2 / 2.0), 2.0);

            return _earthRadius * (2.0 * Math.Atan2(Math.Sqrt(delta), Math.Sqrt(1.0 - delta)));
        }
    }
}
