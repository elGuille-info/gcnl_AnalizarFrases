﻿using System.Text;

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
        //private StringBuilder sbConsole = new();

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
                    // Inicialmente mostrar todo sin tokens
                    BtnMostrar2_Clicked(null, null);
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
            txtResultado.Text = frase.Analizar(conTokens: true, soloEntities: false);
        }

        private void BtnMostrar2_Clicked(object sender, EventArgs e)
        {
            txtResultado.Text = frase.Analizar(conTokens: false, soloEntities: false);
        }

        private void BtnMostrar3_Clicked(object sender, EventArgs e)
        {
            txtResultado.Text = frase.MostrarTokens();
        }

        private void BtnMostrar4_Clicked(object sender, EventArgs e)
        {
            txtResultado.Text = frase.Analizar(conTokens: false, soloEntities: true);
        }

        private void BtnMostrar5_Clicked(object sender, EventArgs e)
        {
            txtResultado.Text = Frases.MostrarResumen(true);
        }

        private void BtnMostrar6_Clicked(object sender, EventArgs e)
        {
            txtResultado.Text = Frases.MostrarResumen(false);
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

        private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
        {
            InfoTamañoVentana();
        }

        private void LabelStatus_SizeChanged(object sender, EventArgs e)
        {
            InfoTamañoVentana();
        }

        private void InfoTamañoVentana()
        {
            LabelStatus.Text = $"Width: {(int)Width}, Height: {(int)Height}";
        }
    }
}