using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Doppler.Sap.Mappers
{
    public static class Dictionaries
    {
        public static readonly Dictionary<int?, string> ConsumerTypesDictionary = new Dictionary<int?, string>
        {
            { 1 , "CF" },
            { 2 , "RI"},
            { 4 , "RFC"},
            { 5 , "RNI"},
            { 6 , "MT"},
            { 7 , "EX"},
            { 8 , "NG"},
            { 9 , "NC"}
        };

        public static readonly Dictionary<int, int> StatesDictionary = new Dictionary<int, int>
        {
            {2189,1}, // Buenos Aires
            {2190,2}, // Catamarca
            {2191,16}, // Chaco
            {2192,17}, // Chubut
            {2193,0}, // Ciudad Autónoma de Buenos Aires
            {2194,4}, // Corrientes
            {2195,3}, // Córdoba
            {2196,5}, // Entre Ríos
            {2197,18}, // Formosa
            {2198,6}, // Jujuy
            {2199,21}, // La Pampa
            {2200,8}, // La Rioja
            {2201,7}, // Mendoza
            {2202,19}, // Misiones
            {2203,20}, // Neuquén
            {2204,22}, // Río Negro
            {2205,9}, // Salta
            {2206,10}, // San Juan
            {2207,11}, // San Luis
            {2208,12}, // Santa Cruz
            {2209,13}, // Santa Fe
            {2210,14}, // Santiago del Estero
            {2211,24}, // Tierra del Fuego
            {2212,15}, // Tucumán
        };

        public static readonly Dictionary<int?, string> userPlanTypesDictionary = new Dictionary<int?, string>
        {
            { 0 , "CM" },
            { 1 , "CD"},
            { 2 , "CD"},
            { 3 , "CD"},
            { 4 , "CD"},
            { 5 , "CR"}
        };
    }
}
