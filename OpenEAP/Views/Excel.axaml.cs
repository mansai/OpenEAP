using AtomUI;
using AtomUI.Desktop.Controls;
using AtomUI.Theme;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.Configuration;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using OpenEAP.Common;
using OpenEAP.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace OpenEAP.Views;

public partial class Excel :Window
{
    private static readonly FilePickerFileType ExcelFiles = new("Excel workbook (*.xlsx)")
    {
        Patterns = new[] { "*.xlsx" },
        MimeTypes = new[] { "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
        AppleUniformTypeIdentifiers = new[] { "org.openxmlformats.spreadsheetml.sheet" },
    };

    private static readonly FilePickerFileType TextFiles = new("Text (TSV/CSV)")
    {
        Patterns = new[] { "*.tsv", "*.csv", "*.txt" },
    };

    private static readonly FilePickerFileType PdfFiles = new("PDF document (*.pdf)")
    {
        Patterns = new[] { "*.pdf" },
        MimeTypes = new[] { "application/pdf" },
        AppleUniformTypeIdentifiers = new[] { "com.adobe.pdf" },
    };

    private static bool IsXlsx(string name) =>
        name.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase);

    private IStorageFile? currentFile;
    public Excel()
    {
        InitializeComponent();
        // Ctrl+P prints (the grid's key handler ignores it, so it bubbles up to here).
        this.KeyDown += (_, e) =>
        {
            if (e.Key == Key.P && e.KeyModifiers == KeyModifiers.Control)
            {
                this.Editor.PrintPdf();
                e.Handled = true;
            }
        };
    }
    
    private void OnPrint(object? sender, RoutedEventArgs e) => this.Editor.PrintPdf();

    private async void OnExportPdf(object? sender, RoutedEventArgs e)
    {
        IStorageFile? file = await this.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Export as PDF",
            DefaultExtension = "pdf",
            SuggestedFileName = "sheet.pdf",
            FileTypeChoices = new[] { PdfFiles },
        });

        if (file is null)
        {
            return;
        }

        await using Stream stream = await file.OpenWriteAsync();
        this.Editor.ExportPdf(stream);
    }

    private void OnNew(object? sender, RoutedEventArgs e)
    {
        this.Editor.NewSheet(50, 26);
        this.currentFile = null;
        this.UpdateTitle();
    }

    private void OnClose(object? sender, RoutedEventArgs e)
    {
        this.Editor.NewSheet(50, 26);
        this.currentFile = null;
        this.UpdateTitle();
    }

    private async void OnOpen(object? sender, RoutedEventArgs e)
    {
        try
        {
            IReadOnlyList<IStorageFile> files = await this.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Open spreadsheet",
                AllowMultiple = false,
                FileTypeFilter = new[] { ExcelFiles, TextFiles },
            });

            if (files.Count == 0)
            {
                return;
            }

            IStorageFile file = files[0];
            await using Stream stream = await file.OpenReadAsync();

            if (IsXlsx(file.Name))
            {
                await this.Editor.OpenXlsxAsync(stream);
            }
            else
            {
                using StreamReader reader = new(stream);
                string text = await reader.ReadToEndAsync();
                this.Editor.LoadRows(Parse(text, DelimiterFor(file.Name)));
            }

            this.currentFile = file;
            this.UpdateTitle();
        }
        catch (Exception ex)
        {
            await this.ShowError("Open failed", ex.Message);
        }
    }

    private async void OnSave(object? sender, RoutedEventArgs e)
    {
        if (this.currentFile is null)
        {
            await this.SaveAs();
            return;
        }

        await this.WriteTo(this.currentFile);
    }

    private async void OnSaveAs(object? sender, RoutedEventArgs e) => await this.SaveAs();

    private async Task SaveAs()
    {
        try
        {
            IStorageFile? file = await this.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Save spreadsheet",
                DefaultExtension = "xlsx",
                SuggestedFileName = "sheet.xlsx",
                FileTypeChoices = new[] { ExcelFiles, TextFiles },
            });

            if (file is null)
            {
                return;
            }

            await this.WriteTo(file);
            this.currentFile = file;
            this.UpdateTitle();
        }
        catch (Exception ex)
        {
            await this.ShowError("Save failed", ex.Message);
        }
    }

    private async Task WriteTo(IStorageFile file)
    {
        // Build the full file content first.
        byte[] bytes;
        if (IsXlsx(file.Name))
        {
            using MemoryStream buffer = new();
            this.Editor.SaveXlsx(buffer);
            bytes = buffer.ToArray();
        }
        else
        {
            string text = ToDelimited(this.Editor.GetRows(), DelimiterFor(file.Name));
            bytes = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false).GetBytes(text);
        }

        // On desktop the picker returns a real path — write directly so the file is fully replaced
        // (OpenWriteAsync doesn't truncate, which corrupts a shorter .xlsx/zip on overwrite).
        string? localPath = file.TryGetLocalPath();
        if (localPath is not null)
        {
            await File.WriteAllBytesAsync(localPath, bytes);
            return;
        }

        await using Stream stream = await file.OpenWriteAsync();
        await stream.WriteAsync(bytes);
        if (stream.CanSeek)
        {
            stream.SetLength(bytes.Length);
        }
    }

    private void UpdateTitle() =>
        this.Title = this.currentFile is null
            ? "Spreadsheet Editor"
            : $"Spreadsheet Editor — {this.currentFile.Name}";

    private async Task ShowError(string title, string message)
    {
        Window dialog = new()
        {
            Title = title,
            Width = 380,
            Height = 140,
            WindowStartupLocation =Avalonia.Controls.WindowStartupLocation.CenterOwner,
            Content = new TextBlock
            {
                Text = message,
                Margin = new Avalonia.Thickness(16),
                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            },
        };
        await dialog.ShowDialog(this);
    }
    public static char DelimiterFor(string fileName) =>
       fileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase) ? ',' : '\t';

    // Parses delimited text into rows of cell strings (no quote handling for v1 TSV).
    public static List<List<string>> Parse(string text, char delimiter)
    {
        List<List<string>> rows = new();
        string normalized = text.Replace("\r\n", "\n").Replace('\r', '\n');
        foreach (string line in normalized.Split('\n'))
        {
            rows.Add(new List<string>(line.Split(delimiter)));
        }

        // drop a trailing empty line produced by a final newline
        if (rows.Count > 1 && rows[^1].Count == 1 && rows[^1][0].Length == 0)
        {
            rows.RemoveAt(rows.Count - 1);
        }

        return rows;
    }

    public static string ToDelimited(IReadOnlyList<IReadOnlyList<string>> rows, char delimiter)
    {
        StringBuilder sb = new();
        foreach (IReadOnlyList<string> row in rows)
        {
            sb.Append(string.Join(delimiter, row)).Append('\n');
        }

        return sb.ToString();
    }
}