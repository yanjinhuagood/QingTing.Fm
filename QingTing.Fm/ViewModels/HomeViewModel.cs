using MaterialDesignThemes.Wpf;
using NAudio.Wave;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QingTing.Fm.Helpers;
using QingTing.Fm.Models;
using QingTing.Fm.Service;
using QingTing.Fm.Utility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace QingTing.Fm.ViewModels
{
    public class HomeViewModel : ViewModelBase
    {
        #region 常量
        const double SliderMax = 10.0;
        #endregion

        #region 字段
        private IWavePlayer wavePlayer;
        private WaveStream reader;
        private readonly DispatcherTimer timer = new DispatcherTimer();
        private string lastPlayed;
        private string inputPath;
        private double sliderPosition;
        ProgramsModel pModel;
        int currentNumber = 0;
        HttpProgram http;
        int programCount, pageCount;
        int pageIndex = 1;
        #endregion

        #region 构造函数
        public HomeViewModel()
        {
            //HttpHelper.GetWebAsync("https://i.qingting.fm/wapi/channels/239329/programs/page/1/pagesize/10");
            http = new HttpProgram();
            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += TimerOnTick;
            StartProgramTask(pageIndex);
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
        public double SliderPosition
        {
            get => sliderPosition;
            set
            {
                if (sliderPosition != value)
                {
                    sliderPosition = value;
                    if (reader != null)
                    {
                        var pos = (long)(reader.Length * sliderPosition / SliderMax);
                        reader.Position = pos; // media foundation will worry about block align for us
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

        public bool _isLoginBusy;
        /// <summary>
        /// 消息是否loading
        /// </summary>
        public bool IsLoginBusy
        {
            get
            {
                return _isLoginBusy;
            }
            set
            {
                _isLoginBusy = value;
                OnPropertyChanged(nameof(IsLoginBusy));
            }
        }

        /// <summary>
        /// 提示消息
        /// </summary>
        private string _cessage;
        /// <summary>
        /// 提示消息
        /// </summary>
        public string Message
        {
            get { return _cessage; }
            set
            {
                if (value != _cessage)
                {
                    _cessage = value;
                    OnPropertyChanged(nameof(Message));
                }
            }
        }

        /// <summary>
        /// 当前页
        /// </summary>
        private int _pageIndexProperty;
        /// <summary>
        /// 当前页
        /// </summary>
        public int PageIndexProperty
        {
            get { return _pageIndexProperty; }
            set
            {
                if (value != _pageIndexProperty)
                {
                    _pageIndexProperty = value;
                    OnPropertyChanged(nameof(PageIndexProperty));
                }
            }
        }

        /// <summary>
        /// 总页
        /// </summary>
        private int _pageCountProperty;
        /// <summary>
        /// 总页
        /// </summary>
        public int PageCountProperty
        {
            get { return _pageCountProperty; }
            set
            {
                if (value != _pageCountProperty)
                {
                    _pageCountProperty = value;
                    OnPropertyChanged(nameof(PageCountProperty));
                }
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
        /// 双击播放
        /// </summary>
        //public ICommand MouseDoubleCommand => new DelegateCommand(obj =>
        //{
        //    if (obj != null)
        //    {
        //        if (pModel != obj as ProgramsModel)
        //        {
        //            if (pModel!=null)
        //            {
        //                pModel.IsPlay = false;
        //            }
        //            pModel = obj as ProgramsModel;
        //            if (!string.IsNullOrEmpty(pModel.FilePath))
        //            {
        //                Stop();
        //                Programs.All(y => y.IsPlay = false);
        //                pModel.IsPlay = true;
        //                inputPath = string.Format("https://od.qingting.fm/{0}", pModel.FilePath);
        //                Play();
        //            }

        //        }

        //    }
        //});

        /// <summary>
        /// 播放
        /// </summary>
        public ICommand PlayCommand => new DelegateCommand(obj =>
        {
            PlayMethod();
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
            PlayMethod((int)obj);
        });
        /// <summary>
        /// 上
        /// </summary>
        public ICommand SkipSongCommand => new DelegateCommand(obj =>
        {
            //currentNumber = currentNumber == 0 ? Programs.Count - 1 : currentNumber--;
            if (currentNumber == 0)
            {
                currentNumber = Programs.Count - 1;
            }
            else
            {
                currentNumber--;
            }
            PlayMethod();
        });
        /// <summary>
        /// 下
        /// </summary>
        public ICommand NextSongCommand => new DelegateCommand(obj =>
        {
            if (IsPlay)
            {
                //currentNumber = currentNumber == Programs.Count ? 0 : currentNumber++;
                if (currentNumber == Programs.Count - 1)
                {
                    currentNumber = 0;
                }
                else
                {
                    currentNumber++;
                }
            }
            PlayMethod(currentNumber);
        });
        public ICommand FetchMoreDataCommand => new DelegateCommand(obj =>
        {
            pageIndex++;
            if (pageIndex >= pageCount)
            {
                Message = "已经没有更多了...";
                LoadingShow();
                return;
            }
            StartProgramTask(pageIndex);
        });

        /// <summary>
        /// 点击页
        /// </summary>
        public ICommand NextPageCommand => new DelegateCommand(obj =>
        {
            StartProgramTask(PageIndexProperty);
        });

        #endregion

        #region 方法
        /// <summary>
        /// 基本信息
        /// </summary>
        private async void StartProgramTask(int page)
        {
            IsLoginBusy = true;
            try
            {
                if (page.Equals(1))
                {
                    Channels = http.GetChannels();
                    //await Task.Delay(1000);
                    programCount = Convert.ToInt32(Channels.ProgramCount);
                    PageIndexProperty = 1;
                    pageCount = programCount / 10;
                    PageCountProperty = pageCount;
                    Title = Channels.Name;
                }
                if (Programs != null && Programs.Count >0)
                {
                    ObservableCollection<ProgramsModel> list = await http.GetShowList(page);
                    Programs = list;
                }
                else
                {
                    Programs = await http.GetShowList(page);
                }
                //System.Threading.Thread.Sleep(1000);
                IsLoginBusy = false;
            }
            catch (Exception ex)
            {
                IsLoginBusy = false;
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
            if (reader != null)
            {
                sliderPosition = Math.Min(SliderMax, reader.Position * SliderMax / reader.Length);
                CurrentTime = reader.CurrentTime.ToString((@"hh\:mm\:ss"));
                OnPropertyChanged("SliderPosition");
            }
        }

        private void PlayMethod(object obj = null)
        {
            if (obj != null)
            {
                currentNumber = (int)obj;
                if (string.IsNullOrEmpty(Programs[currentNumber].FilePath))
                {
                    Message = "此音频为付费音频，已跳过";
                    LoadingShow();
                    currentNumber++;
                }
            }
            pModel = Programs[currentNumber];
            Stop();
            Programs.All(y => { y.IsPlay = false; return true; });
            Title = pModel.Name;
            pModel.IsPlay = true;
            Play();
        }

        //https://od.qingting.fm/m4a/5a83b6a87cb89146f0e31f5a_8746252_64.m4a   m4a/5a78127c7cb89146f209c84f_8688485_64.m4a
        /// <summary>
        /// 创建播放器
        /// </summary>
        private void CreatePlayer()
        {
            wavePlayer = new WaveOutEvent();
            wavePlayer.PlaybackStopped += WavePlayerOnPlaybackStopped;
        }
        /// <summary>
        /// 播放
        /// </summary>
        private void Play()
        {
            if (pModel == null)
            {
                pModel = Programs[currentNumber];
            }
            IsPlay = true;
            //if (String.IsNullOrEmpty(InputPath))
            //{
            //    MessageBox.Show("Select a valid input file or URL first");
            //    return;
            //}
            if (wavePlayer == null)
            {
                CreatePlayer();
            }
            if (lastPlayed != inputPath && reader != null)
            {
                reader.Dispose();
                reader = null;
            }
            if (reader == null)
            {
                inputPath = string.Format("https://od.qingting.fm/{0}", pModel.FilePath);
                reader = new MediaFoundationReader(inputPath);
                TotalTime = reader.TotalTime.ToString((@"hh\:mm\:ss"));
                lastPlayed = inputPath;
                wavePlayer.Init(reader);
            }
            wavePlayer.Play();
            OnPropertyChanged("IsPlaying");
            OnPropertyChanged("IsStopped");
            timer.Start();
        }
        /// <summary>
        /// 停止
        /// </summary>
        private void Stop()
        {
            if (wavePlayer != null)
            {
                wavePlayer.Stop();
                inputPath = null;
                SliderPosition = 0;
                timer.Stop();
                IsPlay = false;
            }
        }
        /// <summary>
        /// 暂停
        /// </summary>
        private void Pause()
        {
            if (wavePlayer != null)
            {
                IsPlay = false;
                wavePlayer.Pause();
                OnPropertyChanged("IsPlaying");
                OnPropertyChanged("IsStopped");
            }
        }
        private void WavePlayerOnPlaybackStopped(object sender, StoppedEventArgs stoppedEventArgs)
        {
            if (wavePlayer.PlaybackState == PlaybackState.Stopped)
            {
                inputPath = null;
                pModel.IsPlay = false;
                SliderPosition = 0;
                timer.Stop();
                NextSongCommand.Execute(null);
            }
            //if (reader != null)
            //{
            //    SliderPosition = 0;
            //    timer.Stop();
            //}
            if (stoppedEventArgs.Exception != null)
            {
                MessageBox.Show(stoppedEventArgs.Exception.Message, "Error Playing File");
            }
            OnPropertyChanged("IsPlaying");
            OnPropertyChanged("IsStopped");
        }

        #endregion

    }
}
