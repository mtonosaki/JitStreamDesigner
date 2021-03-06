﻿// (c) 2020 Manabu Tonosaki
// Licensed under the MIT license.

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

        public CodePos<Distance, Distance> LT => CodePos<Distance, Distance>.From(L, T);
        public CodePos<Distance, Distance> RB => CodePos<Distance, Distance>.From(R, B);
        public CodePos<Distance, Distance> RT => CodePos<Distance, Distance>.From(R, T);
        public CodePos<Distance, Distance> LB => CodePos<Distance, Distance>.From(L, B);
        public CodePos<Distance, Distance> C => CodePos<Distance, Distance>.From((L + R) / 2, (T + B) / 2);
        public Distance Width { get => R - L; set => R = L + value; }
        public Distance Height { get => B - T; set => B = T + value; }
    }
}
