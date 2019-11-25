using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tono;
using Tono.Gui;

namespace JitStreamDesigner
{
    public class DistanceRect
    {
        public Distance L { get; set; }
        public Distance T { get; set; }
        public Distance R { get; set; }
        public Distance B { get; set; }

        public static DistanceRect FromLTRB(Distance L, Distance T, Distance R, Distance B)
        {
            return new DistanceRect
            {
                L = L,
                T = T,
                R = R,
                B = B,
            };
        }

        public static DistanceRect FromLTWH(Distance L, Distance T, Distance W, Distance H)
        {
            return new DistanceRect
            {
                L = L,
                T = T,
                R = L + W,
                B = T + H,
            };
        }

        public static DistanceRect FromCWH(Distance X, Distance Y, Distance W, Distance H)
        {
            return new DistanceRect
            {
                L = X - W / 2,
                R = X + W / 2,
                T = Y - H / 2,
                B = Y + H / 2,
            };
        }

        public CodePos<Distance,Distance> LT { get => CodePos<Distance, Distance>.From(L, T);  }
        public CodePos<Distance, Distance> RB { get => CodePos<Distance, Distance>.From(R, B); }
        public CodePos<Distance, Distance> RT { get => CodePos<Distance, Distance>.From(R, T); }
        public CodePos<Distance, Distance> LB { get => CodePos<Distance, Distance>.From(L, B); }
        public CodePos<Distance, Distance> C { get => CodePos<Distance, Distance>.From((L + R) / 2, (T + B) / 2); }
        public Distance Width { get => R - L; set => R = L + value; }
        public Distance Height { get => B - T; set => B = T + value; }
    }
}
