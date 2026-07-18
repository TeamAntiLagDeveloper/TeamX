using TeamX.App.Forms;
using TeamX.App.Services;

namespace TeamX.App;

internal static class Program
{
    [STAThread]
    static async Task Main()
    {
        ApplicationConfiguration.Initialize();

        try
        {
            var startup = new StartupService();

            // Segurança desativada temporariamente durante o desenvolvimento.
            // startup.RunSecurityChecks();

            bool licenseValid = false;

            try
            {
                licenseValid = await startup.ValidateSavedLicense();
            }
            catch
            {
                licenseValid = false;
            }

            if (licenseValid)
            {
                Application.Run(new Form1());
                return;
            }

            using var activationForm = new MainForm();

            if (activationForm.ShowDialog() == DialogResult.OK)
            {
                Application.Run(new Form1());
            }

            // Se o usuário fechar a tela de ativação,
            // a aplicação apenas termina naturalmente.
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Falha crítica ao iniciar o TeamX:\n\n{ex}",
                "TeamX",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}