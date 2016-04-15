﻿using Ba2Explorer.Settings;
using System;
using System.IO;
using System.Media;
using System.Windows;
using System.Windows.Controls;

namespace Ba2Explorer.View
{
    /// <summary>
    /// Sound player control. Supports only WAV now.
    /// </summary>
    public partial class SoundPlayerElement : UserControl
    {
        private Stream soundSource;
        public Stream SoundSource
        {
            get
            {
                return soundSource;
            }
            set
            {
                soundSource = value;
                SoundSourceChanged();    
            }
        }

        private SoundPlayer soundPlayer;

        /// <summary>
        /// Raised when SoundSource property changed.
        /// </summary>
        private void SoundSourceChanged()
        {
            soundPlayer.Stop();

            if (soundSource == null)
                return;
            
            soundPlayer.Stream = soundSource;

            // TODO:
            // changing this line to soundPlayer.LoadAsync() can cause troubles
            //
            // stream could be disposed while SoundPlayer consumes it
            // SoundPlayer will throw exception then.
            soundPlayer.Load();

            if (AutoplaySoundCheckbox.IsChecked.HasValue &&
                AutoplaySoundCheckbox.IsChecked.Value == true)
            {
                soundPlayer.Play();
            }
        }

        public SoundPlayerElement()
        {
            InitializeComponent();
            soundPlayer = new SoundPlayer();

            if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            {
                this.AutoplaySoundCheckbox.IsChecked =
                    AppSettings.Instance.FilePreview.SoundPlayerAutoplaySounds;

                AppSettings.Instance.FilePreview.OnSaving += FilePreviewSettingsBeforeSave;
            } 

            SoundSourceChanged(); // No source by default.
        }

        public void StopSound()
        {
            soundPlayer.Stop();
        }

        private void FilePreviewSettingsBeforeSave(object sender, EventArgs e)
        {
            AppSettings.Instance.FilePreview.SoundPlayerAutoplaySounds
                = AutoplaySoundCheckbox.IsChecked.HasValue ? AutoplaySoundCheckbox.IsChecked.Value : false;
        }

        ~SoundPlayerElement()
        {
            if (AppSettings.Instance != null)
                AppSettings.Instance.FilePreview.OnSaving -= FilePreviewSettingsBeforeSave;
        }

        private void PlayButtonClicked(object sender, RoutedEventArgs e)
        {
            soundPlayer.Play();
        }

        private void StopButtonClicked(object sender, RoutedEventArgs e)
        {
            StopSound();
        }
    }
}
