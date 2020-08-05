# Spotify Mixer
 
 Данное приложение позволяет создавать свои плейлисты, комбинируя музыку из Spotify и локальную библиотеку  
 На данный момент у Spotify есть лимит на количество треков в одном плейлисте - 10000, поэтому в данной программе можно объединять несколько плейлистов спотифая в один и слушать их вместе.  
 Поддерживаемые аудиоформаты локальных треков: `mp3, flac`
#### Для разработчиков:
1. Зарегистрируйте свое приложение в [Spotify Dashboard](https://developer.spotify.com/dashboard/applications)
2. В настройках приложения добавьте http://localhost:1234 в Redirect URIs
3. На главной странице приложения вам понадобятся `Client ID` и `Client Secret`
4. Вставьте эти значения в файл `SpotifyApp.json`
5. Разработка нового функционала ведется в своих отдельных ветках, добавление через Pull Requests

Фреймворк: .NetFramework 4.7.2  
Необходимые библиотеки (некоторые подключаются как зависимости):
1. MaterialDesignThemes
2. MaterialDesignColors (зависимость)
3. EmbedIO (зависимость)
4. NAudio
5. NAudio.Flac
6. Newtonsoft.Json (зависимость)
7. NLog
8. SpotifyAPI.Web
9. SpotifyAPI.Web.Auth
10. TagLibSharp
11. Unosquare.Swan.Lite (зависимость)