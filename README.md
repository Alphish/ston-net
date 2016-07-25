# STON .NET

A .NET implementation of [Specifically Typed Object Notation](https://github.com/Alphish/ston). Currently available as 0.3.0 Alpha [on NuGet](https://www.nuget.org/packages/Alphicsh.Ston/).

To install it, you can look for **Alphicsh.Ston** on nuget.org feed (you must check the "Include prerelease" option) or type the following command in Package Manager console:

    PM> Install-Package Alphicsh.Ston -Pre

The implementation includes, among other things:

 - `IStonEntity` interface for representing an abstract STON entity (along with related STON element interfaces), and `StonEntity` basic implementation
 - `IStonDocument` interface exposing a STON document functionality (especially references resolving), and `StonDocument` basic implementation
 - `IStonReader` interface exposing a functionality of reading STON from text, and `RegularStonReader` implementation that reads entities and documents from regular STON text
 - `IStonWriter` interface exposing a functionality of writing STON to text (STON or not), and `CanonicalStonWriter` implementation that writes entities and documents to canonical form

More detailed documentation describing specific classes and interfaces available will follow soon.

#Example
 
Example program that reads a STON document and writes it to a canonical form.

```C#
    static void Main(string[] args)
    {
        string inputPath = args[0];
        string outputPath = args[1];

        try
        {
            // loads a STON document from a file
            // internally, it's the same as using the reader's ReadDocument method
            // with file-related StreamReader as TextReader argument

            // additional arguments are provided to allow all extension types and members
            // by default, no extension types and members are allowed
            var document = RegularStonReader.Default.LoadDocument(inputPath, null, null, s => true, s => true);

            // saves a document canonical form to a file
            // internally, it's the same as using the writer's WriteDocument method
            // with file-related StreamWriter as TextWriter argument

            // it can be shortened even further to document.SaveCanonicalForm(outputPath)
            document.Save(outputPath, CanonicalStonWriter.Instance);
            Console.WriteLine("The document has been written successfully.");
        }
        catch (StonParsingException ex)
        {
            Console.WriteLine("Couldn't parse the STON document.");
            Console.WriteLine($"At line { ex.Line+1 }, column { ex.Column }:");
            Console.WriteLine("    " + ex.Message);
        }
        catch (StonException ex)
        {
            Console.WriteLine("The STON document was invalid for the following reason:");
            Console.WriteLine(ex.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine("A generic error occurred when processing the document.");
            Console.WriteLine(ex.Message);
        }
        Console.WriteLine("Press Enter to close...");
        Console.ReadLine();
    }
```
