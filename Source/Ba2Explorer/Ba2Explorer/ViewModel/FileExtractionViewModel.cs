﻿using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Ba2Explorer.ViewModel
{
    public class FileExtractionViewModel : ViewModelBase
    {
        private ArchiveInfo archiveInfo;
        public ArchiveInfo ArchiveInfo
        {
            get { return archiveInfo; }
            set
            {
                archiveInfo = value;
                RaisePropertyChanged();
            }
        }

        public Progress<int> ExtractionProgress { get; set; }

        private bool isExtracting = false;
        public bool IsExtracting
        {
            get { return isExtracting; }
            set
            {
                isExtracting = value;
                RaisePropertyChanged();
            }
        }

        private string destinationFolder = "";
        public string DestinationFolder
        {
            get { return destinationFolder; }
            set
            {
                destinationFolder = value;
                RaisePropertyChanged();
            }
        }

        private IEnumerable<string> filesToExtract = null;
        public IEnumerable<string> FilesToExtract
        {
            get { return filesToExtract; }
            set
            {
                filesToExtract = value;
                RaisePropertyChanged();
            }
        }

        private CancellationTokenSource cancellationToken;

        public FileExtractionViewModel()
        {
            ExtractionProgress = new Progress<int>();
            cancellationToken = new CancellationTokenSource();
        }

        public void Cancel()
        {
            Contract.Ensures(IsExtracting == true);

            cancellationToken.Cancel();
        }

        public void ExtractFiles()
        {
            Debug.WriteLine("begin extr");
            try
            {
                var task = Task.Run(() =>
                {
                    ArchiveInfo.ExtractFiles(FilesToExtract, DestinationFolder, cancellationToken.Token, ExtractionProgress);
                });
            }
            catch (OperationCanceledException)
            {
                // todo;
                MessageBox.Show("cancelled.");
            }
        }
    }
}
