# gcnl AnalizarFrases

Analizar textos usando Google Cloud Natural Language

Código para aplicación de consola y para dispositivos usando .NET MAUI. <br>
Usando la clase Frases para analizar el texto indicado.<br>

<br>

Existen tres proyectos:<br>
Una librería de clases para .NET 7.0 con la definición de la clase Frases.<br>
Una aplicación de consola que usa una referencia al proyecto donde está definida la clase Frases.<br>
Una aplicación para .NET MAUI donde se usa directamente la clase Frases (no funciona con una referencia al proyecto donde está definida Frases).

<br>

> **Nota:** <br>
> En el proyecto para .NET MAUI añado un enlace a la clase _Frases_,<br>
> ya que para poder añadir una referencia a la DLL que contiene esa clase debe estar creado el proyecto como Class Library para MAUI
> y ese tipo de proyecto no sirve para la aplicación de consola (o no me ha funcionado).

<br>

> **Nota sobre key.json:** <br>
> Se necesita el fichero key.json (no incluido por razones evidentes de seguridad)<br>
> En el proyecto de consola, ese fichero debe estar en el mismo directorio que el fichero Program.cs indicando que siempre se copie en el directorio de salida.<br>
> En el proyecto para .NET MAUI, ese fichero debe estar en el directorio Resorurces > Raw y no hace falta indicar nada en ese fichero ya que el contenido de ese directorio siempre se copia.<br>
> Lee el fichero [key.json.readme.txt](https://github.com/elGuille-info/gcnl_AnalizarFrases/blob/master/gcnl%20AnalizarFrases%20MAUI/Resources/Raw/key.json.readme.txt) donde indico lo que se hace para que esté accesible en todos los dispositivos.<br>
> <br>
> [Aquí te explico cómo crear el fichero **key.json**](https://github.com/elGuille-info/CloudNaturalLanguage) para que puedas usar las API de Google Cloud Natural Language.<br>

<br>

## Post en elguillemola.com:

[Ejemplos de Google Cloud Natural Language para consola y .NET MAUI](https://www.elguillemola.com/ejemplos-de-google-cloud-natural-language-para-consola-y-net-maui/)

<br>

## Capturas

![La versión de Windows usando el expander simulado](https://www.elguillemola.com/img/img2023/analizarFrases_windows_expander.png)

<br>
