using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;

using System.Threading;

using dotNSASM;

namespace NSASM_GUI
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Util.Output = (value) => OutputBox.Dispatcher.Invoke(() => OutputBox.Text += value);

            HeadVer.Content = "using " + NSASM.Version;
        }

        private void __print(string format, params object[] args)
        {
            string str = string.Format(format, args);
            OutputBox.Text += str;
        }

        private void Head_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }

        private void MainPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
            this.Close();
        }

        private void OutputBox_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            OutputLine.ScrollToVerticalOffset(OutputBox.VerticalOffset);
        }

        private void CodeBox_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            CodeLine.ScrollToVerticalOffset(CodeBox.VerticalOffset);
        }

        private void OutputBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            int lines = OutputBox.Text.Split('\n').Length;
            string str = "";
            for (int i = 0; i < lines; i++)
            {
                str += (i + 1).ToString();
                if (i < lines - 1) str += "\n";
            }
            OutputLine.Text = str;
        }

        private void CodeBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            int lines = CodeBox.Text.Split('\n').Length;
            string str = "";
            for (int i = 0; i < lines; i++)
            {
                str += (i + 1).ToString();
                if (i < lines - 1) str += "\n";
            }
            CodeLine.Text = str;
        }

        private void BtnRun_Click(object sender, RoutedEventArgs e)
        {
            OutputBox.Clear();
            string code = CodeBox.Text;
            new Thread(() => Util.Execute(code)).Start();
        }

        private void BtnMake_Click(object sender, RoutedEventArgs e)
        {
            const int OFFSET = 0x131E40, LENGTH = 0x8000;
            var bin = Properties.Resources.nsasmBin;

            __print("Original size: {0:G} KiB\n", bin.Length / 1024.0F);
            byte[] bytes = new byte[bin.Length];
            bin.Read(bytes, 0, (int)bin.Length);

            byte[] code = InlineUtil.Compile(CodeBox.Text);
            if (code == null)
            {
                __print("Compilation abort.\n");
                return;
            }
            if (code.Length > LENGTH)
            {
                __print("Code is too big.\n");
                return;
            }
            __print("Code size: {0:G} KiB\n", code.Length / 1024.0F);

            try
            {
                Array.Copy(code, 0, bytes, OFFSET, code.Length);
                DateTime now = DateTime.Now;
                string path = "nsasm-inline-" + now.ToBinary().ToString("x") + ".exe";
                if (PathBox.Text != "") path = PathBox.Text;
                File.WriteAllBytes(path, bytes);
                __print("Compilation OK, at file:\n  " + path + "\n");
            }
            catch (Exception ex)
            {
                __print("Caught exception: \n  " + ex.Message + "\n");
                return;
            }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            CodeBox.Clear();
            OutputBox.Clear();
        }
    }
}
