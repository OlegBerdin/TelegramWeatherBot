using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using static TelegramWeatherBot.TelegramBotClass;

namespace TelegramWeatherBot
{
    internal class Program
    {
        public static HttpClient client = new HttpClient();
        public static readonly string tg_key = "апи кей";
        public static string weather_key = "564a680f255d1ae9e43436060ac2ed1a";
        static async Task Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            int offset = 0;
            while (true)
            {
                var usec = new HttpClient();

                var response = await usec.GetAsync($"https://api.telegram.org/bot{HttpUtility.UrlEncode(tg_key)}/getUpdates?offset={offset}");
                if (response.IsSuccessStatusCode)
                {
                    var tgresult = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(tgresult);
                    var model = JsonConvert.DeserializeObject<Rootobject>(tgresult);
                    if (model.result.Length > 0)
                        offset = model.result.Last().update_id + 1;
                    foreach (var i in model.result)
                    {
                        if (i.message != null)
                            SendMessageToUser(i.message);
                        else
                            SendMessageToUser(i.edited_message);
                    }
                    Thread.Sleep(1000);
                }
            }
        }
        async static void SendMessageToUser(Message message)
        {
            string message_text;
            
                if (message.text.ToLower() == "/start")
                {
                    message_text = "Привет, напиши город, в котором хочешь узнать погоду, а я тебе скажу что там за окном";
                    await client.GetAsync(
                        $"https://api.telegram.org/bot{HttpUtility.UrlEncode(tg_key)}"
                        + $"/sendMessage?chat_id={message.chat.id}&text={HttpUtility.UrlEncode(message_text)}");
                    return;
                }
                var responseWheather = await client.GetAsync($"https://api.openweathermap.org/data/2.5/forecast?q={HttpUtility.UrlEncode(message.text)}&appid={weather_key}&units=metric&lang=ru");
                if (responseWheather.IsSuccessStatusCode)
                {
                    var weather = await responseWheather.Content.ReadAsStringAsync();
                    var model = JsonConvert.DeserializeObject<WeatherClass>(weather);
                    message_text = (
                    $"Погодка в городе таком: {model.city.name}, на {DateTime.Now} - {model.list[0].weather[0].description}\n" +
                    $"Дегризы {Math.Round(model.list[0].main.temp, 1)}°С\n" +
                    $"По ощущениям {Math.Round(model.list[0].main.feels_like, 1)}°С\n" +
                    $"Влажность воздуха - {model.list[0].main.humidity}%\n" +
                    $"Давление атмосферы - {Math.Round(model.list[0].main.grnd_level / 1.33322, 2)} мм\n\n");
                }

                else
                {
                    message_text = "Увы, я не знаю такой город";
                }
                await client.GetAsync($"https://api.telegram.org/bot{HttpUtility.UrlEncode(tg_key)}"
                    + $@"/sendMessage?chat_id={message.chat.id}&text={HttpUtility.UrlEncode(message_text)}");
        }
        
    }
}