using NAudio.Wave;
using QingTing.Fm.Models;
using QingTing.Fm.Service;
using QingTing.Fm.Utility;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using WPFDevelopers.Controls;
using WPFDevelopers.Helpers;

namespace QingTing.Fm.ViewModels
{
    public class HomeViewModel : ViewModelBase
    {
        #region 常量
        const double SliderMax = 10.0;
        #endregion

        #region 字段
        private IWavePlayer _wavePlayer;
        private WaveStream _reader;
        private readonly DispatcherTimer _timer;
        private string _lastPlayed;
        private string _inputPath;
        private double _sliderPosition;
        private int _currentNumber = 0;
        private string _lastTempFilePath = null;
        #endregion

        #region 构造函数
        public HomeViewModel()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(500);
            _timer.Tick += TimerOnTick;
            StartProgramTask(1);
        }

        #endregion

        #region 属性
        /// <summary>
        /// 获取或设置内容标题
        /// </summary>
        private string _title;
        /// <summary>
        /// 获取或设置内容标题
        /// </summary>
        public string Title
        {
            get { return _title; }
            set
            {
                if (value != _title)
                {
                    _title = value;
                    OnPropertyChanged(nameof(Title));
                }
            }
        }
        private ObservableCollection<ProgramsModel> programs;
        /// <summary>
        /// 节目集合
        /// </summary>
        public ObservableCollection<ProgramsModel> Programs
        {
            get
            {
                if (programs == null)
                {
                    programs = new ObservableCollection<ProgramsModel>();
                }
                return programs;
            }
            set
            {
                programs = value;
                OnPropertyChanged(nameof(Programs));
            }
        }
        private ProgramsModel channels;
        /// <summary>
        /// 节目信息
        /// </summary>
        public ProgramsModel Channels
        {
            get
            {
                if (channels == null)
                {
                    channels = new ProgramsModel();
                }
                return channels;
            }
            set
            {
                channels = value;
                OnPropertyChanged(nameof(Channels));
            }
        }

        private ProgramsModel _podcaster;
        /// <summary>
        /// 当前播放
        /// </summary>
        public ProgramsModel Podcaster
        {
            get
            {
                return _podcaster;
            }
            set
            {
                _podcaster = value;
                OnPropertyChanged(nameof(Podcaster));
            }
        }

        public double SliderPosition
        {
            get => _sliderPosition;
            set
            {
                if (_sliderPosition != value)
                {
                    _sliderPosition = value;
                    if (_reader != null)
                    {
                        var pos = (long)(_reader.Length * _sliderPosition / SliderMax);
                        _reader.Position = pos; // media foundation will worry about block align for us
                    }
                    OnPropertyChanged("SliderPosition");
                }
            }
        }
        public bool _isPlay;
        /// <summary>
        /// 是否播放
        /// </summary>
        public bool IsPlay
        {
            get
            {
                return _isPlay;
            }
            set
            {
                _isPlay = value;
                OnPropertyChanged(nameof(IsPlay));
            }
        }
        public bool _isActive;
        /// <summary>
        /// 消息是否显示
        /// </summary>
        public bool IsActive
        {
            get
            {
                return _isActive;
            }
            set
            {
                _isActive = value;
                OnPropertyChanged(nameof(IsActive));
            }
        }

        /// <summary>
        /// 总时长
        /// </summary>
        private string _totalTime = "00:00";
        /// <summary>
        /// 总时长
        /// </summary>
        public string TotalTime
        {
            get { return _totalTime; }
            set
            {
                if (value != _totalTime)
                {
                    _totalTime = value;
                    OnPropertyChanged(nameof(TotalTime));
                }
            }
        }

        /// <summary>
        /// 总时播放
        /// </summary>
        private string _currentTime = "00:00";
        /// <summary>
        /// 当前播放
        /// </summary>
        public string CurrentTime
        {
            get { return _currentTime; }
            set
            {
                if (value != _currentTime)
                {
                    _currentTime = value;
                    OnPropertyChanged(nameof(CurrentTime));
                }
            }
        }

