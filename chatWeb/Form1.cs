using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using SignalRWebApp.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Windows.Forms;
namespace chatWeb
{
    public partial class Form1 : Form
    {

        private HubConnection connection;
        //当前登录用户
        private UserInfo userInfoCurrent;
        //当前登录用户的好友
        private List<UserInfo> friendsInfo;
        //选中当前聊天用户
        private UserInfo talkUserInfo;
        //选中当前添加好友
        private UserInfo addUserInfo;

        public Form1()
        {
            InitializeComponent();


        }

        /// <summary>
        /// 建立SignalR链接，注意每个用户都需要建立一个链接。
        /// </summary>
        /// <param name="id"></param>
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
                    //遍历好友列表，找到是哪位好友给自己发消息
                    friendsInfo.ForEach(friend =>
                    {
                        if (friend.Id.Equals(userId))
                        {
                            //渲染聊天框,;聊天框实现方式：可以给每个好友一个聊天框。
                            //
                            



                        }

                    });




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


        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {


            // 1 创建一个 HttpClient 实例
            HttpClient client = new HttpClient();
            string name = "";
            string password = "";

            // 2 构造URL
            string url = "https://localhost:7106/Home/loginUser?name=xulong&password=123";
            //string url = "https://localhost:7106/Home/loginUser?name="+name+"&password="+password+"";
            try
            {
                // 3 发送 GET 请求
                HttpResponseMessage response = client.GetAsync(url).Result;

                // 4 确保 HTTP 响应状态码为成功
                response.EnsureSuccessStatusCode();

                //5 读取响应内容
                string responseBody = response.Content.ReadAsStringAsync().Result;
                //6 将 JSON 字符串解析为 ResultBean 对象
                ResultBean resultBean = JsonConvert.DeserializeObject<ResultBean>(responseBody);

                if (resultBean.Success)
                {
                    // 6 从 Set-Cookie 头中获取 用户的Id
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
                    //7 创建singalR链接:只有登录成功才需要创建。 
                    chatHub1(id);
                    // 首先将 Data 属性序列化为 JSON 字符串
                    string jsonUserInfo = JsonConvert.SerializeObject(resultBean.Data);
                    // 然后反序列化为 UserInfo 对象
                    UserInfo userInfo = JsonConvert.DeserializeObject<UserInfo>(jsonUserInfo);
                    // 赋值给当前用户
                    userInfoCurrent = userInfo;
                    //8 使用userInfoCurrent渲染UI，取出信息放在页面的某个文本框中


                }
                else
                {
                    //登录失败，弹窗提示重新登录

                }
                return;
            }
            catch (Exception ex)
            {
                // 在这里处理请求错误，例如显示错误消息
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", ex.Message);
                return;
            }

        }


        /// <summary>
        /// 好友列表选中事件：聊天
        /// </summary>        
        public void selectTalkAction() 
        { 
            //1 双击好友列表的好友，获取好友的UserInfo
            UserInfo userInfo = null;

            //2 将好友的UserInfo赋值给当前聊天对象
            talkUserInfo = userInfo;
        
        }


