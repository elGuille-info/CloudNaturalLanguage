//-----------------------------------------------------------------------------
// Ejemplo de Google Cloud Natural Language en C#                   (29/ene/23)
//
// (c)Guillermo Som (Guille), 2023
//-----------------------------------------------------------------------------

using System;
using System.Text;

using Google.Cloud.Language.V1;
using Google.Protobuf.Collections;
//using Google.Protobuf.Collections;
using static Google.Cloud.Language.V1.AnnotateTextRequest.Types;

namespace NaturalLanguageApiDemo
{
    class Program
    {
        static LanguageServiceClient? client;
        static void Main(string[] args)
        {
            //var text = "Yukihiro Matsumoto is great!";
            var text = "'Lawrence of Arabia' is a highly rated film biography about British Lieutenant T.E.Lawrence.Peter O'Toole plays Lawrence in the film.";
            Console.WriteLine("Ejemplos de Google.Cloud.Language");
            Console.WriteLine();
            Console.WriteLine("Pruebas de Google Cloud Natural Language");
            Console.WriteLine();
            Console.WriteLine("  Creando el cliente...");
            client = LanguageServiceClient.Create();
            Console.WriteLine();
            
            bool repitiendo = false;
            do
            {
                if (repitiendo)
                {
                    Console.WriteLine($"Última: '{text}'");
                    Console.WriteLine("Indica la frase que quieres analizar (0 salir, [la última])");
                }
                else
                {
                    Console.WriteLine($"Predeterminada: '{text}'");
                    Console.WriteLine("Indica la frase que quieres analizar (0 salir, [predeterminada])");
                }
                Console.Write("> ");
                var resText = Console.ReadLine();
                if (!string.IsNullOrEmpty(resText))
                {
                    if (resText == "0")
                    {
                        break;
                    }
                    text = resText;
                }
                do
                {
                    Console.WriteLine($"Analizar: '{text}'");
                    Console.Write("1- Todo con tokens, 2- Todo sin tokens, 3- Solo tokens, 0- nueva frase [2] ? ");
                    resText = Console.ReadLine();
                    Console.WriteLine();
                    if (string.IsNullOrEmpty(resText))
                    {
                        resText = "2";
                    }
                    if (resText == "1")
                    {
                        Analizar(text, conTokens: true);
                    }
                    else if (resText == "2")
                    {
                        Analizar(text, conTokens: false);
                    }
                    else if (resText == "3")
                    {
                        AnalizarTokens(text);
                    }
                    else if (resText == "0")
                    {
                        break;
                    }
                    Console.WriteLine();
                } while (true);

                repitiendo= true;
            } while (true);
        }

