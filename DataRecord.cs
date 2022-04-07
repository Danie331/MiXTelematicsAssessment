
using System;

namespace MiXTelematicsAssessment
{
    public sealed class DataRecord
    {
        public int PositionId { get; set; }
        public string VehicleRegistration { get; set; }
        public float Latitude { get; set; }
        public float Longitude { get; set; }
        public DateTime RecordedTimeUTC { get; set; }
    }
}