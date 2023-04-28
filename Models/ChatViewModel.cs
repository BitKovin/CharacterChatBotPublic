using OpenAI_API.Chat;

namespace CharacterChatBot.Models
{
    public class ChatViewModel
    {

        public List<ChatMessage> messages { get; set; } = new List<ChatMessage>();
        public string BotName { get; set; } = "Bot";
        public string ImageUrl { get; set; } = "https://icon-library.com/images/no-profile-pic-icon/no-profile-pic-icon-24.jpg";

        public string CharacterDescription { get;set; }
        public string CharacterGender { get; set; }
        public string CharacterOrigin { get; set; }
        public string ConversationStart { get; set; }

        public string ChatId { get; set; }

        public Conversation conversation;

        public class ChatMessage
        {
            public Guid id { get; set; }
            public string content { get; set; } = "";
            public bool isBot { get; set; }

            public int role = -1;

        }

        public class ChatSave
        {
            public ChatMessage[] messages { get; set; }
            public string BotName { get; set; } = "Bot";
            public string ImageUrl { get; set; } = "https://icon-library.com/images/no-profile-pic-icon/no-profile-pic-icon-24.jpg";

            public string ChatId { get; set; }

            public string CharacterDescription { get; set; }
            public string CharacterGender { get; set; }
            public string CharacterOrigin { get; set; }
            public string ConversationStart { get; set; }

        }

    }

    

}