        /// <summary>
        /// 好友列表选中事件:添加好友
        /// </summary>        
        public void selectAddAction()
        {
            //1 双击好友列表的好友，获取好友的UserInfo
            UserInfo userInfo = null;

            //2 将好友的UserInfo赋值给当前聊天对象
            addUserInfo = userInfo;

        }


        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {

            // 发送消息
            //var userId = txtUserId.Text;
            //var message = txtMessage.Text;

            string receiveId = talkUserInfo.Id;
            string sendId = userInfoCurrent.Id;
            string message = "我是消息";
            /*
             3c7054cb-98c4-4015-bd63-ceb6bdb6f1b6：接受信息用户id
             3fcd8626-1dd8-48a2-bf73-3d161f776122：发送信息用户id
             我是消息：发送的消息                         
             */
            connection.InvokeAsync("SendMessage", receiveId, sendId, message)
                .ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        MessageBox.Show($"发送失败: {task.Exception.GetBaseException()}");
                    }
                });


        }


        /// <summary>
        /// 注册
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {
            // 1 创建一个 HttpClient 实例
            HttpClient client = new HttpClient();

            // 2 构造URL
            string url = "https://localhost:7106/Home/registerUser?name=xulong&password=123";
            try
            {
                // 3 发送 GET 请求
                HttpResponseMessage response = client.GetAsync(url).Result;

                // 4 确保 HTTP 响应状态码为成功
                response.EnsureSuccessStatusCode();

                // 5 读取响应内容
                string responseBody = response.Content.ReadAsStringAsync().Result;


                //6 将 JSON 字符串解析为 resultBean 对象
                ResultBean resultBean = JsonConvert.DeserializeObject<ResultBean>(responseBody);

                //7 前端渲染结果，注册成功或者注册失败  弹框               
                if (resultBean.Success)
                {

                }
                else
                {

                }

                return;
            }
            catch (Exception ex)
            {
                // 在这里处理请求错误，例如显示错误消息
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", ex.Message);
                return;
            }
        }

        /// <summary>
        /// 查询好友列表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button6_Click(object sender, EventArgs e)
        {

            // 1 创建一个 HttpClient 实例
            HttpClient client = new HttpClient();
            string userId = userInfoCurrent.Id;
            // 2 构造URL
            string url = "https://localhost:7106/Home/selectFriend?userId=" + userId + "";
            try
            {
                // 3 发送 GET 请求
                HttpResponseMessage response = client.GetAsync(url).Result;

                // 4 确保 HTTP 响应状态码为成功
                response.EnsureSuccessStatusCode();

                // 5 读取响应内容
                string responseBody = response.Content.ReadAsStringAsync().Result;


                //6 将 JSON 字符串解析为 ResultBean 对象
                ResultBean resultBean = JsonConvert.DeserializeObject<ResultBean>(responseBody);

                //7 前端渲染结果
                if (resultBean.Success)
                {
                    //查询成功展示好友列表
                    // 首先将 Data 属性序列化为 JSON 字符串
                    string jsonUserInfo = JsonConvert.SerializeObject(resultBean.Data);
                    // 然后反序列化为 UserInfo 对象
                    List<UserInfo> friendList = JsonConvert.DeserializeObject<List<UserInfo>>(jsonUserInfo);
                    // 赋值给当前用户的好友
                    friendsInfo = friendList;

                    //8 使用friendsInfo渲染好友列表

                }
                else
                {
                    //查询无好友


                }

                return;
            }
            catch (Exception ex)
            {
                // 在这里处理请求错误，例如显示错误消息
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", ex.Message);
                return;
            }


        }

        /// <summary>
        /// 查询全部用户
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button5_Click(object sender, EventArgs e)
        {
            // 1 创建一个 HttpClient 实例
            HttpClient client = new HttpClient();
            string userId = userInfoCurrent.Id;
            // 2 构造URL
            string url = "https://localhost:7106/Home/selectUser";
            try
            {
                // 3 发送 GET 请求
                HttpResponseMessage response = client.GetAsync(url).Result;

                // 4 确保 HTTP 响应状态码为成功
                response.EnsureSuccessStatusCode();

                // 5 读取响应内容
                string responseBody = response.Content.ReadAsStringAsync().Result;


                //6 将 JSON 字符串解析为 ResultBean 对象
                ResultBean resultBean = JsonConvert.DeserializeObject<ResultBean>(responseBody);

                //7 前端渲染结果
                if (resultBean.Success)
                {
                    //查询成功
                    // 首先将 Data 属性序列化为 JSON 字符串
                    string jsonUserInfo = JsonConvert.SerializeObject(resultBean.Data);
                    // 然后反序列化为 UserInfo 对象
                    List<UserInfo> userInfoList = JsonConvert.DeserializeObject<List<UserInfo>>(jsonUserInfo);



                    //8 使用userInfoList渲染全部用户

                }
                else
                {
                    //查询无好友


                }

                return;
            }
            catch (Exception ex)
            {
                // 在这里处理请求错误，例如显示错误消息
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", ex.Message);
                return;
            }
        }

        /// <summary>
        /// 添加好友
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button7_Click(object sender, EventArgs e)
        {
            // 1 创建一个 HttpClient 实例
            HttpClient client = new HttpClient();
            string userId = userInfoCurrent.Id;
            //双击选择添加好友,获取双击得到的对象，得到Id  
            string friendId = addUserInfo.Id;

            // 2 构造URL
            string url = "https://localhost:7106/Home/addFriend?userId=userId&friendId=friendId";
            try
            {
                // 3 发送 GET 请求
                HttpResponseMessage response = client.GetAsync(url).Result;

                // 4 确保 HTTP 响应状态码为成功
                response.EnsureSuccessStatusCode();

                // 5 读取响应内容
                string responseBody = response.Content.ReadAsStringAsync().Result;


                //6 将 JSON 字符串解析为 resultBean 对象
                ResultBean resultBean = JsonConvert.DeserializeObject<ResultBean>(responseBody);

                //7 前端渲染结果，注册成功或者注册失败  弹框               
                if (resultBean.Success)
                {

                }
                else
                {

                }

                return;
            }
            catch (Exception ex)
            {
                // 在这里处理请求错误，例如显示错误消息
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", ex.Message);
                return;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    } 
}
