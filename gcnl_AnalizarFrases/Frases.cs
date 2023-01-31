//-------------------------------------------------------------------------------
// Frases                                                       (29/ene/23 12.08)
// Para contener las frases (response) del texto analizado.
//
// Usando Google Cloud Natural Language API
//
// (c)Guillermo Som (Guille), 2023
//-------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

//using Google.Apis.Discovery;
using Google.Cloud.Language.V1;
using Google.Protobuf.Collections;

//using Grpc.Net.Client.Balancer;

using static Google.Cloud.Language.V1.AnnotateTextRequest.Types;

namespace gcnl_AnalizarFrases;

public class Frases
{
    /// <summary>
    /// Retorno de carro y nueva línea para Windows, equivalente a vbCrLf.
    /// </summary>
    public const string CrLf = "\r\n";

    /// <summary>
    /// Enumeración para simplificar los sentimientos del texto o frase analizada.
    /// </summary>
    public enum Sentimientos
    {
        Negativo = -1,
        Neutro = 0,
        Positivo = 1
    }

    // Los valores static/compartidos para usar en todas las frases o textos analizados mientras está activo el programa.

    private static LanguageServiceClient? client;
    private static readonly List<Frases> colFrases = new();

    /// <summary>
    /// Colección con todas las frases/textos analizados.
    /// </summary>
    /// <remarks>Una frase puede contener más de una sentencia si está separada con un punto.</remarks>
    public static List<Frases> LasFrases { get { return colFrases; } }

    /// <summary>
    /// Añadir un nuevo texto para analizar.
    /// </summary>
    /// <param name="text">El texto o frase a analizar.</param>
    /// <remarks>Se crea un nuevo objeto del tipo Frase y se añade a la colección de textos analizados.</remarks>
    public static Frases Add(string text)
    {
        return new Frases(text);
    }

    private Sentimientos _sentimiento;
    private string _idioma;
    private string _texto;
    private AnnotateTextResponse _response;
    private Token? _Root;

    /// <summary>
    /// El texto completo de la frase o texto analizado.
    /// </summary>
    public string Texto { get { return _texto; } }

    /// <summary>
    /// Constructor privado.
    /// </summary>
    /// <param name="text"></param>
    private Frases(string text)
    {
        _texto = text;

        if (client == null)
        {
            client = LanguageServiceClient.Create();
        }
        var document = Document.FromPlainText(text);

        try
        {
            _response = client.AnnotateText(document,
                            new Features
                            {
                                ExtractSyntax = true,
                                ExtractEntities = true,
                                ExtractDocumentSentiment = true,
                                ExtractEntitySentiment = true,
                                ClassifyText = true
                            });
        }
        catch
        {
            _response = client.AnnotateText(document,
            new Features
            {
                ExtractSyntax = true,
                ExtractEntities = true,
                ExtractDocumentSentiment = true,
                ExtractEntitySentiment = true
            });
        }
        colFrases.Add(this);

        if (_response.DocumentSentiment.Score > 0.0) _sentimiento = Sentimientos.Positivo;
        else if (_response.DocumentSentiment.Score < 0.0) _sentimiento = Sentimientos.Negativo;
        else _sentimiento = Sentimientos.Neutro;
        _idioma = _response.Language;

        _Root = _response.Tokens.FirstOrDefault((t) => t.DependencyEdge.Label == DependencyEdge.Types.Label.Root);
        // Este caso NUNCA debe darse
        if (_Root == null)
        {
            _Root = _response.Tokens[0];
        }

        Analizar();
    }

    /// <summary>
    /// El contenido de esta frase.
    /// </summary>
    public AnnotateTextResponse Response { get { return _response; } }

    // Propiedades para acceder a las propiedades del texto analizado.

