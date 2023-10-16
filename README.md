# PdfiumViewer.WPF

Apache 2.0 License.

> Note: this is a fork of bezzad/PdfiumViewer project for .NET Framework 4.8 and compatible with VS2017 and Windows 7.
[bezzad/PdfiumViewer](https://github.com/bezzad/PdfiumViewer) is a .Net Core WPF port of [pvginkel/PdfiumViewer](https://github.com/pvginkel/PdfiumViewer)

Additional features compared to the bezzad/PdfiumViewer:
* Thrumbnail view
* Pringting
* Text search and highlighting
* Text selection (copy to clipboard and drag and drop text)
* Support Pdf links
* Improved performance 
* Use pdfium binaris from [bblanchon/pdfium-binaries](https://github.com/bblanchon/pdfium-binaries)
* Lot of bug fixes

![PdfiumViewer.WPF](https://raw.githubusercontent.com/bezzad/PdfiumViewer/master/screenshot.png)

![PdfiumViewer.WPF](https://raw.githubusercontent.com/bezzad/PdfiumViewer/master/screenshot2.png)

![PdfiumViewer.WPF](https://raw.githubusercontent.com/bezzad/PdfiumViewer/master/screenshot3.png)

## Introduction

PdfiumViewer is a PDF viewer based on the PDFium project.

PdfiumViewer provides a number of components to work with PDF files:

* PdfDocument is the base class used to render PDF documents;

* PdfRenderer is a WPF control that can render a PdfDocument;

> Note: If you want to use that in WinForms, please use the main project from [PdfiumViewer WinForm](https://github.com/pvginkel/PdfiumViewer)

## Compatibility

The PdfiumViewer.WPF library has been tested with Windows 7 and Windows 10, and
is fully compatible with both. 

## Using the library

The PdfiumViewer.WPF control requires native PDFium libraries. These are not included in this repository, but included as NuGet packages from bblanchon/pdfium-binaries. 

## Note on the `PdfViewer` control

The PdfiumViewer library primarily consists out of three components:

* The `PdfRenderer` control. This control implements the raw PDF renderer.
  This control displays a PDF document, provides zooming and scrolling
  functionality and exposes methods to perform more advanced actions;
* The `PdfDocument` class provides access to the PDF document and wraps
  the Pdfium library.

## Bugs

Bugs should be reported through github at [https://github.com/majkimester/PdfiumViewer.WPF/issues](https://github.com/majkimester/PdfiumViewer.WPF/issues).

## License

PdfiumViewer is licensed under the Apache 2.0 license. See the license details for how PDFium is licensed.