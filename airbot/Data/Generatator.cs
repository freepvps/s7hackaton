using System;
using System.Collections.Generic;
using System.Text;

namespace airbot.Data
{
    public static class Generatator
    {
        private static Random Random = new Random();

        public static int[] flightHours = { 1, 2, 3 };
        public static string[] cities = { "msk", "prm", "spb" };

        public static Database Generate()
        {
            var database = new Database();
            foreach (var city1 in cities)
            {
                foreach (var city2 in cities)
                {
                    if (city1 == city2) continue;
                    for (var i = 0; i < 128; i++)
                    {
                        var time = new DateTime(2017, 5, 21, 0, 0, 0).AddHours(i * 3);
                        foreach (var h in flightHours)
                        {
                            if (Random.Next(0, 3) != 0)
                            {
                                var r = Random.Next(0, 5);
                                for (var t = 0; t < r; t++)
                                {
                                    database.Flights.Add(new Flight(city1, city2, time, time.AddHours(h))
                                    {
                                        Cost = Random.Next(2000, 10000)
                                    });
                                }
                            }
                        }
                    }
                }
            }
            return database;
        }
    }
}