    /// <summary>
    /// Las entidades (Entities)
    /// </summary>
    public RepeatedField<Entity> Entities { get { return _response.Entities; } }
    /// <summary>
    /// Los tokens (palabras y signos de puntuación)
    /// </summary>
    public RepeatedField<Token> Tokens { get { return _response.Tokens; } }
    /// <summary>
    /// Las categorías solo funcionan con ClassifyText y solo en inglés.
    /// </summary>
    public RepeatedField<ClassificationCategory> Categories { get { return _response.Categories; } }
    /// <summary>
    /// Las sentencias, habrá más de 1 si están separadas con puntos.
    /// </summary>
    public RepeatedField<Sentence> Sentences { get { return _response.Sentences; } }
    /// <summary>
    /// El sentimiento de la frase o texto analizado.
    /// </summary>
    public Sentiment DocumentSentiment { get { return _response.DocumentSentiment; } }

    /// <summary>
    /// La magnitud absoluta del sentimiento, independientemente de que sea positivo o negativo.
    /// </summary>
    public float SentimientoAbsoluto { get { return _response.DocumentSentiment.Magnitude; } }
    /// <summary>
    /// El valor del sentimiento de la frase o texto.
    /// </summary>
    public float SentimientoScore { get { return _response.DocumentSentiment.Score; } }

    /// <summary>
    /// El tipo de sentimiento de la frase.
    /// </summary>
    public Sentimientos Sentimiento { get { return _sentimiento; } }

    /// <summary>
    /// El idioma detectado del texto a analizar.
    /// </summary>
    public string Language { get { return _idioma; } }

    /// <summary>
    /// El token principal (Root)
    /// </summary>
    public Token? Root { get { return _Root; } }

    private List<Entidad> colEntidades = new();
    public List<Entidad> Entidades { get { return colEntidades; } }

    private List<Relacion> colRelaciones = new();
    /// <summary>
    /// Las relaciones, frases con más importancia, según Entities.
    /// </summary>
    public List<Relacion> Relaciones { get { return colRelaciones; } }

    private List<MiToken> colMisTokens = new();
    /// <summary>
    /// Lista de tokens con el valor del índice en que aparecen en el texto.
    /// </summary>
    public List<MiToken> MisTokens { get { return colMisTokens; } }

    /// <summary>
    /// Analizar la frase y asignar los textos importantes y su relaciones.
    /// </summary>
    private void Analizar()
    {
        AsignarEntidades();
        AsignarTokens();
        // Buscar la relación entre los tokens y crear una colección de relaciones
        // indicando las que están ligadas al token-root
        AnalizarRelaciones();
    }

    /// <summary>
    /// Asignar las entidades que tengan más de una palabra y/o tengan una relevancia superior a 0.
    /// </summary>
    private void AsignarEntidades()
    {
        // revisar las entidades y guardar las que tengan más peso
        // En realidad (según las pruebas vistas) no es de mucha utilidad,
        // es más importante el valor asignado a Root.
        // Pero servirá para saber el Salience/Relevancia de esa parte de la frase
        foreach (var entity in _response.Entities)
        {
            if (entity.Salience > 0.0F || entity.Name.Contains(' '))
            {
                colEntidades.Add(new Entidad(entity));
            }
        }
    }

    /// <summary>
    /// Asignar los tokens sabiendo el índice dentro de la frase.
    /// </summary>
    private void AsignarTokens()
    {
        int index = 0;
        foreach (var sentence in _response.Sentences)
        {
            var content = sentence.Text.Content;
            var sentence_begin = sentence.Text.BeginOffset;
            var sentence_end = sentence_begin + content.Length - 1;
            while (index < _response.Tokens.Count && _response.Tokens[index].Text.BeginOffset <= sentence_end)
            {
                //# This token is in this sentence
                var token = _response.Tokens[index];
                var miToken = new MiToken(index, token);
                colMisTokens.Add(miToken);

                index += 1;
            }
        }
    }

    // Ajustar las relaciones entre los tokens añadidos (no se tiene en cuenta el token principal)
    // de forma que se ordenen según la relación entre estos tokens.

