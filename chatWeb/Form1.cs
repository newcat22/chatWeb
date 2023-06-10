using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace chatWeb
{
    public partial class Form1 : Form
    {

        private HubConnection connection;
        public Form1()
        {
            InitializeComponent();
            


        }

        public void chatHub1(string id)
        {


            // 创建连接
            // 创建一个 SignalR 连接
             connection = new HubConnectionBuilder()
                .WithUrl("https://localhost:7106/chatOneFriend", options =>
                {
                    options.Headers.Add("Id", id);
                    options.HttpMessageHandlerFactory = _ =>
                    {
                        var handler = new HttpClientHandler();
                        var container1 = new CookieContainer();
                        // 添加已保存的 cookie
                        handler.CookieContainer = container1;
                        //handler.CookieContainer.Add(new Cookie("test","123"));
                        handler.CookieContainer.Add(new Uri("https://localhost:7106"), new Cookie("test", "123"));
                        // 允许跨域请求携带 cookies
                        handler.UseCookies = true;
                        return handler;
                    };
                })
                .Build();

            // 接收消息
            connection.On<string, string>("ReceiveMessage", (userId, message) =>
            {
                this.Invoke((Action)(() =>
                {
                    Console.WriteLine("456");
                    // 更新 UI
                    //txtMessages.AppendText($"用户 {userId}: {message}\r\n");
                }));
            });

            // 启动连接
            connection.StartAsync().ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    MessageBox.Show($"连接失败: {task.Exception.GetBaseException()}");
                }
                else
                {
                    Console.WriteLine("123");
                    //btnSend.Enabled = true;
                }
            });

        }


        private void button1_Click(object sender, EventArgs e)
        {


            // 创建一个 HttpClient 实例
            HttpClient client = new HttpClient();

            // 构造 URL
            string url = "https://localhost:7106/Home/loginUser?name=xulong&password=123";

            try
            {
                // 发送 GET 请求
                HttpResponseMessage response = client.GetAsync(url).Result;

                // 确保 HTTP 响应状态码为成功
                response.EnsureSuccessStatusCode();

                // 读取响应内容
                string responseBody = response.Content.ReadAsStringAsync().Result;


                // 从 Set-Cookie 头中获取 用户的Id
                var setCookieHeader = response.Headers.GetValues("Set-Cookie").FirstOrDefault();
                string cookie = setCookieHeader;
                string[] pairs = cookie.Split(';');

                string id = "";

                foreach (var pair in pairs)
                {
                    string[] parts = pair.Split('=');
                    string key = parts[0].Trim();
                    if (key == "Id")
                    {
                        id = parts[1]; // "3c7054cb-98c4-4015-bd63-ceb6bdb6f1b6"
                        break;
                    }
                }

                //创建singalR链接 
                chatHub1(id);


                // 将 JSON 字符串解析为 UserToFriend 对象
                //UserToFriend userToFriend = JsonConvert.DeserializeObject<UserToFriend>(responseBody);

                // 返回 UserToFriend 对象
                //return userToFriend;
                return;
            }
            catch (Exception ex)
            {
                // 在这里处理请求错误，例如显示错误消息
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", ex.Message);
                return ;
            }

        }

        private void button3_Click(object sender, EventArgs e)
        {

            // 发送消息
            //var userId = txtUserId.Text;
            //var message = txtMessage.Text;


            /*
             3c7054cb-98c4-4015-bd63-ceb6bdb6f1b6：接受信息用户id
             3fcd8626-1dd8-48a2-bf73-3d161f776122：发送信息用户id
             我是消息：发送的消息                         
             */
            connection.InvokeAsync("SendMessage", "3c7054cb-98c4-4015-bd63-ceb6bdb6f1b6", "3fcd8626-1dd8-48a2-bf73-3d161f776122", "我是消息")
                .ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        MessageBox.Show($"发送失败: {task.Exception.GetBaseException()}");
                    }
                });


        }
    }
}
