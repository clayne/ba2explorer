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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Ba2Explorer.ViewModel;
using Microsoft.Win32;
using System.Collections;

namespace Ba2Explorer
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainViewModel mainViewModel;

        public MainWindow()
        {
            InitializeComponent();

            mainViewModel = (MainViewModel)DataContext;
        }

        private void OpenCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            mainViewModel.ArchiveInfo.OpenWithDialog();
            e.Handled = true;
        }

        private void ExtractCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (!mainViewModel.ArchiveInfo.IsOpened)
            {
                e.CanExecute = false;
                e.Handled = true;
                return;
            }

            e.CanExecute = ArchiveFilesList.SelectedIndex != -1;
            e.Handled = true;
        }

        private void ExtractSingleCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            string sel = ArchiveFilesList.SelectedItem as string;

            mainViewModel.ArchiveInfo.ExtractSingle(sel);
            e.Handled = true;
        }

        private void ExtractAllCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            IList sels = ArchiveFilesList.SelectedItems;

            mainViewModel.ArchiveInfo.ExtractFiles(sels.OfType<string>().ToList());
            e.Handled = true;
        }
    }
}
