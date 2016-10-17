# STON .NET

A .NET implementation of [Specifically Typed Object Notation](https://github.com/Alphish/ston). Currently available as 0.3.0 Alpha [on NuGet](https://www.nuget.org/packages/Alphicsh.Ston/).

To install it, you can look for **Alphicsh.Ston** on nuget.org feed (you must check the "Include prerelease" option) or type the following command in Package Manager console:

    PM> Install-Package Alphicsh.Ston -Pre

In order to learn about specific types provided in the package, please check the [API overview](#api-overview).

A deserialization functionality is planned for the future release.

# Table of contents

 - [STON .NET](#ston-net)
 - [Table of contents](#table-of-contents)
 - [Example](#example)
 - [API overview](#api-overview)

# Example
 
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

# API overview

**STON .NET** library comes with a collection of interfaces and other types useful for representing STON structures in your program, as well as their basic implementations. Additionally, there are classes that allow reading or writing STON entities or documents. The library also exposes various utility classes that can be used when writing custom implementations.

The following sections are present in the API overview:

 - [Alphicsh.Ston namespace](#alphicshston-namespace)
     - [About core implementations](#about-core-implementations)
     - [Entity interfaces](#entity-interfaces)
     - [Type interfaces](#type-interfaces)
     - [Value interfaces](#value-interfaces)
     - [Reference interfaces](#reference-interfaces)
     - [IStonDocument interface](#istondocument-interface)
     - [Text processing interfaces](#text-processing-interfaces)
         - [StonFormat enumeration](#stonformat-enumeration)
         - [IStonReader interface](#istonreader-interface)
         - [IStonWriter interface](#istonwriter-interface)
     - [Exceptions](#exceptions)
 - [Alphicsh.Ston.Building namespace](#alphicshstonbuilding-namespace)
     - [IStonElementFactory interface](#istonelementfactory-interface)
     - [IStonDocumentFactory interface](#istondocumentfactory-interface)
 - [Alphicsh.Ston.Equivalence namespace](#alphicshstonequivalence-namespace)
     - [IStonEntityEquivalenceComparer interface](#istonentityequivalencecomparer-interface)
     - [IStonTypeEquivalenceComparer interface](#istontypeequivalencecomparer-interface)
     - [IStonBindingKeyEquivalenceComparer interface](#istonbindingkeyequivalencecomparer-interface)
 - [Alphicsh.Ston.Helpers namespace](#alphicshstonhelpers-namespace)
     - [NumericStringCalculator static class](#numericstringcalculator-static-class)
     - [Validator static class](#validator-static-class)
 - [Alphicsh.Ston.Tokenization namespace](#alphicshstontokenization-namespace)
     - [StonChartype enumeration](#stonchartype-enumeration)
     - [StonChartable static class](#stonchartable-static-class)
     - [StonTokenReader class](#stontokenreader-class)
     - [StonTokenWriter class](#stontokenwriter-class)

## Alphicsh.Ston namespace

The namespace with the essential STON-related types, mostly representing STON abstract structure. Readers and writers are included here as well.

### About core implementations

The entities, types, values and other abstract structure elements (but not documents, readers and writers) are represented by interfaces with specific properties. At the same time, STON .NET library comes with so-called core implementations, classes that represent these elements. Since these implementations do little more than storing the STON data in memory, they share many relevant traits.

In general, if a class is said to be a core implementation, then:

 - its instances are immutable (in particular, so that once created document would not get broken)
 - any public property of its implemented interface stores a core implementation instance (thus, a core implementation structure contains no custom implementation elements in its hierarchy)
 - it has a public static method `Copy` that creates a copy of an appropriate STON element (core or custom) that represents the same STON structure, but uses core implementation classes
 - it has a public constructor whose parameters directly correspond to its public properties, unless it is abstract
 - it has a public constructor that takes an appropriate STON element (core or custom) and builds its core implementation copy (like *Copy* method), unless it is abstract

### Entity interfaces

`IStonEntity` interface represents a STON entity in general and serves as a base for more specific entities. On its own, it declares `string GlobalIdentifier {get}` property. Its core implementation is an abstract class `StonEntity`.

`IStonValuedEntity` interface derives from *IStonEntity* and represents a STON valued entity, serving as a base for simple-valued and complex-valued entities. Aside from global identifier, it declares `IStonType Type {get}` property. Its core implementation is an abstract class `StonValuedEntity`.

*IStonEntity* and *IStonValuedEntity* should not be implemented directly, but rather through one of derived interfaces available in STON .NET.

`IStonSimpleEntity` interface represents a STON simple-valued entity (deriving from *IStonValuedEntity*) and thus exposes `IStonSimpleValue Value {get}` property. Its core implementation is `StonSimpleEntity`.

`IStonComplexEntity` interface represents a STON complex-valued entity (deriving from *IStonValuedEntity*) and exposes properties characteristic to it (`IStonConstruction Construction {get}`, `IStonMemberInit MemberInit {get}` and `IStonCollectionInit CollectionInit {get}`). Its core implementation is `StonComplexEntity`.

`IStonReferenceEntity` interface represents a STON reference entity (deriving directly from *IStonEntity*) and thus exposes `IStonAddress Address {get}` property. Its core implementation is `StonReferenceEntity`.

---

STON entities also come with extension methods that allow representing STON structures as strings, declared in the `IStonEntity_Extensions` static class. The methods are as follows (the first parameter, i.e. the entity is omitted here):

 - `void ToString(writer)` - converts the entity to a string representation using a given writer
 - `void ToCanonicalForm()` - returns the canonical form of the entity
 - `void Save(path, writer)` - saves a string representation of the entity to a file using a given writer
 - `void SaveCanonicalForm(path)` - saves the canonical form of the entity to a file
 - `void Save(stream, writer)` - writes a string representation of the entity to a stream using a given writer, leaving the stream open
 - `void SaveCanonicalForm(stream)` - writes the canonical form of the entity to a stream, leaving the stream open

The "writer" parameter must be a STON writer, i.e. an instance of `IStonWriter` or `IStonWriter<TEntity, TDocument>`. The writers are described in more detail [here](#istonwriter-interface).

### Type interfaces

`IStonType` interface represents a STON type in general and serves as a base for more specific interfaces. It declares no properties or methods on its own. Its core implementation is an abstract class `StonType`. The interface should not be implemented directly, but rather through one of derived interfaces available in STON .NET.

`IStonNamedType` interface represents a STON named type and exposes properties characteristic to it (`string Name {get}`, `IEnumerable<IStonType> TypeParameters {get}` and `bool IsExtension {get}`). Its core implementation is `StonNamedType`.

`IStonCollectionType` interface represents a STON collection type and thus exposes `IStonType ElementType {get}` property. Its core implementation is `StonCollectionType`.

`IStonUnionType` interface represents a STON union type and thus exposes `IEnumerable<IStonType> PermittedType {get}` property. Its core implementation is `StonUnionType`.

### Value interfaces 

`StonDataType` enumeration describes the kind of data stored in a simple value. The possible values are: `Null`, `Number`, `Binary`, `Named`, `Text`, `Code`. They correspond to the six simple value types available in STON.

`IStonSimpleValue` interface represents a value of a STON simple-valued entity and exposes properties characteristic to it (`StonDataType DataType {get}` and `string Content {get}`). Its core implementation is `StonSimpleValue`.

---

`IStonConstruction` interface represents a construction of a STON complex entity and exposes properties characteristic to it (`IEnumerable<IStonEntity> PositionalParameters {get}` and `IEnumerable<KeyValuePair<string,IStonEntity>> {get}`). Its core implementation is `StonConstruction`.

---

`IStonMemberInit` interface represents a member initialization of a STON complex entity and exposes `IEnumerable<KeyValuePair<IStonBindingKey, IStonEntity>> MemberBindings {get}` property. Its core implementation is `StonMemberInit`.

`IStonBindingKey` interface represents a member binding key in general and serves as a base for more specific interfaces. It declares no properties or methods on its own. Its core implementation is an abstract class `StonBindingKey`. The interface should not be implemented directly, but rather through one of derived interfaces available in STON .NET.

`IStonBindingName` interface represents a member name inside a STON member initialization and exposes properties characteristic to it (`string Name {get}` and `bool IsExtension {get}`). Its core implementation is `StonBindingName`.

`IStonBindingIndex` interface represents a member index inside a STON member initialization and exposes `IEnumerable<IStonEntity> Parameters {get}` property. Its core implementation is `StonBindingIndex`.

---

`IStonCollectionInit` interface represents a collection initialization of a STON complex entity and exposes `IEnumerable<IStonEntity> Elements {get}` property. Its core implementation is `StonCollectionInit`.

### Reference interfaces

`IStonAddress` interface represents an address of a STON reference entity and exposes properties characteristic to it (`IStonInitialContext InitialContext {get}` and `IEnumerable<IStonPathSegment> RelativePath {get}`). Its core implementation is `StonAddress`.

---

`IStonInitialContext` interface represents an initial context of a STON address and serves as a base for more specific interfaces. It declares no properties or methods on its own. Its core implementation is an abstract class `StonInitialContext`. The interface should not be implemented directly, but rather through one of derived interfaces available in STON .NET.

`IStonAncestorInitialContext` interface represents a reference defining entity initial context or ancestor initial context. It exposes `int AncestorOrder {get}` property. If the property is equal to 0, it means the initial context is the reference defining entity; otherwise, it is an ancestor. The core implementation of the interface is `StonAncestorInitialContext`.

`IStonGlobalEntityInitialContext` interface represents a document root initial context or globally identified entity initia context. It exposes `string GlobalIdentifier {get}` property. If the property is same as an empty string, it means the initial context is the document root; otherwise, it is a globally identified entity. The core implementation of the interface is `StonGlobalEntityInitialContext`.

---

`IStonInitialPathSegment` interface represents a path segment of a STON address and serves as a base for more specific interfaces. It declares no properties or methods on its own. Its core implementation is an abstract class `StonPathSegment`. The interface should not be implemented directly, but rather through one of derived interfaces available in STON .NET.

`IStonAncestorPathSegment` interface represents an ancestor path segment and exposes `int AncestorOrder {get}` property. Its core implementation is `StonAncestorPathSegment`.

`IStonMemberPathSegment` interface represents a member path segment (named and indexed alike) and exposes `IStonBindingKey BindingKey {get}` property. Its core implementation is `StonMemberPathSegment`.

`IStonCollectionElementPathSegment` interface represents an ancestor path segment and exposes `IStonEntity ElementIndex {get}` property. Its core implementation is `StonCollectionElementPathSegment`.

### IStonDocument interface

This interface represents a STON document and exposes common document-related members (as described [here](https://github.com/Alphish/ston/blob/master/standards/STON-language-specification-draft.md#document-information)). The members are:

 - `IStonValuedEntity Core {get}` - passes the document core
 - `IStonEntity GetGlobalEntity(globalIdentifier)` - returns a globally identified entity with the specific identifier
 - `IStonComplexEntity GetParentContext(context)` - returns a defining context of a given entity in the contexts hierarchy
 - `IStonEntity GetMember(entity, memberKey)` - returns the member value given a complex entity from the document and a binding key
 - `IStonValuedEntity GetReferencedValue(reference)` - returns the value pointed by the given reference entity from the document
 - `IEnumerable<IStonValuedEntity> GetConstructionOrder()` - enumerates one of valid construction orders
 
---
 
Additionally, extension methods have been defined for *IStonDocument* interface in `IStonDocument_Extensions` static class. They allow representing STON documents as strings. The methods are as follows (the first parameter, i.e. the document is omitted here):

 - `void ToString(writer)` - converts the document to a string representation using a given writer
 - `void ToCanonicalForm()` - returns the canonical form of the document
 - `void Save(path, writer)` - saves a string representation of the document to a file using a given writer
 - `void SaveCanonicalForm(path)` - saves the canonical form of the document to a file
 - `void Save(stream, writer)` - writes a string representation of the document to a stream using a given writer, leaving the stream open
 - `void SaveCanonicalForm(stream)` - writes the canonical form of the document to a stream, leaving the stream open
 
The "writer" parameter must be a STON writer, i.e. an instance of `IStonWriter` or `IStonWriter<TEntity, TDocument>`. The writers are described in more detail [here](#istonwriter-interface).

---
 
The *IStonDocument* interface is implemented by `StonDocument` class. Its most basic constructor takes an *IStonValuedEntity* instance as a document root. Other constructors allow defining whitelists of known application extension types and members or rules determining whether a given name belongs to an application-side extension type or member. Upon creating a STON document, the structure represented by the document root is copied to a core implementation instance. That way the document's contents are guaranteed to be immutable.

### Text processing interfaces

#### StonFormat enumeration

This enumeration represents various formats a string can have in relation to STON. In particular, it is meant to be used with STON reader implementations to describe accepted input, or with STON writer implementations to describe produced output. The following values are available:

 - `Any` - the string is entirely arbitrary; in the context of a STON reader, it means that the reader can build a valid STON structure from some non-STON strings (it might or might not support actual STON strings); in the context of a STON writer, it means that at least in some cases the writer can produce non-STON output
 - `RegularSton` - the string is expected to be a valid regular STON string; in the context of a STON reader, it means that the reader can build a valid STON structure from any valid STON string and will report error for all other strings; in the context of a STON writer, it means the writer is guaranteed to output a valid regular STON output, but at least in some cases it might not be a strict STON output
 - `StrictSton` - the string is expected to be a valid strict STON string; in the context of a STON reader, it means that the reader can build a valid STON structure from any strict STON string and will report error for all other strings; in the context of a STON writer, it means that the writer is guaranteed to output a valid strict STON output, but at least in some cases it might not be a canonical STON output
 - `CanonicalSton` - the string is expected to be a canonical STON string; in the context of a STON reader, it means that the reader can build a valid STON structure from any canonical STON string and will report error for all other strings; in the context of a STON writer, it means the writer is guaranteed to output a canonical form for a given STON structure
 
It is worth noting that with such rules in place all writers supporting a specific STON format (*Any* does not count) output string readable by readers supporting the same format. Furthermore, all readers supporting the same STON format should build the same structure given the same valid string, and report some error given the same invalid string. These properties are useful when interchangeability is involved, as long as readers and writers have their supported format correctly defined.

#### IStonReader interface

This interface exposes methods for reading a STON entity (`IStonEntity ReadEntity(reader)`) or a STON document (`IStonDocument ReadDocument(reader, knownApplicationExtensionTypes, knownApplicationExtensionMembers, extensionTypesRule, extensionMembersRule)`). The reader argument must be an instance of `TextReader` from `System.IO` namespace.

Additionally, there is a property describing the supported STON format (`StonFormat SupportedFormat {get}`); for more details about the usage of that property, read about [StonFormat enumeration](#stonformat-enumeration), especially in the context of STON readers.

The interface also has a generic variant `IStonReader<TEntity, TDocument>`, with *TEntity* and *TDocument* implementing *IStonEntity* and *IStonDocument*, respectively. It can be useful when a custom STON structure implementation defines additional variables not included in IStonEntity/IStonDocument interfaces, for example. Also, whenever a STON reader is implemented, *IStonReader<IStonEntity, IStonDocument>* should be preferred over *IStonReader*, as the former derives from the latter and both are functionally equivalent.

The following extension methods for *IStonReader* and its generic variant are defined in `IStonReader_Extensions` static class (the first parameter, i.e. the reader, is omitted here):

 - `IStonEntity ParseEntity(value)` - builds a STON structure from a given string
 - `IStonDocument ParseDocument(value, knownApplicationExtensionTypes, knownApplicationExtensionMembers, extensionTypesRule, extensionMembersRule)` - builds a STON document from a given string
 - `IStonEntity LoadEntity(path)` - loads a STON structure from a given file
 - `IStonDocument LoadDocument(path, knownApplicationExtensionTypes, knownApplicationExtensionMembers, extensionTypesRule, extensionMembersRule)` - loads a STON document from a given file
 - `IStonEnity LoadEntity(stream)` - loads a STON structure from a given stream, leaving the stream open
 - `IStonDocument LoadDocument(stream, knownApplicationExtensionTypes, knownApplicationExtensionMembers, extensionTypesRule, extensionMembersRule)` - loads a STON document from a given stream, leaving the stream open

The *IStonReader* interface (as well as its equivalent variant *IStonReader&lt;IStonEntity, IStonDocument&gt;*) is implemented by `RegularStonReader` class. The class supports regular STON format and by default creates STON entities and documents using core implementation. Implementations of *IStonElementFactory* and *IStonDocumentFactory* can be passed to constructor if custom STON structure is needed instead. The interfaces are described in more detail [here](#alphicshstonbuilding-namespace).

#### IStonWriter interface

This interfaces exposes methods for representing a STON entity (`void WriteEntity(writer, entity)`) or a STON document (`void WriteDocument(writer, document)`) as a codepoints sequence, or generating other text based on the structure. The writer argument must be an instance of `TextWriter` from `System.IO` namespace.

Additionally, there is a property describing the supported STON format (`StonFormat SupportedFormat {get}`); for more details about the usage of that property, read about [StonFormat enumeration](#stonformat-enumeration), especially in the context of STON writers.

The interface also has a more specific variant `IStonWriter<TEntity, TDocument>`, with *TEntity* and *TDocument* implementing *IStonEntity* and *IStonDocument*, respectively. It can be useful when a custom STON structure implementation defines additional variables not included in IStonEntity/IStonDocument interfaces and these variables affect the output, for example. Also, whenever a STON writer is implemented, *IStonWriter* should be preferred over *IStonWriter<IStonEntity, IStonDocument>*, as the former derives from the latter and both are functionally equivalent.

The STON writer interface is also used when converting STON entities or documents to string (in particular, canonical form) using their extension methods. See [IStonEntity description](#entity-interfaces) and [IStonDocument description](#istondocument-interface) for more details.

The *IStonWriter* interface (as well as its equivalent variant *IStonWriter&lt;IStonEntity, IStonDocument&gt;*) is implemented by `CanonicalStonWriter` class. The class, given a STON entity or document, outputs its canonical form. The `CanonicalStonWriter` is a singleton, with its instance provided through `Instance` static property of `IStonWriter` type.

### Exceptions

The library comes with a collection of STON-related exception types. All of these derive from `StonException` class, which can be also used as a general-purpose exception when handling STON entities.

The other exception types are:

 - `StonImplementationException`, which is thrown when a type implements an "abstract" STON-related interface without implementing any of its derived interfaces; it includes the information about types involved
 - `StonParsingException`, which is thrown when a parsed STON text is found to be invalid in some way; it can include the information about the position where the issue occurs
     - `StonUnexpectedCharacterParsingException`, which is thrown when an unexpected codepoint is encountered when parsing a STON text; it can include the information about the invalid codepoint
 - `StonValueException`, which is thrown when a STON valued entity is found to have an invalid value; it includes the valued entity in question
     - `StonDuplicateMemberException`, which is thrown when a STON complex entity is found to have duplicate members; it can include the duplicate member bindings as well as the document where the entity appears
 - `StonReferenceException`, which is thrown when a STON reference entity is found to be invalid in one way or another; it includes the reference entity in question
     - `StonUnknownReferenceException`, which is thrown when the initial context or one of path segments cannot be resolved; it can include the unresolved path segment and the latest known context
     - `StonCircularReferenceException`, which is thrown when a reference is found pointing to itself, directly or not; it includes the chain of references that eventually point to the original entity
 - `StonDuplicateGlobalEntityException`, which is thrown when at least two entities share the same global identifier; it includes both entities and their identifier
 - `StonCircularConstructionException`, which is thrown when a document cannot be built, because there is no valid construction order; it includes the chain of constructed entities that form a circular dependency
 - `StonExtensionTypeException`, which is thrown when there are problems with processing an extension type; it includes the extension type and the entity where it has been found
     - `StonUnknownExtensionTypeException`, which is thrown when an extension type is recognised neither as a document-side or application-side extension
 - `StonExtensionMemberException`, which is thrown when there are problems with processing an extension member; it includes the member key and the associated entity
     - `StonUnknownExtensionMemberException`, which is thrown when an extension member is recognised neither as a document-side or application-side extension

## Alphicsh.Ston.Building namespace

This namespace contains interfaces and classes for building STON elements and documents. They can be used for easily swapping one STON structure implementation with another as needed, as long as the code building structures uses appropriate interfaces. In library itself, they are used by `RegularStonReader` to allow reading custom STON structures instead of STON .NET built-in ones.

### IStonElementFactory interface

This interface exposes methods for assembling STON elements. The methods available are:

 - entity building
     - `IStonSimpleEntity CreateSimpleEntity(value, type, globalIdentifier)`
     - `IStonComplexEntity CreateComplexEntity(construction, memberInit, collectionInit, type, globalIdentifier)`
     - `IStonReferenceEntity CreateReferenceEntity(address, globalIdentifier)`
 - types building
     - `IStonNamedType CreateNamedType(name, typeParameters, isExtension)`
     - `IStonCollectionType CreateCollectionType(elementType)`
     - `IStonUnionType CreateUnionType(permittedTypes)` 
 - value building
     - `IStonSimpleValue CreateSimpleValue(dataType, content)`
     - `IStonConstruction CreateConstruction(positionalParameters, namedParameters)`
     - `IStonMemberInit CreateMemberInit(memberBindings)`
     - `IStonBindingName CreateBindingName(name, isExtension)`
     - `IStonBindingIndex CreateBindingIndex(parameters)`
     - `IStonCollectionInit CreateCollectionInit(elements)`
 - reference address building
     - `IStonAddress CreateAddress(initialContext, relativePath)`
     - `IStonAncestorInitialContext CreateAncestorInitialContext(ancestorOrder)` (the order of 0 is used for reference defining context)
     - `IStonGlobalEntityInitialContext CreateGlobalEntityInitialContext(globalIdentifier)` (the empty identifier is used for document root)
     - `IStonAncestorPathSegment CreateAncestorPathSegment(ancestorOrder)`
     - `IStonMemberPathSegment CreateMemberPathSegment(bindingKey)`
     - `IStonCollectionElementPathSegment CreateCollectionElementPathSegment(elementIndex)`

The interface is implemented by `CoreStonElementFactory`, which creates instances of STON elements as implemented in STON .NET. The `CoreStonElementFactory` is a singleton, with its instance provided through `Instance` static property of `IStonElementFactory` type.

### IStonDocumentFactory interface

This interface exposes the method for creating STON documents. The method is:
`CreateDocument(coreSource, knownApplicationExtensionTypes, knownApplicationExtensionMembers, extensionTypesRule, extensionMembersRule)`

The interface is implemented by `CoreStonDocumentFactory`, which creates instances of `IStonDocument` as implemented in STON .NET. The `CoreStonDocumentFactory` is a singleton, with its instance provided through `Instance` static property of `IStonDocumentFactory` type.

## Alphicsh.Ston.Equivalence namespace

This namespace contains interfaces and classes for various kinds of equivalence checking. In library itself, they are mostly used when building a STON document (especially when key indices need to be matched).

### IStonEntityEquivalenceComparer interface

This interface exposes methods for checking whether two STON entities are equivalent or not. In essence, the implementation boils down to equality comparisons (`bool Equals(x, y)`) and hashing methods (`int GetHashCode(obj)`) for `IEqualityComparer<IStonEntity>`, `IEqualityComparer<IStonValuedEntity>`, `IEqualityComparer<IStonSimpleEntity>`, `IEqualityComparer<IStonComplexEntity>` and `IEqualityComparer<IStonReferenceEntity>`.

The interface is implemented by `StonSemanticEntityEquivalenceComparer`, which performs a semantic entity equivalence comparison, as described in STON specification [here](https://github.com/Alphish/ston/blob/master/standards/STON-language-specification-draft.md#entity-semantic-equivalence). The semantic equivalence comparer takes a STON document (`IStonDocument` instance) as a construction parameter. That way, reference entities can be compared against valued entities and each other in the context of this document. When comparing entities types, the `StonTypeEquivalenceComparer` instance is used.

### IStonTypeEquivalenceComparer interface

This interface exposes methods for checking whether two STON types are equivalent or not. In essence, the implementation boils down to equality comparisons (`bool Equals(x, y)`) and hashing methods (`int GetHashCode(obj)`) for `IEqualityComparer<IStonType>`, `IEqualityComparer<IStonNamedType>`, `IEqualityComparer<IStonCollectionType>` and `IEqualityComparer<IStonUnionType>`.

The interface is implemented by `StonTypeEquivalenceComparer`, which performs a basic types equivalence comparison, as described in STON specification [here](https://github.com/Alphish/ston/blob/master/standards/STON-language-specification-draft.md#entity-semantic-equivalence). The `StonTypeEquivalenceComparer` is a singleton, with its instance provided through `Instance` static property of `IStonTypeEquivalenceComparer` type.

### IStonBindingKeyEquivalenceComparer interface

This interface exposes methods for checking whether two STON binding keys match or not. In essence, the implementation boils down to equality comparisons (`bool Equals(x, y)`) and hashing methods (`int GetHashCode(obj)`) for `IEqualityComparer<IStonBindingKey>`, `IEqualityComparer<IStonBindingName>` and `IEqualityComparer<IStonBindingIndex>`.

The interface is implemented by `StonBindingKeyEquivalenceComparer`, which performs a comparison of binding keys based on the following rules:

 - two binding names (`IStonBindingName` instances) match if and only if they have the same `Name` and `IsExtension` properties values
 - two binding indices (`IStonBindingIndex` instances) match if and only if their `Parameter` properties hold sequentially equivalent entities collections (that is, both indices have the same number of parameters and entities at corresponding positions are equivalent); the entities equivalence is determined by the provided entities comparer
 - a binding name is never equivalent to a binding index

The binding keys equivalence comparer takes an entity equivalence comparer (`IStonEntityEquivalenceComparer` instance) as a construction parameter. The entity comparer is used when matching binding indices.

## Alphicsh.Ston.Helpers namespace

This namespace contains static classes for functionalities small enough that they do not require whole namespaces.

### NumericStringCalculator static class

`NumericStringCalculator` contains a collection of functions to add and subtract arbitrarily large integers in their decimal representation (sequence of digits from 0 to 9, optionally prepended with a minus sign). In library itself, these functions are used when determining the correct exponent of a number value (e.g. to find that number literal `10e 999 999 999 999 999 999 999 999` should be represented with `1e1000000000000000000000000` content).

The class exposes two methods: `string Add(x, y)` and `string Subtract(x, y)`. Both represent adding to and subtracting from a specific number, respectively. The first parameter is the number written as a sequence of characters, whereas the second parameter is the integer to add or remove. In typical scenario, the exponent string written in the number literal is used as the first argument, while the exponent difference calculated from significand (based on number of digits after decimal point and leading zeros) is the second argument.

### Validator static class
 
`Validator` contains a collection of functions to check whether the structure of a specific STON element is valid, and throw an exception if it is not. The validation is shallow - a complex entity with correct structure will pass the validation even if one of its child entities is actually invalid.

The functions provided are:

 - `ValidateEntity(entity)` (with overloads for all entity interfaces)
 - `ValidateGlobalIdentifier(globalIdentifier)`
 - `ValidateType(type)` (with overloads for all type interfaces)
 - `ValidateSimpleValue(value)`
 - `ValidateConstruction(construction)`
 - `ValidateMemberInit(memberInit)`
 - `ValidateBindingKey(bindingKey)` (with overloads for all binding key interfaces)
 - `ValidateCollectionInit(collectionInit)`
 - `ValidateAddress(address)`
 - `ValidateInitialContext(initialContext)` (with overloads for all initial context interfaces)
 - `ValidatePathSegment(segment)` (with overloads for all path segment interfaces)
 
## Alphicsh.Ston.Tokenization namespace

This namespace contains types useful for processing STON strings. In library itself, they are used in STON reader and writer implementations.

### StonChartype enumeration

This enumeration describes types of characters present in STON strings. It is a flag enumeration - each value corresponds to a single bit, and all possible combinations of character types can be expressed in a single variable. The character types are closely related to STON tokens, as described [here](https://github.com/Alphish/ston/blob/master/standards/STON-language-specification-draft.md#tokens). At the same time, there's no 1:1 relation between them; for example, some tokens can be composed of multiple characters, sometimes of varying type.

### StonChartable static class

This class contains predefined tables of character mappings, and exposes functions to access them. In particular, it contains the mapping between all 128 ASCII codepoints and their STON character types, as well as mappings realted to base16 and base64 notation.

The functions provided are:

 - `StonChartype GetChartype(c)` - extension method of `char` and `int` types, allows retrieving an associated character type for a given codepoint
 - `bool HasChartype(c, chartype)` - extension method of `char` and `int` types, allows checking if the codepoint's type and given character type are matching
 - `int GetBase16Value(c)` - extension method of `char` and `int` types that returns a base16 value associated with a given codepoint
 - `int GetBase64Value(c)` - extension method of `char` and `int` types that returns a base64 value associated with a given codepoint
 - `char GetBase16Symbol(value)` - returns a codepoint representing a specific base16 digit
 - `char GetBase64Symbol(value)` - returns a codepoint representing a specific base64 digit

### StonTokenReader class

This class provides methods to scan through STON text and retrieve tokens. Additionally, it keeps track of the cursor position, making it possible to record position at specific points, or find the place where an issue occurs.

StonTokenReader takes `TextReader` (from `System.IO` namespace) as a constructor parameter. The inner reader is then used to proceed through the STON text.

The properties and methods provided are:

 - `int Position {get}` - the position of the reader cursor counted from the beginning
 - `int Line {get}` - the index of the line where the reader cursor is; lines are separated with LF (line feed) characters, CR (carriage return) characters or CRLF sequences
 - `int Column {get}` - the position of the reader cursor in the current line
 - `int Peek()` - returns the codepoint at the current reader's position, or -1 if the reader reached the end
 - `int Read()` - scans through the codepoint at the current reader's position and returns it (or returns -1 if the reader is already at the end)
 - `int ExpectChartype(chartype)` - checks that the next codepoint matches a specific STON character type and scans through it (or throws an exception if the codepoint does not match expected type)
 - `bool TryChartype(chartype)` - checks whether the next codepoint matches a specific STON character type, scans through it if it is true and returns the outcome
 - `int PeekSignificant()` - scans through whitespace and comments and returns the nearest relevant codepoint
 - `int ReadAndSkip()` - reads the next character and scans through subsequent ignored characters, then returns the initially read character
 - `int ExpectAndSkip(chartype)` - reads the next character, confirms its character type and scans through subsequent ignored characters, then returns the initially read character
 - `bool TryAndSkip(chartype)` - checks whether the next codepoint matches a specific STON character type, scans through it and subsequent ignored characters if it is true and returns the outcome
 - `void ReadCanun(builder)` - scans through a CANUN (Common Alpha-Numeric and Underscore Name) token and writes it to a given string builder
 - `string ReadCanun()` - scans through a CANUN token and returns it
 - `void ReadString(builder, code)` - scans through a string literal (both text and code literals work) and writes its content to a given string builder
 - `string ReadString(code)` - scans through a string literal (both text and code literals work) and returns its content
 - `string ReadBinaryContent(minus)` - scans through an unsigned part of the binary literal and returns its content (with or without minus, depending on the argument provided)
 - `string ReadNumberContent(minus)` - scans through an unsigned part of the number literal and returns its content (with or without minus, depending on the argument provided)
 - `bool ReadCollectionTypeSuffix(requireFull)` - attempts to scan through a collection type suffix token and returns whether the scan has been successful; if full suffix was required but wasn't present, the cursor ends up just before the first significant character after the square bracket

### StonTokenWriter class

This class provides methods to write STON text based on specific content. In particular, it allows easily writing simple values in specific formats.

StonTokenWriter takes `TextWriter` (from `System.IO` namespace) as a constructor parameter. The inner writer is then used to write STON text.

The methods provided are:

 - `void Write(value)` - writes a given character or a string directly
 - `void WriteLine()` - starts a new line, in a manner specific for the inner text writer
 - `void WriteLine(value)` - writes a character or string directly, then starts a new line
 - `void WriteStringLiteral(content, delimiter)` - represents a string content as a string literal and writes it; a double quote, apostrophe or backtick may be used as a delimiter
 - `void WriteBinaryLiteral(content, baseId)` - represents a binary content as a binary literal of a specific base and writes it; the base identifier can be "b", "o", "x", "z" or "n" (however, "n" can be used only for empty content)
 - `void WriteCanonicalNumberLiteral(content)` - represents a number content as a canonical number literal; effectively, it means validating the content and writing it directly
 - `void WritePlainNumberLiteral(content)` - respresents a number content as a plain decimal number, without the exponent part
 - `void WriteScientificNumberLiteral(content, minPrecision)` - represents a number content in scientific notation; the minimum precision parameter determines how many digits must be written
