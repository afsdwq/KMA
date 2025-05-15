using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using 기상청DLL; // 만든 DLL 네임스페이스를 참조

namespace 기상날씨앱
{
    public partial class MainWindow : Window
    {
        // DLL에서 제공하는 App 클래스의 인스턴스를 생성
        private Class1.App weatherApp = new Class1.App();
        public MainWindow()
        {
            InitializeComponent(); // WPF UI 초기화\
        }


        // "날씨 조회" 버튼 클릭 이벤트 핸들러
        private async void GetWeather_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!int.TryParse(TextBox1.Text.Trim(), out int x))
                {
                    MessageBox.Show("X좌표를 숫자로 입력해주세요.");
                    return;
                }

                if (!int.TryParse(TextBox2.Text.Trim(), out int y))
                {
                    MessageBox.Show("Y좌표를 숫자로 입력해주세요.");
                    return;
                }

                List<Class1.list> weatherData = await weatherApp.GetWeatherAsync(x, y);

                if (weatherData == null || weatherData.Count == 0)
                {
                    MessageBox.Show("날씨 데이터를 가져올 수 없습니다.");
                    return;
                }

                string currentTime = DateTime.Now.ToString("HHmm");

                var closest = weatherData
                    .Where(w => !string.IsNullOrEmpty(w.fcstTimeRaw))
                    .Select(w => new {
                        Data = w,
                        TimeDiff = Math.Abs(int.Parse(w.fcstTimeRaw) - int.Parse(currentTime))
                    })
                    .OrderBy(w => w.TimeDiff)
                    .FirstOrDefault();

                if (closest == null)
                {
                    MessageBox.Show("현재 시간에 대한 예보 데이터가 없습니다.");
                    return;
                }

                var data = closest.Data;

                // 데이터 바인딩
                RegionNameText.Text = data.RegionName;
                TimeText.Text = $"{data.fcstTime} 기준 예보";
                TempText.Text = string.IsNullOrEmpty(data.T1H) ? "-" : data.T1H;
                SkyText.Text = string.IsNullOrEmpty(data.SKY) ? "-" : data.SKY;
                PtyText.Text = string.IsNullOrEmpty(data.PTY) ? "-" : data.PTY;
                HumidityText.Text = string.IsNullOrEmpty(data.REH) ? "-" : data.REH;

                if (!string.IsNullOrEmpty(data.Img))
                    WeatherImage.Source = new BitmapImage(new Uri(data.Img));

                WeatherCard.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                MessageBox.Show("오류 발생: " + ex.Message);
            }
        }
    } 
}
