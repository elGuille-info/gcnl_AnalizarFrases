//-------------------------------------------------------------------------------
// Eliza-gcNL                                                   (29/ene/23 11.35)
// Prueba para usar Eliza con Google Cloud Natural Language API
//
// Ver los primeros ejemplos en: NaturalLanguageApiDemo
// usando el tutorial de: https://codelabs.developers.google.com/codelabs/cloud-natural-language-csharp#0
//
// (c)Guillermo Som (Guille), 2023
//-------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
//using System.Runtime.CompilerServices;
//using System.Text;

//using Google.Cloud.Language.V1;
//using Google.Protobuf.Collections;
//using Google.Protobuf.Collections;
//using static Google.Cloud.Language.V1.AnnotateTextRequest.Types;

using gcnl_AnalizarTextos;

namespace gcnl_AnalizarFrases;

class Program
{
    static void Main(string[] args)
    {
        // La evaluación de la relevancia/salience influye si los nombres propios están en mayúsculas o no.
        // El que la frase tenga varias separadas por punto (sentencias), también cambia las relaciones
        // (en principio estaban pensadas para una 'sentencia'.
        string text, ultimaOriginal;

        Console.WriteLine("Pruebas de Google Cloud Natural Language");
        Console.WriteLine();
        text = Frases.FrasesPrueba[^1]; // la última
        ultimaOriginal = text;

        bool repitiendo = false;
        do
        {
            for (int i = 0; i < Frases.FrasesPrueba.Count; i++)
            {
                Console.WriteLine($"{(char)(97 + i)}- {Frases.FrasesPrueba[i]}");
            };
            if (repitiendo)
            {
                Console.WriteLine($"Última: '{ultimaOriginal}'");
                Console.WriteLine("Indica o elige la letra de la frase a analizar (0- salir, 1- última en minúsculas, [la última])");
            }
            else
            {
                ultimaOriginal = text;
                Console.WriteLine($"Predeterminada: '{ultimaOriginal}'");
                Console.WriteLine("Indica o elige la letra de la frase a analizar (0- salir, 1- predeterminada en minúsculas, [predeterminada])");
            }
            Console.Write("> ");
            var resText = Console.ReadLine();
            if (!string.IsNullOrEmpty(resText))
            {
                if (resText == "0")
                {
                    break;
                }
                if (resText == "1")
                {
                    text = ultimaOriginal.ToLower();
                }
                else
                {
                    var seleccionada = resText[0] - 97;
                    if (seleccionada > -1 && seleccionada < 26)
                    {
                        text = Frases.FrasesPrueba[seleccionada];
                        ultimaOriginal = text;
                    }
                    else
                    {
                        if (resText.Length < 3)
                        {
                            Console.WriteLine("Por ahora la frase debe tener más de 2 caracteres.");
                            continue;
                        }
                        text = resText;
                        ultimaOriginal = resText;
                    }
                }
            }
            else
            {
                text = ultimaOriginal;
            }
            Console.WriteLine("  Analizando la frase...");
            var frase = Frases.Add(text);
            do
            {
                Console.WriteLine($"Análisis de: '{text}'");
                Console.Write("1- Todo, 2- Todo sin tokens, 3- Tokens, 4- Entities, 5- Resumen última, 6- Resumen todas, 0- nueva frase [2] ? ");
                resText = Console.ReadLine();
                Console.WriteLine();
                if (string.IsNullOrEmpty(resText))
                {
                    resText = "2";
                }
                if (resText == "1")
                {
                    Console.WriteLine(frase.Analizar(conTokens: true, soloEntities: false));
                }
                else if (resText == "2")
                {
                    Console.WriteLine(frase.Analizar(conTokens: false, soloEntities: false));
                }
                else if (resText == "3")
                {
                    Console.WriteLine(frase.MostrarTokens());
                }
                else if (resText == "4")
                {
                    Console.WriteLine(frase.Analizar(conTokens: false, soloEntities: true));
                }
                else if (resText == "5")
                {
                    Console.WriteLine(Frases.MostrarResumen(true));
                }
                else if (resText == "6")
                {
                    Console.WriteLine(Frases.MostrarResumen(false));
                }
                else if (resText == "0")
                {
                    break;
                }
                else
                {
                    Console.WriteLine($"'{resText}' no es una opción válida.");
                    continue;
                }
            } while (true);

            repitiendo = true;
        } while (true);
    }
}