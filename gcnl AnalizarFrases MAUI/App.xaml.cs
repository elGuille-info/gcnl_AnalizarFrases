using System;
using System.Diagnostics;
using System.IO;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Storage;

namespace gcnl_AnalizarFrases_MAUI
{
    public partial class App : Application
    {
        public string Titulo = "Analizar texto usando Cloud Natural Language";

        // Una forma más simple de cambiar el tamaño de la ventana.
        // Lo dejo porque con este código no se ve el cambio de tamaño de la ventana.
        // Aunque en realidad solo funcionará con Windows, en Android no tiene efecto,
        // así que... lo dejo solo para Windows.
        //
        // Basado en: https://es.askxammy.com/manejo-de-tamano-y-posicion-de-ventanas-en-net-maui/
        // https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/windows?view=net-maui-7.0
        //
#if WINDOWS
        protected override Window CreateWindow(IActivationState activationState)
        {
            //return base.CreateWindow(activationState);
            var window = base.CreateWindow(activationState);

            // Add here your sizing code

            window.Width = 935 + 15;
            window.Height = 880 + 57;

            // Add here your positioning code

            DisplayInfo disp = DeviceDisplay.Current.MainDisplayInfo;
            window.X = (disp.Width / disp.Density - window.Width * disp.Density) / 2;
            window.Y = (disp.Height / disp.Density - window.Height * disp.Density) / 2;

            window.Title = Titulo;

            // En realidad este es código específico para Windows.

            // Cambiar el color de la barra de Windows
            Microsoft.Maui.Handlers.WindowHandler.Mapper.AppendToMapping(nameof(IWindow), (handler, view) =>
            {
                var mauiWindow = handler.VirtualView;
                var nativeWindow = handler.PlatformView;
                nativeWindow.Activate();
                IntPtr windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(nativeWindow);
                var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(windowHandle);
                var window = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);

                // dispatcher is used to give the window time to actually resize
                Dispatcher.Dispatch(() =>
                {
                    // Este es el color que tiene en mi equipo la barra de título.
                    window.TitleBar.BackgroundColor = Microsoft.UI.ColorHelper.FromArgb(255, 0, 120, 212);
                    window.TitleBar.ForegroundColor = Microsoft.UI.Colors.White;
                });
            });

            return window;
        }
#endif

        public App()
        {
            InitializeComponent();

            // Copiar el fichero key.json en LocalApplicationData
            // En iPhone no llega aquí
            CopiarGoogleCredentials();

            //var env = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS");


            // Cambiar el tamaño para la app de Windows,
            // posicionarla, asignar el título y color de la barra del título.
//#if WINDOWS
//            Microsoft.Maui.Handlers.WindowHandler.Mapper.AppendToMapping(nameof(IWindow), (handler, view) =>
//            {
//                // Asignar manualmente el tamaño. 
//                // Según mis cálculos, el tamaño mostrado por Width y Height de la ventana principal
//                // Es 15 menos del ancho aquí indicado y ~57 menos del alto indicado
//                int winWidth = 935+15; // 830+15; // 830; // 1200; // 1700; // 2800;
//                int winHeight = 880+57; //1070+57; //995+57; // 835+57; // 825; // 865; // 1600; //1800

//                var mauiWindow = handler.VirtualView;
//                var nativeWindow = handler.PlatformView;
//                nativeWindow.Activate();
//                IntPtr windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(nativeWindow);
//                var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(windowHandle);
//                var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);

//                // get screen size
//                DisplayInfo disp = DeviceDisplay.Current.MainDisplayInfo;
//                //double x, y;

//                // dispatcher is used to give the window time to actually resize
//                Dispatcher.Dispatch(() =>
//                {
//                    disp = DeviceDisplay.Current.MainDisplayInfo;
                    
//                    // Si Density es diferente de 1, ajustar el tamaño.
//                    if (disp.Density > 1)
//                    {
//                        winWidth = (int)(winWidth * disp.Density);
//                        winHeight = (int)(winHeight * disp.Density);
//                    }

//                    // El tamaño de la pantalla de este equipo.
//                    //int screenW = (int)(disp.Width / disp.Density);
//                    //int screenH = (int)(disp.Height / disp.Density);
//                    //// Si el alto indicado es mayor, ponerlo para que entre en esta pantalla.
//                    //if (winHeight > screenH)
//                    //{
//                    //    winHeight = screenH - 60;
//                    //}
//                    //// Si el ancho indicado es mayor, ponerlo para que entre en esta pantalla.
//                    //if (winWidth > screenW)
//                    //{
//                    //    winWidth = screenW - 60;
//                    //}

//                    appWindow.Resize(new Windows.Graphics.SizeInt32(winWidth, winHeight));

//                    //x = (screenW - winWidth) / 2;
//                    //if (x < 0) { x = 0; }
//                    //y = (screenH - winHeight - 40) / 2;
//                    //if (y < 0) { y = 0; }
//                    //appWindow.Move(new Windows.Graphics.PointInt32((int)x, (int)y));

//                    // El título hay que asignarlo antes de asignar los colores.
//                    appWindow.Title = Titulo;
//                    // Este es el color que tiene en mi equipo la barra de título.
//                    appWindow.TitleBar.BackgroundColor = Microsoft.UI.ColorHelper.FromArgb(255, 0, 120, 212);
//                    appWindow.TitleBar.ForegroundColor = Microsoft.UI.Colors.White;
//                });

//        });
//#endif

            MainPage = new AppShell();
        }

        /// <summary>
        /// Copia el fichero key.json de la carpeta Resources\Raw a LocalApplicationData.
        /// </summary>
        /// <remarks>En este método se asigna la variable de entorno GOOGLE_APPLICATION_CREDENTIALS con el path de key.json</remarks>
        async void CopiarGoogleCredentials()
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync("key.json");
            using var reader = new StreamReader(stream);

            var contents = reader.ReadToEnd();

            var localAppData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
            var gAppCredentials = Path.Combine(localAppData, "key.json");

            using var outputStream = File.OpenWrite(gAppCredentials);
            using var streamWriter = new StreamWriter(outputStream);

            await streamWriter.WriteAsync(contents);

            // Asignar la variable de entorno indicando dónde está el fichero key.json
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", gAppCredentials);
        }
    }
}