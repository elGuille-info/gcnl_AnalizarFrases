namespace gcnl_AnalizarFrases_MAUI
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            listViewFrases.ItemsSource = frases;
        }

        private List<string> frases = new() {
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
        private string text, ultimaOriginal;

        private void ContentPage_Appearing(object sender, EventArgs e)
        {

        }

        private void listViewFrases_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {

        }
    }
}