using System.Text;

using gcnl_AnalizarFrases;

using Google.Cloud.Language.V1;
using Google.Protobuf.Collections;

namespace gcnl_AnalizarFrases_MAUI
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            listViewFrases.ItemsSource = FrasesPrueba;
        }

        private Frases frase = null;
        private string text, ultimaOriginal;
        private StringBuilder sbConsole = new();

        private readonly List<string> FrasesPrueba = new() {
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
        
        private void ContentPage_Appearing(object sender, EventArgs e)
        {
            QuitarAviso();
            // si no hay texto asignado, asignar la última frase de la lista
            if (string.IsNullOrEmpty(txtTexto.Text))
            {
                txtTexto.Text = FrasesPrueba[^1];
            }
            // si frase no está asignada, deshabilitar los botones de mostrar
            bool habilitar = frase != null;
            HabilitarBotones(habilitar);
        }

        private void HabilitarBotones(bool habilitar)
        {
            BtnMostrar1.IsEnabled = habilitar;
            BtnMostrar2.IsEnabled = habilitar;
            BtnMostrar3.IsEnabled = habilitar;
            BtnMostrar4.IsEnabled = habilitar;
            BtnMostrar5.IsEnabled = habilitar;
            BtnMostrar6.IsEnabled = habilitar;
        }

        private async void BtnAnalizar_Clicked(object sender, EventArgs e)
        {
            txtResultado.Text = "";

            string tmp = txtTexto.Text;
            if (string.IsNullOrEmpty(tmp))
            {
                MostrarAviso("Por favor indica el texto a analizar de al menos 3 caracteres", esError: true);
                txtTexto.Focus();
                return;
            }

            text = tmp;
            HabilitarBotones(false);

            await Task.Run(() =>
            {
                MostrarAviso("Analizando el texto...", esError: false);
                frase = Frases.Add(text);

                BtnMostrar1.Dispatcher.Dispatch(() =>
                {
                    // Inicialmente mostrar todo
                    BtnMostrar1_Clicked(null, null);
                });
                QuitarAviso();
            });

            HabilitarBotones(true);
        }

        private void QuitarAviso()
        {
            LabelAviso.Dispatcher.Dispatch(() => { LabelAviso.IsVisible = false; });
            grbAviso.Dispatcher.Dispatch(() => { grbAviso.BackgroundColor = Colors.Transparent; });
        }

        private void MostrarAviso(string aviso, bool esError)
        {
            grbAviso.Dispatcher.Dispatch(() =>
            {
                if (esError)
                {
                    grbAviso.BackgroundColor = Colors.Firebrick;
                }
                else
                {
                    grbAviso.BackgroundColor = Colors.SteelBlue;
                }
            });
            LabelAviso.Dispatcher.Dispatch(() =>
            {
                LabelAviso.Text = aviso;
                LabelAviso.IsVisible = true;
            });
        }

        private void BtnMostrar1_Clicked(object sender, EventArgs e)
        {
            sbConsole.Clear();
            Analizar(frase, conTokens: true, soloEntities: false);
            txtResultado.Text = sbConsole.ToString();
        }

        private void BtnMostrar2_Clicked(object sender, EventArgs e)
        {
            sbConsole.Clear();
            Analizar(frase, conTokens: false, soloEntities: false);
            txtResultado.Text = sbConsole.ToString();
        }

        private void BtnMostrar3_Clicked(object sender, EventArgs e)
        {
            sbConsole.Clear();
            MostrarTokens(frase.Response);
            txtResultado.Text = sbConsole.ToString();
        }

        private void BtnMostrar4_Clicked(object sender, EventArgs e)
        {
            sbConsole.Clear();
            Analizar(frase, conTokens: false, soloEntities: true);
            txtResultado.Text = sbConsole.ToString();
        }

        private void BtnMostrar5_Clicked(object sender, EventArgs e)
        {
            sbConsole.Clear();
            MostrarResumen(true);
            txtResultado.Text = sbConsole.ToString();
        }

        private void BtnMostrar6_Clicked(object sender, EventArgs e)
        {
            sbConsole.Clear();
            MostrarResumen(false);
            txtResultado.Text = sbConsole.ToString();
        }

        private void listViewFrases_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (listViewFrases.SelectedItem == null)
                return;

            ultimaOriginal = e.SelectedItem.ToString();
            txtTexto.Text = ultimaOriginal;
            text = ultimaOriginal;

            QuitarAviso();
        }

        private void MostrarResumen(bool ultima)
        {
            if (ultima)
            {
                sbConsole.AppendLine("Última frase analizada:");
                var i = Frases.LasFrases.Count - 1;
                var frase = Frases.LasFrases[i];
                sbConsole.AppendLine($"Texto: '{frase.Texto}'");
                sbConsole.AppendLine($"Root: '{frase.Root?.Text.Content}', Lemma: '{frase.Root?.Lemma}'");
                sbConsole.Append("  ");
                MostrarTokenPartOfSpeech(frase.Root);
                sbConsole.AppendLine($"Sentimiento: {frase.Sentimiento} ({frase.SentimientoScore}), absoluto: {frase.SentimientoAbsoluto}");
                sbConsole.AppendLine($"Entidades:");
                foreach (var entidad in frase.Entidades)
                {
                    sbConsole.AppendLine($"  {entidad}");
                }
                sbConsole.AppendLine($"Relaciones:");
                foreach (var relacion in frase.Relaciones)
                {
                    sbConsole.AppendLine($"  {relacion}");
                }
                return;
            }
            sbConsole.AppendLine($"Frases analizadas: {Frases.LasFrases.Count}");
            for (int i = 0; i < Frases.LasFrases.Count; i++)
            {
                var frase = Frases.LasFrases[i];
                sbConsole.AppendLine($"{i + 1,4}- Texto: '{frase.Texto}'");
                sbConsole.AppendLine($"      Root: '{frase.Root?.Text.Content}', Lemma: '{frase.Root?.Lemma}'");
                sbConsole.Append("        ");
                MostrarTokenPartOfSpeech(frase.Root);
                sbConsole.AppendLine($"      Sentimiento: {frase.Sentimiento} ({frase.SentimientoScore}), absoluto: {frase.SentimientoAbsoluto}");
                sbConsole.AppendLine($"      Entidades:");
                foreach (var entidad in frase.Entidades)
                {
                    sbConsole.AppendLine($"        {entidad}");
                }
                sbConsole.AppendLine($"      Relaciones:");
                foreach (var relacion in frase.Relaciones)
                {
                    sbConsole.AppendLine($"        {relacion}");
                }
            }
        }

        private void Analizar(Frases frase, bool conTokens, bool soloEntities)
        {
            var response = frase.Response;
            var sentiment = response.DocumentSentiment;

            sbConsole.AppendLine($"Detected language: {response.Language}");
            sbConsole.AppendLine($"Sentiment Score: {sentiment.Score}, Magnitude: {sentiment.Magnitude}");
            sbConsole.AppendLine("***Entities:");
            //Entity? entity1 = null;
            Entity entity1 = null;
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
                // Si se piden solo entidades y el valor de Salience es 0,
                // no mostrarlo si solo tiene una palabra
                if (soloEntities && entity.Salience == 0.0F && entity.Name.Contains(' ') == false) continue; // return;

                sbConsole.AppendLine($"Entity .Name: '{entity.Name}'");
                sbConsole.AppendLine($"  Type: {entity.Type},  Salience: {(int)(entity.Salience * 100)}%");
                if (entity.Mentions.Count > 0)
                {
                    sbConsole.AppendLine($"  Mentions: {entity.Mentions.Count}");
                    foreach (var mention in entity.Mentions)
                    {
                        sbConsole.Append($"    Text: '{mention.Text.Content}' (beginOffset: {mention.Text.BeginOffset}),");
                        sbConsole.AppendLine($" Type: {mention.Type}, Sentiment: {mention.Sentiment}");
                    }
                }
                if (entity.Metadata.Count > 0)
                {
                    sbConsole.AppendLine($"  Metadata: {entity.Metadata}");
                    if (entity.Metadata.ContainsKey("wikipedia_url"))
                    {
                        sbConsole.AppendLine($"    URL: {entity.Metadata["wikipedia_url"]}");
                    }
                }
            }
            if (soloEntities) return;

            // Las categorías solo funcionan con ClassifyText y solo en inglés
            sbConsole.AppendLine("***Categories:");
            foreach (var cat in response.Categories)
            {
                sbConsole.AppendLine($"Category: '{cat.Name}' (Confidence: {cat.Confidence})");
            }
            sbConsole.AppendLine("***Sentences:");
            foreach (var sentence in response.Sentences)
            {
                //sbConsole.AppendLine($" Sentence.Text .Content: '{sentence.Text.Content}'");
                sbConsole.AppendLine($" Sentence.Text.Content: '{sentence.Text.Content}'");
                sbConsole.AppendLine($"   Sentence.Text.BeginOffset: {sentence.Text.BeginOffset}");
                sbConsole.AppendLine($" Sentence.Sentiment .Magnitude: {sentence.Sentiment.Magnitude}, .Score: {sentence.Sentiment.Score}");
            }
            if (conTokens)
            {
                sbConsole.AppendLine("***Tokens:");
                for (int i = 0; i < response.Tokens.Count; i++)
                {
                    MostrarToken(i, response.Tokens, conContenido: false);
                }
            }
        }

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
        private void MostrarTokens(AnnotateTextResponse self)
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

        private void MostrarToken(int nToken, RepeatedField<Token> tokens, bool conContenido = true)
        {
            Token token = tokens[nToken];
            sbConsole.AppendLine($"{nToken}- Token: Text.Content: '{token.Text.Content}' (Text.BeginOffset: {token.Text.BeginOffset}), Lemma: '{token.Lemma}'");
            if (token.DependencyEdge.Label == DependencyEdge.Types.Label.Root)
            {
                sbConsole.Append($"  **DependencyEdge Label: {token.DependencyEdge.Label}");
                sbConsole.Append($", HeadTokenIndex: {token.DependencyEdge.HeadTokenIndex}");
                sbConsole.AppendLine("**");
            }
            else
            {
                sbConsole.Append($"  DependencyEdge Label: {token.DependencyEdge.Label}, HeadTokenIndex: {token.DependencyEdge.HeadTokenIndex}");
                var tokenDependency = tokens[token.DependencyEdge.HeadTokenIndex];
                sbConsole.AppendLine($" ('{tokenDependency.Text.Content}')");
            }
            if (conContenido)
            {
                sbConsole.AppendLine($"  PartOfSpeech:");
                sbConsole.Append($"    ");
                MostrarTokenPartOfSpeech(token);
            }
            else
            {
                sbConsole.AppendLine($"  PartOfSpeech Aspect: {token.PartOfSpeech.Aspect}, Case: {token.PartOfSpeech.Case}, Form: {token.PartOfSpeech.Form}");
                sbConsole.AppendLine($"  PartOfSpeech Gender: {token.PartOfSpeech.Gender}, Mood: {token.PartOfSpeech.Mood}, Number: {token.PartOfSpeech.Number}");
                sbConsole.AppendLine($"  PartOfSpeech Person: {token.PartOfSpeech.Person}, Proper: {token.PartOfSpeech.Proper}");
                sbConsole.AppendLine($"  PartOfSpeech Reciprocity: {token.PartOfSpeech.Reciprocity}, Tag: {token.PartOfSpeech.Tag}");
                sbConsole.AppendLine($"  PartOfSpeech Tense:: {token.PartOfSpeech.Tense}, Voice: {token.PartOfSpeech.Voice}");
            }
        }

        private void MostrarTokenPartOfSpeech(Token token)
        {
            if (token == null) return;

            sbConsole.Append($"Tag: {token.PartOfSpeech.Tag},");
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
                sbConsole.AppendLine(sb.ToString().TrimEnd(','));
            }
            else
            {
                sbConsole.AppendLine();
            }
        }
    }
}