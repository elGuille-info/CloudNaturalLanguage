# Ejemplos para .NET de Google Cloud Natural Language

Ejemplos en Visual Basic y C# para usar las API de Google Cloud Natural Language y analizar un texto.

El ejemplo está basado en el tutorial [Using the Natural Language API with C#](https://codelabs.developers.google.com/codelabs/cloud-natural-language-csharp).

El código original lo he escrito en C# usando y modificando el código de ese tutorial, y la conversión a Visual Basic la he hecho (con algunos arreglos) usando [Telerik Code Converter](https://converter.telerik.com/).

<br>

> **Nota:** <br>
> El fichero key.json no se incluye en este repositorio.<br>
> Debes crearlo usando los pasos indicados en el tutorial de Google Cloud.<br>
> **Sin ese fichero no podrás ejecutar el proyecto**.<br>

<br>

Para crear un proyecto nuevo (si ya tienes el fichero **key.json**)<br>
```
0- Abre una ventana de consola (o terminal)
1- Posicionarse en la carpeta donde crear el proyecto
2- Crear el proyecto
	dotnet new console -n <nombre-proyecto>
	dotnet new console -lang VB -n <nombre-proyecto>
3- Cambiar al directorio del proyecto
	cd <nombre-proyecto>
4- Añadir el paquete de Google Cloud Natural Language API
	dotnet add package Google.Cloud.Language.V1
5- Copiar el fichero key.json con las claves y permisos
	Ese fichero se debe copiar en el directorio del ejecutable.
5.2- Ver estos pasos para crearla:
	https://codelabs.developers.google.com/codelabs/cloud-natural-language-csharp#3
5.3- En IAM, añadir la cuenta (indicada en el proyecto de Google Cloud) en +OTORGAR ACCESO
	Solo estará la principal y/o las otras añadidas
5.4- Si se ha usado otra cuenta, estará en IAM>Cuentas de Servicio
	https://console.cloud.google.com/iam-admin/serviceaccounts (tendrás que elegir el proyecto que has creado para este caso)
6- Modificar Program.cs (o Programa.vb) para usar el código que accede a la API de Natural Language
7- Asegurase que está creada la variable de entorno GOOGLE_APPLICATION_CREDENTIALS apuntando al fichero key.json
	set GOOGLE_APPLICATION_CREDENTIALS=key.json
8- Ejecutar el código
	dotnet run

```
