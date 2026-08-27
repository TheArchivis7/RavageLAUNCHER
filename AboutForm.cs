using System.Drawing;
using System.Windows.Forms;

namespace RavageLauncher;

internal sealed class AboutForm : Form
{
    public AboutForm()
    {
        Text = "About Ravage PVE/*™ Launcher";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        ClientSize = new Size(560, 430);

        BackColor = Color.FromArgb(22, 24, 28);
        ForeColor = Color.Gainsboro;
        Font = new Font("Segoe UI", 9.5f);

        var title = new Label
        {
            Text = "RAVAGE PVE/*™",
            Font = new Font("Segoe UI Semibold", 20f, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(28, 25)
        };

        var subtitle = new Label
        {
            Text = "RAVAGE PVE/*™ Launcher  •  Version 0.3",
            ForeColor = Color.Silver,
            AutoSize = true,
            Location = new Point(31, 68)
        };

        var text = new Label
        {
            Text =
                "No noise. No BS. Just Ravage.\r\n\r\n" +
                "This launcher provides the official mod package used by Ravage PVE /*.\r\n" +
                "Mods are installed temporarily for each game session and removed afterward.\r\n\r\n" +

                "LEGAL\r\n\r\n" +
                "RAVAGE PVE and RAVAGE PVE/* are trademark applications.\r\n" +
                "© 2026 Ravage PVE/*. All rights reserved.\r\n\r\n" +

                "Original Ravage PVE/* modifications, artwork, branding and associated materials " +
                "may not be redistributed, repackaged or presented as another project without permission.\r\n\r\n" +

                "Third-party modifications and assets remain the property of their respective creators " +
                "and are used under their applicable permissions or licenses.\r\n\r\n" +

                "SCUM and other third-party trademarks remain the property of their respective owners. " +
                "Ravage PVE/* is an independent community project and is not affiliated with, endorsed by, or sponsored by Gamepires.",
            ForeColor = Color.Gainsboro,
            AutoSize = false,
            Location = new Point(31, 110),
            Size = new Size(495, 250)
        };

        var closeButton = new Button
        {
            Text = "CLOSE",
            Location = new Point(406, 375),
            Size = new Size(120, 34),
            BackColor = Color.FromArgb(45, 49, 56),
            ForeColor = Color.WhiteSmoke,
            FlatStyle = FlatStyle.Flat,
            DialogResult = DialogResult.OK
        };

        closeButton.FlatAppearance.BorderColor = Color.FromArgb(73, 79, 88);

        Controls.Add(title);
        Controls.Add(subtitle);
        Controls.Add(text);
        Controls.Add(closeButton);

        AcceptButton = closeButton;
        CancelButton = closeButton;
    }
}