    /// <summary>
    /// Analizar las relaciones desde Root/head y crear trozos de la frase según los enlaces de HeadTokenIndex.
    /// </summary>
    private void AnalizarRelaciones()
    {
        // Buscar todas las que van a root: que DependencyEdge.HeadTokenIndex sea igual al de root

        // Comprobar si es nulo (aunque no es necesario, ya que Root siempre se asigna) pero
        // para que el compilador no de el warning
        int indexRoot = _Root == null ? 0 : _Root.DependencyEdge.HeadTokenIndex;
        var alRoot = colMisTokens.Where(t => t.Token.DependencyEdge.HeadTokenIndex == indexRoot).ToList();
        for (int i = 0; i < alRoot.Count; i++)
        {
            var miToken = alRoot[i];
            // No procesar el root
            // procesarlo igualmente, no afecta demasiado
            //if (miToken.Token.DependencyEdge.Label == DependencyEdge.Types.Label.Root) continue;
            var rel = new Relacion(_response, miToken);
            colRelaciones.Add(rel);
            int index = miToken.Index;

            // Buscar todas las que vengan a este DependencyEdge.HeadTokenIndex

            var aEste = colMisTokens.Where(t => t.Token.DependencyEdge.HeadTokenIndex == index).ToList();
            for (int j = 0; j < aEste.Count; j++)
            {
                var a = aEste[j];
                rel.Add(a);
                // ver las relaciones entre el último y los siguientes que apunten al último
                // Las relaciones son
                // El 8 de <- febrero
                // al <- camino <- de <- santiago
                var aSig = colMisTokens.Where(t => t.Token.DependencyEdge.HeadTokenIndex == a.Index).ToList();
                for (int k = 0; k < aSig.Count; k++)
                {
                    var s = aSig[k];
                    rel.Add(s);
                    var aSig2 = colMisTokens.Where(t => t.Token.DependencyEdge.HeadTokenIndex == s.Index).ToList();
                    for (int m = 0; m < aSig2.Count; m++)
                    {
                        var s2 = aSig2[m];
                        rel.Add(s2);
                        // ¿comprobar otro nivel más?
                        var aSig3 = colMisTokens.Where(t => t.Token.DependencyEdge.HeadTokenIndex == s2.Index).ToList();
                        for (int n = 0; n < aSig3.Count; n++)
                        {
                            rel.Add(aSig3[n]);
                            // ¿comprobar otro más?
                            var aSig4 = colMisTokens.Where(t => t.Token.DependencyEdge.HeadTokenIndex == aSig3[n].Index).ToList();
                            for (int p = 0; p < aSig4.Count; p++) { rel.Add(aSig4[p]); }
                        }
                    }
                }
            }
        }
        // Una vez que están todas las relaciones asignar el texto, relevancia y tipo
        int quitar = -1;
        //foreach (var relacion in colRelaciones)
        for (int i = 0; i < colRelaciones.Count; i++)
        {
            var relacion = colRelaciones[i];
            relacion.EvaluarTexto(restablecer: true);
            // si es la frase completa, quitarla, aunque puede que le falte alguna palabra no enlazada...
            if (relacion.Texto == _texto)
            {
                quitar = i;
                break;
            }
        }
        if (quitar > -1) 
        {
            colRelaciones.RemoveAt(quitar);
        }
        // quitar las que el texto esté vacío
        quitar = -1;
        for (int i = 0; i < colRelaciones.Count; i++)
        {
            var relacion = colRelaciones[i];
            // si es la frase completa, quitarla, aunque puede que le falte alguna palabra no enlazada...
            if (string.IsNullOrEmpty(relacion.Texto))
            {
                quitar = i;
                //break;
            }
        }
        if (quitar > -1)
        {
            colRelaciones.RemoveAt(quitar);
        }
        colRelaciones.Sort();
    }

