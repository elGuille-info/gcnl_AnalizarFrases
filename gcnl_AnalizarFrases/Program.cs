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
using System.Text;

using Google.Cloud.Language.V1;
using Google.Protobuf.Collections;
//using Google.Protobuf.Collections;
//using static Google.Cloud.Language.V1.AnnotateTextRequest.Types;

namespace gcnl_AnalizarFrases;

class Program
{
    static void Main(string[] args)
    {
        // La evaluación de la relevancia/salience influye si los nombres propios están en mayúsculas o no.
        // El que la frase tenga varias separadas por punto (sentencias), también cambia las relaciones
        // (en principio estaban pensadas para una 'sentencia'.
        List<string> frases = new() {
            "El 8 de Febrero voy en bici al Camino de Santiago desde Sarria ¿crees que aguantaré?",
            "El 8 de Febrero voy en bici al Camino de Santiago desde Sarria. ¿crees que aguantaré?",
            "El 8 de Febrero voy en bici al Camino de Santiago desde Sarria ¿Crees que aguantaré?",
            "El 8 de Febrero voy en bici al Camino de Santiago desde Sarria. ¿Crees que aguantaré?",
            "El 8 de febrero voy en bici al camino de Santiago desde Sarria ¿crees que aguantaré?",
            "El 8 de febrero voy en bici al camino de Santiago desde Sarria. ¿crees que aguantaré?",
            "Ask not what your country can do for you, ask what you can do for your country.",
            "On February 8 I go, with Ana and her cousin, by bike to the Camino de Santiago from Sarria. Do you think I will endure with the bike?",
            "El 8 de Febrero voy, con Ana y su prima, en bici al Camino de Santiago desde Sarria ¿Crees que aguantaré?",
            "El 8 de Febrero voy, con Ana y su prima, en bici al Camino de Santiago desde Sarria. ¿Crees que aguantaré?",
            "El 8 de Febrero voy, con Ana y su prima, en bici al Camino de Santiago desde Sarria ¿crees que aguantaré?",
            "El 8 de Febrero voy, con Ana y su prima, en bici al Camino de Santiago desde Sarria. ¿crees que aguantaré?",
            "El 8 de Febrero voy, con Ana y su prima, en bici al Camino de Santiago desde Sarria ¿Crees que aguantaré con la bici?",
            "El 8 de Febrero voy, con Ana y su prima, en bici al Camino de Santiago desde Sarria. ¿Crees que aguantaré con la bici?",
            "El 8 de Febrero voy, con Ana y su prima, en bici al Camino de Santiago desde Sarria ¿crees que aguantaré con la bici?",
            "El 8 de Febrero voy, con Ana y su prima, en bici al Camino de Santiago desde Sarria. ¿crees que aguantaré con la bici?",
            "El 8 de febrero iré en bici al camino de Santiago desde Sarria. ¿Crees que lo lograré?",
            "El 8 de febrero iré en bici al camino de Santiago desde Sarria ¿crees que lo lograré?"
        };
        string text, ultimaOriginal;

        Console.WriteLine("Pruebas de Google Cloud Natural Language");
        Console.WriteLine();
        text = frases[^1]; // la última
        ultimaOriginal = text;

        bool repitiendo = false;
        do
        {
            for (int i = 0; i < frases.Count; i++)
            {
                Console.WriteLine($"{(char)(97 + i)}- {frases[i]}");
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
                        text = frases[seleccionada];
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
                    Analizar(frase, conTokens: true, soloEntities: false);
                }
                else if (resText == "2")
                {
                    Analizar(frase, conTokens: false, soloEntities: false);
                }
                else if (resText == "3")
                {
                    //MostrarTokens(frase);
                    MostrarTokens(frase.Response);
                }
                else if (resText == "4")
                {
                    Analizar(frase, conTokens: false, soloEntities: true);
                }
                else if (resText == "5")
                {
                    MostrarResumen(true);
                }
                else if (resText == "6")
                {
                    MostrarResumen(false);
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

    static void MostrarResumen(bool ultima)
    {
        if (ultima)
        {
            Console.WriteLine("Última frase analizada:");
            var i = Frases.LasFrases.Count - 1;
            var frase = Frases.LasFrases[i];
            Console.WriteLine($"Texto: '{frase.Texto}'");
            Console.WriteLine($"Root: '{frase.Root?.Text.Content}', Lemma: '{frase.Root?.Lemma}'");
            Console.Write("  ");
            MostrarTokenPartOfSpeech(frase.Root);
            Console.WriteLine($"Sentimiento: {frase.Sentimiento} ({frase.SentimientoScore}), absoluto: {frase.SentimientoAbsoluto}");
            Console.WriteLine($"Entidades:");
            foreach (var entidad in frase.Entidades)
            {
                Console.WriteLine($"  {entidad}");
            }
            Console.WriteLine($"Relaciones:");
            foreach (var relacion in frase.Relaciones)
            {
                Console.WriteLine($"  {relacion}");
            }
            return;
        }
        Console.WriteLine($"Frases analizadas: {Frases.LasFrases.Count}");
        for (int i = 0; i < Frases.LasFrases.Count; i++)
        {
            var frase = Frases.LasFrases[i];
            Console.WriteLine($"{i + 1,4}- Texto: '{frase.Texto}'");
            Console.WriteLine($"      Root: '{frase.Root?.Text.Content}', Lemma: '{frase.Root?.Lemma}'");
            Console.Write("        ");
            MostrarTokenPartOfSpeech(frase.Root);
            Console.WriteLine($"      Sentimiento: {frase.Sentimiento} ({frase.SentimientoScore}), absoluto: {frase.SentimientoAbsoluto}");
            Console.WriteLine($"      Entidades:");
            foreach (var entidad in frase.Entidades)
            {
                Console.WriteLine($"        {entidad}");
            }
            Console.WriteLine($"      Relaciones:");
            foreach (var relacion in frase.Relaciones)
            {
                Console.WriteLine($"        {relacion}");
            }
        }
    }

    static void Analizar(Frases frase, bool conTokens, bool soloEntities)
    {
        var response = frase.Response;
        var sentiment = response.DocumentSentiment;

        Console.WriteLine($"Detected language: {response.Language}");
        Console.WriteLine($"Sentiment Score: {sentiment.Score}, Magnitude: {sentiment.Magnitude}");
        Console.WriteLine("***Entities:");
        Entity? entity1 = null;
        foreach (var entity in response.Entities)
        {
            // algunos entities están repetidos y seguidos
            if (entity1 == null)
            {
                entity1 = entity;
            }
            else
            {
                if (entity == entity1) continue;
            }
            //if (soloEntities && entity.Salience == 0.0F) continue; // return;
            // Si se piden solo entidades y el valor de Salience es 0,
            // no mostrarlo si solo tiene una palabra
            if (soloEntities && entity.Salience == 0.0F && entity.Name.Contains(' ') == false) continue; // return;

            Console.WriteLine($"Entity .Name: '{entity.Name}'");
            Console.WriteLine($"  Type: {entity.Type},  Salience: {(int)(entity.Salience * 100)}%");
            if (entity.Mentions.Count > 0)
            {
                Console.WriteLine($"  Mentions: {entity.Mentions.Count}");
                foreach (var mention in entity.Mentions)
                {
                    Console.Write($"    Text: '{mention.Text.Content}' (beginOffset: {mention.Text.BeginOffset}),");
                    Console.WriteLine($" Type: {mention.Type}, Sentiment: {mention.Sentiment}");
                }
            }
            if (entity.Metadata.Count > 0)
            {
                Console.WriteLine($"  Metadata: {entity.Metadata}");
                if (entity.Metadata.ContainsKey("wikipedia_url"))
                {
                    Console.WriteLine($"    URL: {entity.Metadata["wikipedia_url"]}");
                }
            }
        }
        if (soloEntities) return;

        // Las categorías solo funcionan con ClassifyText y solo en inglés
        Console.WriteLine("***Categories:");
        foreach (var cat in response.Categories)
        {
            Console.WriteLine($"Category: '{cat.Name}' (Confidence: {cat.Confidence})");
        }
        Console.WriteLine("***Sentences:");
        foreach (var sentence in response.Sentences)
        {
            //Console.WriteLine($" Sentence.Text .Content: '{sentence.Text.Content}'");
            Console.WriteLine($" Sentence.Text.Content: '{sentence.Text.Content}'");
            Console.WriteLine($"   Sentence.Text.BeginOffset: {sentence.Text.BeginOffset}");
            Console.WriteLine($" Sentence.Sentiment .Magnitude: {sentence.Sentiment.Magnitude}, .Score: {sentence.Sentiment.Score}");
        }
        if (conTokens)
        {
            Console.WriteLine("***Tokens:");
            for (int i = 0; i < response.Tokens.Count; i++)
            {
                MostrarToken(i, response.Tokens, conContenido: false);
            }
        }
    }

    //static void MostrarTokens(Frases frase)
    //{
    //    MostrarTokens(frase.Response);
    //}

    /*
    index = 0
      for sentence in self.sentences:
        content  = sentence['text']['content']
        sentence_begin = sentence['text']['beginOffset']
        sentence_end = sentence_begin + len(content) - 1
        while index < len(self.tokens) and self.tokens[index]['text']['beginOffset'] <= sentence_end:
          # This token is in this sentence
          index += 1
    */
    static void MostrarTokens(AnnotateTextResponse self)
    {
        int index = 0;
        foreach (var sentence in self.Sentences)
        {
            var content = sentence.Text.Content;
            var sentence_begin = sentence.Text.BeginOffset;
            var sentence_end = sentence_begin + content.Length - 1;
            while (index < self.Tokens.Count && self.Tokens[index].Text.BeginOffset <= sentence_end)
            {
                //# This token is in this sentence
                MostrarToken(index, self.Tokens, conContenido: true);

                index += 1;
            }
        }
    }
            
    private static void MostrarToken(int nToken, RepeatedField<Token> tokens, bool conContenido = true)
    {
        Token token = tokens[nToken];
        Console.WriteLine($"{nToken}- Token: Text.Content: '{token.Text.Content}' (Text.BeginOffset: {token.Text.BeginOffset}), Lemma: '{token.Lemma}'");
        if (token.DependencyEdge.Label == DependencyEdge.Types.Label.Root)
        {
            Console.Write($"  **DependencyEdge Label: {token.DependencyEdge.Label}");
            Console.Write($", HeadTokenIndex: {token.DependencyEdge.HeadTokenIndex}");
            Console.WriteLine("**");
        }
        else
        {
            Console.Write($"  DependencyEdge Label: {token.DependencyEdge.Label}, HeadTokenIndex: {token.DependencyEdge.HeadTokenIndex}");
            var tokenDependency = tokens[token.DependencyEdge.HeadTokenIndex];
            Console.WriteLine($" ('{tokenDependency.Text.Content}')");
        }
        if (conContenido)
        {
            Console.WriteLine($"  PartOfSpeech:");
            Console.Write($"    ");
            MostrarTokenPartOfSpeech(token);
        }
        else
        {
            Console.WriteLine($"  PartOfSpeech Aspect: {token.PartOfSpeech.Aspect}, Case: {token.PartOfSpeech.Case}, Form: {token.PartOfSpeech.Form}");
            Console.WriteLine($"  PartOfSpeech Gender: {token.PartOfSpeech.Gender}, Mood: {token.PartOfSpeech.Mood}, Number: {token.PartOfSpeech.Number}");
            Console.WriteLine($"  PartOfSpeech Person: {token.PartOfSpeech.Person}, Proper: {token.PartOfSpeech.Proper}");
            Console.WriteLine($"  PartOfSpeech Reciprocity: {token.PartOfSpeech.Reciprocity}, Tag: {token.PartOfSpeech.Tag}");
            Console.WriteLine($"  PartOfSpeech Tense:: {token.PartOfSpeech.Tense}, Voice: {token.PartOfSpeech.Voice}");
        }
    }

    private static void MostrarTokenPartOfSpeech(Token? token)
    {
        if (token == null) return;

        Console.Write($"Tag: {token.PartOfSpeech.Tag},");
        var sb = new StringBuilder();
        // Si tiene Aspect, puede tener Case y Form
        if (token.PartOfSpeech.Aspect != PartOfSpeech.Types.Aspect.Unknown)
        {
            sb.Append($" (Aspect: {token.PartOfSpeech.Aspect},");
            if (token.PartOfSpeech.Case != PartOfSpeech.Types.Case.Unknown)
            {
                sb.Append($" Case: {token.PartOfSpeech.Case},");
            }
            if (token.PartOfSpeech.Form != PartOfSpeech.Types.Form.Unknown)
            {
                sb.Append($" Form: {token.PartOfSpeech.Form},");
            }
            if (sb.ToString().EndsWith(','))
            {
                sb.Length -= 1;
            }
            sb.Append("),");
        }

        // Si tiene Gender, puede tener Mood y Number
        if (token.PartOfSpeech.Gender != PartOfSpeech.Types.Gender.Unknown)
        {
            sb.Append($" (Gender: {token.PartOfSpeech.Gender},");
            if (token.PartOfSpeech.Mood != PartOfSpeech.Types.Mood.Unknown)
            {
                sb.Append($" Mood: {token.PartOfSpeech.Mood},");
            }
            if (token.PartOfSpeech.Number != PartOfSpeech.Types.Number.Unknown)
            {
                sb.Append($" Number: {token.PartOfSpeech.Number},");
            }
            if (sb.ToString().EndsWith(','))
            {
                sb.Length -= 1;
            }
            sb.Append("),");
        }

        if (token.PartOfSpeech.Proper != PartOfSpeech.Types.Proper.Unknown)
        {
            sb.Append($" Proper: {token.PartOfSpeech.Proper},");
        }

        if (token.PartOfSpeech.Person != PartOfSpeech.Types.Person.Unknown)
        {
            sb.Append($" Person: {token.PartOfSpeech.Person},");
        }
        if (token.PartOfSpeech.Reciprocity != PartOfSpeech.Types.Reciprocity.Unknown)
        {
            sb.Append($" Reciprocity: {token.PartOfSpeech.Reciprocity},");
        }
        if (token.PartOfSpeech.Tense != PartOfSpeech.Types.Tense.Unknown)
        {
            sb.Append($" Tense: {token.PartOfSpeech.Tense},");
        }
        if (token.PartOfSpeech.Voice != PartOfSpeech.Types.Voice.Unknown)
        {
            sb.Append($" Voice: {token.PartOfSpeech.Voice}");
        }

        if (sb.ToString().Trim().Length > 0)
        {
            Console.WriteLine(sb.ToString().TrimEnd(','));
        }
        else
        {
            Console.WriteLine();
        }
    }
}