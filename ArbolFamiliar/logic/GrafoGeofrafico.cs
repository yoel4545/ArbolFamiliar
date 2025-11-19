using System;
using System.Collections.Generic;

namespace ArbolFamiliar
{
    
    public class GrafoGeografico
    {
        // Diccionario que guarda las distancias entre cada par de personas
        private Dictionary<Person, Dictionary<Person, double>> distancias;

        public GrafoGeografico(List<Person> personas)
        {
            distancias = new Dictionary<Person, Dictionary<Person, double>>();
            ConstruirGrafo(personas);
        }

        // Método que calcula todas las distancias entre pares únicos de personas
        private void ConstruirGrafo(List<Person> personas)
        {
            foreach (var p1 in personas)
            {
                // Inicializa el diccionario interno para cada persona
                distancias[p1] = new Dictionary<Person, double>();

                foreach (var p2 in personas)
                {
                    // no calcula distancia consigo mismo
                    if (p1 != p2)
                    {
                        // Calcula la distancia entre p1 y p2
                        double distancia = CalcularDistancia(p1, p2);

                        // Guarda distancia en el diccionario
                        distancias[p1][p2] = distancia;
                    }
                }
            }
        }

        // Método que calcula la distancia geográfica entre dos personas usando la fórmula de Haversine
        private double CalcularDistancia(Person a, Person b)
        {
            double R = 6371; // Radio de la Tierra en kilómetros

            // Convertir latitudes y longitudes a radianes
            double lat1 = a.Latitud * Math.PI / 180;
            double lat2 = b.Latitud * Math.PI / 180;
            double dLat = (b.Latitud - a.Latitud) * Math.PI / 180;
            double dLon = (b.Longitud - a.Longitud) * Math.PI / 180;

            // Fórmula de Haversine
            double h = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(lat1) * Math.Cos(lat2) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(h), Math.Sqrt(1 - h));
            return R * c; // Distancia en kilómetros
        }

        // Devuelve el diccionario completo de distancias entre personas
        public Dictionary<Person, Dictionary<Person, double>> GetDistancias()
        {
            return distancias;
        }

        // Devuelve el par de personas más lejano 
        public (Person, Person, double) ObtenerParMasLejano()
        {
            double max = 0;
            Person p1 = null, p2 = null;

            foreach (var origen in distancias)
            {
                foreach (var destino in origen.Value)
                {
                    if (destino.Value > max)
                    {
                        max = destino.Value;
                        p1 = origen.Key;
                        p2 = destino.Key;
                    }
                }
            }

            return (p1, p2, max);
        }

        // Devuelve el par de personas más cercano 
        public (Person, Person, double) ObtenerParMasCercano()
        {
            double min = double.MaxValue;
            Person p1 = null, p2 = null;

            foreach (var origen in distancias)
            {
                foreach (var destino in origen.Value)
                {
                    if (destino.Value < min && destino.Value > 0)
                    {
                        min = destino.Value;
                        p1 = origen.Key;
                        p2 = destino.Key;
                    }
                }
            }

            return (p1, p2, min);
        }

        // Calcula la distancia promedio entre todos los pares de personas
        public double CalcularDistanciaPromedio()
        {
            double total = 0;
            int conteo = 0;

            foreach (var origen in distancias)
            {
                foreach (var destino in origen.Value)
                {
                    total += destino.Value;
                    conteo++;
                }
            }

            return conteo > 0 ? total / conteo : 0;
        }
    }
}