    /*
El 8 de Febrero voy en bici al Camino de Santiago desde Sarria ¿Crees que aguantaré?
{ Texto: 'al Camino de Santiago'; Relevancia: 43; Tipo: Location; Palabras: 4 }
{ Texto: 'en bici'; Relevancia: 32; Tipo: Other; Palabras: 2 }
{ Texto: 'desde Sarria'; Relevancia: 24; Tipo: Location; Palabras: 2 }
{ Texto: 'El 8 de Febrero'; Relevancia: 0; Tipo: Date; Palabras: 4 }
{ Texto: '¿Crees que aguantaré?'; Relevancia: 0; Tipo: Unknown; Palabras: 5 }

// Sin quitar el que tiene el texto completo y los espacios finales
{ Texto: 'El 8 de Febrero voy en bici al Camino de Santiago desde Sarria ¿Crees que aguantaré?'; Relevancia: 43; Tipo: Date; Palabras: 18 }
{ Texto: 'al Camino de Santiago '; Relevancia: 43; Tipo: Location; Palabras: 4 }
{ Texto: 'en bici '; Relevancia: 32; Tipo: Other; Palabras: 2 }
{ Texto: 'desde Sarria '; Relevancia: 24; Tipo: Location; Palabras: 2 }
{ Texto: 'El 8 de Febrero '; Relevancia: 0; Tipo: Date; Palabras: 4 }
{ Texto: '¿Crees que aguantaré?'; Relevancia: 0; Tipo: Unknown; Palabras: 5 }

// Antes: (sin ordenar por relevancia)
{ Texto: 'El 8 de Febrero', Relevancia: 0, Token: 8, HeadTokenIndex: 4, Tokens: 4 }
{ Texto: 'en bici', Relevancia: 32, Token: en, HeadTokenIndex: 4, Tokens: 2 }
{ Texto: 'al Camino de Santiago', Relevancia: 0, Token: al, HeadTokenIndex: 4, Tokens: 4 }
{ Texto: 'desde Sarria', Relevancia: 24, Token: desde, HeadTokenIndex: 4, Tokens: 2 }
{ Texto: '¿Crees que aguantaré?', Relevancia: 0, Token: Crees, HeadTokenIndex: 4, Tokens: 5 }

el 8 de febrero voy en bici al camino de santiago desde sarria ¿crees que aguantaré?
{ Texto: 'al camino de santiago desde sarria'; Relevancia: 42; Tipo: Location; Palabras: 6 }
{ Texto: 'en bici'; Relevancia: 22; Tipo: Other; Palabras: 2 }
{ Texto: 'el 8 de febrero'; Relevancia: 0; Tipo: Date; Palabras: 4 }
{ Texto: '¿crees que aguantaré?'; Relevancia: 0; Tipo: Unknown; Palabras: 5 }    

// Sin quitar el que tiene el texto completo y los espacios finales
{ Texto: 'el 8 de febrero voy en bici al camino de santiago desde sarria ¿crees que aguantaré?'; Relevancia: 42; Tipo: Date; Palabras: 18 }
{ Texto: 'al camino de santiago desde sarria '; Relevancia: 42; Tipo: Location; Palabras: 6 }
{ Texto: 'en bici '; Relevancia: 22; Tipo: Other; Palabras: 2 }
{ Texto: 'el 8 de febrero '; Relevancia: 0; Tipo: Date; Palabras: 4 }
{ Texto: '¿crees que aguantaré?'; Relevancia: 0; Tipo: Unknown; Palabras: 5 }

// Antes: (sin ordenar por relevancia)
{ Texto: 'el 8 de febrero', Relevancia: 0, Token: 8, HeadTokenIndex: 4, Tokens: 4 }
{ Texto: 'en bici', Relevancia: 22, Token: en, HeadTokenIndex: 4, Tokens: 2 }
{ Texto: 'al camino de santiago desde sarria', Relevancia: 42, Token: al, HeadTokenIndex: 4, Tokens: 6 }
{ Texto: '¿crees que aguantaré?', Relevancia: 0, Token: crees, HeadTokenIndex: 4, Tokens: 5 }
    */

    /// <summary>
    /// Clase que extiende (pero no hereda) la clase Token asignando el índice dentro de la frase.
    /// </summary>
    public class MiToken
    {
        private int _index;
        private Token _token;

