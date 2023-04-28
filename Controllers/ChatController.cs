using CharacterChatBot.Models;
using GPT_Test;
using Microsoft.AspNetCore.Mvc;
using OpenAI_API.Chat;
using OpenAI_API.Models;
using System.Xml;
using System.Xml.Serialization;

namespace CharacterChatBot.Controllers
{
    public class ChatController : Controller
    {

        static Dictionary<string, ChatViewModel> Chats = new Dictionary<string, ChatViewModel>();

        static OpenAIChatbot ChatBot = new OpenAIChatbot(<apiKeyHere>);

        static bool loaded = false;

        string userDataPath = "Data.txt";


        public async Task<IActionResult> Send(string id, string message)
        {

            if(message==null)
                message= string.Empty;
            string result = await ChatBot.GenerateResponse(Chats[id].conversation, message);
            Save();

            return RedirectToAction("Index", new { ChatID = id });
        }

        public IActionResult Index(string ChatId = "")
        {

            if (loaded == false)
            {
                Load();
                loaded = true;
            }

            if (Chats.ContainsKey(ChatId))
            {
                ChatViewModel model = GetModelFromId(ChatId);


                return View(model);
            }
            else
            {
                return RedirectToAction("SelectChat");
            }
        }

        public IActionResult CreateChat()
        {
            return View();
        }

        public IActionResult Create(string id, string name, string gender, string desc, string univerce, string pfp)
        {
            if (Chats.ContainsKey(id)) return View("Index", GetModelFromId(id));

            ChatViewModel model = new ChatViewModel();
            model.BotName = name;
            model.conversation = ChatBot.CreateChat(name, gender, desc, univerce);
            model.messages = new List<ChatViewModel.ChatMessage>();
            model.ChatId = id;
            model.ImageUrl = pfp;
            model.CharacterGender = gender;
            model.CharacterOrigin = univerce;
            model.CharacterDescription = desc;

            foreach (var message in model.conversation.Messages)
            {
                model.messages.Add(new ChatViewModel.ChatMessage { content = message.Content, isBot = message.Role != ChatMessageRole.User });
            }

            Chats.Add(id, model);
            return RedirectToAction("Index", new { ChatId = model.ChatId });

        }

        ChatViewModel GetModelFromId(string id)
        {
            if (!Chats.ContainsKey(id)) return null;

            Chats[id].messages.Clear();
            bool SkipFirst = true;
            foreach (var message in Chats[id].conversation.Messages)
            {
                if (SkipFirst)
                {
                    SkipFirst = false;
                    continue;
                }

                if (message.Content == null) continue;

                if (message.Role != ChatMessageRole.User)
                {
                    string[] msg = message.Content.Split(new string[] { "<state>" }, StringSplitOptions.TrimEntries);
                    Chats[id].messages.Add(new ChatViewModel.ChatMessage { content = msg[0], isBot = message.Role != ChatMessageRole.User });
                }
                else
                {
                    Chats[id].messages.Add(new ChatViewModel.ChatMessage { content = message.Content, isBot = message.Role != ChatMessageRole.User });
                }

            }


            return Chats[id];

        }

        public IActionResult SelectChat()
        {
            return View();
        }

        void Save()
        {
            XmlSerializer ser = new XmlSerializer(typeof(ChatViewModel.ChatSave[]));

            if (!System.IO.File.Exists(userDataPath))
            {
                System.IO.File.Create(userDataPath);
            }
            TextWriter writer = new StreamWriter(userDataPath);
            try
            {
                List<ChatViewModel.ChatSave> saves = new List<ChatViewModel.ChatSave>();

                foreach (ChatViewModel model in Chats.Values)
                {
                    var messages = new List<CharacterChatBot.Models.ChatViewModel.ChatMessage>();

                    foreach (var message in model.conversation.Messages)
                    {
                        int role;

                        if (message.Role == ChatMessageRole.User)
                        {
                            role = 1;
                        }
                        else if (message.Role == ChatMessageRole.System)
                        {
                            role = 0;
                        }
                        else
                        {
                            role = 2;
                        }

                        messages.Add(new ChatViewModel.ChatMessage { content = message.Content, role = role });
                    }



                    saves.Add(new ChatViewModel.ChatSave { ChatId = model.ChatId, ImageUrl = model.ImageUrl, BotName = model.BotName, messages = messages.ToArray(), CharacterDescription = model.CharacterDescription, CharacterGender = model.CharacterGender, CharacterOrigin = model.CharacterOrigin, ConversationStart = model.ConversationStart });
                }

                ser.Serialize(writer, saves.ToArray());
            }
            catch { }
            writer.Close();
        }

        void Load()
        {

            XmlSerializer ser = new XmlSerializer(typeof(ChatViewModel.ChatSave[]));
            using (XmlReader reader = XmlReader.Create(userDataPath))
            {
                ChatViewModel.ChatSave[] saves = ser.Deserialize(reader) as ChatViewModel.ChatSave[];

                Chats.Clear();

                foreach (ChatViewModel.ChatSave save in saves)
                {
                    ChatViewModel chat = new ChatViewModel();
                    chat.conversation = ChatBot.CreateChat(save.BotName, save.CharacterGender, save.CharacterDescription, save.CharacterOrigin, save.ConversationStart);
                    foreach (var message in save.messages)
                    {

                        ChatMessage msg = new ChatMessage();

                        ChatMessageRole role = ChatMessageRole.Assistant;

                        if (message.role == 1)
                        {
                            role = ChatMessageRole.User;
                        }
                        else if (message.role == 0)
                        {
                            role = ChatMessageRole.System;

                        }

                        msg.Content = message.content;
                        msg.Role = role;


                        if (role != ChatMessageRole.System || msg.Content == null)
                            chat.conversation.AppendMessage(msg);
                    }

                    chat.BotName = save.BotName;
                    chat.ImageUrl = save.ImageUrl;
                    chat.ChatId = save.ChatId;
                    Chats.Add(save.ChatId, chat);

                }

            }

        }

    }


}
