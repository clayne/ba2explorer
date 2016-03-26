﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Ba2Explorer.ViewModel;
using System.Diagnostics;
using System.ComponentModel;

namespace Ba2Explorer
{
    /// <summary>
    /// Логика взаимодействия для FileExtractionWindow.xaml
    /// </summary>
    public partial class FileExtractionWindow : Window
    {
        public FileExtractionViewModel ViewModel;

        public FileExtractionWindow()
        {
            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            ViewModel = (FileExtractionViewModel)DataContext;
            ViewModel.ExtractionProgress.ProgressChanged += ExtractionProgress_ProgressChanged;

            Debug.WriteLine("file extr activated");
            ViewModel.ExtractFiles();

            base.OnInitialized(e);
        }

        private void ExtractionProgress_ProgressChanged(object sender, int e)
        {
            Debug.WriteLine("progress changed " + e);
            ExtractionProgress.Value = (double)e / ViewModel.FilesToExtract.Count();
        }

        private void CanStopExtraction(object sender, CanExecuteRoutedEventArgs e)
        {
            if (ViewModel?.ArchiveInfo == null)
                return;

            e.CanExecute = ViewModel.IsExtracting;
            e.Handled = true;
        }

        private void CanOpenFolder(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void OpenFolderExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Process.Start(ViewModel.DestinationFolder);
        }

        private void StopExtraction(object sender, ExecutedRoutedEventArgs e)
        {
            Debug.WriteLine("stop extr");
            this.IsEnabled = false;
            Cancel.Content = "Canceling...";

            ViewModel.Cancel();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            // TODO hide close button on window.
            if (ViewModel.IsExtracting)
                e.Cancel = true;
            else
                e.Cancel = false;
        }
    }
}