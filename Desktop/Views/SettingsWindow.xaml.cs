using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Core.Interfaces;
using Core.Managers;
using Core.Services;

namespace Desktop.Views
{
    public partial class SettingsWindow : Window
    {
        private readonly IConfigService _configService;
        private readonly IUserManager _userManager;
        private readonly IJumpscareManager _jumpscareManager;
		private bool _isLoading;

        internal SettingsWindow() : this(new ConfigService())
        {
        }

        internal SettingsWindow(IConfigService configService) 
			: this(configService, new UserManager(configService),
					new JumpscareManager(configService))
        {
        }

        internal SettingsWindow(IConfigService configService, IUserManager userManager, IJumpscareManager jumpscareManager)
        {
            _configService = configService;
            _userManager = userManager;
            _jumpscareManager = jumpscareManager;

            InitializeComponent();
            InitializeControls();
            LoadSettings();
        }

        #region Initialization & Settings Helper Scaffolds

        private void InitializeControls()
        {
            JumpscareComboBox.Items.Clear();
            var jumpscares = _jumpscareManager.GetAll();
            foreach (var jumpscare in jumpscares)
            {
                JumpscareComboBox.Items.Add(jumpscare.Name);
            }
        }

		private void LoadSettings(){

			_isLoading = true;

			try
			{
				var selectedJumpscare = _userManager.GetSelectedJumpscare();
				if (!string.IsNullOrEmpty(selectedJumpscare) && 
						JumpscareComboBox.Items.Contains(selectedJumpscare))
				{
					JumpscareComboBox.SelectedItem = selectedJumpscare;
				}
				else if (JumpscareComboBox.Items.Count > 0)
				{
					JumpscareComboBox.SelectedIndex = 0;
				}

				ushort chance = _userManager.GetJumpscareChance();
				FrequencySlider.Value = MapChanceToSliderValue(chance);

				UpdateCharacterPreview();
			}
			finally
			{
				_isLoading = false;
			}
		}


        private void SaveSettings()
        {
			if(_isLoading) return;

            if (JumpscareComboBox.SelectedItem is string selectedName)
            {
                _userManager.SetSelectedJumpscare(selectedName);
            }

            ushort chance = MapSliderValueToChance(FrequencySlider.Value);
            _userManager.SetJumpscareChance(chance);
        }

		private void UpdateCharacterPreview()
		{
			if (JumpscareComboBox.SelectedItem is string selectedName)
			{
				var jumpscare = _jumpscareManager.GetByName(selectedName);
				if (jumpscare != null && !string.IsNullOrEmpty(jumpscare.AssetsPath))
				{
					string previewPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, jumpscare.AssetsPath, "10.png");

					if (File.Exists(previewPath))
					{
						try
						{
							var bitmap = new BitmapImage();
							bitmap.BeginInit();
							bitmap.UriSource = new Uri(previewPath, UriKind.RelativeOrAbsolute);
							bitmap.CacheOption = BitmapCacheOption.OnLoad;
							bitmap.EndInit();
							bitmap.Freeze();

							CharacterPreviewImage.Source = bitmap;
							return;
						}
						catch
						{
							// ignore
						}
					}
				}
			}

			CharacterPreviewImage.Source = null;
		}

        private static double MapChanceToSliderValue(ushort chance)
        {
            return chance switch
            {
                >= 49152 => 1,
                >= 32768 => 2,
                >= 16384 => 3,
                _ => 4
            };
        }

        private static ushort MapSliderValueToChance(double sliderValue)
        {
            return (int)Math.Round(sliderValue) switch
            {
                1 => 65535,
                2 => 32768,
                3 => 16384,
                4 => 1,
                _ => 32768
            };
        }

        #endregion

        #region Event Handlers

        private void JumpscareComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateCharacterPreview();
            SaveSettings();
        }

        private void FrequencySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SaveSettings();
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
			if(_isLoading) return;
            // Scaffold: Handle volume slider value change
        }

        private void MuteSwitch_Checked(object sender, RoutedEventArgs e)
        {
			if(_isLoading) return;
            // Scaffold: Handle mute toggle checked
        }

        private void MuteSwitch_Unchecked(object sender, RoutedEventArgs e)
        {
			if(_isLoading) return;
            // Scaffold: Handle mute toggle unchecked
        }

        private void AutostartSwitch_Checked(object sender, RoutedEventArgs e)
        {
			if(_isLoading) return;
            // Scaffold: Handle autostart switch checked
        }

        private void AutostartSwitch_Unchecked(object sender, RoutedEventArgs e)
        {
			if(_isLoading) return;
            // Scaffold: Handle autostart switch unchecked
        }

        private void TestJumpscareButton_Click(object sender, RoutedEventArgs e)
        {
			if(_isLoading) return;
            // Scaffold: Handle test jumpscare button click
        }

        #endregion
    }
}
 
