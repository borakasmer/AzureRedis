using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using azureNews.Models;
using Microsoft.Extensions.Configuration;
using ServiceStack.Redis;

namespace azureNews.Controllers
{
    public class HomeController : Controller
    {
        IConfiguration _configuration;
        RedisEndpoint conf;
        public static List<NewsModel> newsModel = new List<NewsModel>();
        public HomeController(IConfiguration configuration)
        {
            _configuration = configuration;
            conf = new RedisEndpoint() { Host = _configuration.GetValue<string>("RedisConfig:Host"), Password = _configuration.GetValue<string>("RedisConfig:Password"), Port = _configuration.GetValue<int>("RedisConfig:Port") };

            #region Dummy Data
            if (newsModel.Count == 0)
            {
                newsModel.AddRange(new NewsModel[]
                {
                new NewsModel(){
                Id = 1,
                title = "What does it actually mean for a commercial plane to hit 801 mph?",
                description = @"That’s extraordinarily fast, and in fact, the plane reportedly landed early, a nice perk for everyone on board.",
                createdDate = DateTime.Now,
                imageUrl="ross-parmly.jpg"
                },
                new NewsModel(){
                Id = 2,
                title = "Someone built an electric Harley-Davidson motorcycle in 1978",
                description = @"Let's turn back the clock hands to 1978, when Steve Fehr of the Transitron Electronic Corporation built a one-off Harley-Davidson MK2 electric motorcycle prototype.",
                createdDate = DateTime.Now,
                imageUrl="electric_teaser.jpg"
                },
                new NewsModel(){
                Id = 3,
                title = "This fisheye lens weighs more than 25 pounds and can see behind itself.",
                description = @"Lenses don’t have to be complicated. In fact, you can punch a pinhole in an old oatmeal canister, add some film on the other side, and start making pictures.",
                createdDate = DateTime.Now,
                imageUrl="fisheye_handheld.jpg"
                },
                 new NewsModel(){
                Id = 4,
                title = "This copy of Super Mario Bros. sold for more than $100,000",
                description = @"TVideo game collecting is serious business. Recently, a pristine copy of Super Mario Bros. broke the $100,000 mark at auction.",
                createdDate = DateTime.Now,
                imageUrl="dims-7_0.jpg"
                }
                });
            }
            #endregion

            using (IRedisClient client = new RedisClient(conf))
            {
                if (client.SearchKeys("flag*").Count == 0)
                {
                    foreach (NewsModel data in newsModel)
                    {
                        var news = client.As<NewsModel>();
                        news.SetValue("flag" + data.Id, data);
                    }
                }
            }
        }
        public IActionResult Index()
        {
            using (IRedisClient client = new RedisClient(conf))
            {
                List<NewsModel> dataList = new List<NewsModel>();
                List<string> allKeys = client.SearchKeys("flag*");
                foreach (string key in allKeys)
                {
                    dataList.Add(client.Get<NewsModel>(key));
                }
                return View(dataList);
            }
        }

        public IActionResult Detail(string Title, int ID)
        {
            using (IRedisClient client = new RedisClient(conf))
            {
                var data = client.Get<NewsModel>($"flag{ID}");
                data = data == null ? new NewsModel() : data;
                return View(data);
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
