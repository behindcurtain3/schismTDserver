using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace schismTD.Logic
{
    public class EloRating
    {
        public const double K = 24;

        public double RatingA { get; set; }
        public double RatingB { get; set; }

        public EloRating(double RA, double RB, double SA, double SB)
        {
            double AResult = (SA > SB) ? 1.0 : (SA < SB) ? 0.0 : 0.5;
            double BResult = (SB > SA) ? 1.0 : (SB < SA) ? 0.0 : 0.5;

            double EA = 1 / (1 + Math.Pow(10, (RB - RA) / 400));
            double EB = 1 / (1 + Math.Pow(10, (RA - RB) / 400));

            double RA2 = RA + K * (AResult - EA);
            double RB2 = RB + K * (BResult - EB);

            RatingA = Math.Round(RA2);
            RatingB = Math.Round(RB2);
        }
    }
}
