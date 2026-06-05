using SimpleHashChecker.Core;
using System.IO;
using System.Windows;

namespace SimpleHashChecker.App;

public partial class App : Application
{
    private async void Application_Startup(object sender, StartupEventArgs e)
    {
        var window = new MainWindow();
        window.Show();

        if (e.Args.Length == 0)
        {
            return;
        }

        var filePath = e.Args[0];
        if (File.Exists(filePath))
        {
            window.LoadFile(filePath);
        }

        var hashArg = e.Args.Skip(1).FirstOrDefault();
        if (hashArg is not null && Enum.TryParse<HashAlgorithmKind>(hashArg, true, out var algorithmKind))
        {
            await window.CalculateHashForSelectedFileAsync(algorithmKind);
        }

        var expectedHash = e.Args.Skip(2).FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(expectedHash))
        {
            window.SetExpectedHash(expectedHash);
        }
    }
}
