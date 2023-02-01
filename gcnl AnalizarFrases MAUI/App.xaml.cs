using System;
using System.Diagnostics;
using System.IO;

namespace gcnl_AnalizarFrases_MAUI
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // Copiar el fichero key.json en LocalApplicationData
            CopiarGoogleCredentials();

            MainPage = new AppShell();
        }

        /// <summary>
        /// Copia el fichero key.json de la carpeta Resources\Raw a LocalApplicationData.
        /// </summary>
        /// <remarks>En este método se asigna la variable de entorno GOOGLE_APPLICATION_CREDENTIALS con el path de key.json</remarks>
        static async void CopiarGoogleCredentials()
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