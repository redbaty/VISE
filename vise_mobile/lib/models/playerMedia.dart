import 'dart:convert';

List<PlayerMedia> playerMediaListFromJson(String str) => List<PlayerMedia>.from(json.decode(str).map((x) => PlayerMedia.fromJson(x)));

String playerMediaListToJson(List<PlayerMedia> data) => json.encode(List<dynamic>.from(data.map((x) => x.toJson())));

PlayerMedia playerMediaFromJson(String str) => PlayerMedia.fromJson(json.decode(str));

String playerMediaToJson(PlayerMedia data) => json.encode(data.toJson());

class PlayerMedia {
    bool isCurrent;
    String title;
    String artist;
    String fileId;
    dynamic mediaImage;
    Map<String, double> currentTime;
    Map<String, double> duration;

    PlayerMedia({
        this.isCurrent,
        this.title,
        this.artist,
        this.fileId,
        this.mediaImage,
        this.currentTime,
        this.duration,
    });

    factory PlayerMedia.fromJson(Map<String, dynamic> json) => PlayerMedia(
        isCurrent: json["isCurrent"],
        title: json["title"],
        artist: json["artist"],
        fileId: json["fileId"],
        mediaImage: json["mediaImage"],
        currentTime: Map.from(json["currentTime"]).map((k, v) => MapEntry<String, double>(k, v.toDouble())),
        duration: Map.from(json["duration"]).map((k, v) => MapEntry<String, double>(k, v.toDouble())),
    );

    Map<String, dynamic> toJson() => {
        "isCurrent": isCurrent,
        "title": title,
        "artist": artist,
        "fileId": fileId,
        "mediaImage": mediaImage,
        "currentTime": Map.from(currentTime).map((k, v) => MapEntry<String, dynamic>(k, v)),
        "duration": Map.from(duration).map((k, v) => MapEntry<String, dynamic>(k, v)),
    };
}