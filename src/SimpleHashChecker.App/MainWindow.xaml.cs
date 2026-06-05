using Microsoft.Win32;
using SimpleHashChecker.Core;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace SimpleHashChecker.App;

public partial class MainWindow : Window
{
    private readonly FileHashService hashService = new();
    private string? selectedFilePath;
    private string currentHash = string.Empty;
    private CancellationTokenSource? hashCancellation;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void BrowseButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            CheckFileExists = true,
            Multiselect = false,
            Title = "Select a file to hash"
        };

        if (dialog.ShowDialog(this) == true)
        {
            SelectFile(dialog.FileName);
        }
    }

    private void Window_DragOver(object sender, DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop)
            ? DragDropEffects.Copy
            : DragDropEffects.None;
        e.Handled = true;
    }

    private void Window_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetData(DataFormats.FileDrop) is string[] { Length: > 0 } files)
        {
            SelectFile(files[0]);
        }
    }

    private async void HashButton_Click(object sender, RoutedEventArgs e)
    {
        if (selectedFilePath is null)
        {
            StatusText.Text = "Select a file first.";
            return;
        }

        if (sender is not FrameworkElement { Tag: string algorithmName }
            || !Enum.TryParse<HashAlgorithmKind>(algorithmName, out var algorithmKind))
        {
            return;
        }

        await CalculateHashAsync(algorithmKind);
    }

    private void CopyButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(currentHash))
        {
            StatusText.Text = "There is no hash to copy yet.";
            return;
        }

        Clipboard.SetText(currentHash);
        StatusText.Text = "Hash copied to clipboard.";
    }

    private void ExpectedHashTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        UpdateComparison();
    }

    public void LoadFile(string filePath)
    {
        SelectFile(filePath);
    }

    public void SetExpectedHash(string expectedHash)
    {
        ExpectedHashTextBox.Text = expectedHash;
        UpdateComparison();
    }

    public async Task CalculateHashForSelectedFileAsync(HashAlgorithmKind algorithmKind)
    {
        await CalculateHashAsync(algorithmKind);
    }

    private void SelectFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            StatusText.Text = "That file could not be opened.";
            return;
        }

        selectedFilePath = filePath;
        currentHash = string.Empty;

        var info = new FileInfo(filePath);
        FileNameText.Text = info.Name;
        FileDetailsText.Text = $"{FormatBytes(info.Length)} | Modified {info.LastWriteTime:g}";
        FilePathText.Text = info.FullName;
        HashHeadingText.Text = "Calculated hash";
        HashTextBox.Text = "Choose MD5, SHA1, SHA256, or SHA512.";
        CompareStatusText.Text = "Waiting";
        CompareStatusText.Foreground = System.Windows.Media.Brushes.DimGray;
        StatusText.Text = "File ready.";
    }

    private async Task CalculateHashAsync(HashAlgorithmKind algorithmKind)
    {
        if (selectedFilePath is null)
        {
            return;
        }

        hashCancellation?.Cancel();
        hashCancellation = new CancellationTokenSource();
        var token = hashCancellation.Token;
        var progress = new Progress<double>(value =>
        {
            HashProgressBar.Value = value * 100;
        });

        try
        {
            HashProgressBar.Value = 0;
            HashProgressBar.Visibility = Visibility.Visible;
            StatusText.Text = $"Calculating {algorithmKind}...";
            HashTextBox.Text = string.Empty;

            currentHash = await hashService.ComputeHashAsync(selectedFilePath, algorithmKind, progress, token);
            HashHeadingText.Text = $"{algorithmKind} hash";
            HashTextBox.Text = currentHash;
            StatusText.Text = $"{algorithmKind} complete.";
            UpdateComparison();
        }
        catch (OperationCanceledException)
        {
            StatusText.Text = "Hash calculation canceled.";
        }
        catch (Exception ex)
        {
            currentHash = string.Empty;
            HashTextBox.Text = ex.Message;
            StatusText.Text = "Could not calculate the hash.";
        }
        finally
        {
            HashProgressBar.Visibility = Visibility.Collapsed;
        }
    }

    private void UpdateComparison()
    {
        if (string.IsNullOrWhiteSpace(ExpectedHashTextBox.Text))
        {
            CompareStatusText.Text = "Waiting";
            CompareStatusText.Foreground = System.Windows.Media.Brushes.DimGray;
            return;
        }

        if (string.IsNullOrWhiteSpace(currentHash))
        {
            CompareStatusText.Text = "Calculate first";
            CompareStatusText.Foreground = System.Windows.Media.Brushes.DimGray;
            return;
        }

        var matches = FileHashService.HashesMatch(ExpectedHashTextBox.Text, currentHash);
        CompareStatusText.Text = matches ? "Match" : "Does not match";
        CompareStatusText.Foreground = matches
            ? System.Windows.Media.Brushes.ForestGreen
            : System.Windows.Media.Brushes.Firebrick;
    }

    private static string FormatBytes(long bytes)
    {
        string[] units = ["B", "KB", "MB", "GB", "TB"];
        var value = (double)bytes;
        var index = 0;

        while (value >= 1024 && index < units.Length - 1)
        {
            value /= 1024;
            index++;
        }

        return $"{value:0.##} {units[index]}";
    }
}
