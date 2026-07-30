using TeamX.App.Forms;
using TeamX.App.Services;

namespace TeamX.App;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        try
        {
            using var startup = new StartupService();
            var licenseValid = ValidateLicense(startup);

            if (startup.RequiresUpdate)
            {
                MessageBox.Show(
                    "Uma nova versão do TeamX é necessária para continuar.\n\n" +
                    "Baixe a atualização em teamantilag.com/teamx",
                    "TeamX — Atualização necessária",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            if (licenseValid)
            {
                Application.Run(new Form1());
                return;
            }

            using var activationForm = new MainForm();
            if (activationForm.ShowDialog() != DialogResult.OK)
                return;

            Application.Run(new Form1());
        }
        catch
        {
            MessageBox.Show(
                "Não foi possível iniciar o TeamX. Tente novamente.",
                "TeamX",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);

            Environment.ExitCode = 1;
        }
    }

    private static bool ValidateLicense(StartupService startup)
    {
        try
        {
            return startup.ValidateSavedLicense()
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
        }
        catch
        {
            return false;
        }
    }
}