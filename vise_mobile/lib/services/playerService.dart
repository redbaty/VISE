import 'package:http/http.dart' as http;
import 'package:vise_mobile/models/playerMedia.dart';

final baseUrl = 'http://192.168.0.24:5000';

class PlaylistService {
  Future<List<PlayerMedia>> getPlaylist() async {
    final url = '$baseUrl/player/playlist';
    final response = await http.get(url);

    if (response.statusCode == 200) {
      return playerMediaListFromJson(response.body);
    }

    return null;
  }

  Future<PlayerMedia> getCurrent() {
    final url = '$baseUrl/player/playlist/current';

    return http.get(url).then((response) {
      if (response.statusCode == 200) {
        return playerMediaFromJson(response.body);
      }

      return null;
    }, onError: (e) {
      return null;
    });
  }
}

class PlayerService {
  final apiUrl;

  PlayerService(this.apiUrl);

  play() {
    return http.get('$apiUrl/player/play');
  }

  pause() {
    return http.get('$apiUrl/player/pause');
  }

  stop() {
    return http.get('$apiUrl/player/stop');
  }

  next() {
    return http.get('$apiUrl/player/next');
  }

  previous() {
    return http.get('$apiUrl/player/previous');
  }

  toggleRepeat() {
    return http.get('$apiUrl/player/toggle-repeat');
  }
}
