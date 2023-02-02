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