        private int _count;
        public int Count
        {
            get { return _count; }
            set 
            { 
                _count = value; 
                OnPropertyChanged(nameof(Count));  
            }
        }

        private int _countPerPage = 30;
        public int CountPerPage
        {
            get { return _countPerPage; }
            set 
            { 
                _countPerPage = value; 
                OnPropertyChanged(nameof(CountPerPage));
            }
        }

        private int _current = 1;
        public int Current
        {
            get { return _current; }
            set 
            { 
                _current = value; 
                OnPropertyChanged(nameof(Current));
                StartProgramTask(Current);
            }
        }

        #endregion

        #region 命令 

        /// <summary>
        /// 最小化操作
        /// </summary>    
        public ICommand MinimizeCommand => new DelegateCommand(obj =>
        {
            App.Current.MainWindow.WindowState = WindowState.Minimized;
        });
        /// <summary>
        /// 最大化or还原操作
        /// </summary>    
        public ICommand MaximizeCommand => new DelegateCommand(obj =>
        {
            if (App.Current.MainWindow.WindowState == WindowState.Maximized)
            {
                App.Current.MainWindow.WindowState = WindowState.Normal;
            }
            else
            {
                App.Current.MainWindow.WindowState = WindowState.Maximized;
            }

        });
        /// <summary>
        /// 关闭操作
        /// </summary>    
        public ICommand CloseCommand => new DelegateCommand(obj =>
        {
            Stop();
            App.Current.MainWindow.Close();
        });

        /// <summary>
        /// 播放
        /// </summary>
        public ICommand PlayCommand => new DelegateCommand(obj =>
        {
            PlayMethodAsync();
        });

        /// <summary>
        /// 暂停
        /// </summary>
        public ICommand PauseCommand => new DelegateCommand(obj =>
        {
            Pause();
        });


        /// <summary>
        /// 双击播放
        /// </summary>
        public ICommand MouseDoubleCommand => new DelegateCommand(obj =>
        {
            PlayMethodAsync();
        });
        /// <summary>
        /// 上
        /// </summary>
        public ICommand SkipSongCommand => new DelegateCommand(obj =>
        {
            if (_currentNumber == 0)
                _currentNumber = Programs.Count - 1;
            else
                _currentNumber--;
            Podcaster = null;
            PlayMethodAsync();
        });
        /// <summary>
        /// 下
        /// </summary>
        public ICommand NextSongCommand => new DelegateCommand(obj =>
        {
            if (IsPlay)
            {
                if (_currentNumber == Channels.Podcasters.Count - 1)
                    _currentNumber = 0;
                else
                    _currentNumber++;
            }
            Podcaster = null;
            PlayMethodAsync();
        });

        #endregion

