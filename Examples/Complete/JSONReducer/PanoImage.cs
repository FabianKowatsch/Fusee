using System;
using System.Collections.Generic;
using System.Text;

namespace JSONReducer
{
    public class PanoImage
    {
        public string filename { get; set; }
        public double LON { get; set; }
        public double LAT { get; set; }
        public double HEIGHT { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public double roll { get; set; }
        public double pitch { get; set; }
        public double heading { get; set; }
        public string id { get; set; }
        public string device { get; set; }
        public string timestamp { get; set; }
        public double x_fahrzeug { get; set; }
        public double y_fahrzeug { get; set; }
        public double z_fahrzeug { get; set; }
        public double heading_fahrzeug { get; set; }
        public double heading_cam { get; set; }
        public double roll_fhzg { get; set; }
        public double pitch_fhzg { get; set; }
        public double qx { get; set; }
        public double qy { get; set; }
        public double qz { get; set; }
        public double qw { get; set; }
        public double roll_orbit { get; set; }
        public double pitch_orbit { get; set; }
        public double heading_orbit { get; set; }
        public double distance { get; set; }
    }
}