        public MiToken(int index, Token token)
        {
            _index = index;
            _token = token;
        }
        public int Index { get { return _index; } }
        public Token Token { get { return _token; } }

        public bool Root { get { return _token.DependencyEdge.Label == DependencyEdge.Types.Label.Root; } }

        public override string ToString()
        {
            return $"{{ Index: {Index}; Texto: '{_token.Text.Content}'; HeadTokenIndex: {_token.DependencyEdge.HeadTokenIndex} }}";
        }
    }

    /// <summary>
    /// Clase para cada grupo de palabras relacionadas con el Root.
    /// </summary>
    /// <remarks>Una vez creada la colección recorrerla y llamar a RestablecerValores.</remarks>
    public class Relacion : IComparable<Relacion>
    {
        private List<MiToken> _misTokens = new();
        private string _texto = "";
        private MiToken _miToken;
        private AnnotateTextResponse _response;
        private Entity.Types.Type _tipo = Entity.Types.Type.Unknown;

        public Relacion(AnnotateTextResponse response, MiToken miToken)
        {
            _response = response;
            _miToken = miToken;
            Relevancia = 0;
            _misTokens.Add(miToken);
        }

        public string Texto { get { return _texto; } }
        public MiToken MiToken { get { return _miToken; } }
        public Token Token { get { return _miToken.Token; } }
        public int Relevancia { get; private set; }

        public List<MiToken> Tokens { get { return _misTokens; } }

        public AnnotateTextResponse Response { get { return _response; } }

        // El tipo de dato de estas palabras, fecha location, etc.
        //public bool Root { get { return _token.DependencyEdge.Label == DependencyEdge.Types.Label.Root } }
        public Entity.Types.Type Tipo { get { return _tipo; } }

        public void Add(MiToken miToken)
        {
            // no añadirlo si ya está en la colección
            var yaEsta = _misTokens.Count(x => x.Index == miToken.Index);
            //var yaEsta = _misTokens.Where(x => x.Index == miToken.Index).Count();
            if (yaEsta > 0) return;

            _misTokens.Add(miToken);

            // Esto hacerlo para que se asignen los valores correctamente
            EvaluarTexto(false);
            // Con esto solo no va
            //AsignarTexto();
        }

        /// <summary>
        /// Ordenar los tokens por índice, asignar el texto, asignar la relevancia y el tipo de entidad.
        /// </summary>
        /// <param name="restablecer">True para restablecer el valor de Tipo y Relevancia antes de evaluar.</param>
        public void EvaluarTexto(bool restablecer)
        {
            if (restablecer)
            {
                _tipo = Entity.Types.Type.Unknown;
                Relevancia = 0;
            }
            AsignarTexto();
            AsignarRelevancia();
        }

        /// <summary>
        /// Ordena los tokens por el índice y asigna el texto.
        /// </summary>
        private void AsignarTexto()
        {
            var ordenados = _misTokens.OrderBy(t => t.Index).ToList();
            StringBuilder _sb = new();
            for (int i = 0; i < ordenados.Count; i++)
            {
                var elToken = ordenados[i];

                // Si empieza o termina por signo de puntuación, de tipo separador, no añadirlo
                if (i == 0 || i == ordenados.Count - 1)
                {
                    if (elToken.Token.DependencyEdge.Label == DependencyEdge.Types.Label.P &&
                        "¿?¡!()[]{}".Contains(elToken.Token.Text.Content, StringComparison.CurrentCulture) == false) 
                    {
                        continue;
                    }
                }
                _sb.Append(elToken.Token.Text.Content);

                // añadir espacio si no es un signo de puntuación y el siguiente tampoco es un signo de puntuación
                if (elToken.Token.DependencyEdge.Label != DependencyEdge.Types.Label.P)
                {
                    if (i < ordenados.Count - 1)
                    {
                        if (ordenados[i + 1].Token.DependencyEdge.Label == DependencyEdge.Types.Label.P)
                        {
                            // no añadir el espacio si empieza por uno de estos signos
                            if ("¿¡([{".IndexOf(ordenados[i + 1].Token.Text.Content) == -1)
                                continue;
                        }
                    }
                    _sb.Append(' ');
                }

                // añadir espacio si es un signo de puntuación y el siguiente no es un signo de puntuación
                // salvo si son signos que no se deben separar
                if ("¿?¡!()[]{}".IndexOf(elToken.Token.Text.Content) == -1)
                {
                    if (elToken.Token.DependencyEdge.Label == DependencyEdge.Types.Label.P)
                    {
                        if (i < ordenados.Count - 1)
                        {
                            if (ordenados[i + 1].Token.DependencyEdge.Label == DependencyEdge.Types.Label.P)
                            {
                                continue;
                            }
                        }
                        _sb.Append(' ');
                    }
                }
            }
            _texto = _sb.ToString().TrimEnd();
        }

