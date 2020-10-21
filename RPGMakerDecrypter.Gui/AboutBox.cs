using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;
using RPGMakerDecrypter.Decrypter;

namespace RPGMakerDecrypter.Gui
{
    partial class AboutBox : Form
    {
        public AboutBox() => InitializeComponent();

        private void AboutBox_Load(object sender, EventArgs e)
        {
            var gui = Assembly.GetEntryAssembly();
            var lib = Assembly.GetAssembly(typeof(RGSSAD));

            var guiInfo = FileVersionInfo.GetVersionInfo(gui.Location);
            var libInfo = FileVersionInfo.GetVersionInfo(lib.Location);

            versionLabel.Text = $"Version: GUI: {guiInfo.FileVersion}, Library: {libInfo.FileVersion}";
        }
    }
}
