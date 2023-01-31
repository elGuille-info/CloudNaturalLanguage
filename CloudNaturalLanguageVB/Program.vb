'--------------------------------------------------------------------------------
' Ejemplo de Google Cloud Natural Language en Visual Basic .NET (31/ene/23 18.50)
'
' (c)Guillermo Som (Guille), 2023
'--------------------------------------------------------------------------------

Imports System
Imports System.Text
Imports gcl = Google.Cloud.Language.V1
Imports Google.Protobuf.Collections
Imports Google.Cloud.Language.V1.AnnotateTextRequest.Types

'Namespace CloudNaturalLanguageVB
Class Program
    Shared client As gcl.LanguageServiceClient '?

    Shared Sub Main(args As String())
        'Dim text = "El 8 de Febrero voy en bici al Camino de Santiago desde Sarria ¿crees que aguantaré?"
        Dim text = "Probando Google Cloud Natural Language con VB.NET ¿Funcionará esto?"

        Console.WriteLine("Ejemplos de Google.Cloud.Language")
        Console.WriteLine()
        Console.WriteLine("Pruebas de Google Cloud Natural Language en Visual Basic .NET")
        Console.WriteLine()
        Console.WriteLine("  Creando el cliente...")
        client = gcl.LanguageServiceClient.Create()
        Console.WriteLine()

        Dim repitiendo As Boolean = False

        Do

            If repitiendo Then
                Console.WriteLine($"Última: '{text}'")
                Console.WriteLine("Indica la frase que quieres analizar (0 salir, [la última])")
            Else
                Console.WriteLine($"Predeterminada: '{text}'")
                Console.WriteLine("Indica la frase que quieres analizar (0 salir, [predeterminada])")
            End If

            Console.Write("> ")
            Dim resText = Console.ReadLine()

            If Not String.IsNullOrEmpty(resText) Then

                If resText = "0" Then
                    Exit Do
                End If

                text = resText
            End If

            Do
                Console.WriteLine($"Analizar: '{text}'")
                Console.Write("1- Todo con tokens, 2- Todo sin tokens, 3- Solo tokens, 0- nueva frase [2] ? ")
                resText = Console.ReadLine()
                Console.WriteLine()

                If String.IsNullOrEmpty(resText) Then
                    resText = "2"
                End If

                If resText = "1" Then
                    Analizar(text, conTokens:=True)
                ElseIf resText = "2" Then
                    Analizar(text, conTokens:=False)
                ElseIf resText = "3" Then
                    AnalizarTokens(text)
                ElseIf resText = "0" Then
                    Exit Do
                End If

                Console.WriteLine()
            Loop While True

            repitiendo = True
        Loop While True
    End Sub

    Private Shared Sub Analizar(text As String, conTokens As Boolean)
        If client Is Nothing Then
            client = gcl.LanguageServiceClient.Create()
        End If

        Dim document = gcl.Document.FromPlainText(text)
        Dim response As gcl.AnnotateTextResponse

        Try
            response = client.AnnotateText(document, New Features With {
                    .ExtractSyntax = True,
                    .ExtractEntities = True,
                    .ExtractDocumentSentiment = True,
                    .ExtractEntitySentiment = True,
                    .ClassifyText = True
                })
        Catch
            response = client.AnnotateText(document, New Features With {
                    .ExtractSyntax = True,
                    .ExtractEntities = True,
                    .ExtractDocumentSentiment = True,
                    .ExtractEntitySentiment = True
                })
        End Try

        Dim sentiment = response.DocumentSentiment
        Console.WriteLine($"Detected language: {response.Language}")
        Console.WriteLine($"Sentiment Score: {sentiment.Score}, Magnitude: {sentiment.Magnitude}")
        Console.WriteLine("***Entities:")
        Dim entity1 As gcl.Entity = Nothing

        For Each entity0 In response.Entities

            If entity1 Is Nothing Then
                entity1 = entity0
            Else
                If entity0.Equals(entity1) Then Continue For
            End If

            Console.WriteLine($"Entity: '{entity0.Name}'")
            Console.WriteLine($"  Type: {entity0.Type},  Salience: {CInt((entity0.Salience * 100))}%")

            If entity0.Mentions.Count > 0 Then
                Console.WriteLine($"  Mentions: {entity0.Mentions.Count}")

                For Each mention In entity0.Mentions
                    Console.Write($"    Text: '{mention.Text.Content}' (beginOffset: {mention.Text.BeginOffset}),")
                    Console.WriteLine($" Type: {mention.Type}, Sentiment: {mention.Sentiment}")
                Next
            End If

            If entity0.Metadata.Count > 0 Then
                Console.WriteLine($"  Metadata: {entity0.Metadata}")

                If entity0.Metadata.ContainsKey("wikipedia_url") Then
                    Console.WriteLine($"    URL: {entity0.Metadata("wikipedia_url")}")
                End If
            End If
        Next

        Console.WriteLine("***Categories:")

        For Each cat In response.Categories
            Console.WriteLine($"Category: '{cat.Name}' (Confidence: {cat.Confidence})")
        Next

        Console.WriteLine("***Sentences:")

        For Each sentence In response.Sentences
            Console.WriteLine($" Sentence.Text.Content: '{sentence.Text.Content}'")
            Console.WriteLine($"   Sentence.Text.BeginOffset: {sentence.Text.BeginOffset}")
            Console.WriteLine($" Sentence.Sentiment .Magnitude: {sentence.Sentiment.Magnitude}, .Score: {sentence.Sentiment.Score}")
        Next

        If conTokens Then
            Console.WriteLine("***Tokens:")

            For i As Integer = 0 To response.Tokens.Count - 1
                MostrarToken(i, response.Tokens, conContenido:=False)
            Next
        End If
    End Sub

    Private Shared Sub AnalizarTokens(text As String)
        If client Is Nothing Then
            client = gcl.LanguageServiceClient.Create()
        End If

        Dim document = gcl.Document.FromPlainText(text)
        Dim response As gcl.AnnotateTextResponse

        Try
            response = client.AnnotateText(document, New Features With {
                    .ExtractSyntax = True,
                    .ExtractEntities = True,
                    .ExtractDocumentSentiment = True,
                    .ExtractEntitySentiment = True,
                    .ClassifyText = True
                })
        Catch
            response = client.AnnotateText(document, New Features With {
                    .ExtractSyntax = True,
                    .ExtractEntities = True,
                    .ExtractDocumentSentiment = True,
                    .ExtractEntitySentiment = True
                })
        End Try

        AnalizarSentecias(response)
    End Sub

    Private Shared Sub AnalizarSentecias(self As gcl.AnnotateTextResponse)
        Dim index As Integer = 0

        For Each sentence In self.Sentences
            Dim content = sentence.Text.Content
            Dim sentence_begin = sentence.Text.BeginOffset
            Dim sentence_end = sentence_begin + content.Length - 1

            While index < self.Tokens.Count AndAlso self.Tokens(index).Text.BeginOffset <= sentence_end
                MostrarToken(index, self.Tokens, conContenido:=True)
                index += 1
            End While
        Next
    End Sub

    Private Shared Sub MostrarToken(nToken As Integer, tokens As RepeatedField(Of gcl.Token), Optional conContenido As Boolean = True)
        Dim token As gcl.Token = tokens(nToken)
        Console.WriteLine($"{nToken}- Token: Text.Content: '{token.Text.Content}', Lemma: '{token.Lemma}'")

        If token.DependencyEdge.Label = gcl.DependencyEdge.Types.Label.Root Then
            Console.Write($"  **DependencyEdge Label: {token.DependencyEdge.Label}")

            If token.DependencyEdge.HeadTokenIndex <> nToken Then
                Console.Write($", HeadTokenIndex: {token.DependencyEdge.HeadTokenIndex}")
            End If

            Console.WriteLine("**")
        Else
            Console.Write($"  DependencyEdge Label: {token.DependencyEdge.Label}, HeadTokenIndex: {token.DependencyEdge.HeadTokenIndex}")
            Dim tokenDependency = tokens(token.DependencyEdge.HeadTokenIndex)
            Console.WriteLine($" ('{tokenDependency.Text.Content}')")
        End If

        If conContenido Then
            Console.WriteLine($"  PartOfSpeech:")
            Console.Write($"    Tag: {token.PartOfSpeech.Tag},")
            Dim sb = New StringBuilder()

            If token.PartOfSpeech.Aspect <> gcl.PartOfSpeech.Types.Aspect.Unknown Then
                sb.Append($" (Aspect: {token.PartOfSpeech.Aspect},")

                If token.PartOfSpeech.[Case] <> gcl.PartOfSpeech.Types.[Case].Unknown Then
                    sb.Append($" Case: {token.PartOfSpeech.[Case]},")
                End If

                If token.PartOfSpeech.Form <> gcl.PartOfSpeech.Types.Form.Unknown Then
                    sb.Append($" Form: {token.PartOfSpeech.Form},")
                End If

                If sb.ToString().EndsWith(","c) Then
                    sb.Length -= 1
                End If

                sb.Append("),")
            End If

            If token.PartOfSpeech.Gender <> gcl.PartOfSpeech.Types.Gender.Unknown Then
                sb.Append($" (Gender: {token.PartOfSpeech.Gender},")

                If token.PartOfSpeech.Mood <> gcl.PartOfSpeech.Types.Mood.Unknown Then
                    sb.Append($" Mood: {token.PartOfSpeech.Mood},")
                End If

                If token.PartOfSpeech.Number <> gcl.PartOfSpeech.Types.Number.Unknown Then
                    sb.Append($" Number: {token.PartOfSpeech.Number},")
                End If

                If sb.ToString().EndsWith(","c) Then
                    sb.Length -= 1
                End If

                sb.Append("),")
            End If

            If token.PartOfSpeech.Proper <> gcl.PartOfSpeech.Types.Proper.Unknown Then
                sb.Append($" Proper: {token.PartOfSpeech.Proper}")
            End If

            If sb.ToString().Trim().Length > 0 Then
                Console.WriteLine(sb.ToString().TrimEnd(","c))
            End If

            sb.Clear()
            sb.Append("   ")

            If token.PartOfSpeech.Person <> gcl.PartOfSpeech.Types.Person.Unknown Then
                sb.Append($" Person: {token.PartOfSpeech.Person},")
            End If

            If token.PartOfSpeech.Reciprocity <> gcl.PartOfSpeech.Types.Reciprocity.Unknown Then
                sb.Append($" Reciprocity: {token.PartOfSpeech.Reciprocity},")
            End If

            If token.PartOfSpeech.Tense <> gcl.PartOfSpeech.Types.Tense.Unknown Then
                sb.Append($" Tense: {token.PartOfSpeech.Tense},")
            End If

            If token.PartOfSpeech.Voice <> gcl.PartOfSpeech.Types.Voice.Unknown Then
                sb.Append($" Voice: {token.PartOfSpeech.Voice}")
            End If

            If sb.ToString().Trim().Length > 0 Then
                Console.WriteLine(sb.ToString())
            End If
        Else
            Console.WriteLine($"  PartOfSpeech Aspect: {token.PartOfSpeech.Aspect}, Case: {token.PartOfSpeech.[Case]}, Form: {token.PartOfSpeech.Form}")
            Console.WriteLine($"  PartOfSpeech Gender: {token.PartOfSpeech.Gender}, Mood: {token.PartOfSpeech.Mood}, Number: {token.PartOfSpeech.Number}")
            Console.WriteLine($"  PartOfSpeech Person: {token.PartOfSpeech.Person}, Proper: {token.PartOfSpeech.Proper}")
            Console.WriteLine($"  PartOfSpeech Reciprocity: {token.PartOfSpeech.Reciprocity}, Tag: {token.PartOfSpeech.Tag}")
            Console.WriteLine($"  PartOfSpeech Tense:: {token.PartOfSpeech.Tense}, Voice: {token.PartOfSpeech.Voice}")
        End If
    End Sub
End Class
'End Namespace