        /// <summary>
        /// Asigna la relevancia con valor mayor y el tipo de entidad.
        /// </summary>
        private void AsignarRelevancia()
        {
            Entity.Types.Type nuevoTipo;
            int nuevaRelevancia;
            // Buscar en los entities y asignar el Salience/Relevancia
            var entidades = _response.Entities.Where(e => Texto.Contains(e.Name, StringComparison.OrdinalIgnoreCase));
            foreach (var enti in entidades)
            {
                nuevaRelevancia = (int)(enti.Salience * 100);
                if (nuevaRelevancia > Relevancia) { Relevancia = nuevaRelevancia; }

                nuevoTipo = enti.Type;

                // Asignar el tipo de entidad, aquí hay que saber qué valor preferimos asignar
                // si el tipo es Unknown, no asignarlo, en caso de que en realidad sea Unknown, ese es el valor predeterminado
                if (nuevoTipo != Entity.Types.Type.Unknown)
                {
                    // asignarlo si aún no está asignado o no es Other ni Number
                    if (_tipo == Entity.Types.Type.Unknown || nuevoTipo != Entity.Types.Type.Other && nuevoTipo != Entity.Types.Type.Number)
                    {
                        _tipo = nuevoTipo;
                    }
                }
            }
        }

        public override string ToString()
        {
            //return $"{{ Texto: '{Texto}'; Relevancia: {Relevancia}; Tipo: {Tipo}; Token: {Token.Text.Content}; HeadTokenIndex: {Token.DependencyEdge.HeadTokenIndex}; Tokens: {_misTokens.Count} }}";
            //StringBuilder sb = new();
            //foreach (var mt in _misTokens)
            //{
            //    sb.Append(mt.Token.Text.Content);
            //    sb.Append(", ");
            //}
            //return $"{{ Texto: '{Texto}'; Relevancia: {Relevancia}; Tipo: {Tipo}; Palabras: {_misTokens.Count} = {sb.ToString().TrimEnd(", ".ToCharArray())} }}";
            return $"{{ Texto: '{Texto}'; Relevancia: {Relevancia}; Tipo: {Tipo}; Palabras: {_misTokens.Count} }}";
        }

        public int CompareTo(Relacion? other)
        {
            if (other == null) return 1;
            // Para ordenar de menor a mayor
            //return Relevancia.CompareTo(other.Relevancia);
            // Para que sea de mayor a menor
            return other.Relevancia.CompareTo(Relevancia);
        }
    }

    /// <summary>
    /// Clase para simplificar el tipo Entity.
    /// </summary>
    public class Entidad
    {
        public Entidad(Entity entity)
        {
            Relevancia = (int)(entity.Salience * 100);
            Nombre = entity.Name;
            Tipo = entity.Type;
        }
        public string Nombre { get; init; }
        public Entity.Types.Type Tipo { get; init; }
        public int Relevancia { get; init; }

        public bool EsLocation { get { return Tipo == Entity.Types.Type.Location; } }
        public bool EsFecha { get { return Tipo == Entity.Types.Type.Date; } }

        public override string ToString()
        {
            return $"{{ Nombre: {Nombre}; Tipo: {Tipo}, Relevancia: {Relevancia} }}";
        }
    }
}