        #region 方法
        /// <summary>
        /// 基本信息
        /// </summary>
        private async void StartProgramTask(int page)
        {
            try
            {
                Channels = await HttpProgram.GetChannelsAsync(page);
                Title = Channels.Name;
            }
            catch (Exception ex)
            {
            }
        }
        private void LoadingShow()
        {
            IsActive = true;
            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(2500);
            }).ContinueWith(t =>
            {
                IsActive = false;

            }, TaskScheduler.FromCurrentSynchronizationContext());
        }
        /// <summary>
        /// 时间
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void TimerOnTick(object sender, EventArgs eventArgs)
        {
            if (_reader != null)
            {
                _sliderPosition = Math.Min(SliderMax, _reader.Position * SliderMax / _reader.Length);
                CurrentTime = _reader.CurrentTime.ToString((@"hh\:mm\:ss"));
                OnPropertyChanged("SliderPosition");
            }
        }

        private async Task PlayMethodAsync()
        {
            if (Podcaster != null)
            {
                if (string.IsNullOrEmpty(Podcaster.FilePath))
                {
                    Message.Push("此音频为付费音频，已跳过...", MessageBoxImage.Information);
                    _currentNumber++;
                }
            }
            else
                Podcaster = Channels.Podcasters[_currentNumber];
            if (Podcaster == null) return;
            if (string.IsNullOrWhiteSpace(_lastTempFilePath))
                await IconicThumbnail(Channels.ImageUrl);
            Stop();
            Channels.Podcasters.All(y => { y.IsPlay = false; return true; });
            Title = Podcaster.Name;
            Podcaster.IsPlay = true;
            Play();
        }

        //https://od.qingting.fm/m4a/5a83b6a87cb89146f0e31f5a_8746252_64.m4a   m4a/5a78127c7cb89146f209c84f_8688485_64.m4a
        /// <summary>
        /// 创建播放器
        /// </summary>
        private void CreatePlayer()
        {
            _wavePlayer = new WaveOutEvent();
            _wavePlayer.PlaybackStopped += WavePlayerOnPlaybackStopped;
        }
        /// <summary>
        /// 播放
        /// </summary>
        private void Play()
        {
            try
            {
                if (Podcaster == null)
                    return;
                IsPlay = true;

                if (_wavePlayer == null)
                {
                    CreatePlayer();
                }
                if (_lastPlayed != _inputPath && _reader != null)
                {
                    _reader.Dispose();
                    _reader = null;
                }
                if (_reader == null)
                {
                    _reader = new MediaFoundationReader(Podcaster.FilePath);
                    TotalTime = _reader.TotalTime.ToString((@"hh\:mm\:ss"));
                    _lastPlayed = _inputPath;
                    _wavePlayer.Init(_reader);
                }
                _wavePlayer.Play();
                OnPropertyChanged("IsPlaying");
                OnPropertyChanged("IsStopped");
                _timer.Start();
            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// 停止
        /// </summary>
        private void Stop()
        {
            if (_wavePlayer != null)
            {
                _wavePlayer.Stop();
                _inputPath = null;
                SliderPosition = 0;
                _timer.Stop();
                IsPlay = false;
            }
        }
        /// <summary>
        /// 暂停
        /// </summary>
        private void Pause()
        {
            if (_wavePlayer != null)
            {
                IsPlay = false;
                _wavePlayer.Pause();
                OnPropertyChanged("IsPlaying");
                OnPropertyChanged("IsStopped");
            }
        }
        private void WavePlayerOnPlaybackStopped(object sender, StoppedEventArgs stoppedEventArgs)
        {
            if (_wavePlayer.PlaybackState == PlaybackState.Stopped)
            {
                _inputPath = null;
                SliderPosition = 0;
                _timer.Stop();
                NextSongCommand.Execute(null);
            }
            if (stoppedEventArgs.Exception != null)
            {
                Message.Push("Error Playing File", MessageBoxImage.Error);
            }
            OnPropertyChanged("IsPlaying");
            OnPropertyChanged("IsStopped");
        }

        async Task IconicThumbnail(string imageUrl)
        {
            if (_lastTempFilePath != null && File.Exists(_lastTempFilePath))
            {
                try
                {
                    File.Delete(_lastTempFilePath);
                }
                catch (Exception)
                {
                }
            }
            if (string.IsNullOrWhiteSpace(imageUrl))
                return;
            string tempFilePath = Path.GetTempFileName();
            _lastTempFilePath = tempFilePath;
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    using (var response = await client.GetAsync(imageUrl))
                    {
                        response.EnsureSuccessStatusCode();
                        using (var stream = await response.Content.ReadAsStreamAsync())
                        using (var fileStream = File.Create(tempFilePath))
                        {
                            await stream.CopyToAsync(fileStream);
                        }
                    }
                }
                Application.Current.MainWindow.SetIconicThumbnail(tempFilePath);
            }
            catch (Exception)
            {
                if (File.Exists(tempFilePath))
                    File.Delete(tempFilePath);
            }
            finally
            {
            }
        }
        
        #endregion
    }
}
