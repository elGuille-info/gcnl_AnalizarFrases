//
// En este proyecto añado un enlace a la clase Frases,
// ya que para poder añadir una referencia a la DLL que contiene esa clase debe estar creado el proyecto como Class Library para MAUI
// y ese tipo de proyecto no sirve para la aplicación de consola (o no me ha funcionado).
//

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using gcnl_AnalizarTextos;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace gcnl_AnalizarFrases_MAUI
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            ListViewFrases.ItemsSource = Frases.FrasesPrueba;
        }

        private Frases frase = null;
        private string text, ultimaOriginal;

        private void ContentPage_Appearing(object sender, EventArgs e)
        {
            ActualizarImagenExpander();

            QuitarAviso();
            // si no hay texto asignado, asignar la última frase de la lista
            if (string.IsNullOrEmpty(TxtTexto.Text))
            {
                TxtTexto.Text = Frases.FrasesPrueba[^1];
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
            TxtResultado.Text = "";

            string tmp = TxtTexto.Text;
            if (string.IsNullOrEmpty(tmp))
            {
                MostrarAviso("Por favor indica el texto a analizar de al menos 3 caracteres", esError: true);
                TxtTexto.Focus();
                return;
            }

            text = tmp;
            HabilitarBotones(false);

            await Task.Run(() =>
            {
                MostrarAviso("Analizando el texto...", esError: false);
                frase = Frases.Add(text);

                BtnMostrar2.Dispatcher.Dispatch(() =>
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
            GrbAviso.Dispatcher.Dispatch(() => { GrbAviso.BackgroundColor = Colors.Transparent; });
        }

        private void MostrarAviso(string aviso, bool esError)
        {
            GrbAviso.Dispatcher.Dispatch(() =>
            {
                GrbAviso.BackgroundColor = esError ? Colors.Firebrick : Colors.SteelBlue;
            });

            LabelAviso.Dispatcher.Dispatch(() =>
            {
                LabelAviso.Text = aviso;
                LabelAviso.IsVisible = true;
            });
        }

        private void BtnMostrar1_Clicked(object sender, EventArgs e)
        {
            TxtResultado.Text = frase.Analizar(conTokens: true, soloEntities: false);
            // Mostrar el texto desde el principio
            TxtResultado.CursorPosition = 0;
        }

        private void BtnMostrar2_Clicked(object sender, EventArgs e)
        {
            TxtResultado.Text = frase.Analizar(conTokens: false, soloEntities: false);
            TxtResultado.CursorPosition = 0;
        }

        private void BtnMostrar3_Clicked(object sender, EventArgs e)
        {
            TxtResultado.Text = frase.MostrarTokens();
            TxtResultado.CursorPosition = 0;
        }

        private void BtnMostrar4_Clicked(object sender, EventArgs e)
        {
            TxtResultado.Text = frase.Analizar(conTokens: false, soloEntities: true);
            TxtResultado.CursorPosition = 0;
        }

        private void BtnMostrar5_Clicked(object sender, EventArgs e)
        {
            TxtResultado.Text = Frases.MostrarResumen(true);
            TxtResultado.CursorPosition = 0;
        }

        private void BtnMostrar6_Clicked(object sender, EventArgs e)
        {
            TxtResultado.Text = Frases.MostrarResumen(false);
            TxtResultado.CursorPosition = 0;
        }

        private void ListViewFrases_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (ListViewFrases.SelectedItem == null)
                return;

            ultimaOriginal = e.SelectedItem.ToString();
            TxtTexto.Text = ultimaOriginal;
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

        //
        // Para ocultar / mostrar los contenedores. (31/Oct/21 18.41)
        //

        private void LabelListaTextos_Tapped(object sender, EventArgs e)
        {
            GrbListaTextos.IsVisible = !GrbListaTextos.IsVisible;
            ActualizarImagenExpander();
        }

        /// <summary>
        /// Muestra las imágenes que correspondan según estén visibles o no.
        /// </summary>
        private void ActualizarImagenExpander()
        {
            AsignarImagenExpander(ImgListaTextos, GrbListaTextos.IsVisible, usarImagenBlanca: true);
        }

        // Para poder usar el expander simulado. (16/sep/22 03.43)

        /// <summary>
        /// Asignar la imagen del expander según esté expandido o no y según sea usarImagenBlanca o no.
        /// </summary>
        /// <param name="imgExpander">El control Image al que asignar la imagen.</param>
        /// <param name="isExpanded">Si está expandido o no.</param>
        /// <param name="usarImagenBlanca">True si se usa la imagen blanca, false si se usa la oscura.</param>
        private static void AsignarImagenExpander(Image imgExpander, bool isExpanded, bool usarImagenBlanca)
        {
            // Si está expandido hay que mostrar collapse y al revés. (02/sep/22 22.11)
            string imgSource = isExpanded ? usarImagenBlanca ? "collapse_white.png" : "collapse.png" 
                                          : usarImagenBlanca ? "expand_white.png" : "expand.png";
            
            //imgExpander.Source = FileImageSource.FromResource($"gcnl_AnalizarFrases_MAUI.Resources.Images.{imgSource}", typeof(MainPage).Assembly);
            // En .NET MAUI solo se indica el nombre de la imagen a usar. (02/feb/23 12.37)
            imgExpander.Source = imgSource;
        }

    }
}