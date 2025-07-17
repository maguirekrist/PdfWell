# PdfWell - C# PDF Parser

A high-performance, feature-rich PDF parser library for .NET that provides comprehensive PDF document processing capabilities including text extraction, form handling, encryption support, and more.

![GitHub release (latest by date including pre-releases)](https://img.shields.io/github/v/release/maguirekrist/PdfWell?include_prereleases)
[![Run Unit Tests](https://github.com/maguirekrist/PdfWell/actions/workflows/run-tests.yml/badge.svg)](https://github.com/maguirekrist/PdfWell/actions/workflows/run-tests.yml)
[![.NET](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)

## Features

### Core PDF Processing
- **Complete PDF Parsing**: Full PDF 1.7 specification support
- **Text Extraction**: Extract text with font information and positioning
- **Page Management**: Access individual pages and their properties
- **Document Metadata**: Extract document information and properties

### Advanced Features
- **AcroForm Support**: Handle interactive forms and form fields
- **Encryption Support**: Decrypt password-protected PDFs (RC4, AES-128, AES-256)
- **Linearized PDFs**: Optimized parsing for large documents
- **Cross-Reference Tables**: Efficient object location and retrieval
- **Stream Processing**: Handle compressed and encoded content streams

### Performance Optimizations
- **Memory Efficient**: Optimized for large PDF processing
- **Fast Parsing**: Multiple string matching algorithms (Boyer-Moore, KMP)
- **Lazy Loading**: On-demand page and object loading
- **Benchmarking**: Built-in performance measurement tools

## Installation

### NuGet Package
```bash
dotnet add package PDFWell
```

### From Source
```bash
git clone https://github.com/maguirekrist/PdfWell.git
cd PdfWell
dotnet build
```

## Quick Start

### Basic PDF Parsing
```csharp
using PDFParser.Parser;

// Load and parse a PDF
var pdfData = File.ReadAllBytes("document.pdf");
var parser = new PdfParser(pdfData);
var document = parser.Parse();

// Access pages
foreach (var page in document.Pages)
{
    Console.WriteLine($"Page {page.PageNumber}: {page.MediaBox}");
    
    // Extract text
    var texts = page.GetTexts();
    foreach (var text in texts)
    {
        Console.WriteLine($"Text: {text.Value} (Font: {text.FontFamily}, Size: {text.FontSize})");
    }
}
```

### Working with Forms
```csharp
// Access AcroForm data
var acroForm = document.GetAcroForm();
if (acroForm != null)
{
    var fields = acroForm.GetFields();
    foreach (var field in fields)
    {
        Console.WriteLine($"Field: {field.FieldName?.Text} (Type: {field.Type?.Name})");
    }
}
```

### Handling Encrypted PDFs
```csharp
// Check if document is encrypted
if (document.IsEncrypted)
{
    var permissions = document.GetDocumentPermissions();
    Console.WriteLine($"Document permissions: {permissions}");
    
    // Note: Password handling is in development
}
```

## API Reference

### Core Classes

#### `PdfParser`
Main entry point for PDF parsing.

```csharp
public class PdfParser
{
    public PdfParser(byte[] pdfData, IMatcher? matcherStrategy = null);
    public PdfDocument Parse();
}
```

#### `PdfDocument`
Represents a parsed PDF document.

```csharp
public class PdfDocument
{
    public List<Page> Pages { get; }
    public DocumentCatalog DocumentCatalog { get; }
    public bool IsEncrypted { get; }
    public bool IsLinearized { get; }
    
    public Page GetPage(int pageNumber);
    public AcroFormDictionary? GetAcroForm();
    public UserAccessPermissions? GetDocumentPermissions();
    public void Save(string path);
}
```

#### `Page`
Represents a single PDF page.

```csharp
public class Page
{
    public int PageNumber { get; }
    public PageBox MediaBox { get; }
    public ReadOnlyDictionary<string, Font> FontDictionary { get; }
    
    public List<DocumentText> GetTexts();
}
```

### Text Extraction

#### `DocumentText`
Represents extracted text with metadata.

```csharp
public readonly struct DocumentText
{
    public string Value { get; }
    public int FontSize { get; }
    public string FontFamily { get; }
    public (int, int) Position { get; }
}
```

### Form Handling

#### `AcroFormDictionary`
Manages interactive form data.

```csharp
public class AcroFormDictionary
{
    public List<AcroFormFieldDictionary> GetFields();
    public BooleanObject? NeedAppearances { get; }
    public NumericObject? SigFlags { get; }
}
```

#### `AcroFormFieldDictionary`
Represents individual form fields.

```csharp
public class AcroFormFieldDictionary
{
    public StringObject? FieldName { get; }
    public NameObject? Type { get; }
    public DirectObject? FieldValue { get; }
    public FieldFlags? FieldFlags { get; }
    public bool IsTerminal { get; }
}
```

## Performance

The library includes built-in benchmarking capabilities:

```csharp
// Run benchmarks
dotnet run --project PDFParser.Benchmark
```

Performance characteristics:
- **Memory Usage**: Optimized for large PDFs with streaming support
- **Parsing Speed**: Multiple string matching algorithms for optimal performance
- **Scalability**: Efficient object table management for complex documents

## Supported PDF Features

### Document Structure
- ✅ PDF 1.7 specification compliance
- ✅ Cross-reference tables and streams
- ✅ Object streams and linearization
- ✅ Document catalog and metadata

### Content Processing
- ✅ Text extraction with positioning
- ✅ Font handling and character mapping
- ✅ Content stream parsing
- ✅ Image and graphics support (basic)

### Interactive Elements
- ✅ AcroForm fields and widgets
- ✅ Form field types (text, button, choice, signature)
- ✅ Field validation and properties
- ✅ Form appearance handling

### Security
- ✅ Password-protected PDF support
- ✅ RC4 encryption (40-bit, 128-bit)
- ✅ AES encryption (128-bit, 256-bit)
- ✅ User and owner password handling
- ✅ Permission flags and access control

### Advanced Features
- ✅ Linearized PDF optimization
- ✅ Compressed stream handling
- ✅ Unicode text support
- ✅ Multiple encoding support

## Development

### Prerequisites
- .NET 8.0 SDK
- Visual Studio 2022 or VS Code

### Building
```bash
git clone https://github.com/maguirekrist/PdfWell.git
cd PdfWell
dotnet restore
dotnet build
```

### Testing
```bash
dotnet test
```

### Benchmarking
```bash
dotnet run --project PDFParser.Benchmark
```

## Contributing

We welcome contributions! Please see our [Contributing Guidelines](CONTRIBUTING.md) for details.

### Development Setup
1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new functionality
5. Run the test suite
6. Submit a pull request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- Built with [BouncyCastle](https://www.bouncycastle.org/) for cryptographic operations
- Inspired by the need for a fast, mutable PDF parser in .NET
- Community contributions and feedback

## Roadmap

- [ ] Password-based decryption
- [ ] PDF creation and modification
- [ ] Digital signature support
- [ ] Advanced graphics operations
- [ ] PDF/A compliance
- [ ] WebAssembly support for browser environments

## Support

- **Issues**: [GitHub Issues](https://github.com/maguirekrist/PdfWell/issues)
- **Discussions**: [GitHub Discussions](https://github.com/maguirekrist/PdfWell/discussions)
- **Documentation**: [Wiki](https://github.com/maguirekrist/PdfWell/wiki)

---

**Note**: This library is currently in active development. While core functionality is stable, some advanced features may be subject to change.