using System;
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

namespace QingTing.Fm.Views
{
    /// <summary>
    /// PageControl.xaml 的交互逻辑
    /// </summary>
    public partial class PageControl : UserControl
    {

        public static RoutedEvent NextPageEvent;

        public event RoutedEventHandler NextPage
        {
            add { AddHandler(NextPageEvent, value); }
            remove { RemoveHandler(NextPageEvent, value); }
        }

        /// <summary>
        /// 总页数
        /// </summary>
        public int PageCountProperty
        {
            get { return (int)GetValue(PageCountPropertyProperty); }
            set { SetValue(PageCountPropertyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PageCountProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PageCountPropertyProperty =
            DependencyProperty.Register("PageCountProperty", typeof(int), typeof(PageControl), new PropertyMetadata(0));
        //int pageIndex = 1, maxPageInterval=5;

        /// <summary>
        /// 当前页
        /// </summary>
        public int PageIndexProperty
        {
            get { return (int)GetValue(PageIndexPropertyProperty); }
            set { SetValue(PageIndexPropertyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PageIndexProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PageIndexPropertyProperty =
            DependencyProperty.Register("PageIndexProperty", typeof(int), typeof(PageControl), new PropertyMetadata(0));

        /// <summary>
        ///     表示当前选中的按钮距离左右两个方向按钮的最大间隔（4表示间隔4个按钮，如果超过则用省略号表示）
        /// </summary>       
        public static readonly DependencyProperty MaxPageIntervalProperty = DependencyProperty.Register(
            "MaxPageInterval", typeof(int), typeof(PageControl), new PropertyMetadata(3, (o, args) =>
            {
                if (o is PageControl pageControl)
                {
                    pageControl.Update();
                }
            }), value =>
            {
                var intValue = (int)value;
                return intValue >= 0;
            });

        /// <summary>
        ///     表示当前选中的按钮距离左右两个方向按钮的最大间隔（4表示间隔4个按钮，如果超过则用省略号表示）
        /// </summary>   
        public int MaxPageInterval
        {
            get => (int)GetValue(MaxPageIntervalProperty);
            set => SetValue(MaxPageIntervalProperty, value);
        }

        public PageControl()
        {
            InitializeComponent();
            //PageIndexProperty = 1;
            //PageCountProperty = 30;
            NextPageEvent = EventManager.RegisterRoutedEvent("NextPage", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(PageControl));
            this.Loaded += PageControl_Loaded;
        }

        private void PageControl_Loaded(object sender, RoutedEventArgs e)
        {
            Update();
        }


        #region 方法
        private void Update()
        {
            _buttonLeft.IsEnabled = PageIndexProperty > 1;
            _buttonRight.IsEnabled = PageIndexProperty < PageCountProperty;
           
            if (MaxPageInterval == 0)
            {
                _buttonFirst.Visibility = Visibility.Collapsed;
                _buttonLast.Visibility = Visibility.Collapsed;
                _textBlockLeft.Visibility = Visibility.Collapsed;
                _textBlockRight.Visibility = Visibility.Collapsed;
                _panelMain.Children.Clear();
                var selectButton = CreateButton(PageIndexProperty);
                selectButton.Click += SelectButton_Click;
                _panelMain.Children.Add(selectButton);
                selectButton.IsChecked = true;
                return;
            }
            _buttonFirst.Visibility = Visibility.Visible;
            _buttonLast.Visibility = Visibility.Visible;
            _textBlockLeft.Visibility = Visibility.Visible;
            _textBlockRight.Visibility = Visibility.Visible;
            //更新最后一页
            if (PageCountProperty == 1)
            {
                _buttonLast.Visibility = Visibility.Collapsed;
            }
            else
            {
                _buttonLast.Visibility = Visibility.Visible;
                _buttonLast.Tag = PageCountProperty.ToString();
            }


            //更新省略号
            var right = PageCountProperty - PageIndexProperty;
            var left = PageIndexProperty - 1;

            //更新中间部分
            _panelMain.Children.Clear();
            if (PageIndexProperty > 1 && PageIndexProperty < PageCountProperty)
            {
                var selectButton = CreateButton(PageIndexProperty);
                selectButton.Click += SelectButton_Click;
                _panelMain.Children.Add(selectButton);
                selectButton.IsChecked = true;
            }
            else if (PageIndexProperty == 1)
            {
                _buttonFirst.IsChecked = true;
            }
            else
            {
                _buttonLast.IsChecked = true;
            }

            var sub = PageIndexProperty;
            for (int i = 0; i < MaxPageInterval - 1; i++)
            {
                if (--sub > 1)
                {
                    var selectButton = CreateButton(sub);
                    selectButton.Click += SelectButton_Click;
                    _panelMain.Children.Insert(0, selectButton);
                }
                else
                {
                    break;
                }
            }
            var add = PageIndexProperty;
            for (int i = 0; i < MaxPageInterval - 1; i++)
            {
                if (++add < PageCountProperty)
                {
                    var selectButton = CreateButton(add);
                    selectButton.Click += SelectButton_Click;
                    _panelMain.Children.Add(selectButton);
                }
                else
                {
                    break;
                }
            }
            RaiseEvent(new RoutedEventArgs(NextPageEvent, this));
        }

        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(e.OriginalSource is RadioButton button)) return;
            PageIndexProperty = int.Parse(button.Tag.ToString());
            Update();
        }

        private RadioButton CreateButton(int page)
        {
            return new RadioButton
            {
                Style = (Style)this.FindResource("PaginationButtonStyle"),
                Tag = page.ToString(),
            };
        }
        #endregion

        private void _buttonLeft_Click(object sender, RoutedEventArgs e)
        {
            PageIndexProperty--;
            Update();
        }

        private void _buttonRight_Click(object sender, RoutedEventArgs e)
        {
            PageIndexProperty++;
            Update();
        }
    }
}
