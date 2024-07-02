# voice-tg-bot

This project is a chatbot developed using .NET6 that can recognize voice from voice messages and vide notes. The chatbot processes the audio, converts it to text, and sends the text back to the user.

### Getting Started

Follow these steps to set up and run the project on your local machine for development and testing purposes.

1. Clone the repository:
   ```sh
   git clone https://github.com/opimando/voice-tg-bot.git
   cd voice-tg-bot
   ```
2. Build the Docker image.
3. Copy the docker-compose.yml file located at src/docker-compose.yml to your project root or preferred directory.
4. Update the docker-compose.yml file to include your API keys:
 ```dyaml
services:
  voicebot:
    image: voicebot
    environment:
      - TgToken=your-telegram-bot-token
```
### Acknowledgments

* [TgBotFramework](https://github.com/opimando/TelegramBotFramework)
* [Vosk](https://alphacephei.com/vosk/)