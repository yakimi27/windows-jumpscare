using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Core;
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
        private readonly Loop? _loop;
		private bool _isLoading = true;
        private CancellationTokenSource? _previewCts;
        private FrameCache? _previewCache;
        private long _previewRequestId;

        internal SettingsWindow() : this(new ConfigService())
        {
        }

        internal SettingsWindow(IConfigService configService) 
			: this(configService, new UserManager(configService),
					new JumpscareManager(configService))
        {
        }

        internal SettingsWindow(IConfigService configService, IUserManager userManager, IJumpscareManager jumpscareManager)
			: this(configService, userManager, jumpscareManager, null)
        {
        }

        internal SettingsWindow(IConfigService configService, IUserManager userManager, IJumpscareManager jumpscareManager, Loop? loop)
        {
            _configService = configService;
            _userManager = userManager;
            _jumpscareManager = jumpscareManager;
            _loop = loop;

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
				else
				{
					var fallbackName = _jumpscareManager.GetFirstValidJumpscareName();
					if (!string.IsNullOrEmpty(fallbackName) && JumpscareComboBox.Items.Contains(fallbackName))
					{
						JumpscareComboBox.SelectedItem = fallbackName;
						_userManager.SetSelectedJumpscare(fallbackName);
					}
					else if (JumpscareComboBox.Items.Count > 0)
					{
						JumpscareComboBox.SelectedIndex = 0;
						if (JumpscareComboBox.SelectedItem is string fallbackFromItems)
						{
							_userManager.SetSelectedJumpscare(fallbackFromItems);
						}
					}
				}

				int chance = _userManager.GetJumpscareChance();
				FrequencySlider.Value = MapChanceToSliderValue(chance);

				AutostartSwitch.IsChecked = _userManager.IsAutostartEnabled();

				UpdateCharacterPreview();
			}
			finally
			{
				_isLoading = false;
			}
		}


        private void SaveSelectedJumpscare()
        {
            if (_isLoading) return;

            if (JumpscareComboBox.SelectedItem is string selectedName)
            {
                _userManager.SetSelectedJumpscare(selectedName);
            }
        }

        private void SaveJumpscareChance()
        {
            if (_isLoading) return;

            int chance = MapSliderValueToChance(FrequencySlider.Value);
            _userManager.SetJumpscareChance(chance);
        }

        private async void UpdateCharacterPreview()
        {
            CancellationTokenSource? cts = null;
            FrameCache? localCache = null;
            bool assignedToPreviewCache = false;

            try
            {
                long requestId = ++_previewRequestId;

                try
                {
                    _previewCts?.Cancel();
                }
                catch (ObjectDisposedException)
                {
                }

                cts = new CancellationTokenSource();
                _previewCts = cts;
                var token = cts.Token;

                if (JumpscareComboBox.SelectedItem is not string selectedName)
                {
                    SetPreviewState(null);
                    _previewCache?.Release();
                    _previewCache = null;
                    return;
                }

                var jumpscare = _jumpscareManager.GetByName(selectedName);
                if (jumpscare == null || string.IsNullOrEmpty(jumpscare.AssetsPath))
                {
                    SetPreviewState(null);
                    _previewCache?.Release();
                    _previewCache = null;
                    return;
                }

                int frameIndex = 0;
                int frameDelay = jumpscare.FrameFrequency > 0 ? jumpscare.FrameFrequency : 50;

                localCache = new FrameCache(jumpscare.FrameAmount, jumpscare.AssetsPath, decodeWidth: 280);
                await localCache.PreloadAsync();

                if (token.IsCancellationRequested || _previewCts != cts || requestId != _previewRequestId)
                {
                    return;
                }

                var frames = localCache.Acquire();
                if (frames.Count == 0)
                {
                    SetPreviewState(null);
                    _previewCache?.Release();
                    _previewCache = null;
                    return;
                }

                _previewCache?.Release();
                _previewCache = localCache;
                assignedToPreviewCache = true;

                while (!token.IsCancellationRequested && requestId == _previewRequestId)
                {
                    SetPreviewState(frames[frameIndex]);
                    frameIndex = (frameIndex + 1) % frames.Count;

                    await Task.Delay(frameDelay, token);
                }
            }
            catch (OperationCanceledException)
            {
                // normal cancel
            }
            catch
            {
                SetPreviewState(null);
                _previewCache?.Release();
                _previewCache = null;
            }
            finally
            {
                if (!assignedToPreviewCache)
                {
                    localCache?.Release();
                }

                if (cts != null)
                {
                    if (_previewCts == cts)
                    {
                        _previewCts = null;
                    }
                    cts.Dispose();
                }
            }
        }

        private void SetPreviewState(ImageSource? source)
        {
            CharacterPreviewImage.Source = source;
            PreviewPlaceholderText.Visibility = source != null ? Visibility.Collapsed : Visibility.Visible;
        }
        private static double MapChanceToSliderValue(int chance)
        {
            return chance switch
            {
                >= 874818 => 1,
                >= 599635 => 2,
                >= 349635 => 3,
                _ => 4
            };
        }

        private static int MapSliderValueToChance(double sliderValue)
        {
            return (int)Math.Round(sliderValue) switch
            {
                1 => 1000000,
                2 => Constants.DefaultJumpscareChance,
                3 => 449635,
                4 => 249635,
                _ => Constants.DefaultJumpscareChance
            };
        }

        #endregion

        #region Event Handlers

        private void JumpscareComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isLoading) return;
            UpdateCharacterPreview();
            SaveSelectedJumpscare();
        }

        private void FrequencySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_isLoading) return;
            SaveJumpscareChance();
        }

        private void AutostartSwitch_Checked(object sender, RoutedEventArgs e)
        {
            if (_isLoading) return;
            _userManager.SetAutostart(true);
        }

        private void AutostartSwitch_Unchecked(object sender, RoutedEventArgs e)
        {
            if (_isLoading) return;
            _userManager.SetAutostart(false);
        }

        private async void TestJumpscareButton_Click(object sender, RoutedEventArgs e)
        {
			if(_isLoading) return;
            if (_loop != null)
            {
                TestJumpscareButton.IsEnabled = false;
                JumpscareComboBox.IsEnabled = false;
                try
                {
                    await _loop.Trigger();
                    await Task.Delay(500);
                }
                finally
                {
                    TestJumpscareButton.IsEnabled = true;
                    JumpscareComboBox.IsEnabled = true;
                }
            }
        }

        #endregion

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            try
            {
                _previewCts?.Cancel();
            }
            catch (ObjectDisposedException)
            {
            }
            _previewCts = null;
            _previewCache?.Release();
            _previewCache = null;
        }
    }
}