        static void Analizar (string text, bool conTokens)
        {
            if (client == null)
            {
                client = LanguageServiceClient.Create();
            }
            var document = Document.FromPlainText(text);
            AnnotateTextResponse response;
            //No se puede usar en español: ClassifyText = true });
            // ni tampoco si hay pocas cosas que "clasificar"
            try
            {
                response = client.AnnotateText(document,
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
                response = client.AnnotateText(document,
                new Features
                {
                    ExtractSyntax = true,
                    ExtractEntities = true,
                    ExtractDocumentSentiment = true,
                    ExtractEntitySentiment = true
                });
            }
            
            var sentiment = response.DocumentSentiment;

            Console.WriteLine($"Detected language: {response.Language}");
            Console.WriteLine($"Sentiment Score: {sentiment.Score}, Magnitude: {sentiment.Magnitude}");
            Console.WriteLine("***Entities:");
            Entity? entity1 = null;
            foreach (var entity in response.Entities)
            {
                // algunos entities están repetidos y seguidos
                if (entity1 == null)
                {
                    entity1 = entity;
                }
                else
                {
                    if (entity == entity1) continue;
                }
                Console.WriteLine($"Entity: '{entity.Name}'");
                Console.WriteLine($"  Type: {entity.Type},  Salience: {(int)(entity.Salience * 100)}%");
                if (entity.Mentions.Count > 0)
                {
                    Console.WriteLine($"  Mentions: {entity.Mentions.Count}");
                    foreach (var mention in entity.Mentions)
                    {
                        Console.Write($"    Text: '{mention.Text.Content}' (beginOffset: {mention.Text.BeginOffset}),");
                        Console.WriteLine($" Type: {mention.Type}, Sentiment: {mention.Sentiment}");
                    }
                }
                if (entity.Metadata.Count> 0)
                {
                    Console.WriteLine($"  Metadata: {entity.Metadata}");
                    if (entity.Metadata.ContainsKey("wikipedia_url"))
                    {
                        Console.WriteLine($"    URL: {entity.Metadata["wikipedia_url"]}");
                    }
                }
            }
            // Las categorías solo funcionan con ClassifyText y solo en inglés
            Console.WriteLine("***Categories:");
            foreach (var cat in response.Categories)
            {
                Console.WriteLine($"Category: '{cat.Name}' (Confidence: {cat.Confidence})");
            }
            Console.WriteLine("***Sentences:");
            foreach (var sentence in response.Sentences)
            {
                Console.WriteLine($" Sentence.Text.Content: '{sentence.Text.Content}'");
                Console.WriteLine($"   Sentence.Text.BeginOffset: {sentence.Text.BeginOffset}");
                Console.WriteLine($" Sentence.Sentiment .Magnitude: {sentence.Sentiment.Magnitude}, .Score: {sentence.Sentiment.Score}");
            }
            if (conTokens)
            {
                Console.WriteLine("***Tokens:");
                for (int i = 0; i< response.Tokens.Count; i++)
                {
                    MostrarToken(i, response.Tokens, conContenido: false);
                }
            }
        }

        static void AnalizarTokens(string text)
        {
            if (client == null)
            {
                client = LanguageServiceClient.Create();
            }
            var document = Document.FromPlainText(text);
            AnnotateTextResponse response;
            //No se puede usar en español: ClassifyText = true });
            // ni tampoco si hay pocas cosas que "clasificar"
            try
            {
                response = client.AnnotateText(document,
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
                response = client.AnnotateText(document,
                new Features
                {
                    ExtractSyntax = true,
                    ExtractEntities = true,
                    ExtractDocumentSentiment = true,
                    ExtractEntitySentiment = true
                });
            }
            AnalizarSentecias(response);
        }

        /*
        index = 0
          for sentence in self.sentences:
            content  = sentence['text']['content']
            sentence_begin = sentence['text']['beginOffset']
            sentence_end = sentence_begin + len(content) - 1
            while index < len(self.tokens) and self.tokens[index]['text']['beginOffset'] <= sentence_end:
              # This token is in this sentence
              index += 1
        */
        static void AnalizarSentecias(AnnotateTextResponse self)
        {
            int index = 0;
            foreach (var sentence in self.Sentences)
            {
                var content = sentence.Text.Content;
                var sentence_begin = sentence.Text.BeginOffset;
                var sentence_end = sentence_begin + content.Length - 1;
                while (index < self.Tokens.Count && self.Tokens[index].Text.BeginOffset <= sentence_end)
                {
                    //# This token is in this sentence
                    MostrarToken(index, self.Tokens, conContenido: true);

                    index += 1;
                }
            }
        }

        private static void MostrarToken(int nToken, RepeatedField<Token> tokens, bool conContenido = true)
        {
            Token token = tokens[nToken];
            Console.WriteLine($"{nToken}- Token: Text.Content: '{token.Text.Content}', Lemma: '{token.Lemma}'");
            if (token.DependencyEdge.Label == DependencyEdge.Types.Label.Root)
            {
                Console.Write($"  **DependencyEdge Label: {token.DependencyEdge.Label}");
                if (token.DependencyEdge.HeadTokenIndex != nToken)
                {
                    Console.Write($", HeadTokenIndex: {token.DependencyEdge.HeadTokenIndex}");
                }
                Console.WriteLine("**");
            }
            else
            {
                Console.Write($"  DependencyEdge Label: {token.DependencyEdge.Label}, HeadTokenIndex: {token.DependencyEdge.HeadTokenIndex}");
                var tokenDependency = tokens[token.DependencyEdge.HeadTokenIndex];
                Console.WriteLine($" ('{tokenDependency.Text.Content}')");
            }
            if (conContenido)
            {
                Console.WriteLine($"  PartOfSpeech:");
                Console.Write($"    Tag: {token.PartOfSpeech.Tag},");
                var sb = new StringBuilder();
                // Si tiene Aspect, puede tener Case y Form
                if (token.PartOfSpeech.Aspect != PartOfSpeech.Types.Aspect.Unknown)
                {
                    sb.Append($" (Aspect: {token.PartOfSpeech.Aspect},");
                    if (token.PartOfSpeech.Case != PartOfSpeech.Types.Case.Unknown)
                    {
                        sb.Append($" Case: {token.PartOfSpeech.Case},");
                    }
                    if (token.PartOfSpeech.Form != PartOfSpeech.Types.Form.Unknown)
                    {
                        sb.Append($" Form: {token.PartOfSpeech.Form},");
                    }
                    if (sb.ToString().EndsWith(','))
                    {
                        sb.Length -= 1;
                    }
                    sb.Append("),");
                }

                // Si tiene Gender, puede tener Mood y Number
                if (token.PartOfSpeech.Gender != PartOfSpeech.Types.Gender.Unknown)
                {
                    sb.Append($" (Gender: {token.PartOfSpeech.Gender},");
                    if (token.PartOfSpeech.Mood != PartOfSpeech.Types.Mood.Unknown)
                    {
                        sb.Append($" Mood: {token.PartOfSpeech.Mood},");
                    }
                    if (token.PartOfSpeech.Number != PartOfSpeech.Types.Number.Unknown)
                    {
                        sb.Append($" Number: {token.PartOfSpeech.Number},");
                    }
                    if (sb.ToString().EndsWith(','))
                    {
                        sb.Length -= 1;
                    }
                    sb.Append("),");
                }

                if (token.PartOfSpeech.Proper != PartOfSpeech.Types.Proper.Unknown)
                {
                    sb.Append($" Proper: {token.PartOfSpeech.Proper}");
                }

                if (sb.ToString().Trim().Length > 0)
                {
                    Console.WriteLine(sb.ToString().TrimEnd(','));
                }
                sb.Clear();
                sb.Append("   ");
                if (token.PartOfSpeech.Person != PartOfSpeech.Types.Person.Unknown)
                {
                    sb.Append($" Person: {token.PartOfSpeech.Person},");
                }
                if (token.PartOfSpeech.Reciprocity != PartOfSpeech.Types.Reciprocity.Unknown)
                {
                    sb.Append($" Reciprocity: {token.PartOfSpeech.Reciprocity},");
                }
                if (token.PartOfSpeech.Tense != PartOfSpeech.Types.Tense.Unknown)
                {
                    sb.Append($" Tense: {token.PartOfSpeech.Tense},");
                }
                if (token.PartOfSpeech.Voice != PartOfSpeech.Types.Voice.Unknown)
                {
                    sb.Append($" Voice: {token.PartOfSpeech.Voice}");
                }
                if (sb.ToString().Trim().Length > 0)
                {
                    Console.WriteLine(sb.ToString());
                }
            }
            else
            {
                Console.WriteLine($"  PartOfSpeech Aspect: {token.PartOfSpeech.Aspect}, Case: {token.PartOfSpeech.Case}, Form: {token.PartOfSpeech.Form}");
                Console.WriteLine($"  PartOfSpeech Gender: {token.PartOfSpeech.Gender}, Mood: {token.PartOfSpeech.Mood}, Number: {token.PartOfSpeech.Number}");
                Console.WriteLine($"  PartOfSpeech Person: {token.PartOfSpeech.Person}, Proper: {token.PartOfSpeech.Proper}");
                Console.WriteLine($"  PartOfSpeech Reciprocity: {token.PartOfSpeech.Reciprocity}, Tag: {token.PartOfSpeech.Tag}");
                Console.WriteLine($"  PartOfSpeech Tense:: {token.PartOfSpeech.Tense}, Voice: {token.PartOfSpeech.Voice}");
            }
        }
    }